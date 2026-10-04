using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Classes;
using Core.Items;
using Core.StateMachine;
using Core.StateMachine.States;
using Model;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utils;

namespace UI.Classes
{
    [Serializable]
    public sealed class ClassChoiceCard
    {
        public Button button;
        public TMP_Text title;
        public TMP_Text description;
    }
    [Serializable]
    public sealed class ClassTabButton
    {
        public Button button;
        public Image background;
        public TMP_Text title;
    }
    [Serializable]
    public sealed class ClassAttackColumn
    {
        public RectTransform equipped;
        public GameObject emptySlot;
        public RectTransform content;
        public GameObject emptyList;
        public GameObject[] emptyCells;
    }

    public sealed class ClassesMenuView : MonoBehaviour
    {
        [Header("Navigation")]
        public GameObject choiceRoot;
        public GameObject mainRoot;
        public GameObject classesRoot;
        public GameObject attacksRoot;
        public Button classesTab;
        public Button attacksTab;
        [FormerlySerializedAs("sideMenuTemplate")]
        public SideMenuFlyoutView sideMenu;
        public ClassChoiceCard[] choices;
        public ClassTabButton[] classTabs;
        [Header("Class board")]
        public RectTransform nodesRoot;
        public ClassNodeView nodePrefab;
        public Image boardSigil;
        public ClassMedallionView classEmblem;
        public Image classPattern;
        public Image classNameplate;
        public TMP_Text classTitle;
        public TMP_Text classDescription;
        public TMP_Text goldText;
        public TMP_Text refundText;
        public Button resetButton;
        public TMP_Text resetHint;
        [Header("Skill details")]
        public ClassMedallionView detailIcon;
        public Image detailPattern;
        public Image detailPreview;
        public TMP_Text detailTitle;
        public TMP_Text detailDescription;
        public TMP_Text detailLevel;
        public TMP_Text detailRequirements;
        public TMP_Text purchaseText;
        public Button purchaseButton;
        [Header("Locked class")]
        public GameObject lockedRoot;
        public TMP_Text lockedTitle;
        public TMP_Text keyCount;
        public TMP_Text unlockText;
        public Button unlockButton;
        public ClassLockedBackdrop lockedBackdrop;
        [Header("Loadout")]
        public ClassAttackColumn[] attackColumns;
        public ClassAttackItemView attackPrefab;
        public RectTransform tooltip;
        public TMP_Text tooltipTitle;
        public TMP_Text tooltipDescription;
        public Image tooltipEmblem;
        public Image tooltipType;
        public Image tooltipHeader;
        public Sprite[] typeIcons;
        [Header("Confirmation")]
        public GameObject confirmation;
        public TMP_Text confirmationText;
        public Button confirmButton;
        public Button cancelButton;
        public CanvasGroup choiceReveal;
        public ClassMedallionView revealEmblem;
        public TMP_Text revealTitle;

        private ClassCatalog _catalog;
        private ClassProgressionService _service;
        private Player _player;
        private GameStateMachine _machine;
        private PauseMenuController _pause;
        private ClassDefinition _selectedClass;
        private ClassNodeDefinition _selectedNode;
        private readonly List<ClassNodeView> _nodes = new();
        private readonly List<ClassAttackItemView> _attacks = new();
        private bool _initialized;
        private bool _refreshPending;
        private bool _showAttacks;
        private bool _choosing;
        private RectTransform _dragGhost;
        private ClassDefinition _resetTarget;

        [Inject]
        public void Construct(ClassCatalog catalog, ClassProgressionService service, Player player,
            GameStateMachine machine, PauseMenuController pause)
        {
            _catalog = catalog; _service = service; _player = player; _machine = machine; _pause = pause;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Initialize();
            if (_selectedClass == null) SelectClass(_catalog.classes.FirstOrDefault(_service.IsUnlocked) ?? _catalog.classes.First());
            Refresh();
        }
        public void Hide()
        {
            StopAllCoroutines(); _choosing = false;
            if (choiceReveal != null) choiceReveal.gameObject.SetActive(false);
            if (confirmation != null) confirmation.SetActive(false);
            HideTooltip(); EndAttackDrag();
            sideMenu?.CloseImmediate();
            gameObject.SetActive(false);
        }

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            sideMenu.ReturnToHubButton.onClick.AddListener(ReturnToHub);
            sideMenu.CloseImmediate();
            // Keep modal overlays above the flyout and ordinary content.
            confirmation.transform.SetAsLastSibling();
            choiceReveal.transform.SetAsLastSibling();
            tooltip.SetAsLastSibling();
            for (var i = 0; i < _catalog.classes.Length; i++)
            {
                var definition = _catalog.classes[i];
                choices[i].button.onClick.AddListener(() => ChooseFirst(definition));
                classTabs[i].button.onClick.AddListener(() => SelectClass(definition));
            }
            classesTab.onClick.AddListener(() => SwitchTab(false));
            attacksTab.onClick.AddListener(() => SwitchTab(true));
            purchaseButton.onClick.AddListener(() => { if (_selectedNode != null) _service.Purchase(_selectedClass, _selectedNode); });
            unlockButton.onClick.AddListener(() => _service.Unlock(_selectedClass));
            resetButton.onClick.AddListener(RequestReset);
            cancelButton.onClick.AddListener(() => confirmation.SetActive(false));
            confirmButton.onClick.AddListener(() => { _service.Reset(_resetTarget); confirmation.SetActive(false); });
            _service.OnChanged += RequestRefresh;
            _player.OnGoldChanged += RequestRefresh;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            UIButtonAudio.InstallInChildren(this, true);
        }

