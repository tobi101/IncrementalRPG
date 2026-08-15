using UnityEngine;
using Utils;

namespace Core.Gameplay.Shards
{
    [CreateAssetMenu(fileName = "ShardPickupConfig", menuName = "IncrementalRPG/Shard Pickup Config")]
    public sealed class ShardPickupConfig : ScriptableObject
    {
        [Header("Presentation")]
        public ShardPickupView pickupPrefab;
        public Sprite icon;

        [Header("Drop")]
        public BigDouble basePickupValue = 10;
        [Min(1)] public int maxPickupCount = 32;
        [Min(0.01f)] public float lifetime = 15f;
        [Min(0f)] public float hitRadius = 0.12f;

        [Header("Scatter")]
        [Min(0f)] public float scatterDuration = 0.4f;
        [Min(0f)] public float minScatterDistance = 0.35f;
        [Min(0f)] public float maxScatterDistance = 0.8f;

        [Header("Collection")]
        [Min(0.01f)] public float baseCollectionDuration = 1f;

        private void OnValidate()
        {
            basePickupValue = BigDoubleMath.SanitizeNonNegativeInteger(basePickupValue, BigDouble.One);
            if (basePickupValue < BigDouble.One)
                basePickupValue = BigDouble.One;

            maxPickupCount = Mathf.Max(1, maxPickupCount);
            lifetime = Mathf.Max(0.01f, lifetime);
            hitRadius = Mathf.Max(0f, hitRadius);
            scatterDuration = Mathf.Max(0f, scatterDuration);
            minScatterDistance = Mathf.Max(0f, minScatterDistance);
            maxScatterDistance = Mathf.Max(minScatterDistance, maxScatterDistance);
            baseCollectionDuration = Mathf.Max(0.01f, baseCollectionDuration);
        }
    }
}
