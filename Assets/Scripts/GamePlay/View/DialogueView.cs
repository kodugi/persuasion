using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            if (DialogueManager.Instance.GetCurrentDialogueData() != null)
            {
                DialogueManager.Instance.SetDialoguePage(0);
            }
            return true;
        }

        private void OnNextButtonClick()
        {
            DialogueManager.Instance.ToNextEntry();
        }

        private void HandleSetDialogueEntryEvent(object sender, SetDialogueEntryEventArgs e)
        {
            _dialoguePanel.SetActive(true);
            _speakerNameText.text = e.GetDialogueEntry().SpeakerName;
            _dialogueText.text = e.GetDialogueEntry().DialogueText;
        }

        private void HandleDialogueEndEvent(object sender, EventArgs e)
        {
            _dialoguePanel.SetActive(false);
        }
    }
}