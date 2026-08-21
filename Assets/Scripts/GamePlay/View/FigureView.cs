using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GamePlay
{
    [RequireComponent(typeof(Image), typeof(Animator))]
    public class FigureView : MonoBehaviour
    {
        private static readonly int GlitchTrigger = Animator.StringToHash("glitch");
        private static readonly int GlitchFlashTrigger = Animator.StringToHash("glitch_flash");
        private static readonly int GameOverTrigger = Animator.StringToHash("gameover");
        private static readonly int DreamGameOverTrigger = Animator.StringToHash("dream_gameover");

        [SerializeField] private FigureProfile _fallbackProfile;
        [SerializeField] private RectTransform _backgroundSuspicionUIRoot;
        [Header("Glitch Effect")]
        [SerializeField, Min(0f)] private float _glitchEffectDuration = 0.92f;
        [SerializeField, Range(0f, 1f)] private float _glitchEffectIntensity = 0.75f;
        [SerializeField, Min(0f)] private float _glitchFlashEffectDuration = 0.68f;
        [SerializeField, Range(0f, 1f)] private float _glitchFlashEffectIntensity = 0.4f;

        [Header("Defeat Presentation Delays")]
        [FormerlySerializedAs("_defeatStartDelay")]
        [SerializeField, Min(0f)] private float _suspicionOverflowDefeatStartDelay = 3f;
        [SerializeField, Min(0f)] private float _turnLimitDefeatStartDelay = 3f;

        [Header("Defeat Presentation (Shared)")]
        [FormerlySerializedAs("_defeatCloseUpDuration")]
        [SerializeField, Min(0f)] private float _defeatShakeDuration = 3f;
        [SerializeField, Min(1f)] private float _defeatCloseUpScale = 1.5f;
        [SerializeField, Min(0f)] private float _defeatShakeStrength = 35f;
        [SerializeField, Min(1)] private int _defeatShakeVibrato = 35;
        [SerializeField, Range(0f, 180f)] private float _defeatShakeRandomness = 90f;

        private Image _image;
        private RectTransform _rectTransform;
        private Animator _animator;
        private UIGlitchEffect _uiGlitchEffect;
        private Coroutine _glitchCoroutine;
        private Coroutine _suspicionOverflowDefeatCoroutine;
        private Coroutine _turnLimitDefeatCoroutine;
        private Sequence _suspicionOverflowDefeatSequence;
        private Sequence _turnLimitDefeatSequence;
        private FigureProfile _activeProfile;
        private int _defaultSiblingIndex;
        private int _backgroundSuspicionDefaultSiblingIndex = -1;
        private Vector2 _defaultAnchorMin;
        private Vector2 _defaultAnchorMax;
        private Vector2 _defaultPivot;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _defaultSiblingIndex = _rectTransform.GetSiblingIndex();
            if (_backgroundSuspicionUIRoot != null)
            {
                _backgroundSuspicionDefaultSiblingIndex = _backgroundSuspicionUIRoot.GetSiblingIndex();
            }

            _defaultAnchorMin = _rectTransform.anchorMin;
            _defaultAnchorMax = _rectTransform.anchorMax;
            _defaultPivot = _rectTransform.pivot;
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
            WinConditionManager.Instance.RaiseDefeatEvent += HandleDefeatEvent;
            ResetGame();
        }

        private void OnDestroy()
        {
            if (SuspicionManager.Instance != null)
            {
                SuspicionManager.Instance.RaiseSuspicionOverflowEvent -= HandleSuspicionOverflowEvent;
                SuspicionManager.Instance.RaiseSetSuspicionEvent -= HandleSetSuspicionEvent;
            }

            if (TutorialController.Instance != null)
            {
                TutorialController.Instance.RaiseSetTutorialStateEvent -= HandleSetTutorialStateEvent;
            }

            if (WinConditionManager.Instance != null)
            {
                WinConditionManager.Instance.RaiseDefeatEvent -= HandleDefeatEvent;
            }
        }

        public void ResetGame()
        {
            if (_glitchCoroutine != null)
            {
                StopCoroutine(_glitchCoroutine);
                _glitchCoroutine = null;
            }

            if (_suspicionOverflowDefeatCoroutine != null)
            {
                StopCoroutine(_suspicionOverflowDefeatCoroutine);
                _suspicionOverflowDefeatCoroutine = null;
            }

            if (_turnLimitDefeatCoroutine != null)
            {
                StopCoroutine(_turnLimitDefeatCoroutine);
                _turnLimitDefeatCoroutine = null;
            }

            _suspicionOverflowDefeatSequence?.Kill();
            _suspicionOverflowDefeatSequence = null;
            _turnLimitDefeatSequence?.Kill();
            _turnLimitDefeatSequence = null;

            _uiGlitchEffect.Stop();
            
            GameInfo gameInfo = GetCurrentGameInfo();
            FigureProfile profile = gameInfo != null ? gameInfo.GetFigureProfile() : null;
            Apply(profile != null ? profile : _fallbackProfile);
            
            if (gameInfo != null && gameInfo.GetMapType() == GameInfo.MapType.Dream3)
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
            _activeProfile = profile;
            if (_rectTransform.parent != null)
            {
                _rectTransform.SetSiblingIndex(_defaultSiblingIndex);
            }

            if (_backgroundSuspicionUIRoot != null &&
                _backgroundSuspicionUIRoot.parent == _rectTransform.parent &&
                _backgroundSuspicionDefaultSiblingIndex >= 0)
            {
                _backgroundSuspicionUIRoot.SetSiblingIndex(_backgroundSuspicionDefaultSiblingIndex);
            }

            _rectTransform.anchorMin = _defaultAnchorMin;
            _rectTransform.anchorMax = _defaultAnchorMax;
            _rectTransform.pivot = _defaultPivot;
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

        private void HandleDefeatEvent(object sender, DefeatEventArgs e)
        {
            switch (e.Reason)
            {
                case DefeatReason.SuspicionOverflow:
                    _suspicionOverflowDefeatCoroutine = StartCoroutine(PlaySuspicionOverflowDefeatSequence());
                    break;
                case DefeatReason.TurnLimitExceeded:
                    _turnLimitDefeatCoroutine = StartCoroutine(PlayTurnLimitDefeatSequence());
                    break;
            }
        }

        private IEnumerator PlaySuspicionOverflowDefeatSequence()
        {
            if (_suspicionOverflowDefeatStartDelay > 0f)
            {
                yield return new WaitForSeconds(_suspicionOverflowDefeatStartDelay);
            }

            if (_glitchCoroutine != null)
            {
                StopCoroutine(_glitchCoroutine);
                _glitchCoroutine = null;
            }

            _animator.SetTrigger(GameOverTrigger);
            _animator.Update(0f);

            Vector2 suspicionHeadPivot = _activeProfile != null
                ? _activeProfile.HeadPosition
                : _defaultPivot;
            suspicionHeadPivot = new Vector2(
                Mathf.Clamp01(suspicionHeadPivot.x),
                Mathf.Clamp01(suspicionHeadPivot.y));

            Vector3 suspicionJumpScareScale = _activeProfile != null
                ? _activeProfile.LocalScale * _defeatCloseUpScale
                : _rectTransform.localScale * _defeatCloseUpScale;
            _rectTransform.pivot = suspicionHeadPivot;
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localScale = suspicionJumpScareScale;

            if (_backgroundSuspicionUIRoot != null &&
                _backgroundSuspicionUIRoot.parent == _rectTransform.parent &&
                _backgroundSuspicionUIRoot.GetSiblingIndex() > _rectTransform.GetSiblingIndex())
            {
                _backgroundSuspicionUIRoot.SetSiblingIndex(_rectTransform.GetSiblingIndex());
            }

            _suspicionOverflowDefeatSequence = DOTween.Sequence();
            GameManager.Instance?.PlayJumpScareSound(_defeatShakeDuration);
            _suspicionOverflowDefeatSequence.Append(
                _rectTransform
                    .DOShakeAnchorPos(
                        _defeatShakeDuration,
                        _defeatShakeStrength,
                        _defeatShakeVibrato,
                        _defeatShakeRandomness,
                        false,
                        false)
                    .SetEase(Ease.Linear));

            yield return _suspicionOverflowDefeatSequence.WaitForCompletion();

            if (BlackOutPanelView.Instance != null)
            {
                yield return BlackOutPanelView.Instance.PlayBlackOut();
            }
            else
            {
                Debug.LogWarning("Suspicion-overflow defeat could not play its blackout because BlackOutPanelView is missing.", this);
            }

            yield return PlayGameOverDialogueIfConfigured();

            if (GameOverPopupView.Instance != null)
            {
                GameOverPopupView.Instance.ShowSuspicionOverflowGameOver();
            }
            else
            {
                Debug.LogError("Suspicion-overflow defeat could not show the game-over screen because GameOverPopupView is missing.", this);
            }

            _suspicionOverflowDefeatSequence = null;
            _suspicionOverflowDefeatCoroutine = null;
        }

        private IEnumerator PlayTurnLimitDefeatSequence()
        {
            if (_turnLimitDefeatStartDelay > 0f)
            {
                yield return new WaitForSeconds(_turnLimitDefeatStartDelay);
            }

            if (_glitchCoroutine != null)
            {
                StopCoroutine(_glitchCoroutine);
                _glitchCoroutine = null;
            }

            _animator.SetTrigger(GameOverTrigger);
            _animator.Update(0f);

            Vector2 turnLimitHeadPivot = _activeProfile != null
                ? _activeProfile.HeadPosition
                : _defaultPivot;
            turnLimitHeadPivot = new Vector2(
                Mathf.Clamp01(turnLimitHeadPivot.x),
                Mathf.Clamp01(turnLimitHeadPivot.y));

            Vector3 turnLimitJumpScareScale = _activeProfile != null
                ? _activeProfile.LocalScale * _defeatCloseUpScale
                : _rectTransform.localScale * _defeatCloseUpScale;
            _rectTransform.pivot = turnLimitHeadPivot;
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localScale = turnLimitJumpScareScale;

            if (_backgroundSuspicionUIRoot != null &&
                _backgroundSuspicionUIRoot.parent == _rectTransform.parent &&
                _backgroundSuspicionUIRoot.GetSiblingIndex() > _rectTransform.GetSiblingIndex())
            {
                _backgroundSuspicionUIRoot.SetSiblingIndex(_rectTransform.GetSiblingIndex());
            }

            _turnLimitDefeatSequence = DOTween.Sequence();
            GameManager.Instance?.PlayJumpScareSound(_defeatShakeDuration);
            _turnLimitDefeatSequence.Append(
                _rectTransform
                    .DOShakeAnchorPos(
                        _defeatShakeDuration,
                        _defeatShakeStrength,
                        _defeatShakeVibrato,
                        _defeatShakeRandomness,
                        false,
                        true)
                    .SetEase(Ease.InOutSine));

            yield return _turnLimitDefeatSequence.WaitForCompletion();

            yield return PlayGameOverDialogueIfConfigured();

            if (GameOverPopupView.Instance != null)
            {
                GameOverPopupView.Instance.ShowTurnLimitGameOver();
            }
            else
            {
                Debug.LogError("Turn-limit defeat could not show the game-over screen because GameOverPopupView is missing.", this);
            }

            _turnLimitDefeatSequence = null;
            _turnLimitDefeatCoroutine = null;
        }

        private IEnumerator PlayGameOverDialogueIfConfigured()
        {
            bool dialogueCompleted = false;
            bool dialogueStarted = GameManager.Instance != null &&
                                   GameManager.Instance.TryPlayGameOverDialogue(
                                       () => dialogueCompleted = true);

            if (dialogueStarted)
            {
                yield return new WaitUntil(() => dialogueCompleted);
            }
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
            GameManager.Instance?.PlayGlitchSound();
        }

        private void TriggerGlitchFlash()
        {
            _animator.SetTrigger(GlitchFlashTrigger);
            _uiGlitchEffect.Play(_glitchFlashEffectDuration, _glitchFlashEffectIntensity);
            GameManager.Instance?.PlayGlitchSound();
        }

        private void HandleSetTutorialStateEvent(object sender, SetTutorialStateEventArgs e)
        {
            if (e.CurrentState == TutorialState.Dream1)
            {
                _animator.SetTrigger(DreamGameOverTrigger);
            }
        }
    }
}