        private void OnDestroy()
        {
            if (!_initialized) return;
            sideMenu.ReturnToHubButton.onClick.RemoveListener(ReturnToHub);
            _service.OnChanged -= RequestRefresh;
            _player.OnGoldChanged -= RequestRefresh;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
        private void OnLocaleChanged(Locale locale) { HideTooltip(); RequestRefresh(); }
        private void RequestRefresh() => _refreshPending = true;
        private void LateUpdate()
        {
            if (_refreshPending && !_choosing) { _refreshPending = false; Refresh(); }
        }
        private void Update()
        {
            if (!_initialized || _pause.IsOpen || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame || _choosing) return;
            if (confirmation.activeSelf)
            {
                sideMenu.SuppressEscapeThisFrame();
                confirmation.SetActive(false);
            }
            else if (sideMenu.IsOpen) sideMenu.Close();
            else ReturnToHub();
        }
        private void ReturnToHub()
        {
            if (!_service.FirstChoicePending && !_choosing) _machine.Enter<HubState>();
        }
        private void ChooseFirst(ClassDefinition definition)
        {
            if (_choosing || !_service.ChooseFirst(definition)) return;
            _choosing = true;
            SelectClass(definition);
            StartCoroutine(RevealChoice(definition));
        }
        private IEnumerator RevealChoice(ClassDefinition definition)
        {
            choiceReveal.gameObject.SetActive(true);
            revealEmblem.Bind(definition);
            revealTitle.text = definition.displayName.GetLocalizedString();
            for (var elapsed = 0f; elapsed < 0.7f; elapsed += Time.unscaledDeltaTime)
            {
                choiceReveal.alpha = Mathf.Clamp01(elapsed / 0.15f);
                revealEmblem.transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1, Mathf.SmoothStep(0, 1, elapsed / 0.5f));
                yield return null;
            }
            _choosing = false;
            choiceReveal.gameObject.SetActive(false);
            Refresh();
        }
        private void SwitchTab(bool attacks)
        {
            _showAttacks = attacks; HideTooltip(); EndAttackDrag(); Refresh();
        }
        private void SelectClass(ClassDefinition definition)
        {
            _selectedClass = definition;
            _selectedNode = definition.nodes.FirstOrDefault(n => n != null);
            foreach (var node in _nodes) { node.gameObject.SetActive(false); Destroy(node.gameObject); }
            _nodes.Clear();
            foreach (var node in definition.nodes.Where(n => n != null))
            {
                var view = Instantiate(nodePrefab, nodesRoot);
                view.gameObject.SetActive(true);
                ((RectTransform)view.transform).anchoredPosition = node.position;
                view.transform.localScale = Vector3.one * Mathf.Clamp(node.visualScale, 0.5f, 1.2f);
                view.Bind(node, selected => { _selectedNode = selected; RefreshBoard(); });
                _nodes.Add(view);
            }
            if (_initialized) Refresh();
        }

        private void Refresh()
        {
            if (!_initialized) return;
            foreach (var label in GetComponentsInChildren<ClassLocalizedLabel>(true)) label.Refresh();
            var first = _service.FirstChoicePending || _choosing;
            choiceRoot.SetActive(first);
            mainRoot.SetActive(!first);
            sideMenu.ReturnToHubButton.interactable = !first;
            for (var i = 0; i < _catalog.classes.Length; i++)
            {
                var definition = _catalog.classes[i];
                choices[i].title.text = definition.displayName.GetLocalizedString();
                choices[i].description.text = definition.description.GetLocalizedString();
                classTabs[i].title.text = definition.displayName.GetLocalizedString();
                classTabs[i].background.color = definition == _selectedClass ? Color.white : new Color(0.58f, 0.58f, 0.58f);
            }
            classesRoot.SetActive(!_showAttacks);
            attacksRoot.SetActive(_showAttacks);
            classesTab.image.color = !_showAttacks ? Color.white : new Color(0.55f, 0.55f, 0.55f);
            attacksTab.image.color = _showAttacks ? Color.white : new Color(0.55f, 0.55f, 0.55f);
            goldText.text = BigDoubleFormatter.FormatFloor(_player.GoldTotal);
            if (_selectedClass != null) RefreshBoard();
            if (_showAttacks && !first) RefreshAttacks();
        }

