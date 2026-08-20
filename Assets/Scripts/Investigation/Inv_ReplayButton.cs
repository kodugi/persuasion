using UnityEngine;
using UnityEngine.UI;

namespace Investigation
{
    public class Inv_ReplayButton: MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>()?.onClick.AddListener(HandleReplayButtonClick);
        }

        private void HandleReplayButtonClick()
        {
            if (ChiefManager.Instance == null)
            {
                Debug.LogWarning("ReplayButtonView could not reset because GameManager is unavailable.", this);
                return;
            }

            ChiefManager.Instance.ResetGame();
        }
    }
}
