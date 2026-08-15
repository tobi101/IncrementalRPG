using System;
using Core.Save;
using Utils;

namespace Model
{
    public class Player : ISaveable
    {
        public event Action OnGoldChanged;
        public event Action OnShardsChanged;
        private PlayerInfo _playerInfo;

        public float ArmorIndex => _playerInfo.ArmorIndex;
        public BigDouble BestSessionGold => _playerInfo.BestSessionGold;
        public int BestSessionKills => _playerInfo.BestSessionKills;

        public BigDouble GoldTotal
        {
            get => _playerInfo.GoldTotal;
            set
            {
                _playerInfo.GoldTotal = BigDoubleMath.SanitizeNonNegativeInteger(value, BigDouble.Zero);
                OnGoldChanged?.Invoke();
            }
        }

        public BigDouble ShardTotal
        {
            get => _playerInfo.ShardTotal;
            private set
            {
                _playerInfo.ShardTotal = BigDoubleMath.SanitizeNonNegativeInteger(value, BigDouble.Zero);
                OnShardsChanged?.Invoke();
            }
        }

        public void AddShards(BigDouble amount)
        {
            if (amount <= 0)
                return;

            ShardTotal += amount;
        }

        public SessionRecordResult UpdateSessionRecords(BigDouble sessionGold, int sessionKills)
        {
            sessionGold = BigDoubleMath.SanitizeNonNegativeInteger(sessionGold, BigDouble.Zero);
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
            _playerInfo.GoldTotal = BigDoubleMath.SanitizeNonNegativeInteger(
                _playerInfo.GoldTotal, PlayerInfo.Default.GoldTotal);
            _playerInfo.ShardTotal = BigDoubleMath.SanitizeNonNegativeInteger(
                _playerInfo.ShardTotal, PlayerInfo.Default.ShardTotal);
            _playerInfo.BestSessionGold = BigDoubleMath.SanitizeNonNegativeInteger(
                _playerInfo.BestSessionGold, BigDouble.Zero);
            _playerInfo.BestSessionKills = Math.Max(0, _playerInfo.BestSessionKills);
        }

        public void Contribute(SaveData data)
        {
            data.SavedPlayerInfo = _playerInfo;
        }
    }
}
