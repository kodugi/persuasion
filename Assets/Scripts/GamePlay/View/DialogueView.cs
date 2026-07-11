using System;
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

        private void OnNextButtonClick()
        {
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
            if (dialogueEntry == null)
            {
                Hide();
                return;
            }

            _dialoguePanel.SetActive(true);
            _speakerNameText.text = dialogueEntry.SpeakerName;
            _dialogueText.text = dialogueEntry.DialogueText;
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

            _dialoguePanel.SetActive(false);
        }
    }
}
