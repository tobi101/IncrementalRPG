using Core.Save;
using Model;
using Reflex.Core;
using Reflex.Extensions;
using UnityEditor;
using UnityEngine;
using Utils;

namespace IncrementalRPG.Editor
{
    public static class SkillTreeTestCommands
    {
        private const string AddGoldMenu = "Tools/Skill Tree/Add 1T Gold";
        private static readonly BigDouble GoldGrant = new BigDouble(1d, 12);

        [MenuItem(AddGoldMenu, false, 100)]
        public static void AddGold()
        {
            if (Application.isPlaying)
            {
                foreach (var scope in Object.FindObjectsByType<ContainerScope>(FindObjectsSortMode.None))
                {
                    var container = scope.gameObject.scene.GetSceneContainer();
                    if (!container.HasBinding<Player>() || !container.HasBinding<SaveService>())
                        continue;

                    var player = container.Resolve<Player>();
                    player.GoldTotal += GoldGrant;
                    container.Resolve<SaveService>().Save();
                    LogBalance(player.GoldTotal);
                    return;
                }

                Debug.LogWarning("[Skill Tree] Start or continue the game before adding test gold in Play Mode.");
                return;
            }

            AddGoldToSave();
        }

        private static void AddGoldToSave()
        {
            var storage = new SaveStorage();
            var data = storage.LoadOrDefault();
            var playerInfo = data.SavedPlayerInfo;
            playerInfo.GoldTotal = BigDoubleMath.SanitizeNonNegativeInteger(playerInfo.GoldTotal, BigDouble.Zero)
                                   + GoldGrant;
            data.SavedPlayerInfo = playerInfo;
            storage.Write(data);
            LogBalance(playerInfo.GoldTotal);
        }

        private static void LogBalance(BigDouble gold) =>
            Debug.Log($"[Skill Tree] Added 1T gold and saved. Balance: {BigDoubleFormatter.FormatFloor(gold)}.");

        [MenuItem(AddGoldMenu, true)]
        private static bool CanAddGold() =>
            !EditorApplication.isCompiling && !EditorApplication.isUpdating &&
            EditorApplication.isPlaying == EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
