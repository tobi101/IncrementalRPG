using System.Collections.Generic;
using System.Reflection;
using Core.Gameplay;
using Core.Gameplay.Bomb;
using Core.Items;
using Core.TestSkillTree;
using Entity;
using NUnit.Framework;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace IncrementalRPG.Tests
{
    public sealed class BombExplosionVisualTests
    {
        private const string PrefabPath = "Assets/Prefabs/Entities/Bombs.prefab";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameObject _root;
        private CreatureView _view;
        private BombExplosionVisualScaler _scaler;

        [SetUp]
        public void SetUp()
        {
            _root = PrefabUtility.LoadPrefabContents(PrefabPath);
            _view = _root.GetComponent<CreatureView>();
            _scaler = _root.GetComponentInChildren<BombExplosionVisualScaler>(true);
        }

        [TearDown]
        public void TearDown() => PrefabUtility.UnloadPrefabContents(_root);

        [TestCase(3f, 0.55f, 1f, 1f)]
        [TestCase(6f, 0.55f, 1f, 1f)]
        [TestCase(4f, 0.8f, 1.5f, 0.75f)]
        public void PeakAnimationMatchesDamageDiameterAndCenter(float radius, float aspectRatio,
            float parentScaleX, float parentScaleY)
        {
            var epicenter = new Vector3(7f, -3f, 0f);
            _root.transform.position = epicenter;
            _root.transform.localScale = new Vector3(parentScaleX, parentScaleY, 1f);
            var barrel = _root.transform.Find("Spine GameObject (barrel)");
            var barrelScale = barrel.localScale;

            _scaler.ScaleToRadius(radius, aspectRatio, epicenter);

            AssertFootprint(radius, aspectRatio, epicenter);
            Assert.That(barrel.localScale, Is.EqualTo(barrelScale));
        }

        [Test]
        public void PooledBarrelCanReturnToSmallerRadiusWithoutAccumulatingScaleOrOffset()
        {
            var config = AssetDatabase.LoadAssetAtPath<EntityConfig>(
                "Assets/SO/Dungeons/0_RocksAndLava/Entities/Bombs.asset");
            _view.Bind(new Creature(config, Vector2Int.zero));
            _scaler.ScaleToRadius(9f, 0.55f, Vector3.zero);
            _view.PlayDeath(null);
            _view.ResetForPool();
            _root.SetActive(false);
            _root.SetActive(true);
            _view.Bind(new Creature(config, Vector2Int.zero));
            _scaler.ScaleToRadius(3f, 0.55f, Vector3.zero);
            _scaler.ScaleToRadius(3f, 0.55f, Vector3.zero);
            _view.PlayDeath(null);

            AssertFootprint(3f, 0.55f, Vector3.zero);
        }

        [Test]
        public void UpgradeAfterSpawnUsesSameCurrentRadiusForAnimationAndDamage()
        {
            var config = ScriptableObject.CreateInstance<BombExplosionConfig>();
            var entity = ScriptableObject.CreateInstance<EntityConfig>();
            var tile = ScriptableObject.CreateInstance<Tile>();
            var gridRoot = new GameObject("Explosion test grid", typeof(Grid));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gridRoot, _root.scene);
            try
            {
                var tiles = new GameObject("Tiles", typeof(Tilemap)).GetComponent<Tilemap>();
                tiles.transform.SetParent(gridRoot.transform, false);
                for (var x = 0; x <= 7; x++)
                    tiles.SetTile(new Vector3Int(x, 0, 0), tile);
                var grid = new TileGrid();
                grid.Initialize(tiles);
                config.baseRadius = 3f;
                config.baseDamage = 10;
                entity.featureType = FeatureType.Bomb;
                entity.maxHP = 100;
                var source = new Creature(entity, Vector2Int.zero);
                var inside = new Creature(entity, new Vector2Int(5, 0));
                var outside = new Creature(entity, new Vector2Int(7, 0));
                grid.Place(source);
                grid.Place(inside);
                grid.Place(outside);
                _view.Bind(source);
                var skills = new SkillTreeService();
                var service = new BombExplosionService(grid, null, config, skills, new PlayerItemStorage(null));
                typeof(BombExplosionService).GetMethod("HandleFeatureSpawned", PrivateInstance)
                    .Invoke(service, new object[] { source, _view, Vector2Int.zero, entity });

                // Apply both additive and multiplicative upgrades while this barrel is already alive.
                ((Dictionary<StatType, float>)typeof(SkillTreeService).GetField("_bonusCache", PrivateInstance)
                    .GetValue(skills))[StatType.BombExplosionRadius] = 1.5f;
                ((Dictionary<StatType, float>)typeof(SkillTreeService).GetField("_multiplierCache", PrivateInstance)
                    .GetValue(skills))[StatType.BombExplosionRadius] = 1.5f;
                source.TakeDamage(100);
                _view.PlayDeath(null);

                Assert.That(service.GetRadius(), Is.EqualTo(6f));
                AssertFootprint(6f, config.aspectRatio, source.WorldPosition);
                Assert.That((double)inside.CurrentHP, Is.EqualTo(90d));
                Assert.That((double)outside.CurrentHP, Is.EqualTo(100d));
            }
            finally
            {
                Object.DestroyImmediate(gridRoot);
                Object.DestroyImmediate(tile);
                Object.DestroyImmediate(entity);
                Object.DestroyImmediate(config);
            }
        }

        private void AssertFootprint(float radius, float aspectRatio, Vector3 epicenter)
        {
            // Measure the actual Spine artwork at the burst's peak, independently of the scaler's calibration.
            var body = _scaler.GetComponent<SkeletonAnimation>();
            var data = body.skeletonDataAsset.GetSkeletonData(false);
            var skeleton = new Skeleton(data);
            data.FindAnimation("explosion").Apply(skeleton, 0f, 0.1f, false, null, 1f,
                MixBlend.Replace, MixDirection.In);
            skeleton.UpdateWorldTransform(Skeleton.Physics.Pose);
            foreach (var slot in skeleton.Slots)
                if (slot.A <= 0.01f) slot.Attachment = null;
            float[] vertices = null;
            skeleton.GetBounds(out var x, out var y, out var width, out var height, ref vertices);
            var min = body.transform.TransformPoint(new Vector3(x, y, 0f));
            var max = body.transform.TransformPoint(new Vector3(x + width, y + height, 0f));
            var center = (min + max) * 0.5f;

            Assert.That(max.x - min.x, Is.EqualTo(2f * radius).Within(0.001f));
            Assert.That(max.y - min.y, Is.EqualTo(2f * radius * aspectRatio).Within(0.001f));
            Assert.That(Vector3.Distance(center, epicenter), Is.LessThan(0.001f));
        }
    }
}
