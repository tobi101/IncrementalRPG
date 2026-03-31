using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entity
{
    [Serializable]
    public class SpawnEntry
    {
        public EntityConfig config;
        [Min(0f)] public float weight = 1f;
    }

    [CreateAssetMenu(fileName = "SpawnTable", menuName = "RPG/Spawn Table")]
    public class SpawnTable : ScriptableObject
    {
        public SpawnEntry[] entries;

        public EntityConfig Pick()
        {
            if (entries == null || entries.Length == 0) return null;

            float total = 0f;
            foreach (var e in entries)
                total += e.weight;

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var e in entries)
            {
                cumulative += e.weight;
                if (roll < cumulative)
                    return e.config;
            }

            return entries[entries.Length - 1].config;
        }
    }
}
