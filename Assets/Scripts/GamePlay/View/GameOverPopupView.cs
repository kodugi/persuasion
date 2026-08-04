using SingletonUtils;
using TMPro;
using UnityEngine;

public class GameOverPopupView : SelfInitializingMonoBehaviourSingleton<GameOverPopupView>
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _messageText;

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

        gameObject.SetActive(false);
        return true;
    }
    
    public void ShowPopup(bool active, string title, string message)
    {
        _titleText.text = title;
        _messageText.text = message;
        gameObject.SetActive(active);
    }
}
