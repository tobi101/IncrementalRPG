using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "DamageZoneConfig", menuName = "IncrementalRPG/Damage Zone Config")]
    public class DamageZoneConfig : ScriptableObject
    {
        public BigDouble baseManualAttackDamage = BigDouble.One;
        [FormerlySerializedAs("tickInterval")]
        public float baseManualAttackCooldown = 1f;
        public BigDouble baseAutoAttackDamage = BigDouble.One;
        public float baseAutoAttackInterval = 1f;

        [Header("Special Attack")]
        public BigDouble baseSpecialAttackDamage = new BigDouble(5);
        [Min(0f)] public float baseSpecialAttackCooldown = 5f;

        public float baseRadius = 0.6f;
        public float aspectRatio = 0.55f;

        private void OnValidate()
        {
            baseManualAttackDamage = BigDoubleMath.SanitizeNonNegativeInteger(baseManualAttackDamage, BigDouble.One);
            baseAutoAttackDamage = BigDoubleMath.SanitizeNonNegativeInteger(baseAutoAttackDamage, BigDouble.One);
            baseSpecialAttackDamage = BigDoubleMath.SanitizeNonNegativeInteger(baseSpecialAttackDamage, new BigDouble(5));
        }
    }
}
