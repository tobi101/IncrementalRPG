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

        public void Setup(Vector2 from, Vector2 to)
        {
            var delta    = to - from;
            var distance = delta.magnitude;
            var angle    = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            var rt  = (RectTransform)transform;
            rt.pivot            = new Vector2(0f, 0.5f);
            rt.sizeDelta        = new Vector2(distance, _thickness);
            rt.anchoredPosition = from;
            rt.localEulerAngles = new Vector3(0f, 0f, angle);
        }
    }
}
