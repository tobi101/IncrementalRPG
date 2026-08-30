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
            if (_byId == null)
            {
                _byId = new Dictionary<string, ItemDefinition>(_items.Length);
                foreach (var item in _items)
                    _byId.Add(item.itemId, item);
            }

            return _byId[itemId];
        }

        private void OnEnable()
        {
            _byId = null;
        }
    }
}
