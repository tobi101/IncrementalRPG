using System;
using UnityEngine;
using UnityEngine.Localization;
using Utils;

namespace Core.Classes
{
    public enum ClassNodeReward { UnlockAttack, UpgradeAttack, UpgradeClassEffect, RuneKey }

    [Serializable]
    public sealed class ClassNodeRequirement
    {
        public ClassNodeDefinition node;
        [Min(1)] public int level = 1;
    }

    [CreateAssetMenu(menuName = "RPG/Classes/Node")]
    public sealed class ClassNodeDefinition : ScriptableObject
    {
        public string id;
        public LocalizedString displayName = new();
        public LocalizedString description = new();
        public Sprite icon;
        public Sprite preview;
        [Tooltip("Position on the class board; independent of prerequisites.")]
        public Vector2 position;
        [Range(0.5f, 1.2f)] public float visualScale = 1;
        [Min(1)] public int maxLevel = 1;
        public BigDouble[] goldCostPerLevel = { new BigDouble(100) };
        public ClassNodeRequirement[] requirements = Array.Empty<ClassNodeRequirement>();
        public ClassNodeReward reward;
        public AttackDefinition attack;
        [Tooltip("Stable parameter ID for combat integration in iteration 2.")]
        public string parameterId;
        [Tooltip("Parameter increments per purchased level. Stored as progression; not applied to combat yet.")]
        public float[] valuesPerLevel = Array.Empty<float>();
        [Min(1)] public int runeKeyReward = 1;

        public int LevelLimit => reward is ClassNodeReward.RuneKey or ClassNodeReward.UnlockAttack ? 1 : Mathf.Max(1, maxLevel);

        public bool TryGetCost(int level, out BigDouble cost)
        {
            cost = BigDouble.Zero;
            if (level < 0 || level >= LevelLimit || goldCostPerLevel == null || level >= goldCostPerLevel.Length)
                return false;
            cost = goldCostPerLevel[level];
            return cost.IsFinite && cost.IsNormalized && cost >= 0 && BigDoubleMath.SanitizeNonNegativeInteger(cost, BigDouble.Zero) == cost;
        }
    }
}
