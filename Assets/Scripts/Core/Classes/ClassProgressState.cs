using System;
using System.Collections.Generic;
using Core.TestSkillTree;
using Utils;

namespace Core.Classes
{
    [Serializable]
    public sealed class ClassProgressState
    {
        public bool FirstClassChosen;
        public List<ClassProgressEntry> Classes = new();
        public string ManualAttackId;
        public string AutomaticAttackId;
        public string SpecialAttackId;
    }

    [Serializable]
    public sealed class ClassProgressEntry : ISkillTreeLevels
    {
        public string ClassId;
        public bool Unlocked;
        public BigDouble GoldSpent;
        public List<ClassNodeProgress> Nodes = new();

        public int GetLevel(string nodeId) => Nodes.Find(n => n.NodeId == nodeId)?.Level ?? 0;

        public void SetLevel(string nodeId, int level)
        {
            var entry = Nodes.Find(n => n.NodeId == nodeId);
            if (entry == null) Nodes.Add(new ClassNodeProgress { NodeId = nodeId, Level = level });
            else entry.Level = level;
        }
    }

    [Serializable]
    public sealed class ClassNodeProgress
    {
        public string NodeId;
        public int Level;
    }
}
