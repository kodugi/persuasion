using UnityEngine;

namespace GamePlay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class GamePlayViewportController : MonoBehaviour
    {
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] private Color _letterboxColor = Color.black;

        private Camera _targetCamera;
        private Rect _viewportRect = new Rect(0f, 0f, 1f, 1f);
        private int _screenWidth = -1;
        private int _screenHeight = -1;

        private void Awake()
        {
            _targetCamera = GetComponent<Camera>();
            ApplyViewport();
        }

        private void OnEnable()
        {
            if (_targetCamera == null)
            {
                _targetCamera = GetComponent<Camera>();
            }

            ApplyViewport();
        }

        private void Update()
        {
            if (_screenWidth != Screen.width || _screenHeight != Screen.height)
            {
                ApplyViewport();
            }
        }

        private void OnDisable()
        {
            if (_targetCamera != null)
            {
                _targetCamera.rect = new Rect(0f, 0f, 1f, 1f);
            }
        }

        private void OnValidate()
        {
            _referenceResolution.x = Mathf.Max(1f, _referenceResolution.x);
            _referenceResolution.y = Mathf.Max(1f, _referenceResolution.y);
            _letterboxColor.a = 1f;
        }

        private void ApplyViewport()
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;

            if (_targetCamera == null || _screenWidth <= 0 || _screenHeight <= 0)
            {
                return;
            }

            float targetAspect = _referenceResolution.x / _referenceResolution.y;
            float screenAspect = (float)_screenWidth / _screenHeight;

            if (screenAspect > targetAspect)
            {
                float width = targetAspect / screenAspect;
                _viewportRect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }
            else
            {
                float height = screenAspect / targetAspect;
                _viewportRect = new Rect(0f, (1f - height) * 0.5f, 1f, height);
            }

            _targetCamera.rect = _viewportRect;
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint ||
                (_viewportRect.width >= 1f && _viewportRect.height >= 1f))
            {
                return;
            }

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float leftWidth = _viewportRect.xMin * screenWidth;
            float rightWidth = (1f - _viewportRect.xMax) * screenWidth;
            float topHeight = (1f - _viewportRect.yMax) * screenHeight;
            float bottomHeight = _viewportRect.yMin * screenHeight;

            Color previousColor = GUI.color;
            int previousDepth = GUI.depth;
            GUI.color = _letterboxColor;
            GUI.depth = int.MinValue;

            DrawBar(new Rect(0f, 0f, leftWidth, screenHeight));
            DrawBar(new Rect(screenWidth - rightWidth, 0f, rightWidth, screenHeight));
            DrawBar(new Rect(leftWidth, 0f, screenWidth - leftWidth - rightWidth, topHeight));
            DrawBar(new Rect(leftWidth, screenHeight - bottomHeight, screenWidth - leftWidth - rightWidth, bottomHeight));

            GUI.depth = previousDepth;
            GUI.color = previousColor;
        }

        private static void DrawBar(Rect rect)
        {
            if (rect.width > 0f && rect.height > 0f)
            {
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
            }
        }
    }
}
