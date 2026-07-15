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
            SetSuspicionUI(SuspicionManager.Instance.GetCurrentSuspicion());
            SetSuspicionPreviewUI(SuspicionManager.Instance.GetCurrentSuspicionPreview());
            _originalColor = _suspicionPreviewGauge.color;
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
            // TODO: 의심도 표현 방법에 따라 변경
            _suspicionGauge.fillAmount = (float)suspicion / (float)SuspicionManager.Instance.GetMaxSuspicion();
            _suspicionText.text = suspicion + "/" + SuspicionManager.Instance.GetMaxSuspicion();
        }

        private void SetSuspicionPreviewUI(int suspicion)
        {
            _suspicionPreviewGauge.fillAmount = (float)suspicion / (float)SuspicionManager.Instance.GetMaxSuspicion();
            _suspicionPreviewText.text = suspicion + "/" + SuspicionManager.Instance.GetMaxSuspicion();
            if (suspicion > SuspicionManager.Instance.GetMaxSuspicion())
            {
                _glowSuspicionPreviewCoroutine = StartCoroutine(GlowSuspicionPreview());
            }
            else
            {
                if (_glowSuspicionPreviewCoroutine != null)
                {
                    Debug.Log("coroutine stopped");
                    StopCoroutine(_glowSuspicionPreviewCoroutine);
                    _suspicionPreviewGauge.color = _originalColor;
                }
            }
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
