using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Gameplay
{
    [RequireComponent(typeof(SortingGroup))]
    public class IsometricSorter : MonoBehaviour
    {
        private SortingGroup _sortingGroup;

        private void Awake()
        {
            _sortingGroup = GetComponent<SortingGroup>();
        }

        private void LateUpdate()
        {
            _sortingGroup.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
        }
    }
}
