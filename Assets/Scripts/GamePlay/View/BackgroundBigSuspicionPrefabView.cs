using UnityEngine;

namespace GamePlay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundBigSuspicionPrefabView : SuspicionPrefabView
    {
        private const int MarkerOccludingSortingOrder = 4;

        private void Awake()
        {
            ApplyMarkerOccludingSorting();
        }

        private void OnValidate()
        {
            ApplyMarkerOccludingSorting();
        }

        public void PlayBlinkAnimation()
        {
            // TODO: implement actual animation
            PlayPreGameOverAnimation();
        }

        private void ApplyMarkerOccludingSorting()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = MarkerOccludingSortingOrder;
            }
        }
    }
}
