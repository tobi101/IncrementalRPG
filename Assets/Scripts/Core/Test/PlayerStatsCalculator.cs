using Model;
using UnityEngine;

namespace Core.Test
{
    public class PlayerStatsCalculator
    {
        private readonly SkillTreeConfig _config;
        private readonly SkillTreeModel _model;

        public PlayerStatsCalculator(SkillTreeConfig config, SkillTreeModel model)
        {
            _config = config;
            _model = model;
        }
        
        public PlayerInfo Calculate(PlayerInfo baseStats)
        {
            var stats = baseStats;
            foreach (var node in _config.AllNodes)
            {
                int level = _model.GetLevel(node.Id);
                if (level == 0) continue;
                foreach (var mod in node.EffectsPerLevel)
                    ApplyModifier(ref stats, mod, level);
            }
            return stats;
        }
        
        private static void ApplyModifier(ref PlayerInfo stats, StatModifier mod, int level)
        {
            switch (mod.Stat)
            {
                case StatType.DamageZoneSize:
                    if (mod.Op == ModifierOp.Add)
                    {
                        stats.ZoneSize.RadiusX += mod.Value * level;
                        stats.ZoneSize.RadiusY += mod.Value * level;
                    }
                    else
                    {
                        float factor = Mathf.Pow(mod.Value, level);
                        stats.ZoneSize.RadiusX *= factor;
                        stats.ZoneSize.RadiusY *= factor;
                    }
                    break;
                case StatType.MapSize:
                    stats.MapSize = mod.Op == ModifierOp.Add
                        ? stats.MapSize + (int)(mod.Value * level)
                        : (int)(stats.MapSize * Mathf.Pow(mod.Value, level));
                    break;
                // GoldMultiplier: добавь поле в PlayerInfo когда появится
            }
        }
    }
}
