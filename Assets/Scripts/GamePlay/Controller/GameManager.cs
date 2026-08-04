using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using MapEditor.Model;
using UnityEngine;
using SingletonUtils;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameInfo _gameInfo;
        [SerializeField] private List<TutorialEntryGroup> _tutorialEntryGroups = new List<TutorialEntryGroup>();
        [SerializeField] private List<GameInfoEntry> _gameInfoList = new List<GameInfoEntry>();
        
        private TurnManager _turnManager;
        private BlockSelectionManager _blockSelectionManager;
        private BoardController _boardController;
        private SuspicionManager _suspicionManager;
        private WinConditionManager _winConditionManager;
        private GameStateManager  _gameStateManager;
        private DialogueManager _dialogueManager;
        private TutorialController _tutorialController;

        private void Awake()
        {
            Initialize();
        }

        private void ResetGame()
        {
            //Initialize();
            SceneManager.LoadScene("GamePlayScene");
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
            Dictionary<string, GameInfo> gameInfoDict = CreateGameInfoDict();
            // hardcoding ends here
            
            _turnManager.Initialize();
            if(GameInfoHolder.GetGameInfo() == null)
            {
                if (ChiefManager.Instance != null && gameInfoDict.TryGetValue(ChiefManager.Instance.per_Scene_ID, out GameInfo gameInfo))
                {
                    GameInfoHolder.SetGameInfo(gameInfo);
                }
                else
                {
                    Debug.LogWarning("designated scene id does not exist in GameInfoList; scene id: " + ChiefManager.Instance?.per_Scene_ID);
                    if(_gameInfo != null)
                    {
                        Debug.LogWarning("using temporary gameinfo instead");
                        GameInfoHolder.SetGameInfo(_gameInfo);
                    }
                }
            }
            else if(_gameInfo != null)
            {
                GameInfoHolder.SetGameInfo(_gameInfo);
            }
            else if (EditorInfoHolder.GetGameInfo() != null)
            {
                GameInfoHolder.SetGameInfo(EditorInfoHolder.GetGameInfo());
            }
            
            _dialogueManager.Initialize(GameInfoHolder.GetGameInfo().GetDialogueDataDict());
            _blockSelectionManager.Initialize(blockList.ToList());
            _boardController.Initialize();
            _suspicionManager.Initialize(maxSuspicion, decrementAmount);
            _winConditionManager.Initialize();
            _gameStateManager.Initialize();
            _tutorialController.Initialize(tutorialEntryDict);
            
            _turnManager.SetTurnState(TurnState.Start);
            _gameStateManager.SetGameState(GameState.Playing);
        }

        // Use this for initialization
        void Start()
        {
            ReplayButtonView.Instance.gameObject.GetComponent<Button>().onClick.AddListener(ResetGame);
        }

        // Update is called once per frame
        void Update()
        {

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

        private Dictionary<string, GameInfo> CreateGameInfoDict()
        {
            Dictionary<string, GameInfo> gameInfoDict = new Dictionary<string, GameInfo>();
            foreach(GameInfoEntry gameInfoEntry in _gameInfoList)
            {
                gameInfoDict.Add(gameInfoEntry.MapName, gameInfoEntry.GameInfo);
            }

            return gameInfoDict;
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
    }
}
