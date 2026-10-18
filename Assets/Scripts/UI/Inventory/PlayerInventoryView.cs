using System;
using System.Collections.Generic;
using System.Linq;
using Core.Items;
using Core.StateMachine;
using Core.StateMachine.States;
using Model;
using Reflex.Attributes;
using Spine.Unity;
using TMPro;
using UDND;
using UDND.Core;
using UDND.DataBinding;
using UDND.Interaction;
using UDND.Inventories;
using UDND.Rules;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace UI.Inventory
{
    [DisallowMultipleComponent]
    public sealed class PlayerInventoryView :
        PlacementInventoryDataBinding<PlayerItemInstanceState, GameItemAdapter>,
        IPlayerInventoryGateway, Core.Forge.IForgeInventoryGateway, ITransferDomainHandler
    {
        [Header("Grid")]
        [SerializeField] private UniversalInventory _runtimeInventory;
        [SerializeField] private RectTransform _slotContainer;

        [Header("Drop Areas")]
        [SerializeField] private Graphic _recycleDropPanel;
        [SerializeField] private SkeletonGraphic _recycleGraphic;
        [SerializeField] private Image _helmetSlot;
        [SerializeField] private Image _chestSlot;
        [SerializeField] private Image _weaponSlot;
        [SerializeField] private Image _bootsSlot;

        [Header("Stove Audio")]
        [SerializeField] private AudioSource _fireLoopSource;
        [SerializeField] private AudioSource _stoveOnSource;

        [Header("Menu")]
        [SerializeField] private SideMenuFlyoutView _sideMenu;

        [Header("Currency")]
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _shardText;

        [Header("Universal Drag And Drop")]
        [SerializeField] private GameObject _dragCanvasPrefab;
        [SerializeField] private GameObject _tooltipCanvasPrefab;
        [SerializeField] private TMP_Text _consumableFeedback;
        [SerializeField] private GameObject _consumableFeedbackRoot;

        [Inject] private PlayerItemStorage _storage;
        [Inject] private ItemCatalog _itemCatalog;
        [Inject] private Player _player;
        [Inject] private RunConsumableService _consumables;
        [Inject] private GameStateMachine _stateMachine;

        private readonly List<(InventoryGridSlot Slot, InventoryItemUseInput Input)> _itemUseInputs = new();
        private EquipmentDropArea _helmetDropArea;
        private EquipmentDropArea _chestDropArea;
        private EquipmentDropArea _weaponDropArea;
        private bool _started;
        private readonly HashSet<string> _usedRewardIds = new();
        private string _feedbackKey;
        private bool _applicationPaused;
        private bool _inventoryRefreshPending;

        protected override void Awake()
        {
            var dragCanvas = Instantiate(_dragCanvasPrefab);
            dragCanvas.GetComponent<Canvas>().sortingOrder = 3000;
            dragCanvas.AddComponent<InventoryDragVisualAlignment>();
            Instantiate(_tooltipCanvasPrefab);

            var slotCount = _slotContainer.childCount;
            for (var i = 0; i < slotCount; i++)
            {
                var cell = _slotContainer.GetChild(i);
                var slot = cell.gameObject.AddComponent<InventoryGridSlot>();
                slot.Configure(cell.GetComponent<Image>());
                cell.gameObject.AddComponent<SlotInputAdapter>();
                var itemUseInput = cell.gameObject.AddComponent<InventoryItemUseInput>();
                _itemUseInputs.Add((slot, itemUseInput));

                var chosenBackground = cell.Find("ItemBackgroundChosen");
                if (chosenBackground != null)
                    chosenBackground.gameObject.SetActive(false);
            }

            _inventory = _runtimeInventory;
            base.Awake();
        }

        private void Start()
        {
            UIButtonAudio.InstallInChildren(this);
            _storage.ConfigureInventoryLayout(_runtimeInventory.Topology);

            _sideMenu.ReturnToHubButton.onClick.AddListener(ReturnToHub);

            foreach (var itemUseInput in _itemUseInputs)
                itemUseInput.Input.Configure(itemUseInput.Slot, _consumables, ShowConsumableFeedback, _storage, ShowInventoryFullFeedback);

            var recycleArea = _recycleDropPanel.gameObject.AddComponent<InventoryRecycleDropArea>();
            recycleArea.Configure(_player, _recycleDropPanel, _recycleGraphic, PlayRecycleAudio);

            _helmetDropArea = CreateEquipmentDropArea(_helmetSlot, EquipmentSlot.Helmet);
            _chestDropArea = CreateEquipmentDropArea(_chestSlot, EquipmentSlot.Chest);
            _weaponDropArea = CreateEquipmentDropArea(_weaponSlot, EquipmentSlot.Weapon);
            if (_bootsSlot != null)
            {
                // Decorative silhouette only; boots are not an equipment/drop slot.
                _bootsSlot.gameObject.SetActive(true);
                _bootsSlot.raycastTarget = false;
                _bootsSlot.preserveAspect = true;
            }

            _storage.OnChanged += RefreshEquipment;
            _storage.OnInventoryRefreshRequested += RequestInventoryRefresh;
            _player.OnGoldChanged += RefreshGold;
            _player.OnShardsChanged += RefreshShards;
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;

            _started = true;
            base.OnEnable();
            RefreshEquipment();
            RefreshGold();
            RefreshShards();
        }

        protected override void OnEnable()
        {
            if (_started)
            {
                base.OnEnable();
                RefreshGold();
                RefreshShards();
            }

            _fireLoopSource?.Play();
            RefreshStoveAudioPause();
        }

        protected override void OnDisable()
        {
            _fireLoopSource?.Stop();
            _stoveOnSource?.Stop();
            ClearConsumableFeedback();
            if (_started)
                base.OnDisable();
        }

        private void PlayRecycleAudio()
        {
            _stoveOnSource?.Play();
            RefreshStoveAudioPause();
        }

        private void OnApplicationFocus(bool focused) => RefreshStoveAudioPause();

        private void OnApplicationPause(bool paused)
        {
            _applicationPaused = paused;
            RefreshStoveAudioPause();
        }

        private void RefreshStoveAudioPause()
        {
            if (_applicationPaused || !Application.isFocused)
            {
                _fireLoopSource?.Pause();
                _stoveOnSource?.Pause();
            }
            else
            {
                _fireLoopSource?.UnPause();
                _stoveOnSource?.UnPause();
            }
        }

        private void OnDestroy()
        {
            if (!_started)
                return;

            _storage.OnChanged -= RefreshEquipment;
            _storage.OnInventoryRefreshRequested -= RequestInventoryRefresh;
            _player.OnGoldChanged -= RefreshGold;
            _player.OnShardsChanged -= RefreshShards;
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
            _sideMenu.ReturnToHubButton.onClick.RemoveListener(ReturnToHub);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        private static bool IsDragging => DragAndDropManager.IsInstanceExist && DragAndDropManager.AutoCreateInstance.IsDragging;

        private void RequestInventoryRefresh()
        {
            // Rebuild after the drag manager releases its source placements/adapters.
            if (IsDragging) _inventoryRefreshPending = true;
            else ReloadUI();
        }

        private void LateUpdate()
        {
            if (_inventoryRefreshPending && !IsDragging) ReloadUI();
        }

        public RuleResult CanStartTransfer(DragContext context, IInventory targetInventory)
        {
            if (!context.Entries.Any(e => e.SourceInventory is EquipmentDragInventory)) return RuleResult.Success();
            if (context.Entries.Count != 1 || context.Entries[0].SourceInventory is not EquipmentDragInventory source ||
                !ReferenceEquals(targetInventory, _runtimeInventory) || context.TargetBaseSlot == null ||
                context.Entries[0].Stack.PrimaryAdapter is not GameItemAdapter adapter || !source.IsCurrentItem(adapter) ||
                !_storage.CanUnequipTo(source.EquipmentSlot, adapter.State.InstanceId, context.TargetBaseSlot.Index))
                return RuleResult.Failure("Equipped item needs a free inventory placement.");
            return RuleResult.Success();
        }

        public RuleResult CanCommitTransfer(TransferDomainContext context)
        {
            if (context.SourceInventory is not EquipmentDragInventory source) return RuleResult.Success();
            var hovered = DragAndDropManager.IsInstanceExist ? DragAndDropManager.AutoCreateInstance.CurrentContext?.TargetBaseSlot : null;
            if (!ReferenceEquals(context.TargetInventory, _runtimeInventory) || context.Kind != TransferKind.Move ||
                context.PlannedTargetBaseSlot == null || (hovered != null && !ReferenceEquals(hovered, context.PlannedTargetBaseSlot)) ||
                context.SourceItemAdapter is not GameItemAdapter adapter || !source.IsCurrentItem(adapter) ||
                !_storage.CanUnequipTo(source.EquipmentSlot, adapter.State.InstanceId, context.PlannedTargetBaseSlot.Index))
                return RuleResult.Failure("Equipped item no longer fits the chosen inventory cells.");
            return RuleResult.Success();
        }

        public void OnTransferSucceeded(TransferDomainContext context)
        {
            if (context.SourceInventory is EquipmentDragInventory source && context.SourceItemAdapter is GameItemAdapter adapter &&
                !_storage.UnequipTo(source.EquipmentSlot, adapter.State.InstanceId, context.TargetBaseSlot.Index))
                throw new InvalidOperationException("Validated equipment placement could not be committed.");
        }

        private void RefreshGold()
        {
            if (_goldText != null)
                _goldText.text = BigDoubleFormatter.FormatFloor(_player.GoldTotal);
        }

        private void RefreshShards()
        {
            if (_shardText != null)
                _shardText.text = BigDoubleFormatter.FormatFloor(_player.ShardTotal);
        }

        public LootBatch Grant(IReadOnlyList<LootDrop> drops)
        {
            var rewards = new List<LootReward>(drops.Count);
            foreach (var drop in drops)
            {
                var definition = drop.Definition;
                if (definition.category == ItemCategory.Currency)
                {
                    var amount = BigDoubleMath.SanitizeNonNegativeInteger(drop.CurrencyAmount, BigDouble.Zero);
                    if (amount <= 0)
                        throw new ArgumentException("Currency reward must have a positive amount.", nameof(drops));
                    switch (definition.currency)
                    {
                        case RewardCurrency.Gold: _player.GoldTotal += amount; break;
                        case RewardCurrency.Shards: _player.AddShards(amount); break;
                        default: throw new ArgumentOutOfRangeException(nameof(definition.currency));
                    }
                    rewards.Add(new LootReward(null, definition, currencyAmount: amount));
                    continue;
                }
                var state = _storage.Create(definition);
                var adapter = new GameItemAdapter(state, definition);
                ItemStack.TryCreate(new[] { adapter }, out var stack);

                bool placed;
                using (BeginSync())
                    placed = _runtimeInventory.TryAddStack(stack);

                if (placed)
                {
                    var placement = _runtimeInventory.Placements.First(candidate =>
                        candidate.Stack.Adapters.Contains(adapter));
                    _storage.Place(new[] { state }, placement.AnchorIndex, placement.Orientation);
                }
                else
                {
                    // Keep overflow in saved storage. ReloadUI places it when a cell becomes available.
                    _storage.Place(new[] { state }, -1, 0);
                }

                rewards.Add(new LootReward(state.InstanceId, definition, !placed));
            }

            _runtimeInventory.NotifyContentRefreshed();
            return new LootBatch(rewards);
        }

        public bool HasForgeSpace()
        {
            ReloadUI();
            var topology = _runtimeInventory.Topology;
            for (var i = 0; i < _runtimeInventory.SlotCount; i++)
            {
                var cell = topology.ToCell(i);
                var clear = true;
                for (var y = 0; y < 2 && clear; y++)
                    for (var x = 0; x < 2 && clear; x++)
                    {
                        clear = topology.TryToIndex(cell + new Vector2Int(x, y), out var index) && index < _runtimeInventory.SlotCount &&
                            topology.ToCell(index) == cell + new Vector2Int(x, y) && _runtimeInventory.GetPlacementAt(index) == null;
                    }
                if (clear) return true;
            }
            return false;
        }

        public bool TryStoreForgedItem(PlayerItemInstanceState state)
        {
            if (_storage.TryGet(state.InstanceId, out _)) return true;
            ReloadUI();
            var adapter = new GameItemAdapter(state, _itemCatalog.Get(state.ItemDefinitionId));
            ItemStack.TryCreate(new[] { adapter }, out var stack);
            using (BeginSync())
                if (!_runtimeInventory.TryAddStack(stack)) return false;
            var placement = _runtimeInventory.Placements.First(p => p.Stack.Adapters.Contains(adapter));
            _storage.Place(new[] { state }, placement.AnchorIndex, placement.Orientation);
            _runtimeInventory.NotifyContentRefreshed();
            return true;
        }

        public bool CanUseReward(LootReward reward) => reward.Definition != null &&
            (reward.Definition.category == ItemCategory.Consumable || reward.Definition.category == ItemCategory.Armor) &&
            string.IsNullOrEmpty(GetRewardUseUnavailableKey(reward));

        public string GetRewardUseUnavailableKey(LootReward reward)
        {
            _storage.ConfigureInventoryLayout(_runtimeInventory.Topology);
            if (reward.Definition != null && reward.Definition.category is ItemCategory.Currency or ItemCategory.Scroll)
                return string.Empty;
            if (reward.Definition == null || _usedRewardIds.Contains(reward.InstanceId) ||
                !_storage.Items.Any(item => item.InstanceId == reward.InstanceId &&
                                            item.ItemDefinitionId == reward.Definition.itemId))
                return ItemText.UseResultKey(ConsumableUseResult.MissingItem);

            if (reward.Definition.category == ItemCategory.Consumable)
                return ItemText.UseResultKey(_consumables.CanUse(reward.InstanceId));
            return _storage.CanEquip(reward.Definition.equipmentSlot, reward.InstanceId)
                ? string.Empty : "equipment.no_space";
        }

        public bool TryUseReward(LootReward reward)
        {
            if (!CanUseReward(reward))
                return false;

            if (reward.Definition.category == ItemCategory.Consumable)
            {
                if (!_consumables.TryUse(reward.InstanceId))
                    return false;
            }
            else if (!_storage.Equip(reward.Definition.equipmentSlot, reward.InstanceId))
                return false;
            _usedRewardIds.Add(reward.InstanceId);
            return true;
        }

        private void ShowConsumableFeedback(ConsumableUseResult result)
        {
            ShowFeedback(result == ConsumableUseResult.Success ? "potion.use.prepared" : ItemText.UseResultKey(result));
        }

        private void ShowInventoryFullFeedback() => ShowFeedback("equipment.no_space");

        private void ShowFeedback(string key)
        {
            _feedbackKey = key;
            RefreshConsumableFeedback();
            _consumableFeedbackRoot?.SetActive(true);
            CancelInvoke(nameof(ClearConsumableFeedback));
            Invoke(nameof(ClearConsumableFeedback), 5f);
        }

        private void ClearConsumableFeedback()
        {
            CancelInvoke(nameof(ClearConsumableFeedback));
            _feedbackKey = null;
            _consumableFeedbackRoot?.SetActive(false);
        }

        private void RefreshConsumableFeedback()
        {
            if (_consumableFeedback != null)
                _consumableFeedback.text = string.IsNullOrEmpty(_feedbackKey) ? string.Empty : ItemText.Get(_feedbackKey);
        }

        private void HandleLocaleChanged(Locale locale)
        {
            RefreshConsumableFeedback();
            if (_started && isActiveAndEnabled)
                ReloadUI();
        }

        protected override IEnumerable<PlacementData<PlayerItemInstanceState>> GetPlacements()
        {
            var visited = new HashSet<string>();
            // Restore occupied cells first, then try saved overflow in the remaining space.
            foreach (var item in _storage.InventoryItems.OrderBy(item => item.AnchorIndex < 0))
            {
                if (!visited.Add(item.InstanceId))
                    continue;

                if (!_itemCatalog.TryGet(item.ItemDefinitionId, out var definition)) continue;
                if (item.AnchorIndex < 0 || !definition.IsStackable)
                {
                    yield return new PlacementData<PlayerItemInstanceState>(
                        new[] { item }, item.AnchorIndex, item.Orientation);
                    continue;
                }

                var stack = new List<PlayerItemInstanceState>();
                foreach (var candidate in _storage.Items)
                {
                    if (candidate.AnchorIndex == item.AnchorIndex &&
                        candidate.Orientation == item.Orientation &&
                        candidate.ItemDefinitionId == item.ItemDefinitionId)
                    {
                        stack.Add(candidate);
                        visited.Add(candidate.InstanceId);
                    }
                }

                for (var start = 0; start < stack.Count; start += definition.maxStackSize)
                {
                    var count = Mathf.Min(definition.maxStackSize, stack.Count - start);
                    var stackPart = stack.GetRange(start, count);
                    yield return new PlacementData<PlayerItemInstanceState>(
                        stackPart,
                        start == 0 ? item.AnchorIndex : -1,
                        item.Orientation);
                }
            }
        }

        protected override GameItemAdapter CreateAdapter(PlayerItemInstanceState item)
        {
            return new GameItemAdapter(item, _itemCatalog.Get(item.ItemDefinitionId));
        }

        protected override PlayerItemInstanceState ExtractData(GameItemAdapter adapter)
        {
            return adapter.State;
        }

        protected override void AddPlacementData(
            PlacementCommitContext<PlayerItemInstanceState, GameItemAdapter> context)
        {
            _storage.Place(context.Data, context.AnchorIndex, context.Orientation);
        }

        protected override void RemovePlacementData(
            PlacementCommitContext<PlayerItemInstanceState, GameItemAdapter> context)
        {
            if (ReferenceEquals(context.EventContext.TargetInventory, _runtimeInventory))
                _storage.Detach(context.Data);
            else
                _storage.Remove(context.Data);
        }

        protected override void OnReloadUI()
        {
            _inventoryRefreshPending = false;
            base.OnReloadUI();

            var placements = new List<(PlayerItemInstanceState Item, int Anchor, int Orientation)>();
            foreach (var placement in _runtimeInventory.Placements)
            {
                foreach (var adapter in placement.Stack.Adapters)
                {
                    var gameItem = (GameItemAdapter)adapter;
                    placements.Add((gameItem.State, placement.AnchorIndex, placement.Orientation));
                }
            }

            _storage.SynchronizePlacements(placements);
        }

        protected override void OnDropCompletedFrom(DragContext context)
        {
            base.OnDropCompletedFrom(context);
            if (_inventoryRefreshPending || _storage.InventoryItems.Any(item => item.AnchorIndex < 0))
                RequestInventoryRefresh();
        }

        private EquipmentDropArea CreateEquipmentDropArea(Image image, EquipmentSlot slot)
        {
            var dropArea = image.gameObject.AddComponent<EquipmentDropArea>();
            dropArea.Configure(_storage, slot, image, ShowInventoryFullFeedback);
            dropArea.EnableDragging(_itemCatalog, this);
            return dropArea;
        }

        private void ReturnToHub()
        {
            _stateMachine.Enter<HubState>();
        }

        private void RefreshEquipment()
        {
            RefreshEquipmentSlot(_helmetDropArea, EquipmentSlot.Helmet);
            RefreshEquipmentSlot(_chestDropArea, EquipmentSlot.Chest);
            RefreshEquipmentSlot(_weaponDropArea, EquipmentSlot.Weapon);
        }

        private void RefreshEquipmentSlot(EquipmentDropArea dropArea, EquipmentSlot slot)
        {
            if (dropArea == null) return;
            var equippedId = _storage.GetEquipped(slot);
            var icon = _storage.TryGet(equippedId, out var item) && _itemCatalog.TryGet(item.ItemDefinitionId, out var definition)
                ? definition.GetIcon(item) : null;
            dropArea.SetEquippedIcon(icon);
        }
    }
}
