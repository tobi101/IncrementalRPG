using Utils;

namespace Model
{
    public struct PlayerInfo
    {
        // Map
        public int AdditionalMapSize;

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
            AdditionalMapSize = 0,
            //-----
            ZoneDamage = 2,
            ZoneDamageSpeed = 1,
            ZoneSize = ZoneSize.Default,
            //-----
            GoldTotal = 100,
            ShardTotal = 0,
            //-----
            SessionDuration = 10f,
            //-----
            SpawnSpeed = 1f,
            StartSpawnObjectCount = 1,
            SpawnObjectCountMax = 5
            //-----
        };
    }
}