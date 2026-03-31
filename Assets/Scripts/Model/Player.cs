using Core.Save;
using Reflex.Attributes;

namespace Model
{
    public class Player
    {
        private PlayerInfo _playerInfo;

        [Inject] private SaveService _saveService;

        public Player()
        {
            if (_saveService != null)
            {
                _playerInfo = _saveService.GetData().SavedPlayerInfo;
            }
        }
    }
}