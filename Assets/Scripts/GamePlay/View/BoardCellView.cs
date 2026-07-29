using System.Collections;
using UnityEngine;

namespace GamePlay
{
    public class BoardCellView: MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        public void Initialize()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
}