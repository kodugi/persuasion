using GamePlay;
using SingletonUtils;
using TMPro;
using UnityEngine;

public class GameStateView : SelfInitializingMonoBehaviourSingleton<GameStateView>
{
    [SerializeField] private TextMeshProUGUI _currentTurnText;
    [SerializeField] private TextMeshProUGUI _targetNumText;
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
        if (GameInfoHolder.GetGameInfo() == null)
        {
            Debug.LogError("GameInfo is null");
            return false;
        }

        TurnManager.Instance.RaiseSetTurnEvent += HandleSetTurnEvent;
        BoardController.Instance.RaiseCellPlacementEvent += HandleCellPlacementEvent;
        SetCurrentTurnText(0);
        SetTargetNumText();
        return true;
    }

    private void HandleSetTurnEvent(object sender, SetTurnEventArgs e)
    {
        SetCurrentTurnText(e.CurrentTurn);
    }

    private void HandleCellPlacementEvent(object sender, CellPlacementEventArgs e)
    {
        SetTargetNumText();
    }

    private void SetCurrentTurnText(int currentTurn)
    {
        _currentTurnText.text = "현재 턴 수: " + currentTurn.ToString() + "/" + GameInfoHolder.GetGameInfo().GetMaxTurns();
    }

    private void SetTargetNumText()
    {
        _targetNumText.text = "목표: " + BoardController.Instance.GetConvertedBlackCellCount().ToString() + "/" + GameInfoHolder.GetGameInfo().GetTargetNumber().ToString();
    }
}
