using System.Collections.Generic;
using System.Reflection;
using Core.Gameplay;
using Core.Gameplay.Bomb;
using Core.Gameplay.Dungeon;
using Core.Gameplay.Shards;
using Core.Items;
using Core.Save;
using Core.TestSkillTree;
using Core.TestSkillTree.View;
using Model;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace IncrementalRPG.Tests
{
    public sealed class NodeUpgradePreviewTests
    {
        private readonly List<Object> _assets = new();
        private SkillTreeService _skills;
        private SkillTreeConfig _config;
        private DamageZone _zone;
        private BombExplosionService _bombs;
        private NodeUpgradePreviewText _text;
        private Player _player;
        private SaveData _save;
        private Locale _originalLocale;

        [SetUp]
        public void SetUp()
        {
            LocalizationSettings.InitializationOperation.WaitForCompletion();
            _originalLocale = LocalizationSettings.SelectedLocale;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en-US");
            _save = new SaveData();
            _player = new Player();
            _player.Load(_save);
            _player.GoldTotal = 10000;
            _config = Asset<SkillTreeConfig>();
            _skills = new SkillTreeService();
            Set(_skills, "_config", _config);
            Set(_skills, "_player", _player);
            var catalog = Asset<ItemCatalog>();
            var items = new PlayerItemStorage(catalog, Asset<Core.Forge.ForgeConfig>());
            items.Load(_save);
            var consumables = new RunConsumableService(items, catalog);
            var damage = Asset<DamageZoneConfig>();
            damage.baseAutoAttackInterval = 1f;
            damage.baseManualAttackDamage = 10;
            damage.baseRadius = 0.6f;
            _zone = new DamageZone(null, damage, null, null, _player, _skills, null, consumables, items);
            _bombs = new BombExplosionService(null, null, Asset<BombExplosionConfig>(), _skills, items);
            _text = new NodeUpgradePreviewText(_skills, _zone, _bombs, Asset<ShardPickupConfig>(),
                new DungeonSelectionService(), Asset<DungeonList>(), _player);
        }

        [TearDown]
        public void TearDown()
        {
            LocalizationSettings.SelectedLocale = _originalLocale;
            foreach (var asset in _assets) Object.DestroyImmediate(asset);
            _assets.Clear();
        }

        [Test]
        public void PreviewIncludesOtherNodesAndEveryMatchingEffectWithoutChangingProgress()
        {
            var parent = Node("parent", Effect(StatType.AutoAttackSpeed, 0.2f));
            var node = Node("speed", Effect(StatType.AutoAttackSpeed, 0.1f, 0.2f),
                Effect(StatType.AutoAttackSpeed, 0.05f, 0.1f));
            _save.SkillTreeState.SetLevel(parent.id, 1);
            _skills.Load(_save);
            var gold = _player.GoldTotal;
            var callbacks = 0;
            _skills.OnUpgraded += () => callbacks++;

            var expectedInterval = _zone.GetAutoAttackInterval(node);
            Assert.That(expectedInterval, Is.EqualTo(1f / 1.35f).Within(0.0001f));
            Assert.That(_text.GetText(node), Is.EqualTo("0.83 s → 0.74 s"));
            Assert.That(_skills.GetBonus(StatType.AutoAttackSpeed), Is.EqualTo(0.2f));
            Assert.That(_skills.GetLevel(node.id), Is.Zero);
            Assert.That(_player.GoldTotal, Is.EqualTo(gold));
            Assert.That(callbacks, Is.Zero);

            _skills.Upgrade(node.id);
            Assert.That(_zone.GetAutoAttackInterval(), Is.EqualTo(expectedInterval));
            Assert.That(_text.GetText(node), Is.EqualTo("0.74 s → 0.61 s"));
            _skills.Upgrade(node.id);
            Assert.That(_text.GetText(node), Is.EqualTo("0.61 s"));
        }

        [Test]
        public void MultipliersAddTheirNextIncrementAndShortEffectArraysStopAtTheirLastRank()
        {
            var node = Node("gold", Effect(StatType.GoldDrop, 0.1f));
            node.maxLevel = 2;
            node.effects[0].effectType = NodeEffectType.Multiplicative;
            _skills.Load(_save);
            Assert.That(_text.GetText(node), Is.EqualTo("×1 → ×1.1"));
            _skills.Upgrade(node.id);
            Assert.That(_skills.GetMultiplier(StatType.GoldDrop, node), Is.EqualTo(1.1f));
            Assert.That(_text.GetText(node), Is.EqualTo("×1.1 → ×1.1"));
        }

        [Test]
        public void DamageAndAdditiveRadiusPreviewsMatchTheValuesAfterPurchase()
        {
            var radius = Node("radius", Effect(StatType.ZoneRadius, 0.2f));
            var damage = Node("damage", Effect(StatType.ManualAttackDamage, 5f));
            var bomb = Node("bomb", Effect(StatType.BombExplosionDamage, 10f));
            _skills.Load(_save);
            Assert.That(_text.GetText(radius), Is.EqualTo("0.6 → 0.8"));
            Assert.That(_text.GetText(damage), Is.EqualTo("10 → 15"));
            var bombDamage = _bombs.GetDamage(bomb);
            _skills.Upgrade(radius.id);
            _skills.Upgrade(damage.id);
            _skills.Upgrade(bomb.id);
            Assert.That(_zone.RadiusX, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(_zone.GetDamage(DamageZone.AttackSource.Manual).ToDouble(), Is.EqualTo(15));
            Assert.That(_bombs.GetDamage(), Is.EqualTo(bombDamage));
            Assert.That(_text.GetText(radius), Is.EqualTo("0.8"));
        }

        [TestCase("en-US", "Locked → Unlocked", "Unlocked")]
        [TestCase("ru-RU", "Закрыто → Открыто", "Открыто")]
        [TestCase("zh", "未解锁 → 已解锁", "已解锁")]
        public void FeatureUnlocksShowTheirLocalizedState(string locale, string before, string after)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(locale);
            var node = Node("unlock", new NodeEffect { effectType = NodeEffectType.FeatureUnlock, feature = GameFeature.AutoAttack });
            _skills.Load(_save);
            Assert.That(_text.GetText(node), Is.EqualTo(before));
            _skills.Upgrade(node.id);
            Assert.That(_text.GetText(node), Is.EqualTo(after));
        }

        [Test]
        public void EveryConfiguredNodeHasAPreviewAtEveryRank()
        {
            var config = AssetDatabase.LoadAssetAtPath<SkillTreeConfig>("Assets/SO/SkillTree/SkillTreeConfig.asset");
            Set(_skills, "_config", config);
            foreach (var node in config.NodeDefinitions)
                for (var rank = 0; rank <= node.LevelLimit; rank++)
                {
                    _save.SkillTreeState.SetLevel(node.id, rank);
                    _skills.Load(_save);
                    var text = _text.GetText(node);
                    Assert.That(text, Is.Not.Empty, node.name + " rank " + rank);
                    Assert.That(text.Contains("→"), Is.EqualTo(rank < node.LevelLimit), node.name);
                }
        }

        private T Asset<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            _assets.Add(asset);
            return asset;
        }

        private NodeDefinition Node(string id, params NodeEffect[] effects)
        {
            var node = Asset<NodeDefinition>();
            node.id = id;
            node.maxLevel = effects[0].valuesPerLevel?.Length ?? 1;
            node.goldCostPerLevel = new BigDouble[] { 10, 20 };
            node.effects = effects;
            _config.entries.Add(new SkillTreeNodeEntry { node = node });
            return node;
        }

        private static NodeEffect Effect(StatType stat, params float[] values) =>
            new() { statType = stat, valuesPerLevel = values };

        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
