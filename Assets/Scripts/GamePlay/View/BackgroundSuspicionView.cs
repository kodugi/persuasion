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
        [SerializeField] private int _suspicionPrefabViewCount;
        [SerializeField] private GameObject _backgroundSuspicionPrefab;
        [SerializeField] private BackgroundBigSuspicionPrefabView _backgroundBigSuspicionPrefabView;
        
        private List<SuspicionPrefabView> _backgroundSuspicionPrefabViews;
        
        protected override bool InitializeCore()
        {
            if (GameStateManager.Instance == null)
            {
                Debug.LogError("GameStateManager is null");
                return false;
            }
            
            GameStateManager.Instance.RaiseSetGameStateEvent += HandleSetGameStateEvent;
            SuspicionManager.Instance.RaiseSuspicionOverflowEvent += HandleSuspicionOverflowEvent;
            _backgroundSuspicionPrefabViews = new List<SuspicionPrefabView>();
            SpawnSuspicionPrefabViews();
            
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

        private void SpawnSuspicionPrefabViews()
        {
            Random random = new Random();
            for (int i = 0; i < _suspicionPrefabViewCount; i++)
            {
                GameObject go = Instantiate(_backgroundSuspicionPrefab, transform);
                go.transform.localPosition = new Vector3(random.Next(-85, 85) / 10.0f, random.Next(-50, 50) / 10.0f, 0);
                SuspicionPrefabView suspicionPrefabView = go.GetComponent<SuspicionPrefabView>();
                _backgroundSuspicionPrefabViews.Add(suspicionPrefabView);
                suspicionPrefabView.Initialize();
                suspicionPrefabView.SetRendererSorting(0, 1);
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
