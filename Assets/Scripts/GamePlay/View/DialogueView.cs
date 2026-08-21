using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SingletonUtils;

namespace GamePlay
{
    public class DialogueView: SelfInitializingMonoBehaviourSingleton<DialogueView>
    {
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;

        private Coroutine _typeDialogueEntry;
        private string _dialogueContent;
        
        protected override bool InitializeCore()
        {
            if (_dialoguePanel == null)
            {
                Debug.LogError("Dialogue panel is null");
                return false;
            }

            if (_nextButton == null)
            {
                Debug.LogError("Next button is null");
                return false;
            }

            if (_speakerNameText == null)
            {
                Debug.LogError("Speaker name is null");
                return false;
            }

            if (_dialogueText == null)
            {
                Debug.LogError("Dialogue text is null");
                return false;
            }

            if (DialogueManager.Instance == null)
            {
                Debug.LogError("Dialogue Manager is null");
                return false;
            }
            _nextButton.onClick.AddListener(OnNextButtonClick);
            DialogueManager.Instance.RaiseSetDialogueEntryEvent += HandleSetDialogueEntryEvent;
            DialogueManager.Instance.RaiseDialoguePageEndEvent += HandleDialogueEndEvent;

            Hide();

            if (!DialogueManager.Instance.HasDialogueData())
            {
                return true;
            }

            if (DialogueManager.Instance.HasCurrentDialogueData())
            {
                DialogueManager.Instance.SetDialoguePage(0);
            }
            return true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnNextButtonClick();
            }
        }

        private void OnNextButtonClick()
        {
            if (_typeDialogueEntry != null)
            {
                StopTypeDialogueEntry();
                _dialogueText.text = _dialogueContent;
                return;
            }
            
            if (!DialogueManager.Instance.HasCurrentDialogueData())
            {
                Hide();
                return;
            }

            DialogueManager.Instance.ToNextEntry();
        }

        private void HandleSetDialogueEntryEvent(object sender, SetDialogueEntryEventArgs e)
        {
            DialogueEntry dialogueEntry = e.GetDialogueEntry();
            if (dialogueEntry == null || dialogueEntry.DialogueText == "")
            {
                Hide();
                return;
            }
            StopTypeDialogueEntry();
            _dialoguePanel.SetActive(true);
            _dialogueContent = dialogueEntry.DialogueText;
            _speakerNameText.text = dialogueEntry.SpeakerName;
            _speakerNameText.transform.parent.gameObject.SetActive(
                !string.IsNullOrEmpty(dialogueEntry.SpeakerName));
            if (dialogueEntry.StateToTrigger == TutorialState.Dream2)
            {
                _typeDialogueEntry = StartCoroutine(TypeDialogueEntry(_dialogueContent, 0.005f));
            }
            else
            {
                _typeDialogueEntry = StartCoroutine(TypeDialogueEntry(_dialogueContent));
            }
        }

        private IEnumerator TypeDialogueEntry(string dialogueContent, float interval = 0.02f)
        {
            _dialogueText.text = "";
            if (interval <= 0.01f)
            {
                int i;
                for (i = 0; i < dialogueContent.Length; i += 10)
                {
                    _dialogueText.text += dialogueContent.Substring(i, 10);
                    yield return new WaitForSeconds(interval * 10);
                }

                if (i < dialogueContent.Length)
                {
                    _dialogueText.text += dialogueContent.Substring(i);
                }
            }
            else
            {
                foreach (char c in dialogueContent)
                {
                    _dialogueText.text += c;
                    yield return new WaitForSeconds(0.02f);
                }
            }
            

            _typeDialogueEntry = null;
        }

        private void StopTypeDialogueEntry()
        {
            if (_typeDialogueEntry == null)
            {
                return;
            }

            StopCoroutine(_typeDialogueEntry);
            _typeDialogueEntry = null;
        }

        private void HandleDialogueEndEvent(object sender, EventArgs e)
        {
            Hide();
        }

        public void Hide()
        {
            if (_dialoguePanel == null)
            {
                return;
            }

            StopTypeDialogueEntry();
            _dialoguePanel.SetActive(false);
        }
    }
}
