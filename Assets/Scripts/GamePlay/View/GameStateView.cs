using GamePlay;
using DG.Tweening;
using SingletonUtils;
using TMPro;
using UnityEngine;

public class GameStateView : SelfInitializingMonoBehaviourSingleton<GameStateView>
{
    private const int CriticalRemainingTurn = 1;

    [SerializeField] private TextMeshProUGUI _currentTurnText;
    [SerializeField] private TextMeshProUGUI _targetNumText;
    [SerializeField] private TextMeshProUGUI _currentStageText;
    [SerializeField] private float _turnSlotOffset = 70f;
    [SerializeField] private float _turnSlotDuration = 0.55f;
    [SerializeField] private Color _criticalTurnColor = new Color(1f, 0.08f, 0.05f, 1f);
    [SerializeField] private float _criticalShakeDuration = 0.28f;
    [SerializeField] private float _criticalShakeStrength = 6f;
    [SerializeField] private int _criticalShakeVibrato = 14;

    private RectTransform _currentTurnRect;
    private Vector2 _currentTurnOrigin;
    private Color _normalTurnColor;
    private int _displayedRemainingTurn = -1;
    private TextMeshProUGUI _outgoingTurnText;
    private Sequence _turnSlotSequence;
    private Tween _criticalShakeTween;

    protected override bool InitializeCore()
    {
        if (_currentTurnText == null)
        {
            Debug.LogError("currentTurnText is null");
            return false;
        }

        if (_targetNumText == null)
        {
            Debug.LogError("targetNumText is null");
            return false;
        }

        if (_currentStageText == null)
        {
            Debug.LogError("currentStageText is null");
            return false;
        }

        if (TurnManager.Instance == null)
        {
            Debug.LogError("TurnManager is null");
            return false;
        }

        if (BoardController.Instance == null)
        {
            Debug.LogError("BoardController is null");
            return false;
        }
        if (GameInfoHolder.GetCurrentGameInfo() == null)
        {
            Debug.LogError("GameInfo is null");
            return false;
        }

        _currentTurnRect = _currentTurnText.rectTransform;
        _currentTurnOrigin = _currentTurnRect.anchoredPosition;
        _normalTurnColor = _currentTurnText.color;

        TurnManager.Instance.RaiseSetTurnEvent += HandleSetTurnEvent;
        BoardController.Instance.RaiseCellPlacementEvent += HandleCellPlacementEvent;
        SetCurrentTurnText(0, false);
        SetTargetNumText();
        SetCurrentStageText();
        return true;
    }

