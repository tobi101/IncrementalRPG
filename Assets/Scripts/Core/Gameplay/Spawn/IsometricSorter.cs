using UnityEngine;
using UnityEngine.Rendering;
using Entity;

namespace Core.Gameplay
{
    [RequireComponent(typeof(SortingGroup))]
    [DefaultExecutionOrder(100)] // After CreatureView synchronizes its ground position in LateUpdate.
    public class IsometricSorter : MonoBehaviour
    {
        private SortingGroup _sortingGroup;
        private CreatureView _creatureView;

        private void Awake()
        {
            _sortingGroup = GetComponent<SortingGroup>();
            _creatureView = GetComponent<CreatureView>();
        }

        private void LateUpdate()
        {
            var groundPosition = _creatureView != null ? _creatureView.FootWorldPosition : transform.position;
            _sortingGroup.sortingOrder = Mathf.RoundToInt(-groundPosition.y * 100);
        }
    }
}
