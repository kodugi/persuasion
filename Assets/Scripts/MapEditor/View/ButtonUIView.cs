using FileUtils;
using GamePlay;
using MapEditor.Model;
using SingletonUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MapEditor
{
    public class ButtonUIView: SelfInitializingMonoBehaviourSingleton<ButtonUIView>
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _importButton;
        [SerializeField] private Button _exportButton;

        protected override bool InitializeCore()
        {
            if (_playButton == null)
            {
                Debug.LogError("playButton is null");
                return false;
            }

            if (_importButton == null)
            {
                Debug.LogError("importButton is null");
                return false;
            }

            if (_exportButton == null)
            {
                Debug.LogError("exportButton is null");
                return false;
            }
            
            _playButton.onClick.AddListener(HandlePlayButtonClick);
            _importButton.onClick.AddListener(HandleImportButtonClick);
            _exportButton.onClick.AddListener(HandleExportButtonClick);
            return true;
        }

        private void HandlePlayButtonClick()
        {
            EditorInfoHolder.SetGameInfo(GameInfoController.Instance.AssembleGameInfo());
            SceneManager.LoadScene("GamePlayScene");
        }

        private void HandleImportButtonClick()
        {
            string json = JsonFileUtils.OpenSingleJsonFile();
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            GameInfo gameInfo = GameInfoSerializer.DeserializeGameInfo(json);
            GameInfoController.Instance.SetGameInfo(gameInfo);
        }

        private void HandleExportButtonClick()
        {
            string json = GameInfoSerializer.SerializeGameInfo(GameInfoController.Instance.AssembleGameInfo());
            JsonFileUtils.SaveJsonFile(json);
        }
    }
}