        private void RefreshBoard()
        {
            var definition = _selectedClass;
            classEmblem.Bind(definition);
            classNameplate.color = Color.Lerp(Color.white, definition.color, 0.2f);
            classPattern.sprite = definition.pattern;
            detailPattern.sprite = definition.pattern;
            boardSigil.sprite = definition.sigil;
            classTitle.text = definition.displayName.GetLocalizedString();
            classDescription.text = definition.description.GetLocalizedString();
            var unlocked = _service.IsUnlocked(definition);
            // Shrine remains usable before the introduction; only purchasing and choosing are gated.
            lockedRoot.SetActive(!unlocked);
            lockedTitle.text = _service.HasChosenFirstClass
                ? ItemText.Get("classes.locked", definition.displayName.GetLocalizedString())
                : ItemText.Get("classes.before_introduction");
            keyCount.text = ItemText.Get("classes.keys", _service.RuneKeys);
            keyCount.transform.parent.gameObject.SetActive(_service.HasChosenFirstClass);
            unlockButton.gameObject.SetActive(_service.HasChosenFirstClass);
            unlockText.text = ItemText.Get("classes.unlock", Math.Max(1, definition.unlockKeyCost));
            unlockButton.interactable = _service.HasChosenFirstClass && _service.RuneKeys >= Math.Max(1, definition.unlockKeyCost);
            resetButton.interactable = _service.CanReset(definition);
            resetHint.text = unlocked && !resetButton.interactable ? ItemText.Get("classes.last_class") : "";
            refundText.text = ItemText.Get("classes.refund", BigDoubleFormatter.FormatFloor(_service.Refund(definition)));
            foreach (var view in _nodes) view.Refresh(definition, _service, view.Definition == _selectedNode);
            lockedBackdrop.SetLocked(!unlocked);
            purchaseButton.interactable = false;
            if (_selectedNode == null)
            {
                detailTitle.text = detailDescription.text = detailLevel.text = detailRequirements.text = purchaseText.text = "";
                detailIcon.gameObject.SetActive(false);
                detailPreview.enabled = false;
                return;
            }
            detailIcon.gameObject.SetActive(true);
            detailPreview.enabled = true;
            var node = _selectedNode;
            detailTitle.text = node.displayName.GetLocalizedString();
            detailDescription.text = node.description.GetLocalizedString();
            detailIcon.Bind(definition);
            detailPreview.sprite = node.preview != null ? node.preview : node.icon;
            detailLevel.text = ItemText.Get("classes.level", _service.Level(definition, node), node.LevelLimit);
            var missing = (node.requirements ?? Array.Empty<ClassNodeRequirement>())
                .Where(r => r != null && r.node != null && _service.Level(definition, r.node) < r.level)
                .Select(r => ItemText.Get("classes.requirement", r.node.displayName.GetLocalizedString(), r.level));
            detailRequirements.text = string.Join("\n", missing);
            var status = _service.PurchaseStatus(definition, node);
            purchaseButton.interactable = status == ClassPurchaseStatus.Available;
            node.TryGetCost(_service.Level(definition, node), out var cost);
            purchaseText.text = status switch
            {
                ClassPurchaseStatus.Complete => ItemText.Get("classes.max"),
                ClassPurchaseStatus.NotConfigured => ItemText.Get("classes.not_configured"),
                ClassPurchaseStatus.ClassLocked => ItemText.Get("classes.class_locked"),
                ClassPurchaseStatus.RequirementsMissing => ItemText.Get("classes.requirements_missing"),
                _ => ItemText.Get("classes.buy", BigDoubleFormatter.FormatFloor(cost))
            };
            if (status == ClassPurchaseStatus.NotEnoughGold) detailRequirements.text = ItemText.Get("classes.not_enough_gold");
        }

        private void RequestReset()
        {
            if (!_service.CanReset(_selectedClass)) return;
            _resetTarget = _selectedClass;
            confirmationText.text = ItemText.Get("classes.reset_confirm", _selectedClass.displayName.GetLocalizedString(),
                BigDoubleFormatter.FormatFloor(_service.Refund(_selectedClass)));
            confirmation.SetActive(true);
            HideTooltip();
        }

