using System;
using Core.TestSkillTree;
using Entity;
using UnityEngine;

namespace Core.Gameplay.Dungeon
{
    [Serializable]
    public class FeatureSpawnConfig
    {
        public FeatureType featureType;
        public StatType spawnSpeedStat;
        [Min(0.1f)] public float spawnInterval = 5f;
        [Min(0.0000001f)] public float minSpawnInterval = 1f;
    }
}
