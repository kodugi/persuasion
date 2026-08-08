using System.Collections;
using System.Collections.Generic;
using AnimationUtilsNameSpace;
using SingletonUtils;
using UnityEngine;

namespace GamePlay
{
    public class RightPanelView : SelfInitializingMonoBehaviourSingleton<RightPanelView>
    {
        private const string SuspicionViewParentName = "SuspicionViewParent";

        [SerializeField] private Transform _suspicionViewParent;

        private readonly List<BoardCellSuspicionView> _suspicionViews = new List<BoardCellSuspicionView>();
        private readonly List<BoardCellSuspicionView> _playingSuspicionOverflowViews = new List<BoardCellSuspicionView>();

        private SuspicionManager _suspicionManager;
        private GameStateManager _gameStateManager;
        private Coroutine _gameOverAnimationCoroutine;
        private bool _isShowingOverflowAnimations;

        protected override bool InitializeCore()
        {
            if (SuspicionManager.Instance == null)
            {
                return false;
            }

            if (_suspicionManager != null)
            {
                _suspicionManager.RaiseSetSuspicionEvent -= HandleSetSuspicionEvent;
                _suspicionManager.RaiseSetSuspicionPreviewEvent -= HandleSetSuspicionEvent;
            }

            if (_gameStateManager != null)
            {
                _gameStateManager.RaiseSetGameStateEvent -= HandleSetGameStateEvent;
            }

            if (GameStateManager.Instance == null)
            {
                return false;
            }

            _suspicionManager = SuspicionManager.Instance;
            _gameStateManager = GameStateManager.Instance;
            _suspicionManager.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
            _suspicionManager.RaiseSetSuspicionPreviewEvent += HandleSetSuspicionEvent;
            _gameStateManager.RaiseSetGameStateEvent += HandleSetGameStateEvent;

            CacheSuspicionViews();
            if (_gameStateManager.GetGameState() == GameState.Lost)
            {
                HandleSetGameStateEvent(this, new SetGameStateEventArgs(GameState.Lost));
            }
            else
            {
                RefreshOverflowAnimationState();
            }
            return true;
        }

        protected override void OnDestroy()
        {
            if (_suspicionManager != null)
            {
                _suspicionManager.RaiseSetSuspicionEvent -= HandleSetSuspicionEvent;
                _suspicionManager.RaiseSetSuspicionPreviewEvent -= HandleSetSuspicionEvent;
                _suspicionManager = null;
            }

            if (_gameStateManager != null)
            {
                _gameStateManager.RaiseSetGameStateEvent -= HandleSetGameStateEvent;
                _gameStateManager = null;
            }

            StopSuspicionOverflowAnimation();
            StopGameOverAnimation();
            base.OnDestroy();
        }

        private void HandleSetSuspicionEvent(object sender, SetSuspicionEventArgs e)
        {
            if (_suspicionManager == null)
            {
                return;
            }

            RefreshOverflowAnimationState();
        }

        private void HandleSetGameStateEvent(object sender, SetGameStateEventArgs e)
        {
            if (e.gameState == GameState.Lost)
            {
                if (_gameOverAnimationCoroutine != null)
                {
                    return;
                }

                _isShowingOverflowAnimations = false;
                StopSuspicionOverflowAnimation();
                _gameOverAnimationCoroutine = StartCoroutine(PlayGameOverAnimation());
            }
            else
            {
                StopGameOverAnimation();
                RefreshOverflowAnimationState();
            }
        }

        private void RefreshOverflowAnimationState()
        {
            if (_suspicionManager == null)
            {
                return;
            }

            if (_gameStateManager != null && _gameStateManager.GetGameState() == GameState.Lost)
            {
                return;
            }

            int maxSuspicion = _suspicionManager.GetMaxSuspicion();
            bool isOverflowed = _suspicionManager.GetCurrentSuspicion() > maxSuspicion
                || _suspicionManager.GetCurrentSuspicionPreview() > maxSuspicion;
            SetOverflowAnimationsActive(isOverflowed);
        }

