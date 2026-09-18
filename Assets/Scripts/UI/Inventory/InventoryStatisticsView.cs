using Core.Gameplay;
using Core.Gameplay.Bomb;
using Core.Items;
using Core.TestSkillTree;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;

namespace UI.Inventory
{
    public sealed class InventoryStatisticsView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _attacksTitle;
        [SerializeField] private TMP_Text _zoneTitle;
        [SerializeField] private TMP_Text _barrelsTitle;
        [SerializeField] private TMP_Text[] _labels;
        [SerializeField] private TMP_Text[] _values;
        [SerializeField] private TMP_Text _hint;
        [Inject] private PlayerItemStorage _storage;
        [Inject] private SkillTreeService _training;
        [Inject] private RunConsumableService _consumables;
        [Inject] private DamageZone _zone;
        [Inject] private BombExplosionService _bombs;
        private bool _started;
        private readonly Vector2[] _attackLabelPositions = new Vector2[4];
        private readonly Vector2[] _attackValuePositions = new Vector2[4];
        private RectTransform _zoneSection, _barrelsSection;
        private Vector2 _zonePosition, _barrelsPosition;
        private float _attackRowSpacing;

        private void Awake()
        {
            for (var i = 0; i < _attackLabelPositions.Length; i++)
            {
                _attackLabelPositions[i] = _labels[i].rectTransform.anchoredPosition;
                _attackValuePositions[i] = _values[i].rectTransform.anchoredPosition;
            }
            _attackRowSpacing = _attackLabelPositions[0].y - _attackLabelPositions[1].y;
            _zoneSection = (RectTransform)_zoneTitle.transform.parent;
            _barrelsSection = (RectTransform)_barrelsTitle.transform.parent;
            _zonePosition = _zoneSection.anchoredPosition;
            _barrelsPosition = _barrelsSection.anchoredPosition;
        }

        private void Start()
        {
            _storage.OnChanged += Refresh;
            _training.OnUpgraded += Refresh;
            _consumables.OnEffectsChanged += Refresh;
            LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
            _started = true;
            Refresh();
        }

        private void OnEnable() { if (_started) Refresh(); }
        private void OnDestroy()
        {
            if (!_started) return;
            _storage.OnChanged -= Refresh;
            _training.OnUpgraded -= Refresh;
            _consumables.OnEffectsChanged -= Refresh;
            LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
        }
        private void LocaleChanged(Locale locale) => Refresh();

        public void Refresh()
        {
            _title.text = ItemText.Get("equipment.statistics");
            _attacksTitle.text = ItemText.Get("equipment.section.attacks");
            _zoneTitle.text = ItemText.Get("equipment.section.zone");
            _barrelsTitle.text = ItemText.Get("equipment.section.barrels");
            _hint.text = ItemText.Get("equipment.panel_hint");
            for (var i = 0; i < EquipmentStats.Ids.Count; i++)
                _labels[i].text = EquipmentStats.Name(EquipmentStats.Ids[i]);

            var autoUnlocked = _training.IsUnlocked(GameFeature.AutoAttack);
            var bombsUnlocked = _training.IsUnlocked(GameFeature.Bombs);
            RefreshVisibility(autoUnlocked, bombsUnlocked);
            _values[0].text = BigDoubleFormatter.Format(_zone.GetDamage(DamageZone.AttackSource.Manual));
            _values[1].text = autoUnlocked ? BigDoubleFormatter.Format(_zone.GetDamage(DamageZone.AttackSource.Auto)) : string.Empty;
            _values[2].text = Frequency(_zone.GetManualAttackCooldown());
            _values[3].text = autoUnlocked ? Frequency(_zone.GetAutoAttackInterval()) : string.Empty;
            _values[4].text = Number(_zone.RadiusX);
            _values[5].text = bombsUnlocked ? BigDoubleFormatter.Format(_bombs.GetDamage()) : string.Empty;
            _values[6].text = bombsUnlocked ? Number(_bombs.GetRadius()) : string.Empty;
        }

        private void RefreshVisibility(bool autoUnlocked, bool bombsUnlocked)
        {
            var removedHeight = 0f;
            for (var i = 0; i < _attackLabelPositions.Length; i++)
            {
                var visible = i == 0 || i == 2 || autoUnlocked;
                SetRowVisible(i, visible);
                var offset = Vector2.up * removedHeight;
                _labels[i].rectTransform.anchoredPosition = _attackLabelPositions[i] + offset;
                _values[i].rectTransform.anchoredPosition = _attackValuePositions[i] + offset;
                if (!visible) removedHeight += _attackRowSpacing;
            }

            // Start from the authored positions so refreshes and unlocks do not accumulate offsets.
            _zoneSection.anchoredPosition = _zonePosition + Vector2.up * removedHeight;
            _barrelsSection.anchoredPosition = _barrelsPosition + Vector2.up * removedHeight;
            SetRowVisible(5, bombsUnlocked);
            SetRowVisible(6, bombsUnlocked);
            _barrelsSection.gameObject.SetActive(bombsUnlocked);
        }

        private void SetRowVisible(int index, bool visible)
        {
            _labels[index].gameObject.SetActive(visible);
            _values[index].gameObject.SetActive(visible);
        }

        private static string Number(float value) => value.ToString("0.##", EquipmentStats.Culture);
        private static string Frequency(float interval) => ItemText.Get("equipment.per_second", Number(1f / interval));
    }
}
