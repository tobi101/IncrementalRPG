using UnityEngine;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "DamageZoneConfig", menuName = "IncrementalRPG/Damage Zone Config")]
    public class DamageZoneConfig : ScriptableObject
    {
        public int damagePerTick = 1;
        public float tickInterval = 1f;
        public float baseRadius = 0.6f;
        public float aspectRatio = 0.55f;
    }
}
