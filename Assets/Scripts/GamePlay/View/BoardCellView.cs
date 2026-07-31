using System.Collections;
using UnityEngine;

namespace GamePlay
{
    public class BoardCellView: MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private CellChangeAnimDirection _cellChangeAnimDirection;

        public void Initialize(CellChangeAnimDirection cellChangeAnimDirection)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _cellChangeAnimDirection = cellChangeAnimDirection;
        }

        public void DestroyGameObject()
        {
            Destroy(gameObject);
        }
        
        public IEnumerator PlayCellPlacementAnimation()
        {
            int steps = 20;
            for (int i = 0; i <= steps; i++)
            {
                _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, (float)i / (float)steps);
                yield return new WaitForSeconds(0.01f);
            }
            _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1f);
            yield return null;
        }
    }

    public enum CellChangeAnimDirection
    {
        // Directions are named after their origins
        Center = 0,
        Left = 1,
        Right = 2,
        Left_Right = 3,
        Up = 4,
        Down = 5,
        Up_Down = 6,
        LeftUp = 7,
        LeftDown = 8,
        RightUp = 9,
        RightDown = 10,
        LeftUp_RightDown = 12,
        LeftDown_RightUp = 13
    }
}