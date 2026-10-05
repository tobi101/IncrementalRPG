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
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
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
        public LocalizeStringEvent title;
        public LocalizeStringEvent description;
    }
    [Serializable]
    public sealed class ClassTabButton
    {
        public Button button;
        public Image background;
        public LocalizeStringEvent title;
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
        public LocalizeStringEvent classTitle;
        public LocalizeStringEvent classDescription;
        public TMP_Text goldText;
        public LocalizeStringEvent refundText;
        public Button resetButton;
        public LocalizeStringEvent resetHint;
        [Header("Skill details")]
        public ClassMedallionView detailIcon;
        public Image detailPattern;
        public Image detailPreview;
        public LocalizeStringEvent detailTitle;
        public LocalizeStringEvent detailDescription;
        public LocalizeStringEvent detailLevel;
        public LocalizeStringEvent detailRequirements;
        public LocalizeStringEvent purchaseText;
        public Button purchaseButton;
        [Header("Locked class")]
        public GameObject lockedRoot;
        public LocalizeStringEvent lockedTitle;
        public LocalizeStringEvent keyCount;
        public LocalizeStringEvent unlockText;
        public Button unlockButton;
        public ClassLockedBackdrop lockedBackdrop;
        [Header("Loadout")]
        public ClassAttackColumn[] attackColumns;
        public ClassAttackItemView attackPrefab;
        public RectTransform tooltip;
        public LocalizeStringEvent tooltipTitle;
        public LocalizeStringEvent tooltipDescription;
        public Image tooltipEmblem;
        public Image tooltipType;
        public Image tooltipHeader;
        public Sprite[] typeIcons;
        [Header("Confirmation")]
        public GameObject confirmation;
        public LocalizeStringEvent confirmationText;
        public Button confirmButton;
        public Button cancelButton;
        public CanvasGroup choiceReveal;
        public ClassMedallionView revealEmblem;
        public LocalizeStringEvent revealTitle;

        private ClassCatalog _catalog;
        private ClassProgressionService _service;
        private Player _player;
        private GameStateMachine _machine;
        private PauseMenuController _pause;
        private ClassDefinition _selectedClass;
        private ClassNodeDefinition _selectedNode;
        private readonly List<ClassNodeView> _nodes = new();
        private readonly List<ClassAttackItemView> _attacks = new();
        private RequirementText[] _requirements = Array.Empty<RequirementText>();
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
                choices[i].title.StringReference = definition.displayName;
                choices[i].description.StringReference = definition.description;
                classTabs[i].title.StringReference = definition.displayName;
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
            ReleaseRequirements();
            if (!_initialized) return;
            sideMenu.ReturnToHubButton.onClick.RemoveListener(ReturnToHub);
            _service.OnChanged -= RequestRefresh;
            _player.OnGoldChanged -= RequestRefresh;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
        private void OnLocaleChanged(Locale locale) => HideTooltip();
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
            revealTitle.StringReference = definition.displayName;
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
            var first = _service.FirstChoicePending || _choosing;
            choiceRoot.SetActive(first);
            mainRoot.SetActive(!first);
            sideMenu.ReturnToHubButton.interactable = !first;
            for (var i = 0; i < _catalog.classes.Length; i++)
            {
                var definition = _catalog.classes[i];
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
            BindLocalizedText(classTitle, definition.displayName);
            BindLocalizedText(classDescription, definition.description);
            var unlocked = _service.IsUnlocked(definition);
            // Shrine remains usable before the introduction; only purchasing and choosing are gated.
            lockedRoot.SetActive(!unlocked);
            var lockedText = FormattedString(_service.HasChosenFirstClass ? "classes.locked" : "classes.before_introduction");
            if (_service.HasChosenFirstClass) lockedText["className"] = definition.displayName;
            lockedTitle.StringReference = lockedText;
            SetLocalizedText(keyCount, "classes.keys", _service.RuneKeys);
            keyCount.transform.parent.gameObject.SetActive(_service.HasChosenFirstClass);
            unlockButton.gameObject.SetActive(_service.HasChosenFirstClass);
            SetLocalizedText(unlockText, "classes.unlock", Math.Max(1, definition.unlockKeyCost));
            unlockButton.interactable = _service.HasChosenFirstClass && _service.RuneKeys >= Math.Max(1, definition.unlockKeyCost);
            resetButton.interactable = _service.CanReset(definition);
            if (unlocked && !resetButton.interactable) SetLocalizedText(resetHint, "classes.last_class");
            else ClearLocalizedText(resetHint);
            SetLocalizedText(refundText, "classes.refund", BigDoubleFormatter.FormatFloor(_service.Refund(definition)));
            foreach (var view in _nodes) view.Refresh(definition, _service, view.Definition == _selectedNode);
            lockedBackdrop.SetLocked(!unlocked);
            purchaseButton.interactable = false;
            ReleaseRequirements();
            if (_selectedNode == null)
            {
                ClearLocalizedText(detailTitle, detailDescription, detailLevel, detailRequirements, purchaseText);
                detailIcon.gameObject.SetActive(false);
                detailPreview.enabled = false;
                return;
            }
            detailIcon.gameObject.SetActive(true);
            detailPreview.enabled = true;
            var node = _selectedNode;
            BindLocalizedText(detailTitle, node.displayName);
            BindLocalizedText(detailDescription, node.description);
            detailIcon.Bind(definition);
            detailPreview.sprite = node.preview != null ? node.preview : node.icon;
            SetLocalizedText(detailLevel, "classes.level", _service.Level(definition, node), node.LevelLimit);
            _requirements = (node.requirements ?? Array.Empty<ClassNodeRequirement>())
                .Where(r => r != null && r.node != null && _service.Level(definition, r.node) < r.level)
                .Select(r => new RequirementText(r)).ToArray();
            if (_requirements.Length > 0) SetLocalizedText(detailRequirements, "classes.requirements", (object)_requirements);
            else ClearLocalizedText(detailRequirements);
            var status = _service.PurchaseStatus(definition, node);
            purchaseButton.interactable = status == ClassPurchaseStatus.Available;
            node.TryGetCost(_service.Level(definition, node), out var cost);
            var purchaseKey = status switch
            {
                ClassPurchaseStatus.Complete => "classes.max",
                ClassPurchaseStatus.NotConfigured => "classes.not_configured",
                ClassPurchaseStatus.ClassLocked => "classes.class_locked",
                ClassPurchaseStatus.RequirementsMissing => "classes.requirements_missing",
                _ => "classes.buy"
            };
            SetLocalizedText(purchaseText, purchaseKey, BigDoubleFormatter.FormatFloor(cost));
            if (status == ClassPurchaseStatus.NotEnoughGold) SetLocalizedText(detailRequirements, "classes.not_enough_gold");
        }

        private static LocalizedString FormattedString(string key, params object[] arguments) =>
            new(ItemText.Table, key) { Arguments = arguments };

        private static void SetLocalizedText(LocalizeStringEvent label, string key, params object[] arguments) =>
            label.StringReference = FormattedString(key, arguments);

        private static void BindLocalizedText(LocalizeStringEvent label, LocalizedString reference)
        {
            if (!ReferenceEquals(label.StringReference, reference)) label.StringReference = reference;
        }

        private static void ClearLocalizedText(params LocalizeStringEvent[] labels)
        {
            foreach (var label in labels)
            {
                label.StringReference = new LocalizedString();
                label.OnUpdateString.Invoke(string.Empty);
            }
        }

        private void ReleaseRequirements()
        {
            foreach (var requirement in _requirements) requirement.Dispose();
            _requirements = Array.Empty<RequirementText>();
        }

        // Smart Strings resolve each prerequisite in the list using its own localized name and level.
        private sealed class RequirementText : IVariableGroup, IDisposable
        {
            private readonly LocalizedString _text;
            public RequirementText(ClassNodeRequirement requirement)
            {
                _text = FormattedString("classes.requirement");
                _text["skill"] = requirement.node.displayName;
                _text["level"] = new IntVariable { Value = requirement.level };
            }
            public bool TryGetValue(string key, out IVariable value)
            {
                value = key == "text" ? _text : null;
                return value != null;
            }
            public void Dispose() => ((IDisposable)_text).Dispose();
        }

        private void RequestReset()
        {
            if (!_service.CanReset(_selectedClass)) return;
            _resetTarget = _selectedClass;
            var text = FormattedString("classes.reset_confirm");
            text["className"] = _selectedClass.displayName;
            text["refund"] = new StringVariable { Value = BigDoubleFormatter.FormatFloor(_service.Refund(_selectedClass)) };
            confirmationText.StringReference = text;
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
                ((RectTransform)item.title.transform).anchoredPosition = new Vector2(0, -85);
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
            tooltipTitle.StringReference = attack.displayName;
            tooltipDescription.StringReference = attack.description;
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