    protected override void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RaiseSetTurnEvent -= HandleSetTurnEvent;
        }

        if (BoardController.Instance != null)
        {
            BoardController.Instance.RaiseCellPlacementEvent -= HandleCellPlacementEvent;
        }

        KillTurnSlotSequence();
        StopCriticalShake();
        base.OnDestroy();
    }

    private void HandleSetTurnEvent(object sender, SetTurnEventArgs e)
    {
        SetCurrentTurnText(e.CurrentTurn, true);
        SetTargetNumText();
    }

    private void HandleCellPlacementEvent(object sender, CellPlacementEventArgs e)
    {
        SetTargetNumText();
    }

    public void ResetGame()
    {
        if (!IsInitialized)
        {
            return;
        }

        SetCurrentTurnText(0, false);
        SetTargetNumText();
        SetCurrentStageText();
    }

    private void SetCurrentTurnText(int currentTurn, bool animate)
    {
        int remainingTurn = Mathf.Max(0, GameInfoHolder.GetCurrentGameInfo().GetMaxTurns() - currentTurn);

        if (!animate || _displayedRemainingTurn < 0)
        {
            SetCurrentTurnTextImmediately(remainingTurn);
            return;
        }

        if (_displayedRemainingTurn == remainingTurn)
        {
            ApplyTurnWarningState(remainingTurn);
            return;
        }

        PlayTurnSlotAnimation(_displayedRemainingTurn, remainingTurn);
        _displayedRemainingTurn = remainingTurn;
    }

    private void SetCurrentTurnTextImmediately(int remainingTurn)
    {
        KillTurnSlotSequence();
        StopCriticalShake();

        _displayedRemainingTurn = remainingTurn;
        _currentTurnText.text = remainingTurn.ToString();
        _currentTurnText.color = GetTurnColor(remainingTurn);
        _currentTurnText.alpha = 1f;
        _currentTurnRect.anchoredPosition = _currentTurnOrigin;

        ApplyTurnWarningState(remainingTurn);
    }

    private void PlayTurnSlotAnimation(int previousRemainingTurn, int nextRemainingTurn)
    {
        KillTurnSlotSequence();
        StopCriticalShake();

        TextMeshProUGUI outgoingText = CreateOutgoingTurnText(previousRemainingTurn);
        RectTransform outgoingRect = outgoingText.rectTransform;
        Vector2 aboveOrigin = _currentTurnOrigin + Vector2.up * _turnSlotOffset;
        Vector2 belowOrigin = _currentTurnOrigin + Vector2.down * _turnSlotOffset;

        _currentTurnText.text = nextRemainingTurn.ToString();
        _currentTurnText.color = GetTurnColor(nextRemainingTurn);
        _currentTurnText.alpha = 1f;
        _currentTurnRect.anchoredPosition = aboveOrigin;

        Sequence sequence = DOTween.Sequence();
        sequence.SetTarget(this);
        sequence.Join(outgoingRect.DOAnchorPos(belowOrigin, _turnSlotDuration).SetEase(Ease.InOutCubic));
        sequence.Join(_currentTurnRect.DOAnchorPos(_currentTurnOrigin, _turnSlotDuration).SetEase(Ease.OutCubic));
        sequence.Join(outgoingText.DOFade(0f, _turnSlotDuration).SetEase(Ease.InQuad));
        sequence.OnComplete(() =>
        {
            _currentTurnText.text = nextRemainingTurn.ToString();
            _currentTurnText.color = GetTurnColor(nextRemainingTurn);
            _currentTurnText.alpha = 1f;
            _currentTurnRect.anchoredPosition = _currentTurnOrigin;
            ApplyTurnWarningState(nextRemainingTurn);
        });
        sequence.OnKill(() =>
        {
            DestroyOutgoingTurnText();
            if (_turnSlotSequence == sequence)
            {
                _turnSlotSequence = null;
            }
        });

        _turnSlotSequence = sequence;
    }

    private TextMeshProUGUI CreateOutgoingTurnText(int remainingTurn)
    {
        _outgoingTurnText = Instantiate(_currentTurnText, _currentTurnText.transform.parent, false);
        _outgoingTurnText.name = _currentTurnText.name + "_Outgoing";
        _outgoingTurnText.text = remainingTurn.ToString();
        _outgoingTurnText.color = GetTurnColor(remainingTurn);
        _outgoingTurnText.alpha = 1f;
        _outgoingTurnText.raycastTarget = false;
        _outgoingTurnText.rectTransform.anchoredPosition = _currentTurnOrigin;
        _outgoingTurnText.transform.SetSiblingIndex(_currentTurnText.transform.GetSiblingIndex() + 1);
        return _outgoingTurnText;
    }

    private Color GetTurnColor(int remainingTurn)
    {
        return remainingTurn == CriticalRemainingTurn ? _criticalTurnColor : _normalTurnColor;
    }

    private void ApplyTurnWarningState(int remainingTurn)
    {
        _currentTurnText.color = GetTurnColor(remainingTurn);

        if (remainingTurn == CriticalRemainingTurn)
        {
            StartCriticalShake();
        }
        else
        {
            StopCriticalShake();
        }
    }

    private void StartCriticalShake()
    {
        if (_criticalShakeTween != null && _criticalShakeTween.IsActive())
        {
            return;
        }

        _currentTurnRect.anchoredPosition = _currentTurnOrigin;
        _criticalShakeTween = _currentTurnRect
            .DOShakeAnchorPos(_criticalShakeDuration, _criticalShakeStrength, _criticalShakeVibrato, 90f, false, true)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StopCriticalShake()
    {
        if (_criticalShakeTween != null && _criticalShakeTween.IsActive())
        {
            _criticalShakeTween.Kill();
        }

        _criticalShakeTween = null;

        if (_currentTurnRect != null)
        {
            _currentTurnRect.anchoredPosition = _currentTurnOrigin;
        }
    }

    private void KillTurnSlotSequence()
    {
        if (_turnSlotSequence != null && _turnSlotSequence.IsActive())
        {
            _turnSlotSequence.Kill();
        }

        _turnSlotSequence = null;
        DestroyOutgoingTurnText();

        if (_currentTurnRect != null)
        {
            _currentTurnRect.anchoredPosition = _currentTurnOrigin;
        }

        if (_currentTurnText != null)
        {
            _currentTurnText.alpha = 1f;
        }
    }

    private void DestroyOutgoingTurnText()
    {
        if (_outgoingTurnText == null)
        {
            return;
        }

        Destroy(_outgoingTurnText.gameObject);
        _outgoingTurnText = null;
    }

    private void SetTargetNumText()
    {
        if (GameInfoHolder.GetCurrentGameInfo().GetTargetNumber() == 0)
        {
            _targetNumText.text = "0/∞";
        }
        else
        {
            _targetNumText.text = BoardController.Instance.GetConvertedBlackCellCount() + "/" + GameInfoHolder.GetCurrentGameInfo().GetTargetNumber();
        }
    }

    private void SetCurrentStageText()
    {
        GameInfo gameInfo = GameInfoHolder.GetCurrentGameInfo();
        _currentStageText.text = gameInfo.GetStageNum() + "/" + gameInfo.GetTotalStageNum();
    }
}
