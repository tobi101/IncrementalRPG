using TMPro;
using UnityEngine;
using Utils;

namespace Core.Gameplay
{
    public class GoldWalletView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        // private GoldWallet _wallet;
        //
        // public void Bind(GoldWallet wallet)
        // {
        //     _wallet = wallet;
        //     _wallet.OnChanged += HandleChanged;
        //     HandleChanged(_wallet.Total);
        // }
        //
        // private void OnDestroy()
        // {
        //     if (_wallet != null)
        //         _wallet.OnChanged -= HandleChanged;
        // }
        //
        // private void HandleChanged(BigDouble total)
        // {
        //     if (_text != null)
        //         _text.text = $"Gold: {total}";
        // }
    }
}
