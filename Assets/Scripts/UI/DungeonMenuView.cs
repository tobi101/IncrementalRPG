using System;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DungeonMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private string _title = "Dungeons";
        [SerializeField] private DungeonMenuItemView _itemPrefab;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private bool _hideOnAwake = true;

        private readonly List<DungeonMenuItemView> _spawnedItems = new();
        private Action<DungeonConfig> _onDungeonSelected;
        private bool _isOpening;

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            if (_hideOnAwake && !_isOpening)
                gameObject.SetActive(false);
        }

        public void Show(DungeonList dungeonList, Action<DungeonConfig> onDungeonSelected)
        {
            _onDungeonSelected = onDungeonSelected;

            if (_titleText != null)
                _titleText.text = _title;

            _isOpening = true;
            gameObject.SetActive(true);
            _isOpening = false;
            Rebuild(dungeonList);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onDungeonSelected = null;
        }

        private void Rebuild(DungeonList dungeonList)
        {
            ClearItems();

            if (_itemPrefab == null || _itemsContainer == null)
            {
                Debug.LogWarning("[DungeonMenuView] Item prefab or items container is not assigned.");
                return;
            }

            if (dungeonList == null || dungeonList.dungeons == null)
                return;

            foreach (var dungeon in dungeonList.dungeons)
            {
                if (dungeon == null) continue;

                var item = Instantiate(_itemPrefab, _itemsContainer);
                item.gameObject.SetActive(true);
                item.Bind(dungeon, HandleDungeonSelected);
                _spawnedItems.Add(item);
            }
        }

        private void HandleDungeonSelected(DungeonConfig dungeon)
        {
            var callback = _onDungeonSelected;
            Hide();
            callback?.Invoke(dungeon);
        }

        private void ClearItems()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _spawnedItems.Clear();
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Hide);

            ClearItems();
        }
    }
}
