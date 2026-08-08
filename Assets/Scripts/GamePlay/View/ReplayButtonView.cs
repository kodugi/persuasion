using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    public class ReplayButtonView: MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>()?.onClick.AddListener(HandleReplayButtonClick);
        }

        private void HandleReplayButtonClick()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("ReplayButtonView could not reset because GameManager is unavailable.", this);
                return;
            }

            GameManager.Instance.ResetGame();
        }
    }
}
