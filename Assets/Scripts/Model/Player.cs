using System;
using Core.Save;
using Utils;

namespace Model
{
    public class Player : ISaveable
    {
        public event Action OnGoldChanged;
        private PlayerInfo _playerInfo;

        public float ArmorIndex => _playerInfo.ArmorIndex;
        public BigDouble BestSessionGold => _playerInfo.BestSessionGold;
        public int BestSessionKills => _playerInfo.BestSessionKills;

        public BigDouble GoldTotal
        {
            get => _playerInfo.GoldTotal;
            set { _playerInfo.GoldTotal = value; OnGoldChanged?.Invoke(); }
        }

        public SessionRecordResult UpdateSessionRecords(BigDouble sessionGold, int sessionKills)
        {
            var isNewGoldRecord = sessionGold > _playerInfo.BestSessionGold;
            var isNewKillsRecord = sessionKills > _playerInfo.BestSessionKills;

            if (isNewGoldRecord)
                _playerInfo.BestSessionGold = sessionGold;

            if (isNewKillsRecord)
                _playerInfo.BestSessionKills = sessionKills;

            return new SessionRecordResult(isNewGoldRecord, isNewKillsRecord);
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
