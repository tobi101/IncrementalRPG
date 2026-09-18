using Core.Items;
using TMPro;
using UI.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace UI
{
    public sealed class PotionHudRowView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _value;
        private LocalizedStringBinding _nameBinding;
        private LocalizedStringBinding _valueBinding;
        private TooltipView _tooltip;
        private LocalizedString _description;

        public void Bind(ItemDefinition definition, ActiveConsumableEffect effect, TooltipView tooltip)
        {
            _nameBinding ??= new LocalizedStringBinding(_name);
            _valueBinding ??= new LocalizedStringBinding(_value);
            _icon.sprite = definition.icon;
            _nameBinding.Bind(definition.localizedName);
            _valueBinding.Bind(new LocalizedString(ItemText.Table, ItemText.EffectKey(effect.EffectId))
            {
                Arguments = new object[] { effect.Value }
            });
            _description = new LocalizedString(definition.localizedDescription.TableReference,
                definition.localizedDescription.TableEntryReference)
            {
                Arguments = new object[] { effect.Value }
            };
            _tooltip = tooltip;
        }

        public void OnPointerEnter(PointerEventData eventData) =>
            _tooltip?.Show(_description, (RectTransform)transform);

        public void OnPointerExit(PointerEventData eventData) => _tooltip?.Hide();

        private void OnDisable() => _tooltip?.Hide();

        private void OnDestroy()
        {
            _nameBinding?.Dispose();
            _valueBinding?.Dispose();
        }
    }
}
