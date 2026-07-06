using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamePlay
{
    public class SuspicionView: SelfInitializingMonoBehaviourSingleton<SuspicionView>
    {
        [SerializeField] private Image _suspicionGauge;
        [SerializeField] private TextMeshProUGUI _suspicionText;
        [SerializeField] private Image _suspicionPreviewGauge;
        [SerializeField] private TextMeshProUGUI _suspicionPreviewText;
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
            return true;
        }

        protected override void OnDestroy()
        {
            if (SuspicionManager.Instance != null)
            {
                SuspicionManager.Instance.RaiseSetSuspicionEvent -= HandleSetSuspicionEvent;
            }

            base.OnDestroy();
        }

        private void SetSuspicionUI(int suspicion)
        {
            // TODO: 의심도 표현 방법에 따라 변경
            _suspicionGauge.fillAmount = (float)suspicion / (float)SuspicionManager.Instance.GetMaxSuspicion();
            _suspicionText.text = "의심도: " + suspicion + "/" + SuspicionManager.Instance.GetMaxSuspicion();
        }

        private void SetSuspicionPreviewUI(int suspicion)
        {
            _suspicionPreviewGauge.fillAmount = (float)suspicion / (float)SuspicionManager.Instance.GetMaxSuspicion();
            _suspicionPreviewText.text = "의심도: " + suspicion + "/" + SuspicionManager.Instance.GetMaxSuspicion();
        }

        private void HandleSetSuspicionEvent(object sender, SetSuspicionEventArgs e)
        {
            SetSuspicionUI(e.Suspicion);
        }

        private void HandleSetSuspicionPreviewEvent(object sender, SetSuspicionEventArgs e)
        {
            SetSuspicionPreviewUI(e.Suspicion);
        }
    }
}
