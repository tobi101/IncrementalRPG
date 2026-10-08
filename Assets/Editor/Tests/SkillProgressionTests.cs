using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Classes;
using Core.Gameplay.Dungeon;
using Core.Items;
using Core.Save;
using Core.TestSkillTree;
using Model;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Utils;

namespace IncrementalRPG.Tests
{
    public sealed class SkillProgressionTests
    {
        private readonly List<UnityEngine.Object> _assets = new();
        private Player _player;

        [SetUp]
        public void SetUp()
        {
            _player = new Player();
            _player.Load(new SaveData());
            _player.GoldTotal = 1000;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in _assets) UnityEngine.Object.DestroyImmediate(asset);
            _assets.Clear();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BothTreesRequireAllParentsAndChargeEachLevelOnce(bool isClass)
        {
            var first = Node(isClass, "first", 10);
            var second = Node(isClass, "second", 20);
            var child = Node(isClass, "child", 30, 40);
            Require(child, first, second);
            ISkillTreeLevels levels = isClass ? new ClassProgressEntry() : new SkillTreeState();
            var tree = new SkillTreeProgression(new[] { first, second, child }, levels, _player);

            Assert.That(tree.TryPurchase(child), Is.False);
            Assert.That(tree.TryPurchase(first), Is.True);
            Assert.That(tree.GetStatus(child), Is.EqualTo(SkillPurchaseStatus.RequirementsMissing));
            Assert.That(tree.TryPurchase(second), Is.True);
            Assert.That(tree.TryPurchase(child), Is.True);
            Assert.That(tree.GetLevel(child), Is.EqualTo(1));
            Assert.That(tree.TryPurchase(child), Is.True);
            Assert.That(tree.TryPurchase(child), Is.False);
            Assert.That(tree.GetStatus(child), Is.EqualTo(SkillPurchaseStatus.Complete));
            Assert.That(_player.GoldTotal.ToDouble(), Is.EqualTo(900).Within(0.001));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrInvalidPricesNeverGrantFreeLevels(bool isClass)
        {
            var node = Node(isClass, "price", 10);
            ISkillTreeLevels levels = isClass ? new ClassProgressEntry() : new SkillTreeState();
            var tree = new SkillTreeProgression(new[] { node }, levels, _player);
            foreach (var prices in new[] { null, Array.Empty<BigDouble>(), new[] { new BigDouble(-1) },
                         new[] { new BigDouble(0.5) }, new[] { SerializedPrice(double.NaN) },
                         new[] { SerializedPrice(double.PositiveInfinity) }, new[] { SerializedPrice(100) } })
            {
                node.goldCostPerLevel = prices;
                Assert.That(tree.GetStatus(node), Is.EqualTo(SkillPurchaseStatus.NotConfigured));
                Assert.That(tree.TryPurchase(node), Is.False);
            }
            Assert.That(tree.GetLevel(node), Is.Zero);
            Assert.That(_player.GoldTotal.ToDouble(), Is.EqualTo(1000));
            node.goldCostPerLevel = new[] { BigDouble.Zero };
            Assert.That(tree.TryPurchase(node), Is.True, "An explicitly configured zero price is valid.");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void InsufficientGoldAndForeignRequirementsDoNotChangeProgress(bool isClass)
        {
            var node = Node(isClass, "expensive", 1001);
            var foreign = Node(isClass, "foreign", 1);
            ISkillTreeLevels levels = isClass ? new ClassProgressEntry() : new SkillTreeState();
            var tree = new SkillTreeProgression(new[] { node }, levels, _player);
            Assert.That(tree.GetStatus(node), Is.EqualTo(SkillPurchaseStatus.NotEnoughGold));
            Assert.That(tree.TryPurchase(node), Is.False);
            node.goldCostPerLevel[0] = 1;
            Require(node, foreign);
            levels.SetLevel(foreign.id, 99);
            Assert.That(tree.GetStatus(node), Is.EqualTo(SkillPurchaseStatus.RequirementsMissing));
            Assert.That(tree.TryPurchase(node), Is.False);
            Assert.That(tree.TryPurchase(foreign), Is.False);
            Assert.That(tree.GetLevel(node), Is.Zero);
            Assert.That(_player.GoldTotal.ToDouble(), Is.EqualTo(1000));
        }

        [Test]
        public void GoldObserversSeeNewLevelAndCannotBuySameTreeReentrantly()
        {
            var node = Node(false, "node", 10, 20);
            var tree = new SkillTreeProgression(new[] { node }, new SkillTreeState(), _player);
            _player.OnGoldChanged += () =>
            {
                Assert.That(tree.GetLevel(node), Is.EqualTo(1));
                Assert.That(tree.TryPurchase(node), Is.False);
            };
            Assert.That(tree.TryPurchase(node), Is.True);
            Assert.That(_player.GoldTotal.ToDouble(), Is.EqualTo(990));
        }

        [Test]
        public void SkillsKeepVisibilityEffectsAndSavedLevels()
        {
            var root = (NodeDefinition)Node(false, "root", 10, 20);
            var child = (NodeDefinition)Node(false, "child", 30);
            Require(child, root);
            child.prerequisites[0].requiredLevel = 2;
            root.effects = new[]
            {
                new NodeEffect { effectType = NodeEffectType.Additive, statType = StatType.ManualAttackDamage, valuesPerLevel = new[] { 5f, 7f } },
                new NodeEffect { effectType = NodeEffectType.Multiplicative, statType = StatType.ManualAttackDamage, valuesPerLevel = new[] { .1f, .2f } }
            };
            child.effects = new[] { new NodeEffect { effectType = NodeEffectType.FeatureUnlock, feature = GameFeature.AutoAttack } };
            var config = Asset<SkillTreeConfig>();
            config.entries.Add(new SkillTreeNodeEntry { node = root });
            config.entries.Add(new SkillTreeNodeEntry { node = child });
            var service = Skills(config, new SaveData());
            Assert.That(service.GetState(child.id), Is.EqualTo(NodeState.Hidden));
            Assert.That(service.TryUpgrade(root.id), Is.EqualTo(NodeUpgradeResult.Upgraded));
            Assert.That(service.GetState(child.id), Is.EqualTo(NodeState.Locked));
            Assert.That(service.TryUpgrade(root.id), Is.EqualTo(NodeUpgradeResult.UpgradedToMax));
            Assert.That(service.GetState(child.id), Is.EqualTo(NodeState.Affordable));
            Assert.That(service.TryUpgrade(child.id), Is.EqualTo(NodeUpgradeResult.UpgradedToMax));
            Assert.That(service.GetBonus(StatType.ManualAttackDamage), Is.EqualTo(12));
            Assert.That(service.GetMultiplier(StatType.ManualAttackDamage), Is.EqualTo(1.3f).Within(.0001f));
            Assert.That(service.IsUnlocked(GameFeature.AutoAttack), Is.True);
            var saved = new SaveData();
            service.Contribute(saved);
            var restored = Skills(config, JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(saved)));
            Assert.That(restored.GetLevel(root.id), Is.EqualTo(2));
            Assert.That(restored.GetBonus(StatType.ManualAttackDamage), Is.EqualTo(12));
            Assert.That(restored.IsUnlocked(GameFeature.AutoAttack), Is.True);
        }

        [Test]
        public void ClassesKeepKeysRefundsLoadoutAndCanRebuyAfterResetAndReload()
        {
            var catalog = Asset<ClassCatalog>();
            catalog.introductionDungeon = Asset<DungeonConfig>();
            catalog.introductionDungeon.dungeonId = "intro";
            catalog.runeKey = Asset<ItemDefinition>();
            catalog.runeKey.itemId = "key";
            catalog.defaultManualAttack = Attack("basic");
            var firstAttack = (ClassNodeDefinition)Node(true, "first_attack", 100);
            firstAttack.reward = ClassNodeReward.UnlockAttack;
            firstAttack.attack = Attack("fire");
            var key = (ClassNodeDefinition)Node(true, "key_node", 50);
            key.reward = ClassNodeReward.RuneKey;
            key.runeKeyReward = 2;
            Require(key, firstAttack);
            var otherAttack = (ClassNodeDefinition)Node(true, "other_attack", 80);
            otherAttack.reward = ClassNodeReward.UnlockAttack;
            otherAttack.attack = Attack("ice", AttackSlot.Automatic);
            var fire = Class("fire", firstAttack, key);
            var ice = Class("ice", otherAttack);
            catalog.classes = new[] { fire, ice };
            var dungeons = new DungeonSelectionService();
            dungeons.MarkCompleted(catalog.introductionDungeon);
            var items = new PlayerItemStorage(Asset<ItemCatalog>());
            var service = new ClassProgressionService(catalog, dungeons, _player, items);

            Assert.That(service.ChooseFirst(fire), Is.True);
            Assert.That(service.Reset(fire), Is.False);
            Assert.That(service.Purchase(fire, key), Is.False);
            Assert.That(service.Purchase(fire, firstAttack), Is.True);
            Assert.That(service.Purchase(fire, key), Is.True);
            Assert.That(service.RuneKeys, Is.EqualTo(2));
            Assert.That(service.Unlock(ice), Is.True);
            Assert.That(service.RuneKeys, Is.EqualTo(1));
            Assert.That(service.Purchase(ice, otherAttack), Is.True);
            Assert.That(service.Equip(AttackSlot.Manual, firstAttack.attack), Is.True);
            Assert.That(service.Equip(AttackSlot.Automatic, otherAttack.attack), Is.True);

            var data = new SaveData();
            service.Contribute(data);
            _player.Contribute(data);
            items.Contribute(data);
            dungeons.Contribute(data);
            data = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(data));
            service = new ClassProgressionService(catalog, dungeons, _player, items);
            service.Load(data);
            firstAttack.goldCostPerLevel[0] = 999;
            Assert.That(service.Refund(fire).ToDouble(), Is.EqualTo(150));
            Assert.That(service.Reset(fire), Is.True);
            Assert.That(_player.GoldTotal.ToDouble(), Is.EqualTo(920).Within(.001));
            Assert.That(service.Level(fire, firstAttack), Is.Zero);
            Assert.That(service.Level(ice, otherAttack), Is.EqualTo(1));
            Assert.That(service.Equipped(AttackSlot.Manual), Is.Null);
            Assert.That(service.Equipped(AttackSlot.Automatic), Is.SameAs(otherAttack.attack));
            Assert.That(service.RuneKeys, Is.EqualTo(1));
            Assert.That(service.Reset(ice), Is.False);
            Assert.That(service.Unlock(fire), Is.True);
            firstAttack.goldCostPerLevel[0] = 100;
            Assert.That(service.Purchase(fire, firstAttack), Is.True);
            Assert.That(service.Purchase(fire, key), Is.True);
            Assert.That(service.RuneKeys, Is.EqualTo(2));
        }

