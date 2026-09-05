using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Start
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _puzzleOnlyButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private AudioClip _buttonClickSound;
        [SerializeField, Range(0f, 1f)] private float _buttonClickVolume = 0.75f;

        private AudioSource _soundEffectAudioSource;

        private void Awake()
        {
            _soundEffectAudioSource = gameObject.AddComponent<AudioSource>();
            _soundEffectAudioSource.playOnAwake = false;
            _soundEffectAudioSource.spatialBlend = 0f;
        }

        private void Start()
        {
            _startButton.onClick.AddListener(StartGame);
            _puzzleOnlyButton.onClick.AddListener(StartPuzzleOnlyGame);
            _continueButton.onClick.AddListener(ContinueGame);
            _exitButton.onClick.AddListener(Exit);

            foreach (Button button in FindObjectsByType<Button>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                button.onClick.AddListener(PlayButtonClickSound);
            }
        }

        private void PlayButtonClickSound()
        {
            if (_buttonClickSound != null)
            {
                _soundEffectAudioSource.PlayOneShot(_buttonClickSound, _buttonClickVolume);
            }
        }

        private void StartGame()
        {
            SaveManager.ResetAllSaveData();
            ContinueGame();
        }

        private void StartPuzzleOnlyGame()
        {
            SaveManager.ResetAllSaveData();
            SceneManager.LoadScene("UniconTemp_PuzzleOnly");
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
