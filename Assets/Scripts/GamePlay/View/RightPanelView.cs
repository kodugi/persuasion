using System.Collections;
using System.Collections.Generic;
using SingletonUtils;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    public class RightPanelView : SelfInitializingMonoBehaviourSingleton<RightPanelView>
    {
        private const string SuspicionOverflowAnimationLayerName = "Suspicion Overflow Animation Layer";

        [SerializeField] private GameObject _suspicionOverflowAnimationPrefab;
        [SerializeField] private int _suspicionOverflowAnimationCount = 2;
        [SerializeField] private int _gameOverAnimationCount = 25;
        [SerializeField] private Vector2 _suspicionOverflowAnimationPadding = new Vector2(24f, 24f);
        [SerializeField] private Vector2 _fallbackAnimationSize = new Vector2(72f, 72f);
        [SerializeField] private Color _fallbackAnimationColor = new Color(1f, 0.18f, 0.32f, 0.72f);

        private readonly List<GameObject> _spawnedOverflowAnimations = new List<GameObject>();
        private readonly List<Coroutine> _overflowAnimationCoroutines = new List<Coroutine>();

        private RectTransform _rectTransform;
        private RectTransform _suspicionOverflowAnimationLayer;
        private SuspicionManager _suspicionManager;
        private GameStateManager _gameStateManager;
        private Coroutine _gameOverAnimationCoroutine;
        private bool _isShowingOverflowAnimations;

        protected override bool InitializeCore()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                Debug.LogError("RightPanelView needs a RectTransform.", this);
                return false;
            }

            if (SuspicionManager.Instance == null)
            {
                return false;
            }

            if (_suspicionManager != null)
            {
                _suspicionManager.RaiseSetSuspicionEvent -= HandleSetSuspicionEvent;
                _suspicionManager.RaiseSetSuspicionPreviewEvent -= HandleSetSuspicionEvent;
            }

            if (_gameStateManager != null)
            {
                _gameStateManager.RaiseSetGameStateEvent -= HandleSetGameStateEvent;
            }

            if (GameStateManager.Instance == null)
            {
                return false;
            }

            _suspicionManager = SuspicionManager.Instance;
            _gameStateManager = GameStateManager.Instance;
            _suspicionManager.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
            _suspicionManager.RaiseSetSuspicionPreviewEvent += HandleSetSuspicionEvent;
            _gameStateManager.RaiseSetGameStateEvent += HandleSetGameStateEvent;
            if (_gameStateManager.GetGameState() == GameState.Lost)
            {
                HandleSetGameStateEvent(this, new SetGameStateEventArgs(GameState.Lost));
            }
            else
            {
                RefreshOverflowAnimationState();
            }
            return true;
        }

        protected override void OnDestroy()
        {
            if (_suspicionManager != null)
            {
                _suspicionManager.RaiseSetSuspicionEvent -= HandleSetSuspicionEvent;
                _suspicionManager.RaiseSetSuspicionPreviewEvent -= HandleSetSuspicionEvent;
                _suspicionManager = null;
            }

            if (_gameStateManager != null)
            {
                _gameStateManager.RaiseSetGameStateEvent -= HandleSetGameStateEvent;
                _gameStateManager = null;
            }

            ClearOverflowAnimations();
            StopGameOverAnimation();
            base.OnDestroy();
        }

        private void HandleSetSuspicionEvent(object sender, SetSuspicionEventArgs e)
        {
            if (_suspicionManager == null)
            {
                return;
            }

            RefreshOverflowAnimationState();
        }

        private void HandleSetGameStateEvent(object sender, SetGameStateEventArgs e)
        {
            if (e.gameState == GameState.Lost)
            {
                if (_gameOverAnimationCoroutine != null)
                {
                    return;
                }

                _isShowingOverflowAnimations = false;
                ClearOverflowAnimations();
                _gameOverAnimationCoroutine = StartCoroutine(PlayGameOverAnimation());
            }
            else
            {
                StopGameOverAnimation();
                RefreshOverflowAnimationState();
            }
        }

        private void RefreshOverflowAnimationState()
        {
            if (_suspicionManager == null)
            {
                return;
            }

            if (_gameStateManager != null && _gameStateManager.GetGameState() == GameState.Lost)
            {
                return;
            }

            int maxSuspicion = _suspicionManager.GetMaxSuspicion();
            bool isOverflowed = _suspicionManager.GetCurrentSuspicion() > maxSuspicion
                || _suspicionManager.GetCurrentSuspicionPreview() > maxSuspicion;
            SetOverflowAnimationsActive(isOverflowed);
        }

        private void SetOverflowAnimationsActive(bool active)
        {
            if (_isShowingOverflowAnimations == active)
            {
                return;
            }

            _isShowingOverflowAnimations = active;

            if (_isShowingOverflowAnimations)
            {
                SpawnOverflowAnimations();
            }
            else
            {
                ClearOverflowAnimations();
            }
        }

        private void SpawnOverflowAnimations()
        {
            ClearOverflowAnimations();

            int spawnCount = Mathf.Max(0, _suspicionOverflowAnimationCount);
            if (spawnCount == 0)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            RectTransform animationRoot = GetAnimationRoot();
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject animationObject = CreateOverflowAnimationObject(animationRoot);
                RectTransform animationRectTransform = animationObject.GetComponent<RectTransform>();
                if (animationRectTransform == null)
                {
                    Debug.LogWarning("Suspicion overflow animation prefab needs a RectTransform.", this);
                    Destroy(animationObject);
                    animationObject = CreateFallbackAnimationObject(animationRoot);
                    animationRectTransform = animationObject.GetComponent<RectTransform>();
                }

                PrepareAnimationObject(animationObject, animationRectTransform);
                SetRandomPositionInsideRightPanel(animationRectTransform);
                PlayAttachedAnimation(animationObject);
                CanvasGroup canvasGroup = EnsureCanvasGroup(animationObject);
                _spawnedOverflowAnimations.Add(animationObject);
                _overflowAnimationCoroutines.Add(StartCoroutine(BeforeGameOverAnimation(canvasGroup)));
            }
        }

        private IEnumerator PlayGameOverAnimation()
        {
            ClearOverflowAnimations();

            Canvas.ForceUpdateCanvases();
            RectTransform animationRoot = GetAnimationRoot();
            int spawnCount = Mathf.Max(0, _gameOverAnimationCount);
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject animationObject = CreateOverflowAnimationObject(animationRoot);
                RectTransform animationRectTransform = animationObject.GetComponent<RectTransform>();
                if (animationRectTransform == null)
                {
                    Debug.LogWarning("Suspicion overflow animation prefab needs a RectTransform.", this);
                    Destroy(animationObject);
                    animationObject = CreateFallbackAnimationObject(animationRoot);
                    animationRectTransform = animationObject.GetComponent<RectTransform>();
                }

                PrepareAnimationObject(animationObject, animationRectTransform);
                SetRandomPositionInsideRightPanel(animationRectTransform);
                PlayAttachedAnimation(animationObject);
                CanvasGroup canvasGroup = EnsureCanvasGroup(animationObject);
                _spawnedOverflowAnimations.Add(animationObject);
                _overflowAnimationCoroutines.Add(StartCoroutine(GameOverAnimation(canvasGroup)));
            }

            yield return null;
        }

        private RectTransform GetAnimationRoot()
        {
            if (_suspicionOverflowAnimationLayer == null)
            {
                GameObject layerObject = new GameObject(SuspicionOverflowAnimationLayerName, typeof(RectTransform));
                layerObject.layer = gameObject.layer;
                layerObject.transform.SetParent(_rectTransform, false);

                _suspicionOverflowAnimationLayer = layerObject.GetComponent<RectTransform>();
                _suspicionOverflowAnimationLayer.anchorMin = Vector2.zero;
                _suspicionOverflowAnimationLayer.anchorMax = Vector2.one;
                _suspicionOverflowAnimationLayer.offsetMin = Vector2.zero;
                _suspicionOverflowAnimationLayer.offsetMax = Vector2.zero;
                _suspicionOverflowAnimationLayer.pivot = new Vector2(0.5f, 0.5f);
                _suspicionOverflowAnimationLayer.localRotation = Quaternion.identity;
                _suspicionOverflowAnimationLayer.localScale = Vector3.one;
            }

            _suspicionOverflowAnimationLayer.SetAsFirstSibling();
            return _suspicionOverflowAnimationLayer;
        }

        private GameObject CreateOverflowAnimationObject(RectTransform animationRoot)
        {
            if (_suspicionOverflowAnimationPrefab != null)
            {
                return Instantiate(_suspicionOverflowAnimationPrefab, animationRoot);
            }

            return CreateFallbackAnimationObject(animationRoot);
        }

        private GameObject CreateFallbackAnimationObject(RectTransform animationRoot)
        {
            GameObject animationObject = new GameObject("Suspicion Overflow Animation", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            animationObject.transform.SetParent(animationRoot, false);
            animationObject.GetComponent<RectTransform>().sizeDelta = _fallbackAnimationSize;
            Image image = animationObject.GetComponent<Image>();
            image.color = _fallbackAnimationColor;
            image.raycastTarget = false;
            return animationObject;
        }

        private void PrepareAnimationObject(GameObject animationObject, RectTransform animationRectTransform)
        {
            animationObject.layer = gameObject.layer;
            animationRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            animationRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            animationRectTransform.pivot = new Vector2(0.5f, 0.5f);
            animationRectTransform.localRotation = Quaternion.identity;
            animationRectTransform.localScale = Vector3.one;

            if (_suspicionOverflowAnimationPrefab == null && animationRectTransform.sizeDelta == Vector2.zero)
            {
                animationRectTransform.sizeDelta = _fallbackAnimationSize;
            }
        }

        private void SetRandomPositionInsideRightPanel(RectTransform target)
        {
            Rect panelRect = _rectTransform.rect;
            Vector2 targetSize = GetTargetSize(target);

            float minX = panelRect.xMin + _suspicionOverflowAnimationPadding.x + targetSize.x * 0.5f;
            float maxX = panelRect.xMax - _suspicionOverflowAnimationPadding.x - targetSize.x * 0.5f;
            float minY = panelRect.yMin + _suspicionOverflowAnimationPadding.y + targetSize.y * 0.5f;
            float maxY = panelRect.yMax - _suspicionOverflowAnimationPadding.y - targetSize.y * 0.5f;

            float x = minX <= maxX ? Random.Range(minX, maxX) : panelRect.center.x;
            float y = minY <= maxY ? Random.Range(minY, maxY) : panelRect.center.y;
            Vector3 worldPosition = _rectTransform.TransformPoint(new Vector2(x, y));
            target.position = worldPosition;

            Vector3 anchoredPosition = target.anchoredPosition3D;
            anchoredPosition.z = 0f;
            target.anchoredPosition3D = anchoredPosition;
        }

        private Vector2 GetTargetSize(RectTransform target)
        {
            Vector2 targetSize = target.rect.size;
            if (targetSize == Vector2.zero)
            {
                targetSize = target.sizeDelta;
            }

            if (targetSize == Vector2.zero)
            {
                targetSize = _fallbackAnimationSize;
            }

            return targetSize;
        }

        private void PlayAttachedAnimation(GameObject animationObject)
        {
            Animator animator = animationObject.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.enabled = true;
                animator.Rebind();
                animator.Update(0f);
                if (animator.layerCount > 0)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    animator.Play(stateInfo.fullPathHash, 0, 0f);
                }
            }

            Animation animation = animationObject.GetComponent<Animation>();
            if (animation != null && animation.clip != null)
            {
                animation.Stop();
                animation.Play();
            }
        }

        private CanvasGroup EnsureCanvasGroup(GameObject animationObject)
        {
            CanvasGroup canvasGroup = animationObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = animationObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            return canvasGroup;
        }

        private IEnumerator BeforeGameOverAnimation(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            int steps = 100;
            for (int i = 0; i < steps; i++)
            {
                canvasGroup.alpha = (float)i / (float)steps;
                yield return new WaitForSeconds(0.01f);
            }

            for (int i = steps; i > 0; i--)
            {
                canvasGroup.alpha = (float)i / (float)steps;
                yield return new WaitForSeconds(0.01f);
            }

            canvasGroup.alpha = 0f;
        }

        private IEnumerator GameOverAnimation(CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                yield break;
            }
            int steps = 100;
            for (int i = 0; i < steps; i++)
            {
                canvasGroup.alpha = (float)i / (float)steps;
                yield return new WaitForSeconds(0.01f);
            }

            canvasGroup.alpha = 1f;
        }

        private void StopGameOverAnimation()
        {
            if (_gameOverAnimationCoroutine != null)
            {
                StopCoroutine(_gameOverAnimationCoroutine);
                _gameOverAnimationCoroutine = null;
            }

            ClearOverflowAnimations();
        }

        private void ClearOverflowAnimations()
        {
            foreach (Coroutine coroutine in _overflowAnimationCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            _overflowAnimationCoroutines.Clear();

            foreach (GameObject animationObject in _spawnedOverflowAnimations)
            {
                if (animationObject != null)
                {
                    Destroy(animationObject);
                }
            }
            _spawnedOverflowAnimations.Clear();
        }
    }
}
