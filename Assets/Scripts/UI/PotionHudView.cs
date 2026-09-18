using System.Collections.Generic;
using Core.Items;
using Reflex.Attributes;
using UnityEngine;

namespace UI
{
    public sealed class PotionHudView : MonoBehaviour
    {
        [SerializeField] private RectTransform _listRoot;
        [SerializeField] private PotionHudRowView _rowTemplate;
        [SerializeField] private TooltipView _tooltip;

        private RunConsumableService _consumables;
        private ItemCatalog _catalog;
        private readonly List<PotionHudRowView> _rows = new();

        [Inject]
        public void Construct(RunConsumableService consumables, ItemCatalog catalog)
        {
            _consumables = consumables;
            _catalog = catalog;
            _consumables.OnEffectsChanged += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            if (_listRoot == null || _rowTemplate == null)
                return;
            _tooltip.Hide();
            var index = 0;
            foreach (var effect in _consumables.Effects)
            {
                if (!effect.IsActive)
                    continue;
                if (index == _rows.Count)
                {
                    var row = Instantiate(_rowTemplate, _listRoot);
                    row.name = "PotionEffectRow";
                    _rows.Add(row);
                }
                _rows[index].Bind(_catalog.Get(effect.SourceItemDefinitionId), effect, _tooltip);
                _rows[index++].gameObject.SetActive(true);
            }
            for (var i = index; i < _rows.Count; i++)
                _rows[i].gameObject.SetActive(false);
            _listRoot.gameObject.SetActive(index > 0);
        }

        private void OnDisable() => _tooltip?.Hide();

        private void OnDestroy()
        {
            if (_consumables != null)
                _consumables.OnEffectsChanged -= Refresh;
        }
    }
}
