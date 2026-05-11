using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class SessionEndPopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _killsText;
        [SerializeField] private TMP_Text _goldRecordText;
        [SerializeField] private TMP_Text _killsRecordText;
        [SerializeField] private Transform _raysTransform;
        [SerializeField] private Button _hubButton;
        [SerializeField] private float _recordScaleMultiplier = 1.15f;
        [SerializeField] private float _recordScaleUpDuration = 0.14f;
        [SerializeField] private float _recordScaleDownDuration = 0.18f;
        [SerializeField] private float _recordScalePauseDuration = 0.45f;
        [SerializeField] private float _raysRotationDegreesPerSecond = 20f;

        private Action _onHubClicked;
        private Vector3 _goldRecordBaseScale = Vector3.one;
        private Vector3 _killsRecordBaseScale = Vector3.one;

        private void Awake()
        {
            InitializeRecordText(_goldRecordText, ref _goldRecordBaseScale);
            InitializeRecordText(_killsRecordText, ref _killsRecordBaseScale);
            _hubButton.onClick.AddListener(OnHubButtonClicked);
            gameObject.SetActive(false);
        }

        public void Show(BigDouble gold, int kills, bool isNewGoldRecord, bool isNewKillsRecord, Action onHubClicked)
        {
            StopAllCoroutines();
            _onHubClicked = onHubClicked;
            _goldText.text = gold.ToString();
            _killsText.text = kills.ToString();
            gameObject.SetActive(true);
            SetRecordText(_goldRecordText, isNewGoldRecord, _goldRecordBaseScale);
            SetRecordText(_killsRecordText, isNewKillsRecord, _killsRecordBaseScale);
            StartRaysRotation();
        }

        public void Hide()
        {
            StopAllCoroutines();
            SetRecordText(_goldRecordText, false, _goldRecordBaseScale);
            SetRecordText(_killsRecordText, false, _killsRecordBaseScale);
            gameObject.SetActive(false);
            _onHubClicked = null;
        }

        private void OnHubButtonClicked()
        {
            _onHubClicked?.Invoke();
        }

        private void StartRaysRotation()
        {
            if (_raysTransform == null || Mathf.Approximately(_raysRotationDegreesPerSecond, 0f))
                return;

            StartCoroutine(RotateRays());
        }

        private IEnumerator RotateRays()
        {
            while (_raysTransform != null && _raysTransform.gameObject.activeInHierarchy)
            {
                _raysTransform.Rotate(0f, 0f, _raysRotationDegreesPerSecond * Time.unscaledDeltaTime);
                yield return null;
            }
        }

        private void InitializeRecordText(TMP_Text recordText, ref Vector3 baseScale)
        {
            if (recordText == null)
                return;

            baseScale = recordText.transform.localScale;
            recordText.gameObject.SetActive(false);
        }

        private void SetRecordText(TMP_Text recordText, bool visible, Vector3 baseScale)
        {
            if (recordText == null)
                return;

            recordText.transform.localScale = baseScale;
            recordText.gameObject.SetActive(visible);

            if (visible)
                StartCoroutine(AnimateRecordTextLoop(recordText.transform, baseScale));
        }

        private IEnumerator AnimateRecordTextLoop(Transform recordTransform, Vector3 baseScale)
        {
            var peakScale = baseScale * _recordScaleMultiplier;

            while (recordTransform != null && recordTransform.gameObject.activeInHierarchy)
            {
                yield return ScaleRecordText(recordTransform, baseScale, peakScale, _recordScaleUpDuration);
                yield return ScaleRecordText(recordTransform, peakScale, baseScale, _recordScaleDownDuration);

                if (_recordScalePauseDuration > 0f)
                    yield return new WaitForSeconds(_recordScalePauseDuration);
            }
        }

        private IEnumerator ScaleRecordText(Transform recordTransform, Vector3 from, Vector3 to, float duration)
        {
            if (duration <= 0f)
            {
                recordTransform.localScale = to;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                recordTransform.localScale = Vector3.LerpUnclamped(from, to, eased);
                yield return null;
            }

            recordTransform.localScale = to;
        }
    }
}
