using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "DamageZoneConfig", menuName = "IncrementalRPG/Damage Zone Config")]
    public class DamageZoneConfig : ScriptableObject
    {
        public BigDouble damagePerTick = BigDouble.One;
        [FormerlySerializedAs("tickInterval")]
        public float baseManualAttackCooldown = 1f;
        public float baseAutoAttackInterval = 1f;

        [Header("Special Attack")]
        [Min(0f)] public float baseSpecialAttackCooldown = 5f;
        [Min(0f)] public float specialAttackDamageMultiplier = 5f;

        public float baseRadius = 0.6f;
        public float aspectRatio = 0.55f;

        private void OnValidate()
        {
            damagePerTick = BigDoubleMath.SanitizeNonNegativeInteger(damagePerTick, BigDouble.One);
        }
    }
}
