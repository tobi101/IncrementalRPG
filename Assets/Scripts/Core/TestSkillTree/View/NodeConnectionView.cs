using UnityEngine;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    // Renders a straight line between two points in Content local space.
    // Prefab requirements: pivot = (0, 0.5), RectTransform + Image (Raycast Target = false).
    [RequireComponent(typeof(Image))]
    public class NodeConnectionView : MonoBehaviour
    {
        [SerializeField] private float _thickness = 4f;

        private RectTransform _rectTransform;
        private float _fullLength;

        public void Setup(Vector2 from, Vector2 to)
        {
            var delta    = to - from;
            var distance = delta.magnitude;
            var angle    = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            _rectTransform = (RectTransform)transform;
            _fullLength = distance;

            _rectTransform.pivot            = new Vector2(0f, 0.5f);
            _rectTransform.sizeDelta        = new Vector2(distance, _thickness);
            _rectTransform.anchoredPosition = from;
            _rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
        }

        public void Refresh(NodeState state)
        {
            var isVisible = state != NodeState.Hidden;
            gameObject.SetActive(isVisible);

            if (isVisible)
                SetRevealProgress(1f);
        }

        public void PrepareReveal()
        {
            gameObject.SetActive(true);
            SetRevealProgress(0f);
        }

        public void SetRevealProgress(float progress)
        {
            var rt = GetRectTransform();
            if (rt == null)
                return;

            if (_fullLength <= 0f)
                _fullLength = rt.sizeDelta.x;

            rt.sizeDelta = new Vector2(_fullLength * Mathf.Clamp01(progress), _thickness);
        }

        private RectTransform GetRectTransform()
        {
            if (_rectTransform == null)
                _rectTransform = (RectTransform)transform;

            return _rectTransform;
        }
    }
}
