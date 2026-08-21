using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Investigation
{
    /// <summary>
    /// Configuration for a sprite animation that is shown while a specific dialogue line is visible.
    /// The runtime player is installed from Resources, so scenes and dialogue prefabs do not need
    /// to reference this feature directly.
    /// </summary>
    public sealed class DialogueLineSpriteAnimation : ScriptableObject
    {
        private const string ResourceName = "Map1IntroEyeAnimation";

        [SerializeField] private string sceneName = "Investigation";
        [SerializeField, TextArea] private string triggerLine;
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.01f)] private float secondsPerFrame = 0.12f;
        [SerializeField] private Vector2 referenceResolution = new Vector2(800f, 600f);
        [SerializeField, Min(1f)] private float displayedCanvasSize = 600f;
        [SerializeField] private int sortingOrder = 1;

        public string SceneName => sceneName;
        public string TriggerLine => triggerLine;
        public Sprite[] Frames => frames;
        public float SecondsPerFrame => secondsPerFrame;
        public Vector2 ReferenceResolution => referenceResolution;
        public float DisplayedCanvasSize => displayedCanvasSize;
        public int SortingOrder => sortingOrder;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallPlayer()
        {
            if (FindFirstObjectByType<DialogueLineSpriteAnimationPlayer>() != null)
            {
                return;
            }

            DialogueLineSpriteAnimation configuration =
                Resources.Load<DialogueLineSpriteAnimation>(ResourceName);
            if (configuration == null)
            {
                return;
            }

            GameObject playerObject = new GameObject(
                "Map1 Intro Eye Animation",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler));

            DontDestroyOnLoad(playerObject);
            playerObject.AddComponent<DialogueLineSpriteAnimationPlayer>()
                .Initialize(configuration);
        }
    }

    /// <summary>
    /// Watches rendered dialogue text instead of depending on Inv_DialogueBox internals.
    /// This keeps the effect isolated from the shared investigation scene and dialogue system.
    /// </summary>
    internal sealed class DialogueLineSpriteAnimationPlayer : MonoBehaviour
    {
        private DialogueLineSpriteAnimation configuration;
        private Image frameImage;
        private TMP_Text activeTriggerText;
        private Coroutine animationCoroutine;
        private bool triggerWasVisible;

        public void Initialize(DialogueLineSpriteAnimation newConfiguration)
        {
            configuration = newConfiguration;

            Canvas canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = configuration.SortingOrder;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = configuration.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            GameObject imageObject = new GameObject(
                "Eye Animation Frame",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.transform.SetParent(transform, false);

            frameImage = imageObject.GetComponent<Image>();
            frameImage.raycastTarget = false;
            frameImage.enabled = false;

            RectTransform imageTransform = frameImage.rectTransform;
            imageTransform.anchorMin = new Vector2(0.5f, 0.5f);
            imageTransform.anchorMax = new Vector2(0.5f, 0.5f);
            imageTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private void Update()
        {
            if (configuration == null ||
                !string.Equals(
                    SceneManager.GetActiveScene().name,
                    configuration.SceneName,
                    StringComparison.Ordinal))
            {
                ResetTrigger();
                return;
            }

            // Once triggered, keep the final frame visible while this dialogue box exists.
            // The TMP component is destroyed together with the dialogue box at the end of
            // the intro, which also gives this isolated effect a reliable cleanup point.
            if (triggerWasVisible)
            {
                if (activeTriggerText != null && activeTriggerText.isActiveAndEnabled)
                {
                    return;
                }

                ResetTrigger();
                return;
            }

            bool triggerIsVisible = TryFindTriggerText(out TMP_Text triggerText);
            if (triggerIsVisible)
            {
                activeTriggerText = triggerText;
                triggerWasVisible = true;
                PlayAnimation();
            }
        }

        private bool TryFindTriggerText(out TMP_Text triggerText)
        {
            if (IsTriggerText(activeTriggerText))
            {
                triggerText = activeTriggerText;
                return true;
            }

            activeTriggerText = null;
            TMP_Text[] visibleTexts = FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            foreach (TMP_Text text in visibleTexts)
            {
                if (IsTriggerText(text))
                {
                    triggerText = text;
                    return true;
                }
            }

            triggerText = null;
            return false;
        }

        private bool IsTriggerText(TMP_Text text)
        {
            return text != null &&
                   text.isActiveAndEnabled &&
                   string.Equals(text.text, configuration.TriggerLine, StringComparison.Ordinal);
        }

        private void PlayAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(PlayFrames());
        }

        private IEnumerator PlayFrames()
        {
            Sprite[] frames = configuration.Frames;
            if (frames == null || frames.Length == 0)
            {
                yield break;
            }

            frameImage.enabled = true;

            for (int i = 0; i < frames.Length; i++)
            {
                SetFrame(frames[i]);

                if (i < frames.Length - 1)
                {
                    yield return new WaitForSecondsRealtime(configuration.SecondsPerFrame);
                }
            }

            // Keep the opened eye visible until the dialogue box itself is closed.
            animationCoroutine = null;
        }

        private void SetFrame(Sprite frame)
        {
            if (frame == null)
            {
                frameImage.enabled = false;
                return;
            }

            frameImage.enabled = true;
            frameImage.sprite = frame;

            Rect spriteRect = frame.rect;
            Texture texture = frame.texture;
            float scale = configuration.DisplayedCanvasSize /
                          Mathf.Max(texture.width, texture.height);

            RectTransform imageTransform = frameImage.rectTransform;
            imageTransform.sizeDelta = spriteRect.size * scale;
            imageTransform.anchoredPosition =
                (spriteRect.center - new Vector2(texture.width, texture.height) * 0.5f) * scale;
        }

        private void ResetTrigger()
        {
            activeTriggerText = null;

            if (!triggerWasVisible && !frameImage.enabled)
            {
                return;
            }

            triggerWasVisible = false;
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            frameImage.enabled = false;
            frameImage.sprite = null;
        }
    }
}
