using System;
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

        private RectTransform _focusedTarget;
        private bool _isShowing;
        private bool _blocksRaycasts;
        private Action _clickHandler;
        private Button _clickCatcherButton;
        private readonly Vector3[] _worldCorners = new Vector3[4];

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
            if (!_isShowing || _focusedTarget == null)
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
            Focus(target == null ? null : target.GetComponent<RectTransform>(), blocksRaycasts);
        }

        public void Focus(GameObject target, bool blocksRaycasts, Action clickHandler)
        {
            Focus(target == null ? null : target.GetComponent<RectTransform>(), blocksRaycasts, clickHandler);
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
            _focusedTarget = target;
            _isShowing = true;
            _blocksRaycasts = blocksRaycasts;
            _clickHandler = clickHandler;
            transform.SetAsLastSibling();
            SetPanelsActive(true);
            SetPanelsRaycastTarget(_blocksRaycasts);
            SetClickCatcherActive(_clickHandler != null);
            UpdateFocus();
        }

        public void Hide()
        {
            _focusedTarget = null;
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
            SetPanelActive(_topDim, active);
            SetPanelActive(_bottomDim, active);
            SetPanelActive(_leftDim, active);
            SetPanelActive(_rightDim, active);
        }

        private void SetPanelsRaycastTarget(bool raycastTarget)
        {
            SetPanelRaycastTarget(_topDim, raycastTarget);
            SetPanelRaycastTarget(_bottomDim, raycastTarget);
            SetPanelRaycastTarget(_leftDim, raycastTarget);
            SetPanelRaycastTarget(_rightDim, raycastTarget);
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

        private void UpdateFocus()
        {
            if (_root == null || _focusedTarget == null)
            {
                return;
            }

            Rect rootRect = _root.rect;
            if (rootRect.width <= 0f || rootRect.height <= 0f)
            {
                return;
            }

            Rect focusRect = GetFocusRect(rootRect);
            SetPanelRect(_topDim, Rect.MinMaxRect(rootRect.xMin, focusRect.yMax, rootRect.xMax, rootRect.yMax), rootRect);
            SetPanelRect(_bottomDim, Rect.MinMaxRect(rootRect.xMin, rootRect.yMin, rootRect.xMax, focusRect.yMin), rootRect);
            SetPanelRect(_leftDim, Rect.MinMaxRect(rootRect.xMin, focusRect.yMin, focusRect.xMin, focusRect.yMax), rootRect);
            SetPanelRect(_rightDim, Rect.MinMaxRect(focusRect.xMax, focusRect.yMin, rootRect.xMax, focusRect.yMax), rootRect);
        }

        private Rect GetFocusRect(Rect rootRect)
        {
            _focusedTarget.GetWorldCorners(_worldCorners);

            Camera targetCamera = GetCanvasCamera(_focusedTarget);
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

            minX = Mathf.Clamp(minX - _padding, rootRect.xMin, rootRect.xMax);
            minY = Mathf.Clamp(minY - _padding, rootRect.yMin, rootRect.yMax);
            maxX = Mathf.Clamp(maxX + _padding, rootRect.xMin, rootRect.xMax);
            maxY = Mathf.Clamp(maxY + _padding, rootRect.yMin, rootRect.yMax);

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private void SetPanelRect(Image panel, Rect localRect, Rect rootRect)
        {
            if (panel == null)
            {
                return;
            }

            if (localRect.width <= 0.1f || localRect.height <= 0.1f)
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
