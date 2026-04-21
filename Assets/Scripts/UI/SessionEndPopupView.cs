using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class SessionEndPopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _killsText;
        [SerializeField] private Button _hubButton;

        private Action _onHubClicked;

        private void Awake()
        {
            _hubButton.onClick.AddListener(OnHubButtonClicked);
            gameObject.SetActive(false);
        }

        public void Show(BigDouble gold, int kills, Action onHubClicked)
        {
            _onHubClicked = onHubClicked;
            _goldText.text = gold.ToString();
            _killsText.text = kills.ToString();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onHubClicked = null;
        }

        private void OnHubButtonClicked()
        {
            _onHubClicked?.Invoke();
        }
    }
}
