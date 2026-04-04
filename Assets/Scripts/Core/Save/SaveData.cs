using System;
using Core.TestSkillTree;
using Model;

namespace Core.Save
{
    [Serializable]
    public class SaveData
    {
        public int Version = 1;

        public PlayerInfo SavedPlayerInfo;
        public SkillTreeState SkillTreeState = new SkillTreeState();
    }
}
