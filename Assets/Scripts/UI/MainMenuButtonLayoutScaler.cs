using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class MainMenuButtonLayoutScaler : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private VerticalLayoutGroup _layoutGroup;
        [SerializeField, Min(0.01f)] private float _buttonAspectRatio = 3.9782813f;
        [SerializeField, Min(1f)] private float _minButtonHeight = 110f;
        [SerializeField, Min(1f)] private float _maxButtonHeight = 135f;

        private readonly List<MainMenuButtonView> _buttons = new();
        private Vector2 _lastContainerSize;
        private int _lastActiveButtonCount = -1;
        private float _lastAppliedHeight = -1f;

        private void Reset()
        {
            _container = transform as RectTransform;
            _layoutGroup = GetComponent<VerticalLayoutGroup>();
        }

        private void Awake()
        {
            EnsureReferences();
            ApplyButtonLayout(true);
        }

        private void OnEnable()
        {
            EnsureReferences();
            ApplyButtonLayout(true);
        }

        private void OnValidate()
        {
            EnsureReferences();
            ApplyButtonLayout(true);
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyButtonLayout(true);
        }

        private void LateUpdate()
        {
            ApplyButtonLayout(false);
        }

        private void EnsureReferences()
        {
            if (_container == null)
                _container = transform as RectTransform;

            if (_layoutGroup == null)
                _layoutGroup = GetComponent<VerticalLayoutGroup>();
        }

        private void ApplyButtonLayout(bool force)
        {
            if (_container == null)
                return;

            GetComponentsInChildren(true, _buttons);

            int activeButtonCount = 0;
            foreach (var button in _buttons)
            {
                if (button != null && button.gameObject.activeSelf)
                    activeButtonCount++;
            }

            if (activeButtonCount == 0)
                return;

            Vector2 containerSize = _container.rect.size;
            if (!force
                && activeButtonCount == _lastActiveButtonCount
                && containerSize == _lastContainerSize)
            {
                return;
            }

            float spacing = _layoutGroup != null ? _layoutGroup.spacing : 0f;
            float totalSpacing = spacing * Mathf.Max(0, activeButtonCount - 1);
            float availableHeight = Mathf.Max(1f, containerSize.y - totalSpacing);
            float heightByAvailableHeight = availableHeight / activeButtonCount;
            float heightByAvailableWidth = Mathf.Max(1f, containerSize.x) / _buttonAspectRatio;
            float targetHeight = Mathf.Min(heightByAvailableHeight, heightByAvailableWidth, _maxButtonHeight);

            if (heightByAvailableHeight >= _minButtonHeight)
                targetHeight = Mathf.Max(targetHeight, _minButtonHeight);

            foreach (var button in _buttons)
            {
                if (button == null)
                    continue;

                var layoutElement = button.GetComponent<LayoutElement>();
                if (layoutElement == null)
                    continue;

                layoutElement.minHeight = targetHeight;
                layoutElement.preferredHeight = targetHeight;
                layoutElement.flexibleHeight = 0f;
            }

            _lastActiveButtonCount = activeButtonCount;
            _lastContainerSize = containerSize;
            _lastAppliedHeight = targetHeight;

            LayoutRebuilder.MarkLayoutForRebuild(_container);
        }
    }
}
