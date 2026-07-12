using MapEditor;
using MapEditor.Model;
using SingletonUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GamePlay
{
    public class EditorReturnButtonView: SelfInitializingMonoBehaviourSingleton<EditorReturnButtonView>
    {
        protected override bool InitializeCore()
        {
            if (gameObject.GetComponent<Button>() == null)
            {
                Debug.LogError("gameobject does not have a button");
                return false;
            }
            
            if (EditorInfoHolder.GetGameInfo() == null)
            {
                gameObject.SetActive(false);
                return true;
            }
            
            gameObject.SetActive(true);
            gameObject.GetComponent<Button>().onClick.AddListener(HandleButtonClick);
            return true;
        }

        private void HandleButtonClick()
        {
            EditorInfoHolder.SetGameInfo(GameInfoController.Instance.AssembleGameInfo());
            SceneManager.LoadScene("EditorScene");
        }
    }
}