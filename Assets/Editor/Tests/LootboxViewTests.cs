using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Items;
using NUnit.Framework;
using Spine;
using Spine.Unity;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace IncrementalRPG.Tests
{
    public sealed class LootboxViewTests
    {
        private const string ChestPath = "Assets/Art/Animations/10_2026/Chest/chest_SkeletonData.asset";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [UnityTearDown]
        public IEnumerator LeavePlayMode()
        {
            if (Application.isPlaying)
                yield return new ExitPlayMode();
        }

        [Test]
        public void GameSceneUsesUpdatedChestAndStraightAlphaWithCanvasGroupSupport()
        {
            var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/GameScene.unity");
            try
            {
                var view = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<LootboxView>(true)).Single();
                var chest = Get<SkeletonGraphic>(view, "_chest");
                Assert.That(AssetDatabase.GetAssetPath(chest.skeletonDataAsset), Is.EqualTo(ChestPath));
                Assert.That(chest.material.IsKeywordEnabled("_STRAIGHT_ALPHA_INPUT"), Is.True);
                Assert.That(chest.material.IsKeywordEnabled("_CANVAS_GROUP_COMPATIBLE"), Is.True);
                Assert.That(chest.MeshGenerator.settings.canvasGroupCompatible, Is.True);
                Assert.That(chest.MeshGenerator.settings.pmaVertexColors, Is.False);
                var texture = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(chest.mainTexture));
                Assert.That(texture.sRGBTexture && texture.alphaIsTransparency, Is.True);
                Assert.That(Get<float>(view, "_spinStartDelay"), Is.EqualTo(.5f));
                Assert.That(Get<float>(view, "_chestFadeOutDuration"), Is.GreaterThan(0f));
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        [Test]
        public void SetupPoseAndOpeningNeverExposeTheOtherLidBeforeTheRevealKey()
        {
            var data = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(ChestPath).GetSkeletonData(false);
            var skeleton = new Skeleton(data);
            Assert.That(skeleton.FindSlot("open_head1").A, Is.Zero);
            Assert.That(skeleton.FindSlot("open_head2").A, Is.Zero);
            var state = new Spine.AnimationState(new AnimationStateData(data));
            state.SetAnimation(0, "open", false).MixDuration = 0f;
            foreach (var time in new[] { 0f, .25f, .499f, .5f })
            {
                state.GetCurrent(0).TrackTime = time;
                state.Apply(skeleton);
                Assert.That(skeleton.FindSlot("close_head").A, Is.EqualTo(time < .5f ? 1f : 0f));
                Assert.That(skeleton.FindSlot("open_head1").A, Is.EqualTo(time < .5f ? 0f : 1f));
                Assert.That(skeleton.FindSlot("open_head2").A, Is.EqualTo(time < .5f ? 0f : 1f));
            }
        }

        [UnityTest]
        public IEnumerator ReelOverlapsOpeningFadePausesAndCancelledTransitionsCanReplay()
        {
            yield return new EnterPlayMode();
            // The automated editor can be in the background. Establish game-view focus
            // through Unity's own test API so the production focus pause remains intact.
            UnityEditorInternal.InternalEditorUtility.OnGameViewFocus(true);
            var root = new GameObject("Lootbox playback test", typeof(RectTransform), typeof(Canvas));
            root.SetActive(false);
            try
            {
                var view = root.AddComponent<LootboxView>();
                var data = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(ChestPath);
                var materialPath = AssetDatabase.GUIDToAssetPath("841cfbaa0261b0042b3a039c52637bb7");
                var chest = SkeletonGraphic.NewSkeletonGraphicGameObject(data, root.transform,
                    AssetDatabase.LoadAssetAtPath<Material>(materialPath));
                chest.MeshGenerator.settings.canvasGroupCompatible = true;
                chest.MeshGenerator.settings.pmaVertexColors = false;
                var viewport = new GameObject("Reel", typeof(RectTransform)).GetComponent<RectTransform>();
                viewport.SetParent(root.transform, false);
                viewport.sizeDelta = new Vector2(600f, 180f);
                Set(view, "_chest", chest);
                Set(view, "_itemViewport", viewport);
                Set(view, "_transitionLabels", Array.Empty<GameObject>());
                Set(view, "_resultPopupParent", root.transform);
                Set(view, "_resultPopupPrefab", AssetDatabase.LoadAssetAtPath<LootRewardPopupView>(
                    "Assets/Prefabs/HUD/LootRewardPopup.prefab"));
                var reward = new LootReward("test reward", AssetDatabase.LoadAssetAtPath<ItemDefinition>(
                    "Assets/SO/Items/ChestScroll.asset"));
                var completions = 0;
                view.SpinCompleted += () => completions++;

                // Preparation is also called before Awake on an inactive transition curtain.
                view.Prepare(reward);
                root.SetActive(true);
                view.PlayOpen();
                view.PlayOpen();
                Assert.That(chest.Skeleton.FindSlot("open_head1").A, Is.Zero);
                yield return WaitUntil(() => chest.color.a < .95f, 2f);
                Assert.That(chest.gameObject.activeSelf, Is.True);
                Assert.That(chest.AnimationState.GetCurrent(0).Animation.Name, Is.EqualTo("open"));
                Assert.That(chest.AnimationState.GetCurrent(0).TrackTime, Is.LessThan(1f));
                var items = Get<List<Image>>(view, "_itemViews");
                Assert.That(items.Any(i => i.gameObject.activeSelf), Is.True, "The reel must already be moving during the fade.");

                view.SetPaused(true);
                var alpha = chest.color.a;
                var trackTime = chest.AnimationState.GetCurrent(0).TrackTime;
                var position = items[0].rectTransform.anchoredPosition;
                yield return new WaitForSecondsRealtime(.15f);
                Assert.That(chest.color.a, Is.EqualTo(alpha));
                Assert.That(chest.AnimationState.GetCurrent(0).TrackTime, Is.EqualTo(trackTime));
                Assert.That(items[0].rectTransform.anchoredPosition, Is.EqualTo(position));
                view.SetPaused(false);

                UnityEditorInternal.InternalEditorUtility.OnGameViewFocus(false);
                yield return new WaitForSecondsRealtime(.1f);
                Assert.That(chest.color.a, Is.EqualTo(alpha));
                Assert.That(items[0].rectTransform.anchoredPosition, Is.EqualTo(position));
                UnityEditorInternal.InternalEditorUtility.OnGameViewFocus(true);

                view.ResetView();
                Assert.That(chest.color.a, Is.EqualTo(1f));
                Assert.That(chest.Skeleton.FindSlot("open_head1").A, Is.Zero);
                Assert.That(items.All(i => !i.gameObject.activeSelf), Is.True);
                yield return new WaitForSeconds(.5f);
                Assert.That(chest.gameObject.activeSelf, Is.True, "A cancelled fade must not hide a reset chest.");

                view.Prepare(reward);
                view.PlayOpen();
                yield return WaitUntil(() => chest.color.a < .95f, 2f);
                root.SetActive(false);
                yield return new WaitForSeconds(.5f);
                view.Prepare(reward);
                root.SetActive(true);
                Assert.That(chest.color.a, Is.EqualTo(1f));
                view.PlayOpen();
                yield return WaitUntil(() => !chest.gameObject.activeSelf, 2f);
                Assert.That(chest.color.a, Is.Zero);
                yield return WaitUntil(() => completions > 0, 6f);
                yield return new WaitForSeconds(.1f);
                Assert.That(completions, Is.EqualTo(1));
                var rollingCount = Get<int>(view, "_rollingItemCount");
                var finalCount = Get<int>(view, "_finalItemCount");
                var winner = items[rollingCount + finalCount / 2];
                Assert.That(winner.sprite, Is.SameAs(reward.Definition.icon));
                Assert.That(winner.rectTransform.anchoredPosition.x, Is.EqualTo(viewport.rect.center.x));
            }
            finally { Object.Destroy(root); }
            yield return null;
            yield return new ExitPlayMode();
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float timeout)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + timeout;
            while (!condition() && Time.realtimeSinceStartupAsDouble < deadline)
                yield return null;
            Assert.That(condition(), Is.True, "Lootbox playback timed out.");
        }

        private static T Get<T>(LootboxView view, string field) =>
            (T)typeof(LootboxView).GetField(field, PrivateInstance).GetValue(view);

        private static void Set(LootboxView view, string field, object value) =>
            typeof(LootboxView).GetField(field, PrivateInstance).SetValue(view, value);
    }
}
