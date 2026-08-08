using System;
using System.Collections.Generic;
using System.Linq;
using AnimationUtilsNameSpace;
using SingletonUtils;
using UnityEngine;
using Random = System.Random;

namespace GamePlay
{
    public class BackgroundSuspicionView: SelfInitializingMonoBehaviourSingleton<BackgroundSuspicionView>
    {
        private List<BoardCellSuspicionView> _backgroundSuspicionPrefabViews;
        
        protected override bool InitializeCore()
        {
            if (GameStateManager.Instance == null)
            {
                Debug.LogError("GameStateManager is null");
                return false;
            }
            
            GameStateManager.Instance.RaiseSetGameStateEvent += HandleSetGameStateEvent;
            _backgroundSuspicionPrefabViews = GetComponentsInChildren<BoardCellSuspicionView>(true).ToList();

            foreach (BoardCellSuspicionView suspicionView in _backgroundSuspicionPrefabViews)
            {
                suspicionView.Initialize();
            }
            return true;
        }

        public void ResetGame()
        {
            StopAllCoroutines();
            foreach (BoardCellSuspicionView suspicionView in _backgroundSuspicionPrefabViews)
            {
                suspicionView.StopGameOverAnimation();
            }
        }

        private void HandleSetGameStateEvent(object sender, SetGameStateEventArgs e)
        {
            if (e.gameState == GameState.Lost)
            {
                StartCoroutine(AnimationUtils.ExecuteAccordingToCountsPreset(_backgroundSuspicionPrefabViews, (suspicionView) =>
                {
                    suspicionView.PlayGameOverAnimation();
                }));
            }
            else
            {
                foreach (BoardCellSuspicionView suspicionView in _backgroundSuspicionPrefabViews)
                {
                    suspicionView.StopGameOverAnimation();
                }
            }
        }
    }
}
