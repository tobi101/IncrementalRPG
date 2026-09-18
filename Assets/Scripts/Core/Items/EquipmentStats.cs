using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Core.Items
{
    // Values are fractions: 0.10 means +10%. These IDs are also written to item saves.
    public static class EquipmentStats
    {
        public const string ManualDamage = "manual_attack_damage";
        public const string AutoDamage = "auto_attack_damage";
        public const string ManualSpeed = "manual_attack_speed";
        public const string AutoSpeed = "auto_attack_speed";
        public const string ZoneRadius = "player_zone_radius";
        public const string BombDamage = "barrel_explosion_damage";
        public const string BombRadius = "barrel_explosion_radius";

        public static readonly IReadOnlyList<string> Ids = Array.AsReadOnly(new[]
        {
            ManualDamage, AutoDamage, ManualSpeed, AutoSpeed, ZoneRadius, BombDamage, BombRadius
        });

        public static bool IsKnown(string id)
        {
            foreach (var known in Ids)
                if (known == id) return true;
            return false;
        }

        public static bool IsSupportedSlot(EquipmentSlot slot) =>
            slot == EquipmentSlot.Helmet || slot == EquipmentSlot.Chest || slot == EquipmentSlot.Weapon;

        public static string Name(string id) => ItemText.Get(IsKnown(id) ? "equipment.stat." + id : "equipment.stat.unknown");

        public static CultureInfo Culture => LocalizationSettings.SelectedLocale?.Identifier.CultureInfo ?? CultureInfo.InvariantCulture;

        public static string FormatPercent(float value) => "+" + (value * 100f).ToString("0.##", Culture) + "%";

        public static string RarityName(ItemRarity rarity) => ItemText.Get("item.rarity." + rarity.ToString().ToLowerInvariant());

        public static Color RarityColor(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Rare => new Color(0.5f, 0.78f, 0.53f),
            ItemRarity.Unique => new Color(0.76f, 0.54f, 0.9f),
            ItemRarity.Legendary => new Color(1f, 0.75f, 0.3f),
            _ => new Color(0.7f, 0.72f, 0.74f)
        };

        public static bool Validate(IEnumerable<ItemStatState> stats, out string error)
        {
            var seen = new HashSet<string>();
            if (stats != null)
                foreach (var stat in stats)
                {
                    if (stat == null || !IsKnown(stat.StatId))
                    { error = "Unknown equipment stat ID."; return false; }
                    if (!seen.Add(stat.StatId))
                    { error = "Duplicate equipment stat: " + stat.StatId; return false; }
                    if (float.IsNaN(stat.Value) || float.IsInfinity(stat.Value) || stat.Value < 0f)
                    { error = "Equipment bonus must be a finite, non-negative fraction: " + stat.StatId; return false; }
                }
            error = null;
            return true;
        }
    }

    public sealed class EquipmentStatIdAttribute : PropertyAttribute { }
}
