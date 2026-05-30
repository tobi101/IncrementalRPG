using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public enum MainMenuAction
    {
        NewGame,
        Continue,
        Settings,
        Authors,
        Exit
    }

    public class MainMenuButtonView : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private MainMenuAction _action;
        [SerializeField] private Button _button;
        [FormerlySerializedAs("_pressedVisuals")]
        [SerializeField] private GameObject[] _hoverVisuals;
        [SerializeField] private RectTransform _pressTarget;
        [SerializeField, Min(0f)] private float _pressedScale = 0.94f;

        public MainMenuAction Action => _action;
        public Button Button => _button;

        private Vector3 _pressTargetBaseScale = Vector3.one;

        private void Reset()
        {
            _button = GetComponent<Button>();
            _pressTarget = transform.Find("ButtonVisualRoot/ButtonImage") as RectTransform;
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_pressTarget == null)
                _pressTarget = transform.Find("ButtonVisualRoot/ButtonImage") as RectTransform;

            if (_pressTarget != null)
                _pressTargetBaseScale = _pressTarget.localScale;

            SetHoverVisualsVisible(false);
            ResetPressTargetScale();
        }

        private void OnEnable()
        {
            SetHoverVisualsVisible(false);
            ResetPressTargetScale();
        }

        private void OnDisable()
        {
            SetHoverVisualsVisible(false);
            ResetPressTargetScale();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract())
                return;

            SetHoverVisualsVisible(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanInteract())
                return;

            SetPressTargetScale(_pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetPressTargetScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHoverVisualsVisible(false);
            ResetPressTargetScale();
        }

        private void SetHoverVisualsVisible(bool visible)
        {
            if (_hoverVisuals == null)
                return;

            foreach (var visual in _hoverVisuals)
            {
                if (visual != null)
                    visual.SetActive(visible);
            }
        }

        private void SetPressTargetScale(float scale)
        {
            if (_pressTarget != null)
                _pressTarget.localScale = _pressTargetBaseScale * scale;
        }

        private void ResetPressTargetScale()
        {
            if (_pressTarget != null)
                _pressTarget.localScale = _pressTargetBaseScale;
        }

        private bool CanInteract()
        {
            return _button == null || _button.interactable;
        }
    }
}
