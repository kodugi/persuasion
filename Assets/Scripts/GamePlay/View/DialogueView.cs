using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SingletonUtils;

namespace GamePlay
{
    public sealed class DialogueView : SelfInitializingMonoBehaviourSingleton<DialogueView>
    {
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        private DialogueManager _session;
        private Image _outsideInputBlocker;
        private float _visibleCharacters;
        private int _characterCount;
        private bool _typing;
        private bool _layoutCaptured;
        private RectLayout _originalPanelLayout;
        private RectLayout _originalTextLayout;
        private float _originalFontSize;
        private float _originalLineSpacing;
        private float _originalAspect;
        private AspectRatioFitter _aspectFitter;
        private bool _originalAspectEnabled;
        private Color _originalTextColor;
        private TextAlignmentOptions _originalAlignment;
        private Image _panelBackground;
        private bool _originalBackgroundEnabled;
        private TutorialCalloutLayout _calloutLayout;

        protected override bool InitializeCore()
        {
            if (_dialoguePanel == null || _nextButton == null || _speakerNameText == null ||
                _dialogueText == null || DialogueManager.Instance == null) return false;
            Unsubscribe();
            _session = DialogueManager.Instance;
            _nextButton.onClick.AddListener(OnNextButtonClick);
            _session.RaiseSetDialogueEntryEvent += HandleSetDialogueEntryEvent;
            _session.RaiseDialoguePageEndEvent += HandleDialogueEndEvent;
            EnsureOutsideInputBlocker();
            if (_session.GetCurrentDialogueEntry() is DialogueEntry entry) Present(entry);
            else Hide();
            return true;
        }

        private void Update()
        {
            if (_session == null || !_dialoguePanel.activeSelf) return;
            RefreshOutsideInputBlocker();
            if (_typing)
            {
                _visibleCharacters += Time.unscaledDeltaTime / Mathf.Max(0.001f, _session.CharacterInterval);
                _dialogueText.maxVisibleCharacters = Mathf.Min(_characterCount, (int)_visibleCharacters);
                _typing = _dialogueText.maxVisibleCharacters < _characterCount;
            }
            if (Input.GetKeyDown(KeyCode.Space)) OnNextButtonClick();
        }

        private void OnNextButtonClick()
        {
            if (_session == null || !_dialoguePanel.activeSelf) return;
            if (_typing)
            {
                _typing = false;
                _dialogueText.maxVisibleCharacters = _characterCount;
                return;
            }
            if (_session.CanAdvance) _session.ToNextEntry();
        }

        private void LateUpdate()
        {
            if (_session?.Presentation == DialoguePresentation.Tutorial && _dialoguePanel.activeInHierarchy)
                _calloutLayout?.Refresh();
        }

        private void HandleSetDialogueEntryEvent(object sender, SetDialogueEntryEventArgs e) => Present(e.GetDialogueEntry());
        private void HandleDialogueEndEvent(object sender, DialoguePageEndEventArgs e) => Hide();

        private void Present(DialogueEntry entry)
        {
            if (entry == null || (string.IsNullOrEmpty(entry.DialogueText) && string.IsNullOrEmpty(_session.Instruction)))
            { Hide(); return; }
            _dialoguePanel.SetActive(true);
            ApplyPresentationLayout();
            _speakerNameText.text = entry.SpeakerName;
            _speakerNameText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(entry.SpeakerName));
            _dialogueText.text = entry.DialogueText;
            string instruction = _session.Instruction;
            if (string.IsNullOrEmpty(instruction) && _session.CanAdvance && _session.Presentation == DialoguePresentation.Tutorial)
                instruction = "클릭 / Space로 계속";
            if (!string.IsNullOrEmpty(instruction))
                _dialogueText.text += (string.IsNullOrEmpty(entry.DialogueText) ? "" : "\n") +
                    (_session.Presentation == DialoguePresentation.Tutorial ? "<size=75%><color=#FFFFFFCC>" : "<size=75%><color=#67447E>") +
                    instruction + "</color></size>";
            if (_session.Presentation == DialoguePresentation.Tutorial) _calloutLayout.Refresh();
            _dialogueText.maxVisibleCharacters = int.MaxValue;
            _dialogueText.ForceMeshUpdate();
            _characterCount = _dialogueText.textInfo.characterCount;
            _visibleCharacters = 0;
            _dialogueText.maxVisibleCharacters = 0;
            _typing = _characterCount > 0;
            _nextButton.gameObject.SetActive(_session.CanAdvance);
            // Keep text above tutorial dimming; interactive steps leave the board accessible.
            _dialoguePanel.transform.SetAsLastSibling();
            RefreshOutsideInputBlocker();
        }

