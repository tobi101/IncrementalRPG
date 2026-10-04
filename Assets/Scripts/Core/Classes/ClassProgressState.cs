using System;
using System.Collections.Generic;
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
    public sealed class ClassProgressEntry
    {
        public string ClassId;
        public bool Unlocked;
        public BigDouble GoldSpent;
        public List<ClassNodeProgress> Nodes = new();
    }

    [Serializable]
    public sealed class ClassNodeProgress
    {
        public string NodeId;
        public int Level;
    }
}
