using System;
using Core.Items;
using UnityEngine;
using Utils;

namespace Core.Forge
{
    [Serializable]
    public sealed class ForgeLevelSettings
    {
        public BigDouble shardCost = 5;
        public float[] rarityWeights = { 60, 30, 9, 1 };
        [Min(.2f)] public float cursorPeriod = 2.2f;
        [Tooltip("Full width of each centered success range, as a fraction of the gauge.")]
        [Range(0, 1)] public float rarityRange = .06f;
        [Range(0, 1)] public float allStatsRange = .24f;
        [Range(0, 1)] public float oneStatRange = .60f;
        [Min(0)] public float allStatsBonus = .10f;
        [Min(0)] public float oneStatBonus = .15f;
    }

    [CreateAssetMenu(fileName = "ForgeConfig", menuName = "RPG/Forge/Config")]
    public sealed class ForgeConfig : ScriptableObject
    {
        [Tooltip("The window always opens from the hub. Crafting becomes available at this reached location (1-based).")]
        [Min(1)] public int firstAvailableLocation = 2;
        public ForgeLevelSettings[] levels = { new(), new(), new(), new() };
        [Tooltip("Helmet, chest and weapon definitions. One definition per type; all types have equal chances.")]
        public ItemDefinition[] equipment;
        [Tooltip("Minimum and maximum fractional bonuses for Common, Rare, Unique, Legendary.")]
        public Vector2[] statRanges = { new(.03f, .07f), new(.06f, .12f), new(.10f, .18f), new(.15f, .25f) };
        [Min(0)] public float goldPerBonusPercent = 2f;

        public ForgeLevelSettings GetLevel(int level) => levels[Mathf.Clamp(level - 1, 0, levels.Length - 1)];

        public bool IsValid()
        {
            if (levels == null || levels.Length == 0 || equipment == null || equipment.Length != 3 ||
                statRanges == null || statRanges.Length != 4 || !Finite(goldPerBonusPercent) || goldPerBonusPercent < 0) return false;
            var slots = new System.Collections.Generic.HashSet<EquipmentSlot>();
            foreach (var item in equipment)
                if (item == null || item.category != ItemCategory.Armor || !EquipmentStats.IsSupportedSlot(item.equipmentSlot) ||
                    !slots.Add(item.equipmentSlot) || item.icon == null) return false;
            foreach (var range in statRanges)
                if (!Finite(range.x) || !Finite(range.y) || range.x < 0 || range.y < range.x) return false;
            foreach (var level in levels)
            {
                if (level == null || level.shardCost <= 0 ||
                    BigDoubleMath.SanitizeNonNegativeInteger(level.shardCost, BigDouble.Zero) != level.shardCost ||
                    level.rarityWeights == null || level.rarityWeights.Length != 4 || !Finite(level.cursorPeriod) || level.cursorPeriod < .2f ||
                    !Finite(level.rarityRange) || !Finite(level.allStatsRange) || !Finite(level.oneStatRange) ||
                    level.rarityRange < 0 || level.allStatsRange < level.rarityRange || level.oneStatRange < level.allStatsRange || level.oneStatRange > 1 ||
                    !Finite(level.allStatsBonus) || !Finite(level.oneStatBonus) || level.allStatsBonus < 0 || level.oneStatBonus < 0) return false;
                double total = 0;
                foreach (var weight in level.rarityWeights) { if (!Finite(weight) || weight < 0) return false; total += weight; }
                if (total <= 0) return false;
            }
            return true;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
