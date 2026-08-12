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
        private List<SuspicionPrefabView> _backgroundSuspicionPrefabViews;
        [SerializeField] private BackgroundBigSuspicionPrefabView _backgroundBigSuspicionPrefabView;
        
        protected override bool InitializeCore()
        {
            if (GameStateManager.Instance == null)
            {
                Debug.LogError("GameStateManager is null");
                return false;
            }
            
            GameStateManager.Instance.RaiseSetGameStateEvent += HandleSetGameStateEvent;
            SuspicionManager.Instance.RaiseSuspicionOverflowEvent += HandleSuspicionOverflowEvent;
            _backgroundSuspicionPrefabViews = GetComponentsInChildren<SuspicionPrefabView>(true).ToList();
            
            foreach (SuspicionPrefabView suspicionView in _backgroundSuspicionPrefabViews)
            {
                suspicionView.Initialize();
            }
            
            _backgroundBigSuspicionPrefabView.Initialize();
            return true;
        }

        public void ResetGame()
        {
            StopAllCoroutines();
            foreach (SuspicionPrefabView suspicionView in _backgroundSuspicionPrefabViews)
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
                foreach (SuspicionPrefabView suspicionView in _backgroundSuspicionPrefabViews)
                {
                    suspicionView.StopGameOverAnimation();
                }
            }
        }

        private void HandleSuspicionOverflowEvent(object sender, SetSuspicionEventArgs e)
        {
            switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
            {
                case GameInfo.MapType.Dream3:
                    _backgroundBigSuspicionPrefabView.PlayBlinkAnimation();
                    break;
            }
        }
    }
}