        private void ApplyPresentationLayout()
        {
            var panel = (RectTransform)_dialoguePanel.transform;
            var text = _dialogueText.rectTransform;
            if (!_layoutCaptured)
            {
                _originalPanelLayout = new RectLayout(panel);
                _originalTextLayout = new RectLayout(text);
                _originalFontSize = _dialogueText.fontSize;
                _originalLineSpacing = _dialogueText.lineSpacing;
                _aspectFitter = panel.GetComponent<AspectRatioFitter>();
                _originalAspect = _aspectFitter == null ? 0 : _aspectFitter.aspectRatio;
                _originalAspectEnabled = _aspectFitter != null && _aspectFitter.enabled;
                _originalTextColor = _dialogueText.color;
                _originalAlignment = _dialogueText.alignment;
                _panelBackground = panel.GetComponent<Image>();
                _originalBackgroundEnabled = _panelBackground != null && _panelBackground.enabled;
                _calloutLayout = new TutorialCalloutLayout(panel, _dialogueText);
                _layoutCaptured = true;
            }
            if (_session.Presentation == DialoguePresentation.Tutorial)
            {
                if (_aspectFitter != null) _aspectFitter.enabled = false;
                if (_panelBackground != null) _panelBackground.enabled = false;
                text.anchorMin = Vector2.zero;
                text.anchorMax = Vector2.one;
                text.offsetMin = text.offsetMax = Vector2.zero;
                _dialogueText.fontSize = 17;
                _dialogueText.lineSpacing = 10;
                _dialogueText.color = Color.white;
                _dialogueText.alignment = TextAlignmentOptions.TopLeft;
            }
            else
            {
                _originalPanelLayout.Apply(panel);
                _originalTextLayout.Apply(text);
                if (_aspectFitter != null) { _aspectFitter.aspectRatio = _originalAspect; _aspectFitter.enabled = _originalAspectEnabled; }
                if (_panelBackground != null) _panelBackground.enabled = _originalBackgroundEnabled;
                _dialogueText.fontSize = _originalFontSize;
                _dialogueText.lineSpacing = _originalLineSpacing;
                _dialogueText.color = _originalTextColor;
                _dialogueText.alignment = _originalAlignment;
                _calloutLayout.Hide();
            }
            Canvas.ForceUpdateCanvases();
        }

        private readonly struct RectLayout
        {
            private readonly Vector2 _min, _max, _pivot, _position, _size;
            public RectLayout(RectTransform rect)
            {
                _min = rect.anchorMin; _max = rect.anchorMax; _pivot = rect.pivot;
                _position = rect.anchoredPosition; _size = rect.sizeDelta;
            }
            public void Apply(RectTransform rect)
            {
                rect.anchorMin = _min; rect.anchorMax = _max; rect.pivot = _pivot;
                rect.anchoredPosition = _position; rect.sizeDelta = _size;
            }
        }

        public bool ContainsScreenPoint(Vector2 screenPoint)
        {
            if (_dialoguePanel == null || !_dialoguePanel.activeInHierarchy) return false;
            var canvas = _dialoguePanel.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)_dialoguePanel.transform, screenPoint, camera);
        }

        public void Hide()
        {
            _typing = false;
            _calloutLayout?.Hide();
            if (_outsideInputBlocker != null) _outsideInputBlocker.gameObject.SetActive(false);
            if (_dialoguePanel != null) _dialoguePanel.SetActive(false);
        }

        private void Unsubscribe()
        {
            if (_nextButton != null) _nextButton.onClick.RemoveListener(OnNextButtonClick);
            if (_session == null) return;
            _session.RaiseSetDialogueEntryEvent -= HandleSetDialogueEntryEvent;
            _session.RaiseDialoguePageEndEvent -= HandleDialogueEndEvent;
            _session = null;
        }

        protected override void OnDestroy()
        {
            Unsubscribe();
            base.OnDestroy();
        }

        private void EnsureOutsideInputBlocker()
        {
            if (_outsideInputBlocker != null || _dialoguePanel == null)
            {
                return;
            }

            Transform dialogueParent = _dialoguePanel.transform.parent;
            if (dialogueParent == null)
            {
                Debug.LogWarning("Dialogue panel needs a parent for its outside input blocker.", this);
                return;
            }

            GameObject blockerObject = new GameObject(
                "Dialogue Outside Input Blocker",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            blockerObject.layer = _dialoguePanel.layer;
            blockerObject.transform.SetParent(dialogueParent, false);

            _outsideInputBlocker = blockerObject.GetComponent<Image>();
            _outsideInputBlocker.color = Color.clear;
            _outsideInputBlocker.raycastTarget = true;

            RectTransform blockerRect = _outsideInputBlocker.rectTransform;
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;

            _dialoguePanel.transform.SetAsLastSibling();
        }

        private void RefreshOutsideInputBlocker()
        {
            EnsureOutsideInputBlocker();
            if (_outsideInputBlocker == null)
            {
                return;
            }

            bool shouldBlock = _dialoguePanel.activeSelf &&
                               DialogueManager.Instance != null &&
                               DialogueManager.Instance.ShouldBlockInteractionOutsideDialogue();
            _outsideInputBlocker.gameObject.SetActive(shouldBlock);
            if (shouldBlock)
            {
                _outsideInputBlocker.transform.SetSiblingIndex(_dialoguePanel.transform.GetSiblingIndex());
                _dialoguePanel.transform.SetAsLastSibling();
            }
        }
    }
}
