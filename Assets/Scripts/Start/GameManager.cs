using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Start
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _exitButton;

        private void Start()
        {
            _startButton.onClick.AddListener(StartGame);
            _continueButton.onClick.AddListener(ContinueGame);
            _exitButton.onClick.AddListener(Exit);
        }

        private void StartGame()
        {
            SaveManager.ResetAllSaveData();
            ContinueGame();
        }

        private void ContinueGame()
        {
            SceneManager.LoadScene("Investigation");
        }

        private void Exit()
        {
            Application.Quit();
        }
    }
}
