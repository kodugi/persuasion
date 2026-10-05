using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    /// <summary>Places an unboxed caption beside the spotlight without covering its input targets.</summary>
    internal sealed class TutorialCalloutLayout
    {
        private const float Margin = 24f;
        private const float Gap = 24f;
        private const float PreferredWidth = 300f;
        private const float MinimumSideWidth = 210f;
        private readonly RectTransform _panel;
        private readonly RectTransform _root;
        private readonly TMP_Text _text;
        private readonly Image _connector;
        private readonly List<Rect> _targets = new List<Rect>();
        private Rect _lastRoot, _lastFocus;
        private string _lastText;
        private bool _hasLayout;

        public TutorialCalloutLayout(RectTransform panel, TMP_Text text)
        {
            _panel = panel;
            _root = (RectTransform)panel.parent;
            _text = text;
            var line = new GameObject("Tutorial White Connector", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            line.layer = panel.gameObject.layer;
            line.transform.SetParent(_root, false);
            _connector = line.GetComponent<Image>();
            _connector.color = Color.white;
            _connector.raycastTarget = false;
            Hide();
        }

        public void Hide()
        {
            _hasLayout = false;
            _connector.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            _targets.Clear();
            TutorialOverlayView.Instance?.GetFocusRects(_root, _targets);
            Rect root = _root.rect;
            Rect focus = _targets.Count == 0 ? new Rect(root.center, Vector2.zero) : _targets[0];
            foreach (var target in _targets)
                focus = Rect.MinMaxRect(Mathf.Min(focus.xMin, target.xMin), Mathf.Min(focus.yMin, target.yMin),
                    Mathf.Max(focus.xMax, target.xMax), Mathf.Max(focus.yMax, target.yMax));
            if (_hasLayout && root == _lastRoot && focus == _lastFocus && _text.text == _lastText) return;
            _lastRoot = root; _lastFocus = focus; _lastText = _text.text; _hasLayout = true;

            float right = root.xMax - Margin - focus.xMax - Gap;
            float left = focus.xMin - Gap - root.xMin - Margin;
            bool beside = _targets.Count > 0 && Mathf.Max(left, right) >= MinimumSideWidth;
            bool onRight = right >= MinimumSideWidth;
            float width = beside ? Mathf.Min(PreferredWidth, onRight ? right : left) : Mathf.Min(420f, root.width - Margin * 2);
            float height = _text.GetPreferredValues(_text.text, width, 0).y + 8f;
            Vector2 position;
            if (beside)
            {
                float x = onRight ? focus.xMax + Gap : focus.xMin - Gap - width;
                position = new Vector2(x, Mathf.Clamp(focus.center.y - height / 2, root.yMin + Margin, root.yMax - Margin - height));
            }
            else
            {
                bool above = root.yMax - focus.yMax > focus.yMin - root.yMin;
                float y = above ? focus.yMax + Gap : focus.yMin - Gap - height;
                if (_targets.Count == 0) y = root.center.y - height / 2;
                position = new Vector2(Mathf.Clamp(focus.center.x - width / 2, root.xMin + Margin, root.xMax - Margin - width),
                    Mathf.Clamp(y, root.yMin + Margin, root.yMax - Margin - height));
            }

            _panel.anchorMin = _panel.anchorMax = Vector2.zero;
            _panel.pivot = Vector2.zero;
            _panel.anchoredPosition = position - root.min;
            _panel.sizeDelta = new Vector2(width, height);
            _text.ForceMeshUpdate();
            UpdateConnector(new Rect(position, new Vector2(width, height)), root);
            _panel.SetAsLastSibling();
        }

        private void UpdateConnector(Rect caption, Rect root)
        {
            _connector.gameObject.SetActive(_targets.Count > 0);
            if (_targets.Count == 0) return;
            // Use the closest actual highlighted cell, not empty space inside the group's bounds.
            Vector2 start = Vector2.zero, end = Vector2.zero;
            float nearest = float.MaxValue;
            foreach (var target in _targets)
            {
                var candidateStart = ClosestPoint(target, caption.center);
                var candidateEnd = ClosestPoint(caption, candidateStart);
                float distance = (candidateEnd - candidateStart).sqrMagnitude;
                if (distance >= nearest) continue;
                nearest = distance; start = candidateStart; end = candidateEnd;
            }
            Vector2 direction = end - start;
            // Leave a little breathing room before the first letter.
            end -= direction.normalized * 8f;
            direction = end - start;
            var rect = _connector.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = start - root.min;
            rect.sizeDelta = new Vector2(direction.magnitude, 1.2f);
            rect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            rect.SetAsLastSibling();
        }

        private static Vector2 ClosestPoint(Rect rect, Vector2 point) =>
            new Vector2(Mathf.Clamp(point.x, rect.xMin, rect.xMax), Mathf.Clamp(point.y, rect.yMin, rect.yMax));
    }
}
