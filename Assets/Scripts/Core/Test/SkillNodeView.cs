using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Test
{
    public class SkillNodeView : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private Button _button;

        [Header("Цвета состояний")]
        [SerializeField] private Color _unlockedColor = Color.white;
        [SerializeField] private Color _partialColor  = new Color(0.9f, 0.8f, 0.1f);
        [SerializeField] private Color _fullColor     = new Color(0.2f, 0.9f, 0.2f);

        public SkillNodeConfig Config { get; private set; }

        // Подписывайся в SkillTreeView для обработки клика по узлу
        public event Action<SkillNodeConfig> OnUpgradeRequested;

        private void Awake()
        {
            _button.onClick.AddListener(() => OnUpgradeRequested?.Invoke(Config));
        }

        public void Setup(SkillNodeConfig config)
        {
            Config = config;
            _nameLabel.text = config.DisplayName;
            if (_icon != null && config.Icon != null)
                _icon.sprite = config.Icon;
        }

        public void Refresh(NodeVisibility visibility, int currentLevel)
        {
            gameObject.SetActive(visibility != NodeVisibility.Hidden);
            if (!gameObject.activeSelf) return;

            _levelLabel.text = $"{currentLevel}/{Config.MaxLevel}";
            _button.interactable = visibility != NodeVisibility.Full;

            _background.color = visibility switch
            {
                NodeVisibility.Partial  => _partialColor,
                NodeVisibility.Full     => _fullColor,
                _                       => _unlockedColor
            };
        }
    }
}
