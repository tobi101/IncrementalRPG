using System;
using Utils;

namespace Model
{
    [Serializable]
    public struct PlayerInfo
    {
        // Player Resources
        public BigDouble GoldTotal;
        public BigDouble ShardTotal;
        public BigDouble BestSessionGold;
        public int BestSessionKills;

        // Armor
        public float ArmorIndex;

        public static PlayerInfo Default => new PlayerInfo
        {
            GoldTotal = 10,
            ShardTotal = 0,
            BestSessionGold = BigDouble.Zero,
            BestSessionKills = 0,
            ArmorIndex = 1f
        };
    }
}
