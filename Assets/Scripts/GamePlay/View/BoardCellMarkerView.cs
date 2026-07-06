using UnityEngine;

namespace GamePlay
{
    public class BoardCellMarkerView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _border;
        [SerializeField] private SpriteRenderer _lockIcon;
        [SerializeField] private SpriteRenderer _preview;

        public void SetSorting(int sortingLayerID, int baseSortingOrder)
        {
            SetRendererSorting(_border, sortingLayerID, baseSortingOrder + 2);
            SetRendererSorting(_lockIcon, sortingLayerID, baseSortingOrder + 3);
            SetRendererSorting(_preview, sortingLayerID, baseSortingOrder + 1);
        }

        public void NormalizeLargeRendererOffsets(float maxExpectedOffset)
        {
            NormalizeLargeRendererOffset(_border, maxExpectedOffset);
            NormalizeLargeRendererOffset(_lockIcon, maxExpectedOffset);
            NormalizeLargeRendererOffset(_preview, maxExpectedOffset);
        }

        public void SetTargetBorderVisible(bool visible)
        {
            if (_border != null)
            {
                _border.enabled = visible;
            }
        }

        public void SetLockedVisible(bool visible)
        {
            if (_lockIcon != null)
            {
                _lockIcon.enabled = visible;
            }
        }

        public void SetPreview(Sprite sprite, Color color)
        {
            if (_preview == null)
            {
                return;
            }

            _preview.sprite = sprite;
            _preview.color = color;
            _preview.enabled = sprite != null;
        }

        public void SetPreview(Sprite sprite)
        {
            if (_preview == null)
            {
                return;
            }

            _preview.sprite = sprite;
            _preview.enabled = sprite != null;
        }

        public void ClearPreview()
        {
            if (_preview == null)
            {
                return;
            }

            _preview.sprite = null;
            _preview.enabled = false;
        }

        private static void SetRendererSorting(SpriteRenderer spriteRenderer, int sortingLayerID, int sortingOrder)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sortingLayerID = sortingLayerID;
            spriteRenderer.sortingOrder = sortingOrder;
        }

        private static void NormalizeLargeRendererOffset(SpriteRenderer spriteRenderer, float maxExpectedOffset)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            Vector3 localPosition = spriteRenderer.transform.localPosition;
            if (Mathf.Abs(localPosition.x) <= maxExpectedOffset && Mathf.Abs(localPosition.y) <= maxExpectedOffset)
            {
                return;
            }

            spriteRenderer.transform.localPosition = new Vector3(0f, 0f, localPosition.z);
        }
    }
}
