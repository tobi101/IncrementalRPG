using UnityEngine;
using UnityEngine.UI;

namespace Core.Test
{
    // Прямая линия между двумя точками на UGUI Canvas.
    // Префаб: RectTransform + Image (1px белый спрайт).
    // Pivot = (0.5, 0.5), Anchor = top-left угол родителя.
    [RequireComponent(typeof(Image))]
    public class SkillTreeLine : MonoBehaviour
    {
        private RectTransform _rect;

        private void Awake() => _rect = GetComponent<RectTransform>();

        public void SetPositions(Vector2 from, Vector2 to)
        {
            Vector2 dir = to - from;
            float length = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            _rect.anchoredPosition = (from + to) * 0.5f;
            _rect.sizeDelta = new Vector2(length, _rect.sizeDelta.y);
            _rect.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
