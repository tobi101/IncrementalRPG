using System;
using System.Collections.Generic;
using System.Linq;
using Core.Items;
using Core.StateMachine;
using Core.StateMachine.States;
using Model;
using Reflex.Attributes;
using UDND.Core;
using UDND.DataBinding;
using UDND.Interaction;
using UDND.Inventories;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    [DisallowMultipleComponent]
    public sealed class PlayerInventoryView :
        PlacementInventoryDataBinding<PlayerItemInstanceState, GameItemAdapter>,
        IPlayerInventoryGateway
    {
        [Header("Grid")]
        [SerializeField] private UniversalInventory _runtimeInventory;
        [SerializeField] private RectTransform _slotContainer;

        [Header("Drop Areas")]
        [SerializeField] private Graphic _recycleDropPanel;
        [SerializeField] private Image _helmetSlot;
        [SerializeField] private Image _chestSlot;
        [SerializeField] private Image _weaponSlot;
        [SerializeField] private Image _bootsSlot;

        [Header("Menu")]
        [SerializeField] private Button _menuToggleButton;
        [SerializeField] private SideMenuFlyoutView _sideMenuTemplate;

        [Header("Universal Drag And Drop")]
        [SerializeField] private GameObject _dragCanvasPrefab;
        [SerializeField] private GameObject _tooltipCanvasPrefab;

        [Inject] private PlayerItemStorage _storage;
        [Inject] private ItemCatalog _itemCatalog;
        [Inject] private Player _player;
        [Inject] private RunConsumableService _consumables;
        [Inject] private GameStateMachine _stateMachine;
        [Inject] private PauseMenuController _pauseMenuController;

        private readonly List<(InventoryGridSlot Slot, InventoryItemUseInput Input)> _itemUseInputs = new();
        private EquipmentDropArea _helmetDropArea;
        private EquipmentDropArea _chestDropArea;
        private EquipmentDropArea _weaponDropArea;
        private EquipmentDropArea _bootsDropArea;
        private SideMenuFlyoutView _sideMenu;
        private bool _started;

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

            _sideMenu = Instantiate(_sideMenuTemplate, transform);
            _sideMenu.name = "InventorySideMenuFlyout";
            _sideMenu.SetToggleButton(_menuToggleButton);
            _sideMenu.ReturnToHubButton.onClick.AddListener(ReturnToHub);
            _pauseMenuController.RegisterSideMenu(_sideMenu);

            foreach (var itemUseInput in _itemUseInputs)
                itemUseInput.Input.Configure(itemUseInput.Slot, _consumables);

            var recycleArea = _recycleDropPanel.gameObject.AddComponent<InventoryRecycleDropArea>();
            recycleArea.Configure(_player, _recycleDropPanel);

            _helmetDropArea = CreateEquipmentDropArea(_helmetSlot, EquipmentSlot.Helmet);
            _chestDropArea = CreateEquipmentDropArea(_chestSlot, EquipmentSlot.Chest);
            _weaponDropArea = CreateEquipmentDropArea(_weaponSlot, EquipmentSlot.Weapon);
            _bootsDropArea = CreateEquipmentDropArea(_bootsSlot, EquipmentSlot.Boots);

            _storage.OnChanged += RefreshEquipment;
            _storage.OnInventoryRefreshRequested += ReloadUI;

            _started = true;
            base.OnEnable();
            RefreshEquipment();
        }

        protected override void OnEnable()
        {
            if (_started)
                base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (_started)
                base.OnDisable();
        }

        private void OnDestroy()
        {
            if (!_started)
                return;

            _storage.OnChanged -= RefreshEquipment;
            _storage.OnInventoryRefreshRequested -= ReloadUI;
            _sideMenu.ReturnToHubButton.onClick.RemoveListener(ReturnToHub);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public LootBatch Grant(IReadOnlyList<ItemDefinition> definitions)
        {
            var rewards = new List<LootReward>(definitions.Count);
            foreach (var definition in definitions)
            {
                var state = _storage.Create(definition);
                var adapter = new GameItemAdapter(state, definition);
                ItemStack.TryCreate(new[] { adapter }, out var stack);

                if (!_runtimeInventory.TryAddStack(stack))
                    continue;

                var placement = _runtimeInventory.Placements.First(candidate =>
                    candidate.Stack.Adapters.Contains(adapter));
                _storage.Place(new[] { state }, placement.AnchorIndex, placement.Orientation);
                rewards.Add(new LootReward(state.InstanceId, definition));
            }

            return new LootBatch(rewards);
        }

        protected override IEnumerable<PlacementData<PlayerItemInstanceState>> GetPlacements()
        {
            var visited = new HashSet<string>();
            foreach (var item in _storage.Items)
            {
                if (!visited.Add(item.InstanceId))
                    continue;

                var definition = _itemCatalog.Get(item.ItemDefinitionId);
                if (item.AnchorIndex < 0 || !definition.stackable)
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

        private EquipmentDropArea CreateEquipmentDropArea(Image image, EquipmentSlot slot)
        {
            var dropArea = image.gameObject.AddComponent<EquipmentDropArea>();
            dropArea.Configure(_storage, slot, image);
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
            RefreshEquipmentSlot(_bootsDropArea, EquipmentSlot.Boots);
        }

        private void RefreshEquipmentSlot(EquipmentDropArea dropArea, EquipmentSlot slot)
        {
            var equippedId = _storage.GetEquipped(slot);
            var icon = string.IsNullOrEmpty(equippedId)
                ? null
                : _itemCatalog.Get(_storage.Get(equippedId).ItemDefinitionId).icon;
            dropArea.SetEquippedIcon(icon);
        }
    }
}
