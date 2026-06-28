using GamePlay;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUIView : MonoBehaviour
{
    [SerializeField] private Button _endPlacementButton;
    private TurnManager _turnManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _endPlacementButton.onClick.AddListener(OnEndPlacementButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEndPlacementButtonClick()
    {
        if(TurnManager.Instance.GetTurnState() == TurnState.PlayerIdle)
        {
            TurnManager.Instance.SetTurnState(TurnState.EnemyIdle);
        }
    }
}