        [Test]
        public void VersionFourSaveKeepsBothIndependentLevelFormats()
        {
            const string json = "{\"Version\":4,\"SkillTreeState\":{\"nodeLevels\":[{\"nodeId\":\"same_id\",\"level\":3}]},\"ClassProgressState\":{\"FirstClassChosen\":true,\"Classes\":[{\"ClassId\":\"fire\",\"Unlocked\":true,\"GoldSpent\":{\"mantissa\":1.5,\"exponent\":2},\"Nodes\":[{\"NodeId\":\"same_id\",\"Level\":2}]}]}}";
            var save = JsonUtility.FromJson<SaveData>(json);
            Assert.That(save.SkillTreeState.GetLevel("same_id"), Is.EqualTo(3));
            Assert.That(save.ClassProgressState.Classes[0].GetLevel("same_id"), Is.EqualTo(2));
            Assert.That(save.ClassProgressState.Classes[0].GoldSpent.ToDouble(), Is.EqualTo(150));
            var roundTrip = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(save));
            Assert.That(roundTrip.SkillTreeState.GetLevel("same_id"), Is.EqualTo(3));
            Assert.That(roundTrip.ClassProgressState.Classes[0].GetLevel("same_id"), Is.EqualTo(2));
        }

        [Test]
        public void ExistingAssetsRetainValidPricesIdsAndRequirements()
        {
            var skills = AssetDatabase.LoadAssetAtPath<SkillTreeConfig>("Assets/SO/SkillTree/SkillTreeConfig.asset");
            var classes = AssetDatabase.LoadAssetAtPath<ClassCatalog>("Assets/SO/Classes/ClassCatalog.asset");
            var trees = new List<SkillNodeDefinition[]> { skills.NodeDefinitions.Cast<SkillNodeDefinition>().ToArray() };
            trees.AddRange(classes.classes.Select(c => c.nodes.Cast<SkillNodeDefinition>().ToArray()));
            foreach (var tree in trees)
            {
                Assert.That(tree.Select(n => n.id).Distinct().Count(), Is.EqualTo(tree.Length));
                foreach (var node in tree)
                {
                    Assert.That(node.id, Is.Not.Empty);
                    for (var level = 0; level < node.LevelLimit; level++)
                        Assert.That(node.TryGetCost(level, out _), Is.True, node.name + " price " + level);
                    foreach (var requirement in node.Requirements)
                    {
                        Assert.That(tree, Does.Contain(requirement.Node), node.name);
                        Assert.That(requirement.Level, Is.InRange(1, requirement.Node.LevelLimit), node.name);
                    }
                }
            }
        }

