using GamePlay;
using SingletonUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MapEditor
{
    public class ButtonUIView: SelfInitializingMonoBehaviourSingleton<ButtonUIView>
    {
        [SerializeField] private Button _playButton;

        protected override bool InitializeCore()
        {
            if (_playButton == null)
            {
                Debug.LogError("playButton is null");
                return false;
            }
            
            _playButton.onClick.AddListener(HandlePlayButtonClick);
            return true;
        }

        private void HandlePlayButtonClick()
        {
            GameInfoHolder.SetGameInfo(GameInfoController.Instance.AssembleGameInfo());
            SceneManager.LoadScene("GamePlayScene");
        }
    }
}