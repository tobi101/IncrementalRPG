using Core.Gameplay;
using Core.Save;
using Reflex.Attributes;

namespace Model
{
    public class Player
    {
        private PlayerInfo _playerInfo;
        private readonly DamageZoneConfig _damageZoneConfig;

        [Inject] private SaveService _saveService;

        public int StartSpawnObjectCount => _playerInfo.StartSpawnObjectCount;

        public ZoneSize ZoneSize => _playerInfo.ZoneSize.Radius > 0f
            ? _playerInfo.ZoneSize
            : new ZoneSize { Radius = _damageZoneConfig.baseRadius };

        public Player(DamageZoneConfig damageZoneConfig)
        {
            _damageZoneConfig = damageZoneConfig;
            if (_saveService != null)
                _playerInfo = _saveService.GetData().SavedPlayerInfo;
        }
    }
}
