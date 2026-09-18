using System.Collections.Generic;
using UnityEngine;

namespace Core.Items
{
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "RPG/Items/Item Catalog")]
    public sealed class ItemCatalog : ScriptableObject
    {
        [SerializeField] private ItemDefinition[] _items;

        private Dictionary<string, ItemDefinition> _byId;

        public ItemDefinition Get(string itemId)
        {
            if (TryGet(itemId, out var item))
                return item;
            throw new KeyNotFoundException($"Unknown item: {itemId}");
        }

        public bool TryGet(string itemId, out ItemDefinition definition)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, ItemDefinition>(_items.Length);
                foreach (var item in _items)
                    _byId.Add(item.itemId, item);
            }

            definition = null;
            return itemId != null && _byId.TryGetValue(itemId, out definition);
        }

        private void OnEnable()
        {
            _byId = null;
        }
    }
}
