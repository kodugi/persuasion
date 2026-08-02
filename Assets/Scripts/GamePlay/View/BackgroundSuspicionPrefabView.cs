using System.Collections;
using UnityEngine;

namespace GamePlay
{
    public class BackgroundSuspicionPrefabView: MonoBehaviour
    {
        private Coroutine _gameOverAnimationCoroutine;
        private SpriteRenderer _sr;
        
        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            
            gameObject.SetActive(false);
        }
        
        public void PlayGameOverAnimation()
        {
            gameObject.SetActive(true);
            _gameOverAnimationCoroutine = StartCoroutine(GameOverAnimation());
        }
        
        public IEnumerator GameOverAnimation()
        {
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