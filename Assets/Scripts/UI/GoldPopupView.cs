using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GoldPopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private Action _onComplete;

        public void Show(int amount, float startY, Action onComplete)
        {
            StopAllCoroutines();
            _onComplete = onComplete;
            _text.text = $"+{amount}";
            ((RectTransform)transform).anchoredPosition = new Vector2(0f, startY);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1f);
            gameObject.SetActive(true);
            StartCoroutine(Animate(startY));
        }

        private IEnumerator Animate(float startY)
        {
            const float duration = 1.2f;
            const float moveDistance = 60f;

            var startColor = _text.color;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                ((RectTransform)transform).anchoredPosition = new Vector2(0f, startY - moveDistance * t);
                _text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                yield return null;
            }

            gameObject.SetActive(false);
            _onComplete?.Invoke();
        }
    }
}