        private void RefreshAttacks()
        {
            HideTooltip();
            foreach (var item in _attacks) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
            _attacks.Clear();
            foreach (AttackSlot slot in Enum.GetValues(typeof(AttackSlot)))
            {
                var column = attackColumns[(int)slot];
                var equipped = _service.Equipped(slot);
                column.emptySlot.SetActive(equipped == null);
                if (equipped != null) AddAttack(equipped, column.equipped, true);
                var available = _service.AvailableAttacks(slot).Where(a => a != equipped).ToArray();
                column.emptyList.SetActive(available.Length == 0);
                for (var i = 0; i < column.emptyCells.Length; i++)
                    column.emptyCells[i].SetActive(i >= available.Length);
                for (var i = 0; i < available.Length; i++)
                {
                    var item = AddAttack(available[i], column.content, false);
                    item.anchorMin = item.anchorMax = new Vector2(0.5f, 1);
                    item.pivot = new Vector2(0.5f, 1);
                    item.anchoredPosition = new Vector2(0, -i * 126 - 8);
                }
                column.content.sizeDelta = new Vector2(0, Mathf.Max(5, available.Length) * 126 + 16);
            }
        }
        private RectTransform AddAttack(AttackDefinition attack, Transform parent, bool equipped)
        {
            var item = Instantiate(attackPrefab, parent);
            item.gameObject.SetActive(true);
            var owner = _catalog.OwnerOf(attack);
            item.Bind(this, attack, equipped, owner != null ? owner.color : new Color(0.65f, 0.65f, 0.65f));
            if (equipped)
            {
                item.transform.Find("Frame").gameObject.SetActive(false);
                item.icon.rectTransform.sizeDelta = new Vector2(80, 80);
                item.title.rectTransform.anchoredPosition = new Vector2(0, -85);
            }
            _attacks.Add(item);
            var rect = (RectTransform)item.transform;
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }
        public void SetAttack(AttackSlot slot, AttackDefinition attack)
        {
            _service.Equip(slot, attack);
            IncrementalRPG.Scripts.AudioManager.AudioManager.Resolve()?.PlayUiClick();
        }
        public void ShowTooltip(AttackDefinition attack, RectTransform target)
        {
            if (attack == null) return;
            var owner = _catalog.OwnerOf(attack);
            tooltipTitle.text = attack.displayName.GetLocalizedString();
            tooltipDescription.text = attack.description.GetLocalizedString();
            tooltipEmblem.sprite = owner != null ? owner.emblem : typeIcons[0];
            if (tooltipEmblem is ClassEmblemImage emblem) emblem.sampleClassBanner = owner != null;
            tooltipType.sprite = typeIcons[(int)attack.slot];
            var headerColor = owner != null ? owner.color * 0.3f : new Color(0.18f, 0.18f, 0.18f);
            headerColor.a = 1;
            tooltipHeader.color = headerColor;
            tooltip.gameObject.SetActive(true);
            var root = (RectTransform)tooltip.parent;
            var point = root.InverseTransformPoint(target.position);
            var size = tooltip.rect.size;
            tooltip.anchoredPosition = new Vector2(Mathf.Clamp(point.x + 280, root.rect.xMin + size.x / 2 + 20, root.rect.xMax - size.x / 2 - 20),
                Mathf.Clamp(point.y, root.rect.yMin + size.y / 2 + 20, root.rect.yMax - size.y / 2 - 20));
        }
        public void HideTooltip() { if (tooltip != null) tooltip.gameObject.SetActive(false); }
        public void BeginAttackDrag(AttackDefinition attack, PointerEventData data)
        {
            HideTooltip(); EndAttackDrag();
            var ghost = new GameObject("AttackDrag", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            _dragGhost = (RectTransform)ghost.transform;
            _dragGhost.SetParent(tooltip.parent, false);
            _dragGhost.sizeDelta = new Vector2(110, 110);
            ghost.GetComponent<CanvasGroup>().blocksRaycasts = false;
            var graphic = ghost.GetComponent<Image>(); graphic.sprite = attack.icon; graphic.preserveAspect = true; graphic.raycastTarget = false;
            MoveAttackDrag(data);
        }
        public void MoveAttackDrag(PointerEventData data)
        {
            if (_dragGhost != null && RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_dragGhost.parent, data.position,
                    data.pressEventCamera, out var point)) _dragGhost.anchoredPosition = point;
        }
        public void EndAttackDrag() { if (_dragGhost != null) Destroy(_dragGhost.gameObject); _dragGhost = null; }
    }
}
