using TMPro;
using UnityEngine;

namespace Core.Gameplay
{
    public class CoinPileView : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _amountText;

        private CoinPile _pile;

        public void Bind(CoinPile pile, Vector3 worldPosition)
        {
            _pile = pile;
            transform.position = worldPosition;
            _pile.OnChanged += Refresh;
            Refresh();
        }

        public void Unbind()
        {
            if (_pile == null) return;
            _pile.OnChanged -= Refresh;
            _pile = null;
        }

        private void Refresh()
        {
            if (_amountText != null)
                _amountText.text = _pile.Amount.ToString();
        }
    }
}
