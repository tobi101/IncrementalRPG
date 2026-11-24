using Core.StateMachine.Features;
using Entity;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public sealed class BossHealthBarView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject _artwork;
        [SerializeField] private Image _fill;
        [SerializeField] private TMP_Text _counter;

        private GameplayFeature _gameplay;
        private Creature _boss;

        [Inject]
        private void Construct(GameplayFeature gameplay)
        {
            _gameplay = gameplay;
            _gameplay.OnBossChanged += Bind;
            Bind(_gameplay.CurrentBoss);
        }

        private void Bind(Creature boss)
        {
            if (_boss != null)
                _boss.OnHealthChanged -= RefreshHealth;

            _boss = boss;
            _counter.enabled = false;
            _artwork.SetActive(_boss != null);
            if (_boss == null) return;

            _boss.OnHealthChanged += RefreshHealth;
            RefreshHealth(_boss.CurrentHP, _boss.Config.maxHP);
        }

        private void RefreshHealth(BigDouble current, BigDouble maximum)
        {
            _fill.fillAmount = maximum > 0 ? (float)(current / maximum) : 0f;
            if (_counter.enabled)
                RefreshCounter(current, maximum);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_boss == null) return;

            RefreshCounter(_boss.CurrentHP, _boss.Config.maxHP);
            _counter.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData) => _counter.enabled = false;

        private void RefreshCounter(BigDouble current, BigDouble maximum)
        {
            _counter.text = BigDoubleFormatter.Format(current, 0, 2) + "/" + BigDoubleFormatter.Format(maximum, 0, 2);
        }

        private void OnDisable() => _counter.enabled = false;

        private void OnDestroy()
        {
            if (_gameplay != null)
                _gameplay.OnBossChanged -= Bind;
            if (_boss != null)
                _boss.OnHealthChanged -= RefreshHealth;
        }
    }
}
