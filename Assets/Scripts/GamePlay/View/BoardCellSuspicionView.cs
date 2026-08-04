using System.Collections;
using UnityEngine;

namespace GamePlay
{
    public class BoardCellSuspicionView: MonoBehaviour
    {
        private SpriteRenderer _sr;
        private Animator _anim;
        
        public void Initialize()
        {
            _sr = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
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
        
        public void PlayPreGameOverAnimation()
        {
            _anim.ResetTrigger("StopSuspicionOverflow");
            _anim.SetTrigger("SuspicionOverflow");
        }

        public void StopPreGameOverAnimation()
        {
            Debug.Log("stopping pre-gameover animation for " + gameObject.name);
            _anim.ResetTrigger("SuspicionOverflow");
            _anim.SetTrigger("StopSuspicionOverflow");
        }
        
        public void PlayGameOverAnimation()
        {
            _anim.ResetTrigger("StopGameOver");
            _anim.SetTrigger("GameOver");
        }
        
        public void StopGameOverAnimation()
        {
            _anim.ResetTrigger("GameOver");
            _anim.SetTrigger("StopGameOver");
        }
    }
}
