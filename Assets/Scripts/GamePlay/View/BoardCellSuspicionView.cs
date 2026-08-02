using System.Collections;
using UnityEngine;

namespace GamePlay
{
    public class BoardCellSuspicionView: MonoBehaviour
    {
        private SpriteRenderer _sr;
        private Coroutine _beforeGameOverAnimationCoroutine;
        private Coroutine _gameOverAnimationCoroutine;
        
        public void Initialize()
        {
            _sr = GetComponent<SpriteRenderer>();
            
            gameObject.SetActive(false);
        }
        
        public void SetRendererSorting(int sortingLayerID, int sortingOrder)
        {
            if (_sr == null)
            {
                Debug.LogError("spriteRenderer is null");
                return;
            }

            _sr.sortingLayerID = sortingLayerID;
            _sr.sortingOrder = sortingOrder + 5;
        }
        
        public void PlayBeforeGameOverAnimation()
        {
            gameObject.SetActive(true);
            _beforeGameOverAnimationCoroutine = StartCoroutine(BeforeGameOverAnimation());
        }

        public IEnumerator BeforeGameOverAnimation()
        {
            int steps = 100;
            for (int i = 0; i < steps; i++)
            {
                _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, (float)i / (float)steps);
                yield return new WaitForSeconds(0.01f);
            }

            yield return new WaitForSeconds(0.5f);
            
            for (int i = steps; i > 0; i--)
            {
                _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, (float)i / (float)steps);
                yield return new WaitForSeconds(0.01f);
            }
        }

        public void StopBeforeGameOverAnimation()
        {
            if (_beforeGameOverAnimationCoroutine != null)
            {
                StopCoroutine(_beforeGameOverAnimationCoroutine);
            }
            gameObject.SetActive(false);
        }
        
        public void PlayGameOverAnimation()
        {
            gameObject.SetActive(true);
            _gameOverAnimationCoroutine = StartCoroutine(GameOverAnimation());
        }
        
        public IEnumerator GameOverAnimation()
        {
            // this part is currently unnecessary because the animation plays by itself on Active
            /*int steps = 100;
            for (int i = 0; i < steps; i++)
            {
                _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, (float)i / (float)steps);
                yield return new WaitForSeconds(0.01f);
            }*/

            yield return null;
        }
        
        public void StopGameOverAnimation()
        {
            if (_gameOverAnimationCoroutine != null)
            {
                StopCoroutine(_gameOverAnimationCoroutine);
            }
            gameObject.SetActive(false);
        }
    }
}