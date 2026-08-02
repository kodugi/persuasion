using UnityEngine;
using UnityEngine.UI;

namespace Investigation
{
    public class Inv_FloatItemCTRL : MonoBehaviour
    {
        public Inv_GameManager inventoryManager;
        public int index;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform uiRect = gameObject.GetComponent<RectTransform>();

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint
            );

            uiRect.anchoredPosition = localPoint;

            if (!Input.GetMouseButtonUp(0))
            {
                return;
            }
            inventoryManager.ItemRelease(index);
            Destroy(gameObject);
        }
    }
}