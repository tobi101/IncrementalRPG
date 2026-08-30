using System;
using System.Collections.Generic;
using System.Linq;
using Core.Save;
using Utils;

namespace Core.Items
{
    [Serializable]
    public sealed class PlayerItemInstanceState
    {
        public string InstanceId;
        public string ItemDefinitionId;
        public int Quantity = 1;
        public int AnchorIndex = -1;
        public int Orientation;
        public bool HasRolledData;
        public ItemRarity Rarity;
        public BigDouble SellPrice;
        public List<ItemStatState> Stats = new();
    }

    [Serializable]
    public sealed class ItemStatState
    {
        public string StatId;
        public float Value;
    }

    [Serializable]
    public sealed class PlayerItemStorageState
    {
        public List<PlayerItemInstanceState> Items = new();
        public string EquippedHelmetId;
        public string EquippedChestId;
        public string EquippedWeaponId;
        public string EquippedBootsId;
    }

    public readonly struct LootReward
    {
        public string InstanceId { get; }
        public ItemDefinition Definition { get; }

        public LootReward(string instanceId, ItemDefinition definition)
        {
            InstanceId = instanceId;
            Definition = definition;
        }
    }

    public sealed class LootBatch
    {
        public IReadOnlyList<LootReward> Rewards { get; }

        public LootBatch(IReadOnlyList<LootReward> rewards)
        {
            Rewards = rewards;
        }
    }

    public sealed class PlayerItemStorage : ISaveable
    {
        private PlayerItemStorageState _state = new();

        public event Action OnChanged;
        public event Action OnInventoryRefreshRequested;

        public IReadOnlyList<PlayerItemInstanceState> Items => _state.Items;

        public PlayerItemInstanceState Create(ItemDefinition definition)
        {
            var stats = new List<ItemStatState>(definition.defaultStats.Length);
            foreach (var stat in definition.defaultStats)
            {
                stats.Add(new ItemStatState
                {
                    StatId = stat.statId,
                    Value = stat.value
                });
            }

            return Create(definition, definition.rarity, stats, definition.sellPrice);
        }

        public PlayerItemInstanceState Create(
            ItemDefinition definition,
            ItemRarity rarity,
            IReadOnlyList<ItemStatState> stats,
            BigDouble sellPrice)
        {
            var rolledStats = new List<ItemStatState>(stats.Count);
            foreach (var stat in stats)
            {
                rolledStats.Add(new ItemStatState
                {
                    StatId = stat.StatId,
                    Value = stat.Value
                });
            }

            return new PlayerItemInstanceState
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                ItemDefinitionId = definition.itemId,
                Quantity = 1,
                AnchorIndex = -1,
                Orientation = 0,
                HasRolledData = true,
                Rarity = rarity,
                SellPrice = sellPrice,
                Stats = rolledStats
            };
        }

        public PlayerItemInstanceState Get(string instanceId)
        {
            return _state.Items.First(item => item.InstanceId == instanceId);
        }

        public void Place(IReadOnlyList<PlayerItemInstanceState> items, int anchorIndex, int orientation)
        {
            foreach (var item in items)
            {
                if (!_state.Items.Contains(item))
                    _state.Items.Add(item);

                item.AnchorIndex = anchorIndex;
                item.Orientation = orientation;
            }

            OnChanged?.Invoke();
        }

        public void Detach(IReadOnlyList<PlayerItemInstanceState> items)
        {
            foreach (var item in items)
                item.AnchorIndex = -1;

            OnChanged?.Invoke();
        }

        public void Remove(IReadOnlyList<PlayerItemInstanceState> items)
        {
            foreach (var item in items)
            {
                _state.Items.Remove(item);
                ClearEquipmentReference(item.InstanceId);
            }

            OnChanged?.Invoke();
        }

        public bool Consume(string instanceId)
        {
            var item = _state.Items.FirstOrDefault(candidate => candidate.InstanceId == instanceId);
            if (item == null)
                return false;

            _state.Items.Remove(item);
            ClearEquipmentReference(instanceId);
            OnChanged?.Invoke();
            OnInventoryRefreshRequested?.Invoke();
            return true;
        }

