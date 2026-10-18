using System;
using System.Collections.Generic;
using System.Linq;
using Core.Forge;
using Core.Save;
using UDND.Core;
using Utils;

namespace Core.Items
{
    [Serializable]
    public sealed class PlayerItemInstanceState
    {
        public string InstanceId;
        public string ItemDefinitionId;
        public int Quantity = 1;
        public string RolledNameKey;
        public int RolledIconIndex = -1;
        public string AppearanceId;
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

    public readonly struct LootDrop
    {
        public ItemDefinition Definition { get; }
        public BigDouble CurrencyAmount { get; }

        public LootDrop(ItemDefinition definition, BigDouble currencyAmount)
        {
            Definition = definition;
            CurrencyAmount = currencyAmount;
        }
    }

    public readonly struct LootReward
    {
        public string InstanceId { get; }
        public ItemDefinition Definition { get; }
        public bool IsPendingPlacement { get; }
        public BigDouble CurrencyAmount { get; }

        public LootReward(string instanceId, ItemDefinition definition, bool isPendingPlacement = false,
            BigDouble currencyAmount = default)
        {
            InstanceId = instanceId;
            Definition = definition;
            IsPendingPlacement = isPendingPlacement;
            CurrencyAmount = currencyAmount;
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
        private static readonly EquipmentSlot[] SupportedSlots = { EquipmentSlot.Helmet, EquipmentSlot.Chest, EquipmentSlot.Weapon };
        private readonly ItemCatalog _catalog;
        private readonly ForgeConfig _forgeConfig;
        private IInventoryTopology _inventoryTopology;
        private PlayerItemStorageState _state = new();

        public PlayerItemStorage(ItemCatalog catalog, ForgeConfig forgeConfig)
        { _catalog = catalog; _forgeConfig = forgeConfig; }

        public bool MigratedOnLoad { get; private set; }

        public event Action OnChanged;
        public event Action OnInventoryRefreshRequested;

        public IReadOnlyList<PlayerItemInstanceState> Items => _state.Items;
        public IEnumerable<PlayerItemInstanceState> InventoryItems => _state.Items.Where(item => !IsEquipped(item.InstanceId));

        public void ConfigureInventoryLayout(IInventoryTopology topology) => _inventoryTopology = topology;

        public bool IsEquipped(string instanceId) => !string.IsNullOrEmpty(instanceId) &&
            (_state.EquippedHelmetId == instanceId || _state.EquippedChestId == instanceId || _state.EquippedWeaponId == instanceId);

        public PlayerItemInstanceState Create(ItemDefinition definition)
        {
            return Create(definition, definition.rarity, definition.CopyDefaultStats(), definition.sellPrice);
        }

        public PlayerItemInstanceState Create(
            ItemDefinition definition,
            ItemRarity rarity,
            IReadOnlyList<ItemStatState> stats,
            BigDouble sellPrice)
        {
            if (definition.category == ItemCategory.Currency)
                throw new ArgumentException("Currency rewards are credited directly to the player.", nameof(definition));
            if (definition.category == ItemCategory.Armor)
            {
                if (!EquipmentStats.IsSupportedSlot(definition.equipmentSlot))
                    throw new ArgumentException("Armor requires Helmet, Chest or Weapon slot.", nameof(definition));
                if (!EquipmentStats.Validate(stats, out var error))
                    throw new ArgumentException(error, nameof(stats));
            }
            stats ??= Array.Empty<ItemStatState>();
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

        public bool TryGet(string instanceId, out PlayerItemInstanceState item)
        {
            item = _state.Items.FirstOrDefault(candidate => candidate.InstanceId == instanceId);
            return item != null;
        }

        public void Place(IReadOnlyList<PlayerItemInstanceState> items, int anchorIndex, int orientation)
        {
            foreach (var item in items)
            {
                if (!_state.Items.Contains(item))
                    _state.Items.Add(item);

                item.AnchorIndex = IsEquipped(item.InstanceId) ? -1 : anchorIndex;
                item.Orientation = IsEquipped(item.InstanceId) ? 0 : orientation;
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
                if (!IsEquipped(item.InstanceId) && actualPlacements.TryGetValue(item.InstanceId, out var placement))
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
            foreach (var slot in SupportedSlots)
                if (TryGetCompatibleItem(slot, GetEquipped(slot), out var item))
                    yield return item;
        }

        public float GetEquippedStatTotal(string statId)
        {
            if (!EquipmentStats.IsKnown(statId)) return 0f;
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

        public float GetEquipmentMultiplier(string statId) => 1f + GetEquippedStatTotal(statId);

        private bool TryGetCompatibleItem(EquipmentSlot slot, string instanceId, out PlayerItemInstanceState item)
        {
            item = null;
            return EquipmentStats.IsSupportedSlot(slot) &&
                   !string.IsNullOrEmpty(instanceId) && TryGet(instanceId, out item) &&
                   _catalog.TryGet(item.ItemDefinitionId, out var definition) &&
                   definition.category == ItemCategory.Armor && definition.equipmentSlot == slot;
        }

        public bool CanEquip(EquipmentSlot slot, string instanceId) =>
            !string.IsNullOrEmpty(instanceId) && TryPlanEquipment(slot, instanceId, out _, out _, out _);

        public bool CanUnequipTo(EquipmentSlot slot, string instanceId, int anchor, int orientation = 0)
        {
            if (_inventoryTopology == null || !_inventoryTopology.IsValidIndex(anchor) || GetEquipped(slot) != instanceId ||
                !TryGetCompatibleItem(slot, instanceId, out var item)) return false;
            var shape = new GameItemAdapter(item, _catalog.Get(item.ItemDefinitionId)).PlacementShape;
            var offsets = _inventoryTopology.GetPlacementOffsets(shape, orientation);
            var occupied = GetOccupiedInventoryCells();
            var cell = _inventoryTopology.ToCell(anchor);
            return offsets.Count > 0 && offsets.All(offset =>
                _inventoryTopology.TryToIndex(cell + offset, out var index) && !occupied.Contains(index));
        }

        public bool UnequipTo(EquipmentSlot slot, string instanceId, int anchor, int orientation = 0)
        {
            if (!CanUnequipTo(slot, instanceId, anchor, orientation)) return false;
            var item = Get(instanceId);
            ClearEquipmentReference(instanceId);
            item.AnchorIndex = anchor;
            item.Orientation = orientation;
            OnChanged?.Invoke();
            OnInventoryRefreshRequested?.Invoke();
            return true;
        }

        public bool Equip(EquipmentSlot slot, string instanceId)
        {
            if (!TryPlanEquipment(slot, instanceId, out var incoming, out var outgoing, out var returnAnchor)) return false;
            if (GetEquipped(slot) == instanceId) return true;
            // Commit both destinations together, only after a return placement was found.
            if (outgoing != null) { outgoing.AnchorIndex = returnAnchor; outgoing.Orientation = 0; }
            if (incoming != null) { incoming.AnchorIndex = -1; incoming.Orientation = 0; }
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
            }

            OnChanged?.Invoke();
            OnInventoryRefreshRequested?.Invoke();
            return true;
        }

        private bool TryPlanEquipment(EquipmentSlot slot, string instanceId,
            out PlayerItemInstanceState incoming, out PlayerItemInstanceState outgoing, out int returnAnchor)
        {
            incoming = outgoing = null;
            returnAnchor = -1;
            if (!EquipmentStats.IsSupportedSlot(slot)) return false;
            if (!string.IsNullOrEmpty(instanceId) &&
                (!TryGetCompatibleItem(slot, instanceId, out incoming) || !EquipmentStats.Validate(incoming.Stats, out _))) return false;
            if (GetEquipped(slot) == instanceId) return true;
            if (!TryGetCompatibleItem(slot, GetEquipped(slot), out outgoing)) return true;
            return TryFindReturnPlacement(outgoing, incoming?.InstanceId, out returnAnchor);
        }

        private bool TryFindReturnPlacement(PlayerItemInstanceState item, string vacatedId, out int anchor)
        {
            anchor = -1;
            if (_inventoryTopology == null || !_catalog.TryGet(item.ItemDefinitionId, out var definition)) return false;
            var occupied = GetOccupiedInventoryCells(vacatedId);
            var offsets = _inventoryTopology.GetPlacementOffsets(new GameItemAdapter(item, definition).PlacementShape, 0);
            if (offsets.Count == 0) return false;
            for (var candidate = 0; candidate < _inventoryTopology.CellCount; candidate++)
            {
                var cell = _inventoryTopology.ToCell(candidate);
                if (offsets.All(offset => _inventoryTopology.TryToIndex(cell + offset, out var index) && !occupied.Contains(index)))
                { anchor = candidate; return true; }
            }
            return false;
        }

        private HashSet<int> GetOccupiedInventoryCells(string vacatedId = null)
        {
            var occupied = new HashSet<int>();
            foreach (var other in InventoryItems)
            {
                if (other.InstanceId == vacatedId || !_inventoryTopology.IsValidIndex(other.AnchorIndex) ||
                    !_catalog.TryGet(other.ItemDefinitionId, out var otherDefinition)) continue;
                var cell = _inventoryTopology.ToCell(other.AnchorIndex);
                foreach (var offset in _inventoryTopology.GetPlacementOffsets(new GameItemAdapter(other, otherDefinition).PlacementShape, other.Orientation))
                    if (_inventoryTopology.TryToIndex(cell + offset, out var index)) occupied.Add(index);
            }
            return occupied;
        }

        public void Load(SaveData data)
        {
            MigratedOnLoad = false;
            _state = data.PlayerItemStorageState ?? new PlayerItemStorageState();
            _state.Items ??= new List<PlayerItemInstanceState>();
            // test2 was the early rare weapon prototype. Keep its instance/equipment
            // reference and migrate it through the current weapon definition.
            foreach (var item in _state.Items.Where(item => item.ItemDefinitionId == "test2"))
            {
                item.ItemDefinitionId = "forged_weapon";
                if (!item.HasRolledData) item.Rarity = ItemRarity.Rare;
                item.HasRolledData = true;
                MigratedOnLoad = true;
            }
            // The other retired prototypes were misc/consumable/scroll placeholders.
            foreach (var item in _state.Items.Where(item => item.ItemDefinitionId is
                         "test1" or "test3" or "test4" or "test5" or "test6").ToArray())
            {
                _state.Items.Remove(item);
                ClearEquipmentReference(item.InstanceId);
            }
            if (data.Version < 2)
            {
                foreach (var item in _state.Items)
                {
                    item.AnchorIndex = -1;
                    item.Orientation = 0;
                }
            }

            ExpandLegacyQuantities();
            foreach (var item in _state.Items)
            {
                if (!_catalog.TryGet(item.ItemDefinitionId, out var definition)) continue;
                // The old 2x1 helmet could be rotated. A single-cell placement only accepts orientation zero.
                if (definition.category == ItemCategory.Armor && definition.equipmentSlot == EquipmentSlot.Helmet)
                    item.Orientation = 0;
                // Only legacy, unrolled records may inherit definition data once.
                if (!item.HasRolledData)
                {
                    item.Stats = definition.CopyDefaultStats();
                    item.Rarity = definition.rarity;
                    item.SellPrice = definition.sellPrice;
                    item.HasRolledData = true;
                }
                MigratedOnLoad |= EquipmentItemMigration.Migrate(item, definition, _forgeConfig);
            }
            foreach (var slot in SupportedSlots)
            {
                var id = GetEquipped(slot);
                if (!CanEquip(slot, id)) { ClearEquipmentReference(id); continue; }
                var equipped = Get(id);
                if (equipped.AnchorIndex != -1 || equipped.Orientation != 0)
                {
                    equipped.AnchorIndex = -1;
                    equipped.Orientation = 0;
                    MigratedOnLoad = true;
                }
            }
            _state.EquippedBootsId = null;
            OnChanged?.Invoke();
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
                        RolledNameKey = item.RolledNameKey,
                        RolledIconIndex = item.RolledIconIndex,
                        AppearanceId = item.AppearanceId,
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
