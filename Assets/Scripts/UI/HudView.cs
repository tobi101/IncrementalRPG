using System.Collections.Generic;
using Core.StateMachine.Features;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _sessionGoldText;
        [SerializeField] private TMP_Text _killsText;
        [SerializeField] private GoldPopupView _popupPrefab;
        [SerializeField] private RectTransform _popupContainer;

        private const int PoolSize = 10;
        private const float LerpSpeed = 8f;

        private GameplayFeature _gameplay;
        private readonly Queue<GoldPopupView> _popupPool = new();

        private const float BatchWindow = 0.2f;

        private double _goldDisplayed;
        private double _goldTarget;
        private float _killsDisplayed;
        private int _killsTarget;
        private int _activePopupCount;
        private int _pendingPopupGold;
        private float _batchTimer;

        [Inject]
        public void Construct(GameplayFeature gameplay)
        {
            _gameplay = gameplay;

            if (_popupPrefab != null)
            {
                for (var i = 0; i < PoolSize; i++)
                {
                    var popup = Instantiate(_popupPrefab, _popupContainer);
                    popup.gameObject.SetActive(false);
                    _popupPool.Enqueue(popup);
                }
            }

            _gameplay.OnSessionGoldEarned += HandleSessionGoldEarned;
            _gameplay.OnSessionKillsChanged += HandleSessionKillsChanged;
        }

        private void OnEnable()
        {
            ResetPopups();
            _activePopupCount = 0;
            _pendingPopupGold = 0;
            _batchTimer = 0f;
            _goldDisplayed = 0;
            _goldTarget = 0;
            _killsDisplayed = 0;
            _killsTarget = 0;
            if (_sessionGoldText != null) _sessionGoldText.text = "0";
            if (_killsText != null) _killsText.text = "0";
        }

        private void ResetPopups()
        {
            if (_popupContainer == null) return;
            _popupPool.Clear();
            foreach (Transform child in _popupContainer)
            {
                child.gameObject.SetActive(false);
                var popup = child.GetComponent<GoldPopupView>();
                if (popup != null) _popupPool.Enqueue(popup);
            }
        }

        private void Update()
        {
            if (_batchTimer > 0f)
            {
                _batchTimer -= Time.deltaTime;
                if (_batchTimer <= 0f)
                {
                    SpawnPopup(_pendingPopupGold);
                    _pendingPopupGold = 0;
                }
            }

            if (_goldDisplayed < _goldTarget)
            {
                _goldDisplayed += (_goldTarget - _goldDisplayed) * Time.deltaTime * LerpSpeed;
                if (_goldTarget - _goldDisplayed < 0.5) _goldDisplayed = _goldTarget;
                _sessionGoldText.text = new BigDouble(_goldDisplayed).ToString();
            }

            if ((int)_killsDisplayed < _killsTarget)
            {
                _killsDisplayed += (_killsTarget - _killsDisplayed) * Time.deltaTime * LerpSpeed;
                if (_killsTarget - _killsDisplayed < 0.5f) _killsDisplayed = _killsTarget;
                _killsText.text = ((int)_killsDisplayed).ToString();
            }
        }

        private void HandleSessionGoldEarned(BigDouble sessionTotal, int delta)
        {
            _goldTarget = (double)sessionTotal;
            if (_pendingPopupGold == 0) _batchTimer = BatchWindow;
            _pendingPopupGold += delta;
        }

        private void HandleSessionKillsChanged(int total)
        {
            _killsTarget = total;
        }

        private void SpawnPopup(int amount)
        {
            if (_popupPrefab == null) return;

            var popup = _popupPool.Count > 0
                ? _popupPool.Dequeue()
                : Instantiate(_popupPrefab, _popupContainer);

            var startY = _activePopupCount * 50f;
            _activePopupCount++;
            popup.Show(amount, startY, () =>
            {
                _activePopupCount = Mathf.Max(0, _activePopupCount - 1);
                _popupPool.Enqueue(popup);
            });
        }

        private void OnDestroy()
        {
            if (_gameplay == null) return;
            _gameplay.OnSessionGoldEarned -= HandleSessionGoldEarned;
            _gameplay.OnSessionKillsChanged -= HandleSessionKillsChanged;
        }
    }
}