        private void SetOverflowAnimationsActive(bool active)
        {
            if (_isShowingOverflowAnimations == active)
            {
                return;
            }

            _isShowingOverflowAnimations = active;

            if (_isShowingOverflowAnimations)
            {
                PlaySuspicionOverflowAnimation();
            }
            else
            {
                StopSuspicionOverflowAnimation();
            }
        }

        private void CacheSuspicionViews()
        {
            _suspicionViews.Clear();
            if (_suspicionViewParent == null)
            {
                _suspicionViewParent = transform.Find(SuspicionViewParentName);
            }

            if (_suspicionViewParent == null)
            {
                Debug.LogWarning("RightPanelView could not find SuspicionViewParent.", this);
                return;
            }

            if (_suspicionViewParent.parent == transform)
            {
                _suspicionViewParent.SetAsFirstSibling();
            }

            _suspicionViews.AddRange(_suspicionViewParent.GetComponentsInChildren<BoardCellSuspicionView>(true));
            foreach (BoardCellSuspicionView suspicionView in _suspicionViews)
            {
                suspicionView.Initialize();
            }
        }

        private void PlaySuspicionOverflowAnimation()
        {
            StopSuspicionOverflowAnimation();

            int animationCount = Random.Range(0, Mathf.Min(2, _suspicionViews.Count) + 1);
            List<BoardCellSuspicionView> candidates = new List<BoardCellSuspicionView>(_suspicionViews);
            for (int i = 0; i < animationCount; i++)
            {
                int candidateIndex = Random.Range(0, candidates.Count);
                BoardCellSuspicionView suspicionView = candidates[candidateIndex];
                candidates.RemoveAt(candidateIndex);

                PlaySuspicionOverflowAnimation(suspicionView);
                _playingSuspicionOverflowViews.Add(suspicionView);
            }
        }

        private void PlaySuspicionOverflowAnimation(BoardCellSuspicionView suspicionView)
        {
            if (suspicionView == null)
            {
                return;
            }

            switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
            {
                case GameInfo.MapType.Dream1:
                case GameInfo.MapType.Dream2:
                case GameInfo.MapType.Dream3:
                case GameInfo.MapType.Dream4:
                    break;
                default:
                    suspicionView.PlayPreGameOverAnimation();
                    break;
            }
        }

        private void StopSuspicionOverflowAnimation()
        {
            foreach (BoardCellSuspicionView suspicionView in _playingSuspicionOverflowViews)
            {
                StopSuspicionOverflowAnimation(suspicionView);
            }

            _playingSuspicionOverflowViews.Clear();
        }

        private IEnumerator PlayGameOverAnimation()
        {
            StopSuspicionOverflowAnimation();
            yield return AnimationUtils.ExecuteAccordingToCountsPreset(_suspicionViews, PlayGameOverAnimation);
        }

        private void PlayGameOverAnimation(BoardCellSuspicionView suspicionView)
        {
            if (suspicionView == null)
            {
                return;
            }
            
            suspicionView.PlayGameOverAnimation();
        }

        private void StopGameOverAnimation()
        {
            StopAnimationCoroutine(_gameOverAnimationCoroutine);
            _gameOverAnimationCoroutine = null;

            foreach (BoardCellSuspicionView suspicionView in _suspicionViews)
            {
                StopGameOverAnimation(suspicionView);
            }
        }

        private void StopSuspicionOverflowAnimation(BoardCellSuspicionView suspicionView)
        {
            if (suspicionView == null)
            {
                return;
            }

            if (suspicionView.gameObject.activeInHierarchy)
            {
                suspicionView.StopPreGameOverAnimation();
            }
            
        }

        private void StopGameOverAnimation(BoardCellSuspicionView suspicionView)
        {
            if (suspicionView == null)
            {
                return;
            }

            if (suspicionView.gameObject.activeInHierarchy)
            {
                suspicionView.StopGameOverAnimation();
            }
        }

        private void StopAnimationCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
}
