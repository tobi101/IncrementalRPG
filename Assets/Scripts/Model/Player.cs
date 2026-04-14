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

        public BigDouble GoldTotal
        {
            get => _playerInfo.GoldTotal;
            set { _playerInfo.GoldTotal = value; OnGoldChanged?.Invoke(); }
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
