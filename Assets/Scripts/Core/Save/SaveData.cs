using System;
using Model;

namespace Core.Save
{
    [Serializable]
    public class SaveData
    {
        public int Version = 1;

        public PlayerInfo SavedPlayerInfo;
    }
}
