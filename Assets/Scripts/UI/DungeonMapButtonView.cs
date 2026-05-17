using System;
using Core.Gameplay.Dungeon;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class DungeonMapButtonView : MonoBehaviour
    {
        [SerializeField] private DungeonConfig _dungeon;
        [SerializeField] private Button _button;
        [FormerlySerializedAs("_selectedState")]
        [SerializeField] private GameObject _buttonGlow;
        [SerializeField] private GameObject _mapSectionGlow;
        [SerializeField] private GameObject _mapSectionFrameGlow;

        private Action<DungeonMapButtonView> _onClicked;

        public DungeonConfig Dungeon => _dungeon;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();
        }

        public void Bind(DungeonConfig dungeon, Action<DungeonMapButtonView> onClicked)
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClicked);

            _dungeon = dungeon;
            _onClicked = onClicked;

            if (_button != null)
            {
                _button.interactable = true;
                _button.onClick.AddListener(HandleClicked);
            }
        }

        public void SetSelected(bool selected)
        {
            SetGraphicAlpha(_buttonGlow, selected);
            SetGraphicAlpha(_mapSectionGlow, selected);
            SetGraphicAlpha(_mapSectionFrameGlow, selected);
        }

        private void HandleClicked()
        {
            _onClicked?.Invoke(this);
        }

        private static void SetGraphicAlpha(GameObject target, bool visible)
        {
            if (target == null)
                return;

            var graphic = target.GetComponent<Graphic>();
            if (graphic == null)
                return;

            var color = graphic.color;
            color.a = visible ? 1f : 0f;
            graphic.color = color;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClicked);
        }
    }
}