        private T Asset<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            _assets.Add(asset);
            return asset;
        }

        private static BigDouble SerializedPrice(double mantissa)
        {
            // Invalid values can arrive through Unity serialization, bypassing the validating constructor.
            object price = BigDouble.Zero;
            typeof(BigDouble).GetField("mantissa", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(price, mantissa);
            return (BigDouble)price;
        }

        private SkillNodeDefinition Node(bool isClass, string id, params double[] prices)
        {
            SkillNodeDefinition node;
            if (isClass)
            {
                var classNode = Asset<ClassNodeDefinition>();
                classNode.reward = ClassNodeReward.UpgradeAttack;
                node = classNode;
            }
            else node = Asset<NodeDefinition>();
            node.id = id;
            node.maxLevel = prices.Length;
            node.goldCostPerLevel = prices.Select(p => new BigDouble(p)).ToArray();
            return node;
        }

        private static void Require(SkillNodeDefinition node, params SkillNodeDefinition[] parents)
        {
            if (node is ClassNodeDefinition classNode)
                classNode.requirements = parents.Select(p => new ClassNodeRequirement { node = (ClassNodeDefinition)p, level = 1 }).ToArray();
            else ((NodeDefinition)node).prerequisites = parents.Select(p => new NodePrerequisite { node = (NodeDefinition)p, requiredLevel = 1 }).ToList();
        }

        private SkillTreeService Skills(SkillTreeConfig config, SaveData data)
        {
            var service = new SkillTreeService();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SkillTreeService).GetField("_config", flags).SetValue(service, config);
            typeof(SkillTreeService).GetField("_player", flags).SetValue(service, _player);
            service.Load(data);
            return service;
        }

        private ClassDefinition Class(string id, params ClassNodeDefinition[] nodes)
        {
            var definition = Asset<ClassDefinition>();
            definition.id = id;
            definition.nodes = nodes;
            return definition;
        }

        private AttackDefinition Attack(string id, AttackSlot slot = AttackSlot.Manual)
        {
            var attack = Asset<AttackDefinition>();
            attack.id = id;
            attack.slot = slot;
            return attack;
        }
    }
}
