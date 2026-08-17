using SingletonUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopupView : SelfInitializingMonoBehaviourSingleton<GameOverPopupView>
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Button _replayButton;

    [Header("Suspicion Overflow Game Over")]
    [SerializeField] private string _suspicionOverflowTitle = "GAME OVER";
    [SerializeField] private string _suspicionOverflowMessage = "의심도가 한계치를 초과했습니다.";

    [Header("Turn Limit Game Over")]
    [SerializeField] private string _turnLimitTitle = "GAME OVER";
    [SerializeField] private string _turnLimitMessage = "제한된 턴을 모두 사용했습니다.";

    protected override bool InitializeCore()
    {
        if (_titleText == null)
        {
            Debug.LogError("titleText is null");
            return false;
        }

        if (_messageText == null)
        {
            Debug.LogError("messageText is null");
            return false;
        }

        if (_replayButton == null)
        {
            _replayButton = GetComponentInChildren<Button>(true);
        }

        if (_replayButton == null)
        {
            Debug.LogError("replayButton is null");
            return false;
        }

        gameObject.SetActive(false);
        return true;
    }

    public void ShowSuspicionOverflowGameOver()
    {
        _titleText.text = _suspicionOverflowTitle;
        _messageText.text = _suspicionOverflowMessage;
        gameObject.SetActive(true);
        _replayButton.Select();
    }

    public void ShowTurnLimitGameOver()
    {
        _titleText.text = _turnLimitTitle;
        _messageText.text = _turnLimitMessage;
        gameObject.SetActive(true);
        _replayButton.Select();
    }
    
    public void ShowPopup(bool active, string title, string message)
    {
        _titleText.text = title;
        _messageText.text = message;
        gameObject.SetActive(active);
    }

    public void ResetGame()
    {
        gameObject.SetActive(false);
    }
}
