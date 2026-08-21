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
        [Header("Dialogue Figures")]
        [SerializeField] private Image _centerDialogueFigure;
        [SerializeField] private Image _rightDialogueFigure;

        [Header("Figure Layout")]
        [Tooltip("Matches all three figure slots to the responsive layout used by InvestigationScene dialogues.")]
        [SerializeField] private bool _matchInvestigationFigureLayout = true;
        [SerializeField, Min(1f)] private float _investigationReferenceWidth = 800f;
        [SerializeField, Min(0.01f)] private float _investigationPortraitAspectRatio = 0.35f;
        [SerializeField] private float _investigationHorizontalInset = 150f;
        [SerializeField] private float _investigationVerticalOffset = -70f;
        [SerializeField] private float _investigationHeightExtension = 400f;
        [SerializeField] private float _investigationShortPortraitOffset = 50f;
        [SerializeField] private float _investigationTallPortraitOffset = -30f;

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
        private float _currentLeftFigureVerticalOffset;
        private Vector2 _lastLayoutParentSize;
        private bool _hasLayoutParentSize;

        private enum FigureSlot
        {
            Left,
            Center,
            Right
        }

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

            ApplyInvestigationFigureLayout(_rectTransform, FigureSlot.Left, 0f);
            ApplyInvestigationFigureLayout(
                _centerDialogueFigure != null ? _centerDialogueFigure.rectTransform : null,
                FigureSlot.Center,
                0f);
            ApplyInvestigationFigureLayout(
                _rightDialogueFigure != null ? _rightDialogueFigure.rectTransform : null,
                FigureSlot.Right,
                0f);
            CacheLayoutParentSize();

            if (_uiGlitchEffect == null)
            {
                _uiGlitchEffect = gameObject.AddComponent<UIGlitchEffect>();
            }
        }

        private void LateUpdate()
        {
            if (!_matchInvestigationFigureLayout || !TryGetLayoutParentSize(out Vector2 parentSize))
            {
                return;
            }

            if (_hasLayoutParentSize &&
                Mathf.Approximately(parentSize.x, _lastLayoutParentSize.x) &&
                Mathf.Approximately(parentSize.y, _lastLayoutParentSize.y))
            {
                return;
            }

            _lastLayoutParentSize = parentSize;
            _hasLayoutParentSize = true;

            // A defeat sequence temporarily owns the main figure's transform.
            if (!IsDefeatPresentationActive())
            {
                ApplyInvestigationFigureLayout(
                    _rectTransform,
                    FigureSlot.Left,
                    _currentLeftFigureVerticalOffset);
            }

            ApplyInvestigationFigureLayout(
                _centerDialogueFigure != null ? _centerDialogueFigure.rectTransform : null,
                FigureSlot.Center,
                GetDialogueFigureVerticalOffset(_centerDialogueFigure != null
                    ? _centerDialogueFigure.sprite
                    : null));
            ApplyInvestigationFigureLayout(
                _rightDialogueFigure != null ? _rightDialogueFigure.rectTransform : null,
                FigureSlot.Right,
                GetDialogueFigureVerticalOffset(_rightDialogueFigure != null
                    ? _rightDialogueFigure.sprite
                    : null));
        }

        private void Start()
        {
            SuspicionManager.Instance.RaiseSuspicionOverflowEvent += HandleSuspicionOverflowEvent;
            SuspicionManager.Instance.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
            TutorialController.Instance.RaiseSetTutorialStateEvent += HandleSetTutorialStateEvent;
            WinConditionManager.Instance.RaiseDefeatEvent += HandleDefeatEvent;
            DialogueManager.Instance.RaiseSetDialogueEntryEvent += HandleSetDialogueEntryEvent;
            DialogueManager.Instance.RaiseDialoguePageEndEvent += HandleDialoguePageEndEvent;
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

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.RaiseSetDialogueEntryEvent -= HandleSetDialogueEntryEvent;
                DialogueManager.Instance.RaiseDialoguePageEndEvent -= HandleDialoguePageEndEvent;
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
            HideDialogueFigure(_centerDialogueFigure);
            HideDialogueFigure(_rightDialogueFigure);
            _image.enabled = true;
            _animator.enabled = true;
            
            GameInfo gameInfo = GetCurrentGameInfo();
            FigureProfile profile = gameInfo != null ? gameInfo.GetFigureProfile() : null;
            Apply(profile != null ? profile : _fallbackProfile);
            
            if (gameInfo != null && gameInfo.GetMapType() == GameInfo.MapType.Dream3)
            {
                _glitchCoroutine = StartCoroutine(PlayRepetitiveGlitchAnimation());
            }

            ApplyDialogueFigure(DialogueManager.Instance?.GetCurrentDialogueEntry());
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
            _currentLeftFigureVerticalOffset = profile.InvestigationVerticalOffset;

            if (_matchInvestigationFigureLayout)
            {
                ApplyInvestigationFigureLayout(
                    _rectTransform,
                    FigureSlot.Left,
                    _currentLeftFigureVerticalOffset);
            }
            else
            {
                _rectTransform.anchoredPosition = profile.AnchoredPosition;
                _rectTransform.sizeDelta = profile.SizeDelta;
            }

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
            GamePlaySoundManager.Instance?.Play(GamePlaySoundId.JumpScare, _defeatShakeDuration);
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
            GamePlaySoundManager.Instance?.Play(GamePlaySoundId.JumpScare, _defeatShakeDuration);
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
            GamePlaySoundManager.Instance?.Play(GamePlaySoundId.Glitch);
        }

        private void TriggerGlitchFlash()
        {
            _animator.SetTrigger(GlitchFlashTrigger);
            _uiGlitchEffect.Play(_glitchFlashEffectDuration, _glitchFlashEffectIntensity);
            GamePlaySoundManager.Instance?.Play(GamePlaySoundId.Glitch);
        }

        private void HandleSetTutorialStateEvent(object sender, SetTutorialStateEventArgs e)
        {
            if (e.CurrentState == TutorialState.Dream1)
            {
                _animator.SetTrigger(DreamGameOverTrigger);
            }
        }

        private void HandleSetDialogueEntryEvent(object sender, SetDialogueEntryEventArgs e)
        {
            DialogueEntry dialogueEntry = e.GetDialogueEntry();
            ApplyDialogueFigure(dialogueEntry);

            if (GetCurrentGameInfo()?.GetMapType() == GameInfo.MapType.Dream4 &&
                dialogueEntry != null &&
                dialogueEntry.StateToTrigger == TutorialState.Dream2)
            {
                GamePlaySoundManager.Instance?.Play(GamePlaySoundId.Laughter);
            }
        }

        private void ApplyDialogueFigure(DialogueEntry dialogueEntry)
        {
            if (dialogueEntry == null)
            {
                return;
            }

            ApplyDialogueFigure(dialogueEntry.FigurePosition, dialogueEntry.FigureSprite);
            ApplyDialogueFigure(dialogueEntry.AdditionalFigurePosition, dialogueEntry.AdditionalFigureSprite);
            ApplyDialogueFigure(dialogueEntry.TertiaryFigurePosition, dialogueEntry.TertiaryFigureSprite);
        }

        private void ApplyDialogueFigure(DialogueFigurePosition position, Sprite sprite)
        {
            switch (position)
            {
                case DialogueFigurePosition.Left:
                    SetLeftDialogueFigure(sprite);
                    break;
                case DialogueFigurePosition.Center:
                    SetDialogueFigure(_centerDialogueFigure, FigureSlot.Center, sprite);
                    break;
                case DialogueFigurePosition.Right:
                    SetDialogueFigure(_rightDialogueFigure, FigureSlot.Right, sprite);
                    break;
            }
        }

        private void HandleDialoguePageEndEvent(object sender, DialoguePageEndEventArgs e)
        {
            if (e.GetLastDialogueEntry()?.HideFiguresAfterDialogue == true)
            {
                HideDialogueFigure(_centerDialogueFigure);
                HideDialogueFigure(_rightDialogueFigure);
                return;
            }

            // Intermediate page breaks keep their figures. Once the complete dialogue ends,
            // center/right portraits are cleared so they never remain over the puzzle.
            if (DialogueManager.Instance != null && DialogueManager.Instance.HasCurrentDialogueData())
            {
                return;
            }

            HideDialogueFigure(_centerDialogueFigure);
            HideDialogueFigure(_rightDialogueFigure);
        }

        private void SetLeftDialogueFigure(Sprite sprite)
        {
            if (sprite == null)
            {
                _image.enabled = false;
                return;
            }

            // The regular left figure is animator-driven. Disable it while a dialogue
            // explicitly owns the sprite so the animation cannot overwrite the requested pose.
            _animator.enabled = false;
            _image.sprite = sprite;
            _image.enabled = true;
            _currentLeftFigureVerticalOffset = GetDialogueFigureVerticalOffset(sprite);
            ApplyInvestigationFigureLayout(
                _rectTransform,
                FigureSlot.Left,
                _currentLeftFigureVerticalOffset);
        }

        private void SetDialogueFigure(Image image, FigureSlot slot, Sprite sprite)
        {
            if (image == null)
            {
                Debug.LogWarning("A dialogue figure image is not assigned in FigureView.");
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
            ApplyInvestigationFigureLayout(
                image.rectTransform,
                slot,
                GetDialogueFigureVerticalOffset(sprite));
        }

        private static void HideDialogueFigure(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = null;
            image.enabled = false;
        }

        private void ApplyInvestigationFigureLayout(
            RectTransform figureTransform,
            FigureSlot slot,
            float portraitVerticalOffset)
        {
            if (!_matchInvestigationFigureLayout ||
                figureTransform == null ||
                !(figureTransform.parent is RectTransform parentTransform) ||
                _investigationReferenceWidth <= 0f)
            {
                return;
            }

            Vector2 parentSize = parentTransform.rect.size;
            float referenceScale = parentSize.x / _investigationReferenceWidth;
            float portraitHeight = parentSize.y + _investigationHeightExtension * referenceScale;
            float portraitWidth = portraitHeight * _investigationPortraitAspectRatio;
            float verticalPosition =
                (_investigationVerticalOffset + portraitVerticalOffset) * referenceScale;

            Vector2 anchor;
            float horizontalPosition;

            switch (slot)
            {
                case FigureSlot.Left:
                    anchor = new Vector2(0f, 0.5f);
                    horizontalPosition = _investigationHorizontalInset * referenceScale;
                    break;
                case FigureSlot.Right:
                    anchor = new Vector2(1f, 0.5f);
                    horizontalPosition = -_investigationHorizontalInset * referenceScale;
                    break;
                default:
                    anchor = new Vector2(0.5f, 0.5f);
                    horizontalPosition = 0f;
                    break;
            }

            figureTransform.anchorMin = anchor;
            figureTransform.anchorMax = anchor;
            figureTransform.pivot = new Vector2(0.5f, 0.5f);
            figureTransform.anchoredPosition = new Vector2(horizontalPosition, verticalPosition);
            figureTransform.sizeDelta = new Vector2(portraitWidth, portraitHeight);
        }

        private float GetDialogueFigureVerticalOffset(Sprite sprite)
        {
            if (sprite == null)
            {
                return 0f;
            }

            string spriteName = sprite.name;
            if (spriteName.StartsWith("Player") || spriteName.Contains("Granny"))
            {
                return _investigationShortPortraitOffset;
            }

            return spriteName.Contains("Man2")
                ? _investigationTallPortraitOffset
                : 0f;
        }

        private void CacheLayoutParentSize()
        {
            if (!TryGetLayoutParentSize(out _lastLayoutParentSize))
            {
                return;
            }

            _hasLayoutParentSize = true;
        }

        private bool TryGetLayoutParentSize(out Vector2 parentSize)
        {
            if (_rectTransform != null && _rectTransform.parent is RectTransform parentTransform)
            {
                parentSize = parentTransform.rect.size;
                return true;
            }

            parentSize = default;
            return false;
        }

        private bool IsDefeatPresentationActive()
        {
            return _suspicionOverflowDefeatCoroutine != null ||
                   _turnLimitDefeatCoroutine != null ||
                   _suspicionOverflowDefeatSequence != null ||
                   _turnLimitDefeatSequence != null;
        }
    }
}
