using GamePlay;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUIView : MonoBehaviourSingleton<ButtonUIView>
{
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private Button _endPlacementButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _endTurnButton.onClick.AddListener(OnEndTurnButtonClick);
        _endPlacementButton.onClick.AddListener(OnEndPlacementButtonClick);
        TurnManager.Instance.RaiseSetTurnStateEvent += HandleSetTurnStateEvent;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEndTurnButtonClick()
    {
        if (TutorialController.Instance != null && !TutorialController.Instance.CanClickEndTurn())
        {
            return;
        }

        if(TurnManager.Instance.GetTurnState() == TurnState.PlayerIdle)
        {
            TurnManager.Instance.SetTurnState(TurnState.EnemyIdle);
            TutorialController.Instance?.NotifyEndTurnClicked();
        }
        else if(TurnManager.Instance.GetTurnState() == TurnState.PlayerPlacingContinue)
        {
            TurnManager.Instance.SetTurnState(TurnState.PlayerPlacingEnd);
            TurnManager.Instance.SetTurnState(TurnState.EnemyIdle);
            TutorialController.Instance?.NotifyEndTurnClicked();
        }
    }

    private void OnEndPlacementButtonClick()
    {
        if (TutorialController.Instance != null && !TutorialController.Instance.CanClickEndPlacement())
        {
            return;
        }

        TurnManager.Instance.SetTurnState(TurnState.PlayerPlacingEnd);
    }

    private void HandleSetTurnStateEvent(object sender, SetTurnStateEventArgs e)
    {
        if (TurnManager.Instance.GetTurnState() != e.turnState)
        {
            return;
        }

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

    public RectTransform GetEndTurnButtonTarget()
    {
        return _endTurnButton == null ? null : _endTurnButton.GetComponent<RectTransform>();
    }

    public RectTransform GetEndPlacementButtonTarget()
    {
        return _endPlacementButton == null ? null : _endPlacementButton.GetComponent<RectTransform>();
    }
}
