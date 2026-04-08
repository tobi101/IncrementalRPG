using System;
using Core.Gameplay;
using Core.Save;
using Utils;

namespace Model
{
    public class Player : ISaveable
    {
        public event Action OnGoldChanged;
        private PlayerInfo _playerInfo;
        private readonly DamageZoneConfig _damageZoneConfig;

        public int StartSpawnObjectCount => _playerInfo.StartSpawnObjectCount;

        public ZoneSize ZoneSize => _playerInfo.ZoneSize.Radius > 0f
            ? _playerInfo.ZoneSize
            : new ZoneSize { Radius = _damageZoneConfig.baseRadius };

        public BigDouble GoldTotal
        {
            get => _playerInfo.GoldTotal;
            set { _playerInfo.GoldTotal = value; OnGoldChanged?.Invoke(); }
        }

        public Player(DamageZoneConfig damageZoneConfig)
        {
            _damageZoneConfig = damageZoneConfig;
        }

        public void Load(SaveData data)
        {
            _playerInfo = data.SavedPlayerInfo;
        }

        public void Contribute(SaveData data)
        {
            data.SavedPlayerInfo = _playerInfo;
        }
    }
}
