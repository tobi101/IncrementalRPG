using System;
using System.Linq;
using Core.Forge;
using Core.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace UI.Forge
{
    public sealed class ForgeResultView : MonoBehaviour
    {
        [SerializeField] private RectTransform _composition;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _rarity;
        [SerializeField] private TMP_Text _stats;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Image _icon;
        [SerializeField] private Button _take;
        [SerializeField] private Button _break;
        [SerializeField] private RewardRevealEffect _effect;
        private ForgeService _forge;
        private ItemCatalog _catalog;
        private Action _finished;

        private void Awake()
        {
            _take.onClick.AddListener(Take);
            _break.onClick.AddListener(Break);
            UIButtonAudio.InstallInChildren(this);
        }
        private void OnEnable() => LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
        private void OnDisable() => LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
        private void LocaleChanged(Locale _) => Refresh();
        private void OnRectTransformDimensionsChange()
        {
            if (_composition == null) return;
            var size = ((RectTransform)transform).rect.size;
            _composition.localScale = Vector3.one * Mathf.Min(size.x / 1920f, size.y / 1080f);
        }
        public void Show(ForgeService forge, ItemCatalog catalog, Action finished)
        {
            _forge = forge; _catalog = catalog; _finished = finished;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
            OnRectTransformDimensionsChange();
            _effect.Play(false);
        }
        public void Hide() { _effect.Stop(); gameObject.SetActive(false); }
        private void Refresh()
        {
            if (_forge?.Pending == null) return;
            var item = _forge.Pending.Item;
            var definition = _catalog.Get(item.ItemDefinitionId);
            _title.text = definition.GetDisplayName(item).ToUpperInvariant();
            _icon.sprite = definition.GetIcon(item);
            _rarity.text = EquipmentStats.RarityName(item.Rarity).ToUpperInvariant();
            _rarity.color = EquipmentStats.RarityColor(item.Rarity);
            _effect.SetAccent(_rarity.color);
            _stats.text = string.Join("\n", item.Stats.Select(s => $"{EquipmentStats.Name(s.StatId)}  <color=#EFCB88>{EquipmentStats.FormatPercent(s.Value)}</color>"));
            _status.text = ItemText.Get("forge.outcome." + _forge.Pending.Outcome.ToString().ToLowerInvariant());
            _take.GetComponentInChildren<TMP_Text>().text = ItemText.Get("forge.take");
            _break.GetComponentInChildren<TMP_Text>().text = ItemText.Get("forge.break", BigDoubleFormatter.Format(item.SellPrice));
        }
        private void Take()
        {
            if (!_forge.Take()) { _status.text = ItemText.Get("forge.no_space"); return; }
            Hide(); _finished?.Invoke();
        }
        private void Break()
        {
            if (!_forge.Break()) return;
            Hide(); _finished?.Invoke();
        }
    }
}
