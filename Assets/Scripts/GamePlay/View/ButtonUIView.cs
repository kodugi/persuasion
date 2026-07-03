using GamePlay;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUIView : MonoBehaviour
{
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private Button _endPlacementButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _endTurnButton.onClick.AddListener(OnEndTurnButtonClick);
        _endPlacementButton.onClick.AddListener(OnEndPlacementButtonClick);
        TurnManager.Instance.RaiseSetTurnEvent += HandleSetTurnEvent;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEndTurnButtonClick()
    {
        if(TurnManager.Instance.GetTurnState() == TurnState.PlayerIdle)
        {
            TurnManager.Instance.SetTurnState(TurnState.EnemyIdle);
        }
        else if(TurnManager.Instance.GetTurnState() == TurnState.PlayerPlacingContinue)
        {
            TurnManager.Instance.SetTurnState(TurnState.PlayerPlacingEnd);
            TurnManager.Instance.SetTurnState(TurnState.EnemyIdle);
        }
    }

    private void OnEndPlacementButtonClick()
    {
        TurnManager.Instance.SetTurnState(TurnState.PlayerPlacingEnd);
    }

    private void HandleSetTurnEvent(object sender, SetTurnEventArgs e)
    {
        switch (e.turnState)
        {
            case TurnState.PlayerPlacingContinue:
                _endPlacementButton.interactable = true;
                break;
            default:
                _endPlacementButton.interactable = false;
                break;
        }
    }
}
