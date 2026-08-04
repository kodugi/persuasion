using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SingletonUtils;

namespace GamePlay
{
    public class SuspicionView: SelfInitializingMonoBehaviourSingleton<SuspicionView>
    {
        [SerializeField] private Image _suspicionGauge;
        [SerializeField] private TextMeshProUGUI _suspicionText;
        [SerializeField] private Image _suspicionPreviewGauge;
        [SerializeField] private TextMeshProUGUI _suspicionPreviewText;
        [SerializeField] private RectTransform _focusTarget;

        private Coroutine _slideSuspicionCoroutine;
        private Coroutine _slideSuspicionPreviewCoroutine;
        private Coroutine _glowSuspicionPreviewCoroutine;
        private Color _originalColor;

        protected override bool InitializeCore()
        {
            if (SuspicionManager.Instance == null)
            {
                return false;
            }

            SuspicionManager.Instance.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
            SuspicionManager.Instance.RaiseSetSuspicionPreviewEvent += HandleSetSuspicionPreviewEvent;
            _originalColor = _suspicionPreviewGauge.color;
            SetSuspicionUI(SuspicionManager.Instance.GetCurrentSuspicion());
            SetSuspicionPreviewUI(SuspicionManager.Instance.GetCurrentSuspicionPreview());
            return true;
        }

        protected override void OnDestroy()
        {
            if (SuspicionManager.Instance != null)
            {
                SuspicionManager.Instance.RaiseSetSuspicionEvent -= HandleSetSuspicionEvent;
                SuspicionManager.Instance.RaiseSetSuspicionPreviewEvent -= HandleSetSuspicionPreviewEvent;
            }

            base.OnDestroy();
        }

        private void SetSuspicionUI(int suspicion)
        {
            if (_slideSuspicionCoroutine != null)
            {
                StopCoroutine(_slideSuspicionCoroutine);
            }

            _slideSuspicionCoroutine = StartCoroutine(SlideSuspicionGauge(_suspicionGauge,
                (float)suspicion / (float)SuspicionManager.Instance.GetMaxSuspicion()));
            _suspicionText.text = suspicion + "/" + SuspicionManager.Instance.GetMaxSuspicion();
        }

        private void SetSuspicionPreviewUI(int suspicion)
        {
            if (_slideSuspicionPreviewCoroutine != null)
            {
                StopCoroutine(_slideSuspicionPreviewCoroutine);
            }

            _slideSuspicionPreviewCoroutine = StartCoroutine(SlideSuspicionGauge(_suspicionPreviewGauge,
                (float)suspicion / (float)SuspicionManager.Instance.GetMaxSuspicion()));
            _suspicionPreviewText.text = suspicion + "/" + SuspicionManager.Instance.GetMaxSuspicion();
            if (suspicion > SuspicionManager.Instance.GetMaxSuspicion())
            {
                if (_glowSuspicionPreviewCoroutine == null)
                {
                    _glowSuspicionPreviewCoroutine = StartCoroutine(GlowSuspicionPreview());
                }
            }
            else
            {
                if (_glowSuspicionPreviewCoroutine != null)
                {
                    StopCoroutine(_glowSuspicionPreviewCoroutine);
                    _glowSuspicionPreviewCoroutine = null;
                    _suspicionPreviewGauge.color = _originalColor;
                }
            }
        }
        
        IEnumerator SlideSuspicionGauge(Image suspicionGauge, float end)
        {
            end = Mathf.Clamp01(end);
            float start = suspicionGauge.fillAmount;
            int steps = 20;
            float progress = (end - start) / steps;

            for (int i = 0; i < steps; i++)
            {
                suspicionGauge.fillAmount += progress;
                yield return new WaitForSeconds(0.01f);
            }
            
            suspicionGauge.fillAmount = end;
            yield return null;
        }

        private void HandleSetSuspicionEvent(object sender, SetSuspicionEventArgs e)
        {
            SetSuspicionUI(e.Suspicion);
        }

        private void HandleSetSuspicionPreviewEvent(object sender, SetSuspicionEventArgs e)
        {
            SetSuspicionPreviewUI(e.Suspicion);
        }

        public RectTransform GetFocusTarget()
        {
            return _focusTarget != null ? _focusTarget : GetComponent<RectTransform>();
        }

        IEnumerator GlowSuspicionPreview()
        {
            for (int i = 0; true; i = (i + 3) % 360)
            {
                _suspicionPreviewGauge.color = new Color(_originalColor.r + 0.5f * (1 - (float)Math.Cos(Math.PI / 180 * i)), _originalColor.g,
                    _originalColor.b, _originalColor.a);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}
