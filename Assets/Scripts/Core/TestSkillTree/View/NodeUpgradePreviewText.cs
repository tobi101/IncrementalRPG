using System;
using System.Collections.Generic;
using Core.Gameplay;
using Core.Gameplay.Bomb;
using Core.Gameplay.Dungeon;
using Core.Gameplay.Shards;
using Core.Items;
using Model;
using UnityEngine;
using Utils;

namespace Core.TestSkillTree.View
{
    public sealed class NodeUpgradePreviewText
    {
        private readonly SkillTreeService _skills;
        private readonly DamageZone _zone;
        private readonly BombExplosionService _bombs;
        private readonly ShardPickupConfig _shards;
        private readonly DungeonSelectionService _selection;
        private readonly DungeonList _dungeons;
        private readonly Player _player;

        public NodeUpgradePreviewText(SkillTreeService skills, DamageZone zone, BombExplosionService bombs,
            ShardPickupConfig shards, DungeonSelectionService selection, DungeonList dungeons, Player player)
        {
            _skills = skills;
            _zone = zone;
            _bombs = bombs;
            _shards = shards;
            _selection = selection;
            _dungeons = dungeons;
            _player = player;
        }

        public string GetText(NodeDefinition node)
        {
            var complete = _skills.GetLevel(node.id) >= node.LevelLimit;
            var lines = new List<string>();
            var stats = new HashSet<StatType>();
            var features = new HashSet<GameFeature>();
            foreach (var effect in node.effects ?? Array.Empty<NodeEffect>())
            {
                if (effect == null) continue;
                string current, next;
                if (effect.effectType == NodeEffectType.FeatureUnlock)
                {
                    if (effect.feature == GameFeature.None || !features.Add(effect.feature)) continue;
                    current = ItemText.Get(_skills.IsUnlocked(effect.feature) ? "skill.preview.unlocked" : "skill.preview.locked");
                    next = ItemText.Get("skill.preview.unlocked");
                }
                else
                {
                    if (!stats.Add(effect.statType)) continue;
                    current = FormatStat(effect.statType, null);
                    next = complete ? current : FormatStat(effect.statType, node);
                }
                lines.Add(complete ? current : current + " → " + next);
            }
            return string.Join("\n", lines);
        }

        private string FormatStat(StatType stat, NodeDefinition preview)
        {
            var bonus = _skills.GetBonus(stat, preview);
            var multiplier = _skills.GetMultiplier(stat, preview);
            switch (stat)
            {
                case StatType.ManualAttackDamage:
                    return BigDoubleFormatter.Format(_zone.GetDamage(DamageZone.AttackSource.Manual, preview));
                case StatType.AutoAttackDamage:
                    return BigDoubleFormatter.Format(_zone.GetDamage(DamageZone.AttackSource.Auto, preview));
                case StatType.SpecialAttackDamage:
                    return BigDoubleFormatter.Format(_zone.GetDamage(DamageZone.AttackSource.Special, preview));
                case StatType.ManualAttackSpeed:
                    return Seconds(_zone.GetManualAttackCooldown(preview));
                case StatType.AutoAttackSpeed:
                    return Seconds(_zone.GetAutoAttackInterval(preview));
                case StatType.ZoneRadius:
                    return Number(_zone.GetRadius(preview));
                case StatType.BombExplosionDamage:
                    return BigDoubleFormatter.Format(_bombs.GetDamage(preview));
                case StatType.BombExplosionRadius:
                    return Number(_bombs.GetRadius(preview));
                case StatType.ShardPickupSpeed:
                    return Seconds(Mathf.Max(0.01f, _shards.baseCollectionDuration /
                        Mathf.Max(0.01f, (1f + bonus) * multiplier)));
                case StatType.GoldDrop:
                case StatType.SlimeShardDrop:
                case StatType.SkeletonShardDrop:
                case StatType.DemonShardDrop:
                    // Creature rewards vary by creature/level, so show the total training multiplier.
                    return "×" + Number(multiplier);
                case StatType.CrystalShardDropBonus:
                    return Signed(bonus);
            }

            var level = GetLevel();
            return stat switch
            {
                StatType.MapSize => level != null ? Number(level.minPlayZoneSize + (int)bonus) : Signed((int)bonus),
                StatType.SpawnSpeed => level != null
                    ? Seconds(Mathf.Max(level.minSpawnInterval, level.spawnInterval * (1f - bonus)))
                    : Signed(-bonus * 100f) + "%",
                StatType.BombSpawnSpeed => level != null
                    ? Seconds(Mathf.Max(level.minBombSpawnInterval, level.bombSpawnInterval * (1f - bonus)))
                    : Signed(-bonus * 100f) + "%",
                StatType.SessionTime => level != null
                    ? Seconds(100f / Mathf.Max(0.0001f, level.heatIndex) - _player.ArmorIndex / 2.5f + bonus)
                    : Signed(bonus) + " " + ItemText.Get("skill.preview.seconds"),
                StatType.InitialEnemySpawnDensity => Number(Mathf.Max(0f, (level?.initialEnemySpawnDensity ?? 0f) + bonus) * 100f) + "%",
                StatType.InitialBombSpawnDensity => Number(Mathf.Max(0f, (level?.initialBombSpawnDensity ?? 0f) + bonus) * 100f) + "%",
                _ => Signed(bonus)
            };
        }

        private DungeonLevelConfig GetLevel()
        {
            var dungeon = _selection.SelectedDungeon ?? _dungeons.GetFirstPlayable();
            return dungeon != null && dungeon.TryGetLevel(_selection.GetStartLevelIndex(dungeon), out var level)
                ? level : null;
        }

        private static string Number(float value) => BigDoubleFormatter.Format(value);
        private static string Signed(float value) => (value >= 0f ? "+" : "") + Number(value);
        private static string Seconds(float value) => Number(value) + " " + ItemText.Get("skill.preview.seconds");
    }
}
