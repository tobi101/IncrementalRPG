using System;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Core.Items;
using Core.TestSkillTree;
using Model;

namespace Core.Save
{
    [Serializable]
    public class SaveData
    {
        public int Version = 3;
        public Core.Forge.ForgeAttempt ForgeAttempt;

        public PlayerInfo SavedPlayerInfo = PlayerInfo.Default;
        public SkillTreeState SkillTreeState = new SkillTreeState();
        public DungeonProgressState DungeonProgressState = new DungeonProgressState();
        public PlayerItemStorageState PlayerItemStorageState = new PlayerItemStorageState();
        public List<PreparedConsumableState> PreparedConsumables = new();
    }
}
