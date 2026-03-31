using UnityEngine;

namespace Core.Gameplay
{
    [CreateAssetMenu(fileName = "DamageZoneConfig", menuName = "IncrementalRPG/Damage Zone Config")]
    public class DamageZoneConfig : ScriptableObject
    {
        public int damagePerTick = 10;
        public float tickInterval = 1f;
        public float detectionRadiusX = 1f;
        public float detectionRadiusY = 0.5f;
        public float collectTime = 2f;
    }
}