        public void SynchronizePlacements(IEnumerable<(PlayerItemInstanceState Item, int Anchor, int Orientation)> placements)
        {
            var actualPlacements = new Dictionary<string, (int Anchor, int Orientation)>();
            foreach (var placement in placements)
                actualPlacements.Add(placement.Item.InstanceId, (placement.Anchor, placement.Orientation));

            var changed = false;
            foreach (var item in _state.Items)
            {
                var anchor = -1;
                var orientation = 0;
                if (actualPlacements.TryGetValue(item.InstanceId, out var placement))
                {
                    anchor = placement.Anchor;
                    orientation = placement.Orientation;
                }

                if (item.AnchorIndex == anchor && item.Orientation == orientation)
                    continue;

                item.AnchorIndex = anchor;
                item.Orientation = orientation;
                changed = true;
            }

            if (changed)
                OnChanged?.Invoke();
        }

        public string GetEquipped(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Helmet => _state.EquippedHelmetId,
                EquipmentSlot.Chest => _state.EquippedChestId,
                EquipmentSlot.Weapon => _state.EquippedWeaponId,
                EquipmentSlot.Boots => _state.EquippedBootsId,
                _ => null
            };
        }

        public IEnumerable<PlayerItemInstanceState> GetEquippedItems()
        {
            return _state.Items.Where(item =>
                item.InstanceId == _state.EquippedHelmetId ||
                item.InstanceId == _state.EquippedChestId ||
                item.InstanceId == _state.EquippedWeaponId ||
                item.InstanceId == _state.EquippedBootsId);
        }

        public float GetEquippedStatTotal(string statId)
        {
            var total = 0f;
            foreach (var item in GetEquippedItems())
            {
                foreach (var stat in item.Stats)
                {
                    if (stat.StatId == statId)
                        total += stat.Value;
                }
            }

            return total;
        }

        public void Equip(EquipmentSlot slot, string instanceId)
        {
            switch (slot)
            {
                case EquipmentSlot.Helmet:
                    _state.EquippedHelmetId = instanceId;
                    break;
                case EquipmentSlot.Chest:
                    _state.EquippedChestId = instanceId;
                    break;
                case EquipmentSlot.Weapon:
                    _state.EquippedWeaponId = instanceId;
                    break;
                case EquipmentSlot.Boots:
                    _state.EquippedBootsId = instanceId;
                    break;
            }

            OnChanged?.Invoke();
        }

        public void Load(SaveData data)
        {
            _state = data.PlayerItemStorageState ?? new PlayerItemStorageState();
            _state.Items ??= new List<PlayerItemInstanceState>();
            if (data.Version < 2)
            {
                foreach (var item in _state.Items)
                {
                    item.AnchorIndex = -1;
                    item.Orientation = 0;
                }
            }

            ExpandLegacyQuantities();
            OnInventoryRefreshRequested?.Invoke();
        }

        public void Contribute(SaveData data)
        {
            data.PlayerItemStorageState = _state;
        }

        private void ExpandLegacyQuantities()
        {
            var expanded = new List<PlayerItemInstanceState>();
            foreach (var item in _state.Items)
            {
                var quantity = Math.Max(1, item.Quantity);
                item.Quantity = 1;
                item.Stats ??= new List<ItemStatState>();
                expanded.Add(item);

                for (var i = 1; i < quantity; i++)
                {
                    expanded.Add(new PlayerItemInstanceState
                    {
                        InstanceId = Guid.NewGuid().ToString("N"),
                        ItemDefinitionId = item.ItemDefinitionId,
                        Quantity = 1,
                        AnchorIndex = item.AnchorIndex,
                        Orientation = item.Orientation,
                        HasRolledData = item.HasRolledData,
                        Rarity = item.Rarity,
                        SellPrice = item.SellPrice,
                        Stats = item.Stats.Select(stat => new ItemStatState
                        {
                            StatId = stat.StatId,
                            Value = stat.Value
                        }).ToList()
                    });
                }
            }

            _state.Items = expanded;
        }

        private void ClearEquipmentReference(string instanceId)
        {
            if (_state.EquippedHelmetId == instanceId)
                _state.EquippedHelmetId = null;
            if (_state.EquippedChestId == instanceId)
                _state.EquippedChestId = null;
            if (_state.EquippedWeaponId == instanceId)
                _state.EquippedWeaponId = null;
            if (_state.EquippedBootsId == instanceId)
                _state.EquippedBootsId = null;
        }
    }
}
