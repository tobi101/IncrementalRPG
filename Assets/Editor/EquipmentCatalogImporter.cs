using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Forge;
using Core.Items;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace IncrementalRPG.Editor
{
    public static class EquipmentCatalogImporter
    {
        public const string CatalogPath = "Assets/SO/Forge/EquipmentAppearances.asset";
        public const string ConfigPath = "Assets/SO/Forge/ForgeConfig.asset";

        [MenuItem("Tools/Items/Import Equipment Catalog and Balance")]
        public static void Import()
        {
            var input = Read<CatalogInput>("ItemCatalog.data.json");
            var translations = Read<TranslationInput>("ItemCatalog.localization.json");
            var balance = Read<BalanceInput>("ForgeBalance.data.json");
            if (input.items.Length != 36 || translations.entries.Length != 180 || balance.levels.Length != 4)
                throw new InvalidDataException("Expected 36 appearances, 180 names and four forge levels.");
            var entries = input.items.Select(item =>
            {
                var slot = (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), item.equipmentSlot);
                var sprites = AssetDatabase.LoadAllAssetsAtPath(item.imagePath).OfType<Sprite>().ToArray();
                if (sprites.Length != 1 || item.nameVariants.Length != 5)
                    throw new InvalidDataException($"Expected one sprite and five names for {item.imagePath}.");
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprites[0], out string guid, out long fileId);
                if (guid != item.textureGuid || fileId.ToString() != item.spriteFileId)
                    throw new InvalidDataException($"Sprite identity changed: {item.imagePath}.");
                return new EquipmentAppearance
                {
                    id = item.appearanceId,
                    slot = slot,
                    rarity = item.rarityRu switch
                    {
                        "Обычная" => ItemRarity.Common, "Редкая" => ItemRarity.Rare,
                        "Уникальная" => ItemRarity.Unique, "Легендарная" => ItemRarity.Legendary,
                        _ => throw new InvalidDataException("Unknown rarity: " + item.rarityRu)
                    },
                    icon = sprites[0],
                    nameKeys = item.nameVariants.Select(name => "equipment.name." + name.balanceNameId).ToArray()
                };
            }).ToArray();

            var catalog = AssetDatabase.LoadAssetAtPath<EquipmentAppearanceCatalog>(CatalogPath);
            var isNew = catalog == null;
            if (isNew) catalog = ScriptableObject.CreateInstance<EquipmentAppearanceCatalog>();
            catalog.appearances = entries;
            if (!catalog.IsValid()) throw new InvalidDataException("Invalid equipment appearance catalog.");
            if (isNew) AssetDatabase.CreateAsset(catalog, CatalogPath);
            EditorUtility.SetDirty(catalog);
            var changedAssets = new List<UnityEngine.Object> { catalog };

            var collection = LocalizationEditorSettings.GetStringTableCollection(ItemText.Table);
            if (collection == null) throw new InvalidDataException("Missing LocalesTable collection.");
            foreach (var locale in new[] { "ru-RU", "en-US", "zh" })
            {
                var table = collection.GetTable(new LocaleIdentifier(locale)) as StringTable;
                if (table == null) throw new InvalidDataException("Missing locale: " + locale);
                foreach (var entry in translations.entries)
                {
                    var value = locale == "ru-RU" ? entry.ru : locale == "en-US" ? entry.en : entry.zh;
                    if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException("Missing translation: " + entry.key);
                    table.AddEntry(entry.key, value);
                }
                EditorUtility.SetDirty(table);
                changedAssets.Add(table);
            }
            EditorUtility.SetDirty(collection.SharedData);
            changedAssets.Add(collection.SharedData);

            var config = AssetDatabase.LoadAssetAtPath<ForgeConfig>(ConfigPath);
            if (config == null || config.levels.Length != 4 || config.equipment.Length != 3)
                throw new InvalidDataException("Expected existing forge config and three base definitions.");
            foreach (var definition in config.equipment)
            {
                definition.appearanceCatalog = catalog;
                definition.icon = catalog.appearances.First(a => a.slot == definition.equipmentSlot && a.rarity == ItemRarity.Common).icon;
                EditorUtility.SetDirty(definition);
                changedAssets.Add(definition);
            }
            for (var i = 0; i < 4; i++)
            {
                var source = balance.levels[i];
                var target = config.levels[i];
                target.shardCost = source.shardCost;
                target.rarityWeights = source.rarityWeights;
                target.statBudgets = source.statBudgets;
                target.rarityRange = source.rarityRange;
                target.allStatsRange = source.allStatsRange;
                target.oneStatRange = source.oneStatRange;
            }
            config.goldPerBonusPercent = balance.ordinaryGoldPerBonusPercent;
            if (!config.IsValid()) throw new InvalidDataException("Imported forge config is invalid.");
            EditorUtility.SetDirty(config);
            changedAssets.Add(config);
            foreach (var asset in changedAssets) AssetDatabase.SaveAssetIfDirty(asset);
            Debug.Log("Imported 36 equipment appearances, 180 names in three languages, and four forge balance levels.");
        }

        private static T Read<T>(string filename) => JsonUtility.FromJson<T>(
            File.ReadAllText(Path.Combine(Application.dataPath, "..", "Docs", filename)));

#pragma warning disable 0649 // Populated by JsonUtility from the source catalog.
        [Serializable] private sealed class CatalogInput { public ItemInput[] items; }
        [Serializable] private sealed class ItemInput
        {
            public string appearanceId, equipmentSlot, imagePath, textureGuid, spriteFileId, rarityRu;
            public NameInput[] nameVariants;
        }
        [Serializable] private sealed class NameInput { public int balanceNameId; }
        [Serializable] private sealed class TranslationInput { public Translation[] entries; }
        [Serializable] private sealed class Translation { public string key, ru, en, zh; }
        [Serializable] private sealed class BalanceInput
        {
            public float ordinaryGoldPerBonusPercent;
            public LevelInput[] levels;
        }
        [Serializable] private sealed class LevelInput
        {
            public int shardCost;
            public float[] rarityWeights;
            public int[] statBudgets;
            public float rarityRange, allStatsRange, oneStatRange;
        }
#pragma warning restore 0649
    }
}
