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
        [SerializeField] private GameObject _backgroundSuspicionUIPrefab;
        [SerializeField] private BackgroundBigSuspicionPrefabView _backgroundBigSuspicionPrefabView;
        [SerializeField] private RectTransform _backgroundSuspicionUIParent;
        
        private List<SuspicionPrefabView> _backgroundSuspicionPrefabViews;
        
        protected override bool InitializeCore()
        {
            if (GameStateManager.Instance == null)
            {
                Debug.LogError("GameStateManager is null");
                return false;
            }

            if (_backgroundSuspicionUIParent == null)
            {
                Debug.LogError("BackgroundSuspicionUIParent is null");
                return false;
            }

            if (_backgroundSuspicionUIPrefab == null ||
                _backgroundSuspicionUIPrefab.GetComponent<RectTransform>() == null ||
                _backgroundSuspicionUIPrefab.GetComponent<SuspicionPrefabView>() == null)
            {
                Debug.LogError("BackgroundSuspicionUIPrefab is not a valid UI suspicion prefab");
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

            _backgroundBigSuspicionPrefabView.StopGameOverAnimation();
        }

        private void SpawnSuspicionPrefabViews()
        {
            Random random = new Random();
            for (int i = 0; i < _suspicionPrefabViewCount; i++)
            {
                GameObject go = Instantiate(_backgroundSuspicionUIPrefab, _backgroundSuspicionUIParent);
                RectTransform rectTransform = (RectTransform)go.transform;
                Vector2 randomAnchor = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
                rectTransform.anchorMin = randomAnchor;
                rectTransform.anchorMax = randomAnchor;
                rectTransform.anchoredPosition = Vector2.zero;

                SuspicionPrefabView suspicionPrefabView = go.GetComponent<SuspicionPrefabView>();
                _backgroundSuspicionPrefabViews.Add(suspicionPrefabView);
                suspicionPrefabView.Initialize();
            }
        }

        private void HandleSetGameStateEvent(object sender, SetGameStateEventArgs e)
        {
            if (e.gameState == GameState.Lost)
            {
                GameInfo.MapType mapType = GameInfoHolder.GetCurrentGameInfo().GetMapType();
                bool isDreamMap = mapType == GameInfo.MapType.Dream1 ||
                                  mapType == GameInfo.MapType.Dream2 ||
                                  mapType == GameInfo.MapType.Dream3 ||
                                  mapType == GameInfo.MapType.Dream4;
                DefeatReason defeatReason = WinConditionManager.Instance.GetLastDefeatReason();

                if (!isDreamMap && defeatReason != DefeatReason.SuspicionOverflow)
                {
                    foreach (SuspicionPrefabView suspicionView in _backgroundSuspicionPrefabViews)
                    {
                        suspicionView.StopGameOverAnimation();
                    }

                    _backgroundBigSuspicionPrefabView.StopGameOverAnimation();
                    return;
                }

                if (!isDreamMap && defeatReason == DefeatReason.SuspicionOverflow)
                {
                    ChiefManager.Instance?.PlayBGM(ChiefManager.GameOverSoundId);
                }

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

                _backgroundBigSuspicionPrefabView.StopGameOverAnimation();
            }
        }

        private void HandleSuspicionOverflowEvent(object sender, SetSuspicionEventArgs e)
        {
            switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
            {
                case GameInfo.MapType.Dream3:
                    _backgroundBigSuspicionPrefabView.PlayBlinkAnimation();
                    ChiefManager.Instance?.PlayBGM(ChiefManager.BigEyeSoundId);
                    break;
            }
        }
    }
}
