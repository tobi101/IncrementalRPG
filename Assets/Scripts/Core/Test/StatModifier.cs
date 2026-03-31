using System;

namespace Core.Test
{
    public enum ModifierOp { Add, Multiply }

    [Serializable]
    public struct StatModifier
    {
        public StatType Stat;
        public ModifierOp Op;
        public float Value; // применяется Value * currentLevel
    }
}
