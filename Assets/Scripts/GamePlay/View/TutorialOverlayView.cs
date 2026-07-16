using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SingletonUtils;

namespace GamePlay
{
    public class TutorialOverlayView : MonoBehaviourSingleton<TutorialOverlayView>
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Image _topDim;
        [SerializeField] private Image _bottomDim;
        [SerializeField] private Image _leftDim;
        [SerializeField] private Image _rightDim;
        [SerializeField] private Image _clickCatcher;
        [SerializeField] private Color _dimColor = new Color(0f, 0f, 0f, 0.65f);
        [SerializeField] private float _padding = 16f;

        private const float MinRectSize = 0.1f;

        private readonly List<RectTransform> _focusedTargets = new List<RectTransform>();
        private readonly List<GameObject> _focusedWorldTargets = new List<GameObject>();
        private readonly List<Image> _dimPanels = new List<Image>();
        private readonly List<Rect> _focusRects = new List<Rect>();
        private readonly List<Rect> _dimRects = new List<Rect>();
        private readonly List<float> _xCuts = new List<float>();
        private readonly List<float> _yCuts = new List<float>();
        private bool _isShowing;
        private bool _blocksRaycasts;
        private Action _clickHandler;
        private Button _clickCatcherButton;
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private readonly Vector3[] _boundsCorners = new Vector3[8];

        protected override void Awake()
        {
            base.Awake();

            if (!IsSingletonInstance)
            {
                return;
            }

            EnsureInitialized();
            Hide();
        }

        private void LateUpdate()
        {
            if (!_isShowing)
            {
                return;
            }

            UpdateFocus();
        }

        public void Focus(GameObject target)
        {
            Focus(target, true);
        }

        public void Focus(GameObject target, bool blocksRaycasts)
        {
            Focus(target, blocksRaycasts, null);
        }

        public void Focus(GameObject target, bool blocksRaycasts, Action clickHandler)
        {
            if (target == null)
            {
                Hide();
                return;
            }

            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Focus(rectTransform, blocksRaycasts, clickHandler);
                return;
            }

