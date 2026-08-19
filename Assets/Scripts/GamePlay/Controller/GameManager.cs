using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using MapEditor.Model;
using UnityEngine;
using SingletonUtils;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [SerializeField] private List<GameInfo> _gameInfoList;
        [SerializeField] private List<TutorialEntryGroup> _tutorialEntryGroups = new List<TutorialEntryGroup>();
        [SerializeField] private List<StageEntry> _stageList = new List<StageEntry>();
        
        private TurnManager _turnManager;
        private BlockSelectionManager _blockSelectionManager;
        private BoardController _boardController;
        private SuspicionManager _suspicionManager;
        private WinConditionManager _winConditionManager;
        private GameStateManager  _gameStateManager;
        private DialogueManager _dialogueManager;
        private TutorialController _tutorialController;
        private Coroutine _queuedResetCoroutine;
        private Coroutine _delayedResetCoroutine;

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        private void Update()
        {
            _tutorialController?.Tick(Time.deltaTime);
        }
        
        private void Initialize()
        {
            _turnManager = new TurnManager();
            _blockSelectionManager = new BlockSelectionManager();
            _boardController = new BoardController();
            _suspicionManager = new SuspicionManager();
            _winConditionManager = new WinConditionManager();
            _gameStateManager = new GameStateManager();
            _dialogueManager = new DialogueManager();
            _tutorialController = new TutorialController();
            
            // TODO: replace hardcoding with actual values
            int maxSuspicion = 100;
            int decrementAmount = 38;
            
            // IBlock[] blockList = { new BasicBlock(), new LieBlock(), new ThreatBlock(), new ReligiousBlock() };
            IBlock[] blockList = { new BasicBlock() };
            Dictionary<TutorialState, List<TutorialEntry>> tutorialEntryDict = CreateTutorialEntryDict();
            Dictionary<string, List<GameInfo>> stageDict = CreateStageDict();
            // hardcoding ends here
            
            _turnManager.Initialize();
            if(GameInfoHolder.GetGameInfoList() == null)
            {
                if (ChiefManager.Instance != null && stageDict.TryGetValue(ChiefManager.Instance.per_Scene_ID, out List<GameInfo> gameInfoList))
                {
                    GameInfoHolder.SetGameInfoList(gameInfoList);
                }
                else
                {
                    Debug.LogWarning("designated scene id does not exist in GameInfoList; scene id: " + ChiefManager.Instance?.per_Scene_ID);
                    if(_gameInfoList != null && _gameInfoList.Count > 0)
                    {
                        Debug.LogWarning("using temporary gameinfo instead");
                        GameInfoHolder.SetGameInfoList(_gameInfoList);
                    }
                }
            }
            else if(_gameInfoList != null && _gameInfoList.Count > 0)
            {
                GameInfoHolder.SetGameInfoList(_gameInfoList);
            }
            else if (EditorInfoHolder.GetGameInfo() != null)
            {
                GameInfoHolder.SetGameInfo(EditorInfoHolder.GetGameInfo());
            }
            
            _dialogueManager.Initialize();
            _blockSelectionManager.Initialize(blockList.ToList());
            _boardController.Initialize();
            _suspicionManager.Initialize(maxSuspicion, decrementAmount);
            _winConditionManager.Initialize();
            _gameStateManager.Initialize();
            _tutorialController.Initialize(tutorialEntryDict);
            
            _turnManager.SetTurnState(TurnState.Start);
            _gameStateManager.SetGameState(GameState.Playing);

            _gameStateManager.RaiseSetGameStateEvent += HandleSetGameStateEvent;
        }

        public void ResetGame()
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

            if (_turnManager == null ||
                _blockSelectionManager == null ||
                _boardController == null ||
                _suspicionManager == null ||
                _winConditionManager == null ||
                _gameStateManager == null ||
                _dialogueManager == null ||
                _tutorialController == null)
            {
                Debug.LogWarning("GameManager could not reset because initialization has not completed.", this);
                return;
            }

            StartCoroutine(ResetCore());
        }

        public void ResetGameAfterDelay(float delaySeconds)
        {
            if (_delayedResetCoroutine != null)
            {
                StopCoroutine(_delayedResetCoroutine);
            }

            if (delaySeconds <= 0f)
            {
                _delayedResetCoroutine = null;
                ResetGame();
                return;
            }

            _delayedResetCoroutine = StartCoroutine(ResetGameAfterDelayCore(delaySeconds));
        }

        private IEnumerator ResetGameAfterDelayCore(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            _delayedResetCoroutine = null;
            ResetGame();
        }

        private IEnumerator ResetCore()
        {
            yield return new WaitForSeconds(0.5f);
            _winConditionManager.BeginReset();

            _gameStateManager.ResetGame();
            _dialogueManager.ResetGame();
            _tutorialController.ResetGame();
            _blockSelectionManager.ResetGame();
            _boardController.ResetGame();
            _suspicionManager.ResetGame();
            _turnManager.ResetGame();

            _winConditionManager.EndReset();

            if (BoardView.Instance is BoardView boardView)
            {
                boardView.ResetGame();
            }

            GameStateView.Instance?.ResetGame();
            BackgroundSuspicionView.Instance?.ResetGame();
            SuspicionView.Instance?.ResetGame();
            FindAnyObjectByType<UIImageView>().ResetGame();
            FindAnyObjectByType<FigureView>()?.ResetGame();
            BlackOutPanelView.Instance?.ResetGame();
            GameOverPopupView.Instance?.ResetGame();
        }

        public void QueueResetGame()
        {
            if (_queuedResetCoroutine != null)
            {
                return;
            }

            _queuedResetCoroutine = StartCoroutine(ResetGameAfterCurrentEvent());
        }

        private IEnumerator ResetGameAfterCurrentEvent()
        {
            yield return null;
            _queuedResetCoroutine = null;
            ResetGame();
        }

        private Dictionary<TutorialState, List<TutorialEntry>> CreateTutorialEntryDict()
        {
            Dictionary<TutorialState, List<TutorialEntry>> tutorialEntryDict =
                new Dictionary<TutorialState, List<TutorialEntry>>();

            bool hasSerializedGroups = _tutorialEntryGroups != null && _tutorialEntryGroups.Count > 0;
            if (hasSerializedGroups)
            {
                foreach (TutorialEntryGroup group in _tutorialEntryGroups)
                {
                    if (group == null)
                    {
                        continue;
                    }

                    AddTutorialEntries(tutorialEntryDict, group.State, group.Entries);
                }

                if (!tutorialEntryDict.ContainsKey(TutorialState.None))
                {
                    tutorialEntryDict.Add(TutorialState.None, new List<TutorialEntry>());
                }

                return tutorialEntryDict;
            }

            return CreateFallbackTutorialEntryDict();
        }
        
        private Dictionary<string, List<GameInfo>> CreateStageDict()
        {
            Dictionary<string, List<GameInfo>> stageDict = new Dictionary<string, List<GameInfo>>();
            foreach(StageEntry stageEntry in _stageList)
            {
                stageDict.Add(stageEntry.MapName, stageEntry.GameInfoList);
            }

            return stageDict;
        }

        private static Dictionary<TutorialState, List<TutorialEntry>> CreateFallbackTutorialEntryDict()
        {
            Dictionary<TutorialState, List<TutorialEntry>> tutorialEntryDict =
                new Dictionary<TutorialState, List<TutorialEntry>>();

            List<TutorialEntry> tutorialEntries1 = new List<TutorialEntry>();
            tutorialEntries1.Add(TutorialEntry.CreateCellEntry(new Vector2Int(2, 3), typeof(ConceptCell)));

            List<TutorialEntry> tutorialEntries2 = new List<TutorialEntry>();
            tutorialEntries2.Add(TutorialEntry.CreateCellEntry(new Vector2Int(4, 3), typeof(ConceptCell)));

            List<TutorialEntry> tutorialEntries3 = new List<TutorialEntry>();
            tutorialEntries3.Add(TutorialEntry.CreateGameObjectEntry("Canvas/RightPanel/ButtonsPanel/EndTurnButton"));

            tutorialEntryDict.Add(TutorialState.PlaceFirstCell, tutorialEntries1);
            tutorialEntryDict.Add(TutorialState.PlaceSecondCell, tutorialEntries2);
            tutorialEntryDict.Add(TutorialState.ExplainEndTurn, tutorialEntries3);
            tutorialEntryDict.Add(TutorialState.None, new List<TutorialEntry>());

            return tutorialEntryDict;
        }

        private static void AddTutorialEntries(
            Dictionary<TutorialState, List<TutorialEntry>> tutorialEntryDict,
            TutorialState state,
            List<TutorialEntry> tutorialEntries)
        {
            if (!tutorialEntryDict.TryGetValue(state, out List<TutorialEntry> entries))
            {
                entries = new List<TutorialEntry>();
                tutorialEntryDict.Add(state, entries);
            }

            if (tutorialEntries == null)
            {
                return;
            }

            foreach (TutorialEntry tutorialEntry in tutorialEntries)
            {
                if (tutorialEntry != null)
                {
                    entries.Add(tutorialEntry);
                }
            }
        }

        private void HandleSetGameStateEvent(System.Object sender, SetGameStateEventArgs e)
        {
            if (e.gameState == GameState.Lost)
            {
                if (GameInfoHolder.GetCurrentGameInfo().GetMapType() == GameInfo.MapType.Dream4 &&
                    _winConditionManager.GetLastDefeatReason() == DefeatReason.Scripted)
                {
                    StartCoroutine(DreamGameOver());
                }
                else
                {
                    // TODO: what happens after game over?
                    //ResetGameAfterDelay(2f);
                }
            }
        }

        private IEnumerator DreamGameOver()
        {
            yield return new WaitForSeconds(5f);
            ToInvestigation();
        }

        private void ToInvestigation()
        {
            Debug.Log("ToInvestigation");
            ChiefManager.Instance?.StartInvestigation();
        }

        [Serializable]
        private class TutorialEntryGroup
        {
            public TutorialState State;
            public List<TutorialEntry> Entries = new List<TutorialEntry>();
        }

        [Serializable]
        private class GameInfoEntry
        {
            public string MapName;
            public GameInfo GameInfo;
        }

        [Serializable]
        private class StageEntry
        {
            public string MapName;
            public List<GameInfo> GameInfoList;
        }
    }
}
