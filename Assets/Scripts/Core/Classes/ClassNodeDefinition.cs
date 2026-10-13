using System;
using System.Collections.Generic;
using Core.TestSkillTree;
using UnityEngine;

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
    public sealed class ClassNodeDefinition : SkillNodeDefinition
    {
        public Sprite preview;
        [Tooltip("Position on the class board; independent of prerequisites.")]
        public Vector2 position;
        [Range(0.5f, 1.2f)] public float visualScale = 1;
        public ClassNodeRequirement[] requirements = Array.Empty<ClassNodeRequirement>();
        public ClassNodeReward reward;
        public AttackDefinition attack;
        [Tooltip("Stable parameter ID for combat integration in iteration 2.")]
        public string parameterId;
        [Tooltip("Parameter increments per purchased level. Stored as progression; not applied to combat yet.")]
        public float[] valuesPerLevel = Array.Empty<float>();
        [Min(1)] public int runeKeyReward = 1;

        public override int LevelLimit => reward is ClassNodeReward.RuneKey or ClassNodeReward.UnlockAttack ? 1 : base.LevelLimit;

        public override IEnumerable<SkillNodeRequirement> Requirements
        {
            get
            {
                if (requirements == null) yield break;
                foreach (var requirement in requirements)
                    yield return new SkillNodeRequirement(requirement?.node, requirement?.level ?? 1);
            }
        }
    }
}
