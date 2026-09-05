using System;
using System.Collections;
using System.Collections.Generic;
using SingletonUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay
{
    /// <summary>
    /// Unity entry point for GamePlayScene. Runtime services and configuration parsing
    /// are delegated so this component only owns scene lifecycle and transitions.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [Header("Game content")]
        [SerializeField] private List<GameInfo> _gameInfoList = new List<GameInfo>();
        [SerializeField] private List<TutorialEntryGroup> _tutorialEntryGroups =
            new List<TutorialEntryGroup>();
        [SerializeField] private List<StageEntry> _stageList = new List<StageEntry>();
        [SerializeField] private List<PlayableBlockType> _availableBlocks =
            new List<PlayableBlockType> { PlayableBlockType.Basic };

        [Header("Suspicion")]
        [SerializeField, Min(1)] private int _maxSuspicion = 100;
        [SerializeField, Min(0)] private int _suspicionDecrementPerTurn = 38;

        private GamePlayRuntime _runtime;
        private Coroutine _resetCoroutine;
        private Coroutine _queuedResetCoroutine;
        private Coroutine _delayedResetCoroutine;

        protected override void Awake()
        {
            base.Awake();
            if (!IsSingletonInstance)
            {
                return;
            }

            InitializeRuntime();
        }

        protected override void OnDestroy()
        {
            if (_runtime != null)
            {
                _runtime.State.RaiseSetGameStateEvent -= HandleSetGameStateEvent;
                _runtime.WinCondition.RaiseDefeatEvent -= HandleDefeatEvent;
                _runtime.Dispose();
                _runtime = null;
            }

            base.OnDestroy();
        }

        private void Start()
        {
            _runtime?.StartGame(BlockSelectionView.Instance);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Slash))
            {
                ReturnToStartScene();
                return;
            }

            _runtime?.Tutorial.Tick(Time.deltaTime);
        }

        public void ResetGame()
        {
            CancelScheduledResets();

            if (_runtime == null || !_runtime.IsInitialized)
            {
                Debug.LogWarning("GameManager cannot reset before initialization completes.", this);
                return;
            }

            if (_resetCoroutine == null)
            {
                _resetCoroutine = StartCoroutine(ResetCore());
            }
        }

        public void ResetGameAfterDelay(float delaySeconds)
        {
            if (_delayedResetCoroutine != null)
            {
                StopCoroutine(_delayedResetCoroutine);
                _delayedResetCoroutine = null;
            }

            if (delaySeconds <= 0f)
            {
                ResetGame();
                return;
            }

            _delayedResetCoroutine = StartCoroutine(ResetGameAfterDelayCore(delaySeconds));
        }

        public void QueueResetGame()
        {
            if (_queuedResetCoroutine == null && _resetCoroutine == null)
            {
                _queuedResetCoroutine = StartCoroutine(ResetGameAfterCurrentEvent());
            }
        }

        public bool TryPlayGameOverDialogue(Action onCompleted)
        {
            if (_runtime == null ||
                !GameInfoHolder.TryGetCurrentGameInfo(out GameInfo gameInfo) ||
                !gameInfo.TryGetGameOverDialogue(out DialogueData dialogueData))
            {
                return false;
            }

            return _runtime.Dialogue.TryPlayDialogue(dialogueData, onCompleted, false);
        }

        private void InitializeRuntime()
        {
            GamePlaySceneConfiguration configuration = new GamePlaySceneConfiguration(
                _gameInfoList,
                _tutorialEntryGroups,
                _stageList);

            if (!configuration.SelectGameInfoForCurrentScene())
            {
                enabled = false;
                return;
            }

            _runtime = new GamePlayRuntime();
            _runtime.Initialize(
                GamePlaySceneConfiguration.CreateBlocks(_availableBlocks),
                configuration.CreateTutorialLookup(),
                _maxSuspicion,
                _suspicionDecrementPerTurn);

            _runtime.State.RaiseSetGameStateEvent += HandleSetGameStateEvent;
            _runtime.WinCondition.RaiseDefeatEvent += HandleDefeatEvent;
        }

        private void CancelScheduledResets()
        {
            if (_delayedResetCoroutine != null)
            {
                StopCoroutine(_delayedResetCoroutine);
                _delayedResetCoroutine = null;
            }

            if (_queuedResetCoroutine != null)
            {
                StopCoroutine(_queuedResetCoroutine);
                _queuedResetCoroutine = null;
            }
        }

        private IEnumerator ResetGameAfterDelayCore(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            _delayedResetCoroutine = null;
            ResetGame();
        }

        private IEnumerator ResetGameAfterCurrentEvent()
        {
            yield return null;
            _queuedResetCoroutine = null;
            ResetGame();
        }

        private IEnumerator ResetCore()
        {
            yield return new WaitForSeconds(0.5f);

            // Delay the shared GameInfo switch until the old board is no longer visible.
            GameInfoHolder.CommitPendingGameInfoChange();
            GamePlaySoundManager.Instance?.ResetAfterGameOver();
            _runtime.BeginReset();
            ResetViews();

            // Let the new board render before turn-start dialogue and tutorial events fire.
            yield return null;

            _runtime.EndReset();
            _resetCoroutine = null;
        }

        private void ResetViews()
        {
            if (BoardView.Instance is BoardView boardView)
            {
                boardView.ResetGame();
            }

            GameStateView.Instance?.ResetGame();
            BackgroundSuspicionView.Instance?.ResetGame();
            SuspicionView.Instance?.ResetGame();
            FindAnyObjectByType<UIImageView>()?.ResetGame();
            FindAnyObjectByType<FigureView>()?.ResetGame();
            BlackOutPanelView.Instance?.ResetGame();
            GameOverPopupView.Instance?.ResetGame();
        }

        private void HandleSetGameStateEvent(object sender, SetGameStateEventArgs eventArgs)
        {
            switch (eventArgs.gameState)
            {
                case GameState.Won:
                    HandleStageWon();
                    break;
                case GameState.Lost:
                    HandleGameLost();
                    break;
            }
        }

        private void HandleStageWon()
        {
            if (GameInfoHolder.HasMoreGameInfos())
            {
                GameInfoHolder.ToNext();
                ResetGame();
                return;
            }

            ToInvestigation();
        }

        private void HandleGameLost()
        {
            if (!GameInfoHolder.TryGetCurrentGameInfo(out GameInfo gameInfo) ||
                gameInfo.GetMapType() != GameInfo.MapType.Dream4 ||
                _runtime.WinCondition.GetLastDefeatReason() != DefeatReason.Scripted)
            {
                return;
            }

            StartCoroutine(DreamGameOver());
        }

        private void HandleDefeatEvent(object sender, DefeatEventArgs eventArgs)
        {
            if (eventArgs.Reason == DefeatReason.DreamAutoReset)
            {
                QueueResetGame();
                return;
            }

            if (eventArgs.Reason != DefeatReason.DreamRetry)
            {
                return;
            }

            if (!TryPlayGameOverDialogue(QueueResetGame))
            {
                QueueResetGame();
            }
        }

        private IEnumerator DreamGameOver()
        {
            yield return new WaitForSeconds(5f);
            ToInvestigation();
        }

        private static void ToInvestigation()
        {
            if (ChiefManager.Instance == null)
            {
                Debug.LogWarning("Cannot start Investigation because ChiefManager is not available.");
                return;
            }

            ChiefManager.Instance.StartInvestigation();
        }

        private static void ReturnToStartScene()
        {
            if (ChiefManager.Instance != null)
            {
                ChiefManager.Instance.ReturnToStartScene();
                return;
            }

            SceneManager.LoadScene("StartScene");
        }
    }
}
