using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    [RequireComponent(typeof(Image), typeof(Animator))]
    public class FigureView : MonoBehaviour
    {
        private static readonly int GlitchTrigger = Animator.StringToHash("glitch");
        private static readonly int GlitchFlashTrigger = Animator.StringToHash("glitch_flash");
        private static readonly int GameOverTrigger = Animator.StringToHash("gameover");

        [SerializeField] private FigureProfile _fallbackProfile;
        [Header("Glitch Effect")]
        [SerializeField, Min(0f)] private float _glitchEffectDuration = 0.92f;
        [SerializeField, Range(0f, 1f)] private float _glitchEffectIntensity = 0.75f;
        [SerializeField, Min(0f)] private float _glitchFlashEffectDuration = 0.68f;
        [SerializeField, Range(0f, 1f)] private float _glitchFlashEffectIntensity = 0.4f;

        private Image _image;
        private RectTransform _rectTransform;
        private Animator _animator;
        private UIGlitchEffect _uiGlitchEffect;
        private Coroutine _glitchCoroutine;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _animator = GetComponent<Animator>();
            _uiGlitchEffect = GetComponent<UIGlitchEffect>();

            if (_uiGlitchEffect == null)
            {
                _uiGlitchEffect = gameObject.AddComponent<UIGlitchEffect>();
            }
        }

        private void Start()
        {
            SuspicionManager.Instance.RaiseSuspicionOverflowEvent += HandleSuspicionOverflowEvent;
            SuspicionManager.Instance.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
            TutorialController.Instance.RaiseSetTutorialStateEvent += HandleSetTutorialStateEvent;
            ResetGame();
        }

        public void ResetGame()
        {
            if (_glitchCoroutine != null)
            {
                StopCoroutine(_glitchCoroutine);
                _glitchCoroutine = null;
            }

            _uiGlitchEffect.Stop();
            
            GameInfo gameInfo = GetCurrentGameInfo();
            FigureProfile profile = gameInfo != null ? gameInfo.GetFigureProfile() : null;
            Apply(profile != null ? profile : _fallbackProfile);
            
            if (gameInfo.GetMapType() == GameInfo.MapType.Dream3)
            {
                _glitchCoroutine = StartCoroutine(PlayRepetitiveGlitchAnimation());
            }
        }

        public void Apply(FigureProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning("FigureView could not apply a figure because its FigureProfile is missing.", this);
                return;
            }

            _image.sprite = profile.Sprite;
            _rectTransform.anchoredPosition = profile.AnchoredPosition;
            _rectTransform.sizeDelta = profile.SizeDelta;
            _rectTransform.localScale = profile.LocalScale;

            if (profile.AnimatorController == null)
            {
                Debug.LogWarning($"FigureProfile '{profile.name}' has no Animator Controller.", profile);
                return;
            }

            _animator.runtimeAnimatorController = profile.AnimatorController;
            _animator.Rebind();

            if (!string.IsNullOrWhiteSpace(profile.InitialState))
            {
                int stateHash = Animator.StringToHash(profile.InitialState);
                if (_animator.HasState(0, stateHash))
                {
                    _animator.Play(stateHash, 0, 0f);
                }
                else
                {
                    Debug.LogWarning(
                        $"FigureProfile '{profile.name}' does not have state '{profile.InitialState}' on layer 0.",
                        profile);
                }
            }

            // Immediately samples the controller's default or requested initial state.
            _animator.Update(0f);
        }

        private static GameInfo GetCurrentGameInfo()
        {
            var gameInfoList = GameInfoHolder.GetGameInfoList();
            return gameInfoList != null && gameInfoList.Count > 0
                ? GameInfoHolder.GetCurrentGameInfo()
                : null;
        }

        private void HandleSuspicionOverflowEvent(object sender, SetSuspicionEventArgs e)
        {
            if (SuspicionManager.Instance.GetCurrentSuspicion() <= SuspicionManager.Instance.GetMaxSuspicion())
            {
                switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
                {
                    case GameInfo.MapType.Dream2:
                        TriggerGlitch();
                        break;
                }
            }
        }

        private void HandleSetSuspicionEvent(object sender, SetSuspicionEventArgs e)
        {
            
        }

        private IEnumerator PlayRepetitiveGlitchAnimation()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(3f, 6f));
                TriggerGlitchFlash();
            }
        }

        private void TriggerGlitch()
        {
            _animator.SetTrigger(GlitchTrigger);
            _uiGlitchEffect.Play(_glitchEffectDuration, _glitchEffectIntensity);
        }

        private void TriggerGlitchFlash()
        {
            _animator.SetTrigger(GlitchFlashTrigger);
            _uiGlitchEffect.Play(_glitchFlashEffectDuration, _glitchFlashEffectIntensity);
        }

        private void HandleSetTutorialStateEvent(object sender, SetTutorialStateEventArgs e)
        {
            if (e.CurrentState == TutorialState.Dream1)
            {
                _animator.SetTrigger(GameOverTrigger);
            }
        }
    }
}
