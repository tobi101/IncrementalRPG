using UnityEngine;

namespace Core.Items
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "RPG/Items/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;
        public Sprite icon;

        [Header("Inventory")]
        public bool stackable;
        [Min(1)] public int maxStackSize = 1;

        private void OnValidate()
        {
            if (!stackable)
                maxStackSize = 1;
            else
                maxStackSize = Mathf.Max(1, maxStackSize);
        }
    }
}
