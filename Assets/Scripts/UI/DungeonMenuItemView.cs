using System;
using Core.Gameplay.Dungeon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DungeonMenuItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelCountText;
        [SerializeField] private Image _iconImage;

        private DungeonConfig _dungeon;
        private Action<DungeonConfig> _onClicked;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();
        }

        public void Bind(DungeonConfig dungeon, Action<DungeonConfig> onClicked)
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClicked);

            _dungeon = dungeon;
            _onClicked = onClicked;

            if (_nameText != null)
                _nameText.text = dungeon != null ? dungeon.DisplayName : string.Empty;

            if (_levelCountText != null)
                _levelCountText.text = dungeon != null ? dungeon.LevelCount.ToString() : "0";

            if (_iconImage != null)
            {
                var hasIcon = dungeon != null && dungeon.icon != null;
                _iconImage.sprite = hasIcon ? dungeon.icon : null;
                _iconImage.enabled = hasIcon;
            }

            if (_button != null)
            {
                _button.interactable = dungeon != null && dungeon.HasPlayableLevels;
                _button.onClick.AddListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            if (_dungeon == null || !_dungeon.HasPlayableLevels) return;
            _onClicked?.Invoke(_dungeon);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClicked);
        }
    }
}
