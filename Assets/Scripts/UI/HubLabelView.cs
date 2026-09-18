using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class HubLabelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _backdrop;
        [Tooltip("Space around the visible letters, in canvas units on each side.")]
        [SerializeField] private Vector2 _padding = new(40f, 28f);

        private bool _layoutDirty = true;

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            _layoutDirty = true;
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void LateUpdate()
        {
            if (!_layoutDirty || _text == null || !_text.isActiveAndEnabled)
                return;

            _text.ForceMeshUpdate();
            FitBackdrop();
            _layoutDirty = false;
        }

        private void OnTextChanged(Object changedText)
        {
            // Defer resizing until LateUpdate: TMP can notify during a Canvas rebuild.
            if (changedText == _text)
                _layoutDirty = true;
        }

        private void FitBackdrop()
        {
            if (_text == null || _backdrop == null)
                return;

            // Text and backdrop are siblings with matching centered pivots in HubLabel.
            // Glyph quads keep the light centered on the letters, excluding font descenders.
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            bool hasVisibleCharacters = false;
            for (int i = 0; i < _text.textInfo.characterCount; i++)
            {
                var character = _text.textInfo.characterInfo[i];
                if (!character.isVisible)
                    continue;

                hasVisibleCharacters = true;
                min = Vector2.Min(min, character.bottomLeft);
                max = Vector2.Max(max, character.topRight);
            }

            _backdrop.enabled = hasVisibleCharacters;
            if (!hasVisibleCharacters)
                return;

            RectTransform rect = _backdrop.rectTransform;
            rect.anchoredPosition = _text.rectTransform.anchoredPosition + (min + max) * 0.5f;
            rect.sizeDelta = max - min + _padding * 2f;
        }

        private void OnRectTransformDimensionsChange()
        {
            _layoutDirty = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _padding = Vector2.Max(Vector2.zero, _padding);
            _layoutDirty = true;
        }
#endif
    }
}
