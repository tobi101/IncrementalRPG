using System;
using System.Collections.Generic;
using System.Linq;
using Core.Gameplay.Dungeon;
using Core.Items;
using Core.Save;
using Model;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Core.Forge
{
    public enum ForgeOutcome { None, OneStat, AllStats, Rarity }

    [Serializable]
    public sealed class ForgeAttempt
    {
        public PlayerItemInstanceState Item;
        public int Level;
        public string ScrollDefinitionId;
        public float Precision;
        public float Empowerment;
        public float CursorTravel;
        public bool Resolved;
        public ForgeOutcome Outcome;
        // Snapshot the paid attempt's rules so a balance edit cannot change it after loading.
        public ForgeLevelSettings Settings;
        public float ExtraStatMin;
        public float ExtraStatMax;
        public float GoldPerBonusPercent;
    }

    public interface IForgeInventoryGateway
    {
        bool HasForgeSpace();
        bool TryStoreForgedItem(PlayerItemInstanceState item);
    }

    public sealed class ForgeService : ISaveable
    {
        private readonly ForgeConfig _config;
        private readonly Player _player;
        private readonly PlayerItemStorage _storage;
        private readonly ItemCatalog _catalog;
        private readonly DungeonSelectionService _progress;
        private readonly DungeonList _dungeons;
        private readonly IForgeInventoryGateway _inventory;
        public event Action OnChanged;
        public ForgeAttempt Pending { get; private set; }
        public ForgeConfig Config => _config;
        public float CursorPosition => Pending == null ? 0 : Mathf.PingPong(Pending.CursorTravel, 1f);

        public ForgeService(ForgeConfig config, Player player, PlayerItemStorage storage, ItemCatalog catalog,
            DungeonSelectionService progress, DungeonList dungeons, IForgeInventoryGateway inventory)
        { _config = config; _player = player; _storage = storage; _catalog = catalog; _progress = progress; _dungeons = dungeons; _inventory = inventory; }

        public int Level
        {
            get
            {
                var reached = 1;
                foreach (var dungeon in _dungeons.dungeons ?? Array.Empty<DungeonConfig>())
                    if (dungeon != null && dungeon.HasPlayableLevels)
                        reached = Mathf.Max(reached, _progress.GetStartLevelIndex(dungeon) + 1);
                return Mathf.Clamp(reached - _config.firstAvailableLocation + 1, 0, _config.levels.Length);
            }
        }

        public IEnumerable<(ItemDefinition Definition, int Count)> Scrolls => _storage.Items
            .Select(i => _catalog.TryGet(i.ItemDefinitionId, out var d) ? d : null)
            .Where(d => d != null && d.category == ItemCategory.Scroll && IsSupportedScroll(d))
            .GroupBy(d => d).OrderBy(g => g.Key.itemId).Select(g => (g.Key, g.Count()));

        public string GetStartError(string scrollId)
        {
            if (Pending != null) return "forge.busy";
            if (!_config.IsValid()) return "forge.invalid_config";
            if (Level == 0) return "forge.locked";
            if (_player.ShardTotal < _config.GetLevel(Level).shardCost) return "forge.no_shards";
            if (!string.IsNullOrEmpty(scrollId) && !TryFindScroll(scrollId, out _, out _)) return "forge.no_scroll";
            if (!_inventory.HasForgeSpace()) return "forge.no_space";
            return null;
        }

        public bool TryStart(string scrollId, out string error)
        {
            error = GetStartError(scrollId);
            if (error != null) return false;
            ItemDefinition scroll = null;
            PlayerItemInstanceState scrollItem = null;
            if (!string.IsNullOrEmpty(scrollId)) TryFindScroll(scrollId, out scrollItem, out scroll);
            var level = Level;
            var settings = _config.GetLevel(level);
            var candidates = _config.equipment;
            var slot = scroll?.forgeModifierId switch
            {
                "guaranteed_helmet" => EquipmentSlot.Helmet,
                "guaranteed_chest" => EquipmentSlot.Chest,
                "guaranteed_weapon" => EquipmentSlot.Weapon,
                _ => EquipmentSlot.None
            };
            var definition = slot == EquipmentSlot.None ? candidates[Random.Range(0, candidates.Length)] : candidates.Single(d => d.equipmentSlot == slot);
            var rarity = scroll?.forgeModifierId == "guaranteed_rare" ? ItemRarity.Rare : RollRarity(settings);
            var stats = new List<ItemStatState>();
            var available = EquipmentStats.Ids.ToList();
            var range = _config.statRanges[(int)rarity];
            for (var i = 0; i < (int)rarity + 2; i++) AddStat(stats, available, range);
            var empowerment = scroll?.forgeModifierId == "stat_strength" ? scroll.forgeModifierValue : 0f;
            foreach (var stat in stats) stat.Value *= 1f + empowerment;
            var item = _storage.Create(definition, rarity, stats, Price(stats, _config.goldPerBonusPercent));
            if (definition.forgeNameKeys.Length > 0) item.RolledNameKey = definition.forgeNameKeys[Random.Range(0, definition.forgeNameKeys.Length)];
            if (definition.forgeIcons.Length > 0) item.RolledIconIndex = Random.Range(0, definition.forgeIcons.Length);
            var extraRange = _config.statRanges[Mathf.Min(3, (int)rarity + 1)];
            // All validation and random generation finish before either resource is spent.
            if (!_player.TrySpendShards(settings.shardCost)) { error = "forge.no_shards"; return false; }
            if (scrollItem != null) _storage.Consume(scrollItem.InstanceId);
            Pending = new ForgeAttempt
            {
                Item = item, Level = level, ScrollDefinitionId = scroll?.itemId, Precision = scroll?.forgeModifierId == "qte_precision" ? scroll.forgeModifierValue : 0f,
                Empowerment = empowerment, Settings = JsonUtility.FromJson<ForgeLevelSettings>(JsonUtility.ToJson(settings)),
                ExtraStatMin = extraRange.x, ExtraStatMax = extraRange.y, GoldPerBonusPercent = _config.goldPerBonusPercent
            };
            OnChanged?.Invoke();
            return true;
        }

        public void Advance(float deltaTime)
        {
            if (Pending == null || Pending.Resolved || deltaTime <= 0) return;
            Pending.CursorTravel = Mathf.Repeat(Pending.CursorTravel + Mathf.Min(deltaTime, .05f) * 2f / Pending.Settings.cursorPeriod, 2f);
        }

        public float CenterRange => Pending == null ? 0 : Mathf.Min(Pending.Settings.allStatsRange, Pending.Settings.rarityRange + Pending.Precision);

        public bool Stop()
        {
            if (Pending == null || Pending.Resolved) return false;
            var attempt = Pending;
            var distance = Mathf.Abs(CursorPosition - .5f) * 2f;
            attempt.Outcome = distance <= CenterRange ? ForgeOutcome.Rarity :
                distance <= attempt.Settings.allStatsRange ? ForgeOutcome.AllStats :
                distance <= attempt.Settings.oneStatRange ? ForgeOutcome.OneStat : ForgeOutcome.None;
            var stats = attempt.Item.Stats;
            switch (attempt.Outcome)
            {
                case ForgeOutcome.Rarity when attempt.Item.Rarity < ItemRarity.Legendary:
                    attempt.Item.Rarity++;
                    var available = EquipmentStats.Ids.Where(id => stats.All(s => s.StatId != id)).ToList();
                    AddStat(stats, available, new Vector2(attempt.ExtraStatMin, attempt.ExtraStatMax));
                    stats[stats.Count - 1].Value *= 1f + attempt.Empowerment;
                    break;
                case ForgeOutcome.Rarity: // Legendary is the cap; the perfect hit still improves the item.
                case ForgeOutcome.AllStats:
                    foreach (var stat in stats) stat.Value *= 1f + attempt.Settings.allStatsBonus;
                    break;
                case ForgeOutcome.OneStat:
                    stats[Random.Range(0, stats.Count)].Value *= 1f + attempt.Settings.oneStatBonus;
                    break;
            }
            attempt.Item.SellPrice = Price(stats, attempt.GoldPerBonusPercent);
            attempt.Resolved = true;
            OnChanged?.Invoke();
            return true;
        }

        public bool Take()
        {
            if (Pending == null || !Pending.Resolved || !_inventory.TryStoreForgedItem(Pending.Item)) return false;
            Pending = null;
            OnChanged?.Invoke();
            return true;
        }

        public bool Break()
        {
            if (Pending == null || !Pending.Resolved) return false;
            var price = Pending.Item.SellPrice;
            Pending = null;
            _player.GoldTotal += price;
            OnChanged?.Invoke();
            return true;
        }

        public void Persist() => OnChanged?.Invoke();
        public void Load(SaveData data)
        {
            // Unity serializes an absent inline class as an empty object. It is not a paid attempt.
            var attempt = data.ForgeAttempt;
            Pending = string.IsNullOrEmpty(attempt?.Item?.InstanceId) ? null : attempt;
        }
        public void Contribute(SaveData data) => data.ForgeAttempt = Pending;

        private bool TryFindScroll(string id, out PlayerItemInstanceState item, out ItemDefinition definition)
        {
            item = _storage.Items.FirstOrDefault(i => i.ItemDefinitionId == id);
            definition = null;
            return item != null && _catalog.TryGet(id, out definition) && definition.category == ItemCategory.Scroll && IsSupportedScroll(definition);
        }

        private static bool IsSupportedScroll(ItemDefinition d) =>
            d.forgeModifierId is "qte_precision" or "guaranteed_helmet" or "guaranteed_chest" or "guaranteed_weapon" or "guaranteed_rare" or "stat_strength";

        private static ItemRarity RollRarity(ForgeLevelSettings settings)
        {
            var roll = Random.value * settings.rarityWeights.Sum();
            var cumulative = 0f;
            for (var i = 0; i < 4; i++) { cumulative += settings.rarityWeights[i]; if (settings.rarityWeights[i] > 0 && roll < cumulative) return (ItemRarity)i; }
            for (var i = 3; i >= 0; i--) if (settings.rarityWeights[i] > 0) return (ItemRarity)i;
            return ItemRarity.Common;
        }

        private static void AddStat(List<ItemStatState> stats, List<string> available, Vector2 range)
        {
            var index = Random.Range(0, available.Count);
            stats.Add(new ItemStatState { StatId = available[index], Value = Mathf.Round(Random.Range(range.x, range.y) * 10000f) / 10000f });
            available.RemoveAt(index);
        }

        private static BigDouble Price(IEnumerable<ItemStatState> stats, float rate) => Math.Floor(stats.Sum(s => (double)s.Value) * 100d * rate + .00001d);
    }
}
