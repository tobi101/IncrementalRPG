using System;
using Core.TestSkillTree;
using Model;

namespace Core.Save
{
    [Serializable]
    public class SaveData
    {
        public int Version = 1;

        public PlayerInfo SavedPlayerInfo = PlayerInfo.Default;
        public SkillTreeState SkillTreeState = new SkillTreeState();
    }
}
