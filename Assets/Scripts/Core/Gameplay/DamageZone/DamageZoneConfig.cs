using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "DamageZoneConfig", menuName = "IncrementalRPG/Damage Zone Config")]
    public class DamageZoneConfig : ScriptableObject
    {
        public int damagePerTick = 1;
        [FormerlySerializedAs("tickInterval")]
        public float baseManualAttackCooldown = 1f;
        public float baseAutoAttackInterval = 1f;

        [Header("Special Attack")]
        [Min(0f)] public float baseSpecialAttackCooldown = 5f;
        [Min(0f)] public float specialAttackDamageMultiplier = 5f;

        public float baseRadius = 0.6f;
        public float aspectRatio = 0.55f;
    }
}