            EnsureInitialized();
            _focusedTargets.Clear();
            _focusedWorldTargets.Clear();
            _focusedWorldTargets.Add(target);
            Show(blocksRaycasts, clickHandler);
        }

        public void Focus(RectTransform target)
        {
            Focus(target, true);
        }

        public void Focus(RectTransform target, bool blocksRaycasts)
        {
            Focus(target, blocksRaycasts, null);
        }

        public void Focus(RectTransform target, bool blocksRaycasts, Action clickHandler)
        {
            if (target == null)
            {
                Hide();
                return;
            }

            EnsureInitialized();
            _focusedTargets.Clear();
            _focusedWorldTargets.Clear();
            _focusedTargets.Add(target);
            Show(blocksRaycasts, clickHandler);
        }

        public void Focus(
            IList<RectTransform> targets,
            IList<GameObject> worldTargets,
            bool blocksRaycasts,
            Action clickHandler)
        {
            EnsureInitialized();
            _focusedTargets.Clear();
            _focusedWorldTargets.Clear();

            AddTargets(targets, _focusedTargets);
            AddTargets(worldTargets, _focusedWorldTargets);

            if (_focusedTargets.Count == 0 && _focusedWorldTargets.Count == 0)
            {
                Hide();
                return;
            }

            Show(blocksRaycasts, clickHandler);
        }

        private void Show(bool blocksRaycasts, Action clickHandler)
        {
            _isShowing = true;
            _blocksRaycasts = blocksRaycasts;
            _clickHandler = clickHandler;
            transform.SetAsLastSibling();
            UpdateFocus();
            SetPanelsRaycastTarget(_blocksRaycasts);
            SetClickCatcherActive(_clickHandler != null);
        }

        public void Hide()
        {
            _focusedTargets.Clear();
            _focusedWorldTargets.Clear();
            _focusRects.Clear();
            _isShowing = false;
            _blocksRaycasts = false;
            _clickHandler = null;
            SetPanelsActive(false);
            SetClickCatcherActive(false);
        }

        private void EnsureInitialized()
        {
            if (_root == null)
            {
                _root = GetComponent<RectTransform>();
            }

            if (_root == null)
            {
                Debug.LogWarning("TutorialOverlayView needs a RectTransform root.", this);
                return;
            }

            _topDim = EnsurePanel(_topDim, "Tutorial Overlay Top");
            _bottomDim = EnsurePanel(_bottomDim, "Tutorial Overlay Bottom");
            _leftDim = EnsurePanel(_leftDim, "Tutorial Overlay Left");
            _rightDim = EnsurePanel(_rightDim, "Tutorial Overlay Right");
            RegisterDimPanel(_topDim);
            RegisterDimPanel(_bottomDim);
            RegisterDimPanel(_leftDim);
            RegisterDimPanel(_rightDim);
            _clickCatcher = EnsureClickCatcher(_clickCatcher, "Tutorial Overlay Click Catcher");
        }

        private Image EnsurePanel(Image panel, string panelName)
        {
            if (panel == null)
            {
                GameObject panelObject = new GameObject(panelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObject.transform.SetParent(_root, false);
                panel = panelObject.GetComponent<Image>();
            }

            panel.color = _dimColor;
            panel.raycastTarget = true;
            return panel;
        }

        private void RegisterDimPanel(Image panel)
        {
            if (panel != null && !_dimPanels.Contains(panel))
            {
                _dimPanels.Add(panel);
            }
        }

        private Image EnsureClickCatcher(Image clickCatcher, string panelName)
        {
            if (clickCatcher == null)
            {
                GameObject panelObject = new GameObject(panelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                panelObject.transform.SetParent(_root, false);
                clickCatcher = panelObject.GetComponent<Image>();
            }

            clickCatcher.color = Color.clear;
            clickCatcher.raycastTarget = true;
            SetFullScreenRect(clickCatcher.rectTransform);

            _clickCatcherButton = clickCatcher.GetComponent<Button>();
            if (_clickCatcherButton == null)
            {
                _clickCatcherButton = clickCatcher.gameObject.AddComponent<Button>();
            }

            _clickCatcherButton.transition = Selectable.Transition.None;
            _clickCatcherButton.onClick.RemoveListener(HandleClickCatcherClicked);
            _clickCatcherButton.onClick.AddListener(HandleClickCatcherClicked);
            return clickCatcher;
        }

        private static void SetFullScreenRect(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void SetPanelsActive(bool active)
        {
            foreach (Image panel in _dimPanels)
            {
                SetPanelActive(panel, active);
            }
        }

        private void SetPanelsRaycastTarget(bool raycastTarget)
        {
            foreach (Image panel in _dimPanels)
            {
                SetPanelRaycastTarget(panel, raycastTarget);
            }
        }

        private static void SetPanelActive(Image panel, bool active)
        {
            if (panel != null)
            {
                panel.gameObject.SetActive(active);
            }
        }

        private static void SetPanelRaycastTarget(Image panel, bool raycastTarget)
        {
            if (panel != null)
            {
                panel.raycastTarget = raycastTarget;
            }
        }

        private void SetClickCatcherActive(bool active)
        {
            if (_clickCatcher == null)
            {
                return;
            }

            _clickCatcher.gameObject.SetActive(active);
            _clickCatcher.raycastTarget = active;
            if (active)
            {
                _clickCatcher.transform.SetAsLastSibling();
            }
        }

        private void HandleClickCatcherClicked()
        {
            _clickHandler?.Invoke();
        }

        private static void AddTargets<T>(IEnumerable<T> source, List<T> destination) where T : UnityEngine.Object
        {
            if (source == null)
            {
                return;
            }

            foreach (T item in source)
            {
                if (item != null && !destination.Contains(item))
                {
                    destination.Add(item);
                }
            }
        }

        private void UpdateFocus()
        {
            if (_root == null)
            {
                return;
            }

            Rect rootRect = _root.rect;
            if (rootRect.width <= 0f || rootRect.height <= 0f)
            {
                return;
            }

            _focusRects.Clear();
            foreach (RectTransform focusedTarget in _focusedTargets)
            {
                if (focusedTarget != null && TryGetRectTransformFocusRect(focusedTarget, rootRect, out Rect focusRect))
                {
                    _focusRects.Add(focusRect);
                }
            }

            foreach (GameObject focusedWorldTarget in _focusedWorldTargets)
            {
                if (focusedWorldTarget != null && TryGetWorldFocusRect(focusedWorldTarget, rootRect, out Rect focusRect))
                {
                    _focusRects.Add(focusRect);
                }
            }

            if (_focusRects.Count == 0)
            {
                Hide();
                return;
            }

            BuildDimRects(rootRect);
            EnsureDimPanelCount(_dimRects.Count);

            for (int i = 0; i < _dimPanels.Count; i++)
            {
                if (i < _dimRects.Count)
                {
                    SetPanelRect(_dimPanels[i], _dimRects[i], rootRect);
                }
                else
                {
                    SetPanelActive(_dimPanels[i], false);
                }
            }

            if (_clickCatcher != null && _clickCatcher.gameObject.activeSelf)
            {
                _clickCatcher.transform.SetAsLastSibling();
            }
        }

        private bool TryGetRectTransformFocusRect(RectTransform target, Rect rootRect, out Rect focusRect)
        {
            focusRect = default;
            target.GetWorldCorners(_worldCorners);

            Camera targetCamera = GetCanvasCamera(target);
            Camera rootCamera = GetCanvasCamera(_root);
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < _worldCorners.Length; i++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(targetCamera, _worldCorners[i]);
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screenPoint, rootCamera, out Vector2 localPoint))
                {
                    continue;
                }

                minX = Mathf.Min(minX, localPoint.x);
                minY = Mathf.Min(minY, localPoint.y);
                maxX = Mathf.Max(maxX, localPoint.x);
                maxY = Mathf.Max(maxY, localPoint.y);
            }

            return TryCreateFocusRect(rootRect, minX, minY, maxX, maxY, out focusRect);
        }

        private bool TryGetWorldFocusRect(GameObject target, Rect rootRect, out Rect focusRect)
        {
            focusRect = default;
            if (!TryGetTargetBounds(target, out Bounds bounds))
            {
                return false;
            }

            Camera worldCamera = Camera.main;
            if (worldCamera == null)
            {
                return false;
            }

            SetBoundsCorners(bounds);
            Camera rootCamera = GetCanvasCamera(_root);
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < _boundsCorners.Length; i++)
            {
                Vector3 screenPoint = worldCamera.WorldToScreenPoint(_boundsCorners[i]);
                if (screenPoint.z < 0f)
                {
                    continue;
                }

                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screenPoint, rootCamera, out Vector2 localPoint))
                {
                    continue;
                }

                minX = Mathf.Min(minX, localPoint.x);
                minY = Mathf.Min(minY, localPoint.y);
                maxX = Mathf.Max(maxX, localPoint.x);
                maxY = Mathf.Max(maxY, localPoint.y);
            }

            return TryCreateFocusRect(rootRect, minX, minY, maxX, maxY, out focusRect);
        }

        private bool TryCreateFocusRect(
            Rect rootRect,
            float minX,
            float minY,
            float maxX,
            float maxY,
            out Rect focusRect)
        {
            focusRect = default;
            if (minX == float.MaxValue || minY == float.MaxValue || maxX == float.MinValue || maxY == float.MinValue)
            {
                return false;
            }

            minX = Mathf.Clamp(minX - _padding, rootRect.xMin, rootRect.xMax);
            minY = Mathf.Clamp(minY - _padding, rootRect.yMin, rootRect.yMax);
            maxX = Mathf.Clamp(maxX + _padding, rootRect.xMin, rootRect.xMax);
            maxY = Mathf.Clamp(maxY + _padding, rootRect.yMin, rootRect.yMax);

            if (maxX - minX <= MinRectSize || maxY - minY <= MinRectSize)
            {
                return false;
            }

            focusRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        private static bool TryGetTargetBounds(GameObject target, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (hasBounds)
            {
                return true;
            }

            Collider2D[] colliders2D = target.GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D collider2D in colliders2D)
            {
                if (collider2D == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = collider2D.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(collider2D.bounds);
                }
            }

            if (hasBounds)
            {
                return true;
            }

            Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                if (collider == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = collider.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }

            return hasBounds;
        }

        private void SetBoundsCorners(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            _boundsCorners[0] = new Vector3(min.x, min.y, min.z);
            _boundsCorners[1] = new Vector3(min.x, min.y, max.z);
            _boundsCorners[2] = new Vector3(min.x, max.y, min.z);
            _boundsCorners[3] = new Vector3(min.x, max.y, max.z);
            _boundsCorners[4] = new Vector3(max.x, min.y, min.z);
            _boundsCorners[5] = new Vector3(max.x, min.y, max.z);
            _boundsCorners[6] = new Vector3(max.x, max.y, min.z);
            _boundsCorners[7] = new Vector3(max.x, max.y, max.z);
        }

        private void BuildDimRects(Rect rootRect)
        {
            _dimRects.Clear();
            _xCuts.Clear();
            _yCuts.Clear();

            AddCut(_xCuts, rootRect.xMin);
            AddCut(_xCuts, rootRect.xMax);
            AddCut(_yCuts, rootRect.yMin);
            AddCut(_yCuts, rootRect.yMax);

            foreach (Rect focusRect in _focusRects)
            {
                AddCut(_xCuts, focusRect.xMin);
                AddCut(_xCuts, focusRect.xMax);
                AddCut(_yCuts, focusRect.yMin);
                AddCut(_yCuts, focusRect.yMax);
            }

            _xCuts.Sort();
            _yCuts.Sort();

            for (int x = 0; x < _xCuts.Count - 1; x++)
            {
                for (int y = 0; y < _yCuts.Count - 1; y++)
                {
                    Rect dimRect = Rect.MinMaxRect(_xCuts[x], _yCuts[y], _xCuts[x + 1], _yCuts[y + 1]);
                    if (dimRect.width <= MinRectSize || dimRect.height <= MinRectSize)
                    {
                        continue;
                    }

                    if (!IsPointInsideAnyFocusRect(dimRect.center))
                    {
                        _dimRects.Add(dimRect);
                    }
                }
            }
        }

        private static void AddCut(List<float> cuts, float value)
        {
            foreach (float cut in cuts)
            {
                if (Mathf.Abs(cut - value) <= MinRectSize)
                {
                    return;
                }
            }

            cuts.Add(value);
        }

        private bool IsPointInsideAnyFocusRect(Vector2 point)
        {
            foreach (Rect focusRect in _focusRects)
            {
                if (focusRect.Contains(point))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureDimPanelCount(int count)
        {
            while (_dimPanels.Count < count)
            {
                Image panel = EnsurePanel(null, "Tutorial Overlay Dim " + _dimPanels.Count);
                panel.raycastTarget = _blocksRaycasts;
                RegisterDimPanel(panel);
            }
        }

        private void SetPanelRect(Image panel, Rect localRect, Rect rootRect)
        {
            if (panel == null)
            {
                return;
            }

            if (localRect.width <= MinRectSize || localRect.height <= MinRectSize)
            {
                panel.gameObject.SetActive(false);
                return;
            }

            panel.gameObject.SetActive(true);

            RectTransform rectTransform = panel.rectTransform;
            rectTransform.anchorMin = new Vector2(
                Mathf.InverseLerp(rootRect.xMin, rootRect.xMax, localRect.xMin),
                Mathf.InverseLerp(rootRect.yMin, rootRect.yMax, localRect.yMin));
            rectTransform.anchorMax = new Vector2(
                Mathf.InverseLerp(rootRect.xMin, rootRect.xMax, localRect.xMax),
                Mathf.InverseLerp(rootRect.yMin, rootRect.yMax, localRect.yMax));
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static Camera GetCanvasCamera(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return null;
            }

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return canvas.worldCamera;
        }
    }
}
