using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SingletonUtils;

namespace GamePlay
{
    [Serializable]
    public class DialoguePanelLayout
    {
        [SerializeField] private Sprite _panelSprite;
        [SerializeField] private Vector2 _panelSizeDelta;
        [SerializeField] private bool _overridePanelAnchoredPosition;
        [SerializeField] private Vector2 _panelAnchoredPosition;
        [SerializeField] private Vector2 _dialogueTextSizeDelta;

        public static DialoguePanelLayout Capture(
            Image panelImage,
            RectTransform panelRectTransform,
            RectTransform dialogueTextRectTransform)
        {
            DialoguePanelLayout layout = new DialoguePanelLayout();
            if (panelImage != null)
            {
                layout._panelSprite = panelImage.sprite;
            }

            if (panelRectTransform != null)
            {
                layout._panelSizeDelta = panelRectTransform.sizeDelta;
                layout._overridePanelAnchoredPosition = true;
                layout._panelAnchoredPosition = panelRectTransform.anchoredPosition;
            }

            if (dialogueTextRectTransform != null)
            {
                layout._dialogueTextSizeDelta = dialogueTextRectTransform.sizeDelta;
            }

            return layout;
        }

        public bool HasOverrides()
        {
            return _panelSprite != null ||
                   _panelSizeDelta != Vector2.zero ||
                   _overridePanelAnchoredPosition ||
                   _dialogueTextSizeDelta != Vector2.zero;
        }

        public void Apply(
            Image panelImage,
            RectTransform panelRectTransform,
            RectTransform dialogueTextRectTransform,
            DialoguePanelLayout fallbackLayout)
        {
            Sprite panelSprite = _panelSprite;
            if (panelSprite == null && fallbackLayout != null)
            {
                panelSprite = fallbackLayout._panelSprite;
            }

            if (panelImage != null && panelSprite != null)
            {
                panelImage.sprite = panelSprite;
            }

            Vector2 panelSizeDelta = ResolveVector(_panelSizeDelta, fallbackLayout == null ? Vector2.zero : fallbackLayout._panelSizeDelta);
            if (panelRectTransform != null && panelSizeDelta != Vector2.zero)
            {
                panelRectTransform.sizeDelta = panelSizeDelta;
            }

            bool shouldApplyPanelAnchoredPosition = _overridePanelAnchoredPosition;
            Vector2 panelAnchoredPosition = _panelAnchoredPosition;
            if (!shouldApplyPanelAnchoredPosition && fallbackLayout != null)
            {
                shouldApplyPanelAnchoredPosition = fallbackLayout._overridePanelAnchoredPosition;
                panelAnchoredPosition = fallbackLayout._panelAnchoredPosition;
            }

            if (panelRectTransform != null && shouldApplyPanelAnchoredPosition)
            {
                panelRectTransform.anchoredPosition = panelAnchoredPosition;
            }

            Vector2 dialogueTextSizeDelta = ResolveVector(
                _dialogueTextSizeDelta,
                fallbackLayout == null ? Vector2.zero : fallbackLayout._dialogueTextSizeDelta);
            if (dialogueTextRectTransform != null && dialogueTextSizeDelta != Vector2.zero)
            {
                dialogueTextRectTransform.sizeDelta = dialogueTextSizeDelta;
            }
        }

        private static Vector2 ResolveVector(Vector2 value, Vector2 fallbackValue)
        {
            return value == Vector2.zero ? fallbackValue : value;
        }
    }

    public class DialogueView: SelfInitializingMonoBehaviourSingleton<DialogueView>
    {
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private Image _dialoguePanelImage;
        [SerializeField] private RectTransform _dialoguePanelRectTransform;
        [SerializeField] private RectTransform _dialogueTextRectTransform;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private DialoguePanelLayout _widePanelLayout = new DialoguePanelLayout();

        private Coroutine _typeDialogueEntry;
        private string _dialogueContent;
        private DialoguePanelLayout _normalPanelLayout;
        
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

            if (!CachePanelReferences())
            {
                return false;
            }

            _normalPanelLayout = DialoguePanelLayout.Capture(
                _dialoguePanelImage,
                _dialoguePanelRectTransform,
                _dialogueTextRectTransform);

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
            ApplyPanelLayout(dialogueEntry.UseWidePanel);
            _dialoguePanel.SetActive(true);
            _dialogueContent = dialogueEntry.DialogueText;
            _speakerNameText.text = dialogueEntry.SpeakerName;
            _typeDialogueEntry = StartCoroutine(TypeDialogueEntry(_dialogueContent));
        }

        private IEnumerator TypeDialogueEntry(string dialogueContent)
        {
            _dialogueText.text = "";
            foreach (char c in dialogueContent)
            {
                _dialogueText.text += c;
                yield return new WaitForSeconds(0.02f);
            }

            _typeDialogueEntry = null;
        }

        private bool CachePanelReferences()
        {
            if (_dialoguePanelRectTransform == null)
            {
                _dialoguePanelRectTransform = _dialoguePanel.GetComponent<RectTransform>();
            }

            if (_dialoguePanelImage == null)
            {
                _dialoguePanelImage = _dialoguePanel.GetComponent<Image>();
            }

            if (_dialogueTextRectTransform == null)
            {
                _dialogueTextRectTransform = _dialogueText.rectTransform;
            }

            if (_dialoguePanelRectTransform == null)
            {
                Debug.LogError("Dialogue panel rect transform is null");
                return false;
            }

            if (_dialoguePanelImage == null)
            {
                Debug.LogError("Dialogue panel image is null");
                return false;
            }

            if (_dialogueTextRectTransform == null)
            {
                Debug.LogError("Dialogue text rect transform is null");
                return false;
            }

            return true;
        }

        private void ApplyPanelLayout(bool useWidePanel)
        {
            DialoguePanelLayout panelLayout = _normalPanelLayout;
            if (useWidePanel && _widePanelLayout != null && _widePanelLayout.HasOverrides())
            {
                panelLayout = _widePanelLayout;
            }

            if (panelLayout != null)
            {
                panelLayout.Apply(
                    _dialoguePanelImage,
                    _dialoguePanelRectTransform,
                    _dialogueTextRectTransform,
                    _normalPanelLayout);
            }
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
