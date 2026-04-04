using Utils;

namespace Model
{
    public struct PlayerInfo
    {
        // Map
        public int MapSize;

        // Damage Zone
        public int ZoneDamage;
        public float ZoneDamageSpeed;

        public ZoneSize ZoneSize;

        // Player Resources
        public BigDouble GoldTotal;
        public BigDouble ShardTotal;

        // Session Resources
        public float SessionDuration;

        // Spawn
        public float SpawnSpeed;
        public int StartSpawnObjectCount;
        public int SpawnObjectCountMax;

        public static PlayerInfo Default => new PlayerInfo
        {
            MapSize = 0,
            //-----
            ZoneDamage = 0,
            ZoneDamageSpeed = 0,
            ZoneSize = new ZoneSize { Radius = 0f },
            //-----
            GoldTotal = 100,
            ShardTotal = 0,
            //-----
            SessionDuration = 0f,
            //-----
            SpawnSpeed = 0f,
            StartSpawnObjectCount = 0,
            SpawnObjectCountMax = 0
            //-----
        };
    }
}