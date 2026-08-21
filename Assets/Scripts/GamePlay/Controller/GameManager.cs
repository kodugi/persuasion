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

        [Header("GamePlay Audio Clips")]
        [SerializeField] private AudioClip _jumpScareClip;
        [SerializeField] private AudioClip _gameOverClip;
        [SerializeField] private List<AudioClip> _laughClips = new List<AudioClip>();
        [SerializeField] private AudioClip _placeSoulClip;
        [SerializeField] private AudioClip _eyeClip;
        [SerializeField] private AudioClip _bigEyeClip;
        [SerializeField] private AudioClip _glitchClip;

        [Header("GamePlay Audio Timing")]
        [SerializeField, Range(0f, 1f)] private float _effectVolume = 0.85f;
        [SerializeField, Range(0f, 1f)] private float _gameOverVolume = 0.85f;
        [SerializeField, Range(0f, 1f)] private float _laughVolume = 0.9f;
        [SerializeField, Range(0f, 1f)] private float _jumpScareVolume = 1f;
        [SerializeField, Min(0f)] private float _scriptedDreamJumpScareDelay = 2.2f;
        [SerializeField, Min(0f)] private float _scriptedDreamJumpScareDuration = 0.8f;
        
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
        private AudioSource _effectAudioSource;
        private AudioSource _gameOverAudioSource;
        private AudioSource _laughAudioSource;
        private AudioSource _jumpScareAudioSource;
        private Coroutine _laughCoroutine;
        private Coroutine _jumpScareStopCoroutine;
        private Coroutine _scriptedDreamJumpScareCoroutine;
        private bool _audioLockedForJumpScare;
        private bool _didPauseAudioListener;
        private bool _audioListenerWasPaused;

        protected override void Awake()
        {
            base.Awake();
            Initialize();
            InitializeAudio();
        }

        protected override void OnDestroy()
        {
            DisposeAudio();
            _dialogueManager?.ClearPlaybackHistory();
            base.OnDestroy();
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
            if (ChiefManager.Instance != null &&
                stageDict.TryGetValue(ChiefManager.Instance.per_Scene_ID, out List<GameInfo> gameInfoList))
            {
                // A scene transition explicitly selects a stage. Always honor it even when
                // GameInfoHolder still contains data from the previous persuasion scene.
                GameInfoHolder.SetGameInfoList(gameInfoList);
            }
            else if(GameInfoHolder.GetGameInfoList() == null)
            {
                Debug.LogWarning("designated scene id does not exist in GameInfoList; scene id: " + ChiefManager.Instance?.per_Scene_ID);
                if(_gameInfoList != null && _gameInfoList.Count > 0)
                {
                    Debug.LogWarning("using temporary gameinfo instead");
                    GameInfoHolder.SetGameInfoList(_gameInfoList);
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
            _winConditionManager.RaiseDefeatEvent += HandleDefeatEvent;
        }

        public void ResetGame()
        {
            ResetAudioPlayback();

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

        public void PlayEyeSound()
        {
            PlayEffect(_eyeClip);
        }

        public void PlayBigEyeSound()
        {
            PlayEffect(_bigEyeClip);
        }

        public void PlayGlitchSound()
        {
            PlayEffect(_glitchClip);
        }

        public void PlayJumpScareSound(float duration)
        {
            if (_audioLockedForJumpScare || _jumpScareClip == null)
            {
                return;
            }

            _audioLockedForJumpScare = true;
            StopLoopingAudio();

            AudioSource[] activeAudioSources = FindObjectsByType<AudioSource>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (AudioSource audioSource in activeAudioSources)
            {
                audioSource.Stop();
            }

            _audioListenerWasPaused = AudioListener.pause;
            _didPauseAudioListener = true;
            AudioListener.pause = true;

            _jumpScareAudioSource.clip = _jumpScareClip;
            _jumpScareAudioSource.volume = _jumpScareVolume;
            _jumpScareAudioSource.Play();

            if (_jumpScareStopCoroutine != null)
            {
                StopCoroutine(_jumpScareStopCoroutine);
            }

            _jumpScareStopCoroutine = StartCoroutine(StopJumpScareAfterDuration(duration));
        }

        private void InitializeAudio()
        {
            _effectAudioSource = CreateAudioSource();
            _gameOverAudioSource = CreateAudioSource();
            _laughAudioSource = CreateAudioSource();
            _jumpScareAudioSource = CreateAudioSource();
            _jumpScareAudioSource.ignoreListenerPause = true;

            _boardController.RaiseCellPlacementEvent += HandleAudioCellPlacementEvent;
            _dialogueManager.RaiseSetDialogueEntryEvent += HandleAudioDialogueEntryEvent;
            _gameStateManager.RaiseSetGameStateEvent += HandleAudioGameStateEvent;
            ResetAudioPlayback();

            GameInfo gameInfo = GameInfoHolder.GetCurrentGameInfo();
            if (_dialogueManager.HasCurrentDialogueData() &&
                gameInfo != null &&
                gameInfo.GetMapType() == GameInfo.MapType.Dream4)
            {
                StartLaughter();
            }
        }

        private AudioSource CreateAudioSource()
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            return audioSource;
        }

        private void DisposeAudio()
        {
            if (_boardController != null)
            {
                _boardController.RaiseCellPlacementEvent -= HandleAudioCellPlacementEvent;
            }

            if (_dialogueManager != null)
            {
                _dialogueManager.RaiseSetDialogueEntryEvent -= HandleAudioDialogueEntryEvent;
            }

            if (_gameStateManager != null)
            {
                _gameStateManager.RaiseSetGameStateEvent -= HandleAudioGameStateEvent;
            }

            ResetAudioPlayback();
        }

        private void HandleAudioCellPlacementEvent(object sender, CellPlacementEventArgs e)
        {
            PlayEffect(_placeSoulClip);
        }

        private void HandleAudioDialogueEntryEvent(object sender, SetDialogueEntryEventArgs e)
        {
            GameInfo gameInfo = GameInfoHolder.GetCurrentGameInfo();
            if (gameInfo != null && gameInfo.GetMapType() == GameInfo.MapType.Dream4)
            {
                StartLaughter();
            }
        }

        private void HandleAudioGameStateEvent(object sender, SetGameStateEventArgs e)
        {
            if (e.gameState == GameState.Playing)
            {
                ResetAudioPlayback();
                return;
            }

            if (e.gameState != GameState.Lost)
            {
                return;
            }

            GameInfo gameInfo = GameInfoHolder.GetCurrentGameInfo();
            if (gameInfo == null)
            {
                return;
            }

            DefeatReason defeatReason = _winConditionManager.GetLastDefeatReason();
            if (gameInfo.GetMapType() == GameInfo.MapType.Normal &&
                defeatReason == DefeatReason.SuspicionOverflow)
            {
                StartGameOverLoop();
            }
            else if (gameInfo.GetMapType() == GameInfo.MapType.Dream4 &&
                     defeatReason == DefeatReason.Scripted)
            {
                if (_scriptedDreamJumpScareCoroutine != null)
                {
                    StopCoroutine(_scriptedDreamJumpScareCoroutine);
                }

                _scriptedDreamJumpScareCoroutine = StartCoroutine(PlayScriptedDreamJumpScare());
            }
        }

        private void PlayEffect(AudioClip clip)
        {
            if (_audioLockedForJumpScare || clip == null || _effectAudioSource == null)
            {
                return;
            }

            _effectAudioSource.PlayOneShot(clip, _effectVolume);
        }

        private void StartGameOverLoop()
        {
            if (_audioLockedForJumpScare || _gameOverClip == null || _gameOverAudioSource.isPlaying)
            {
                return;
            }

            _gameOverAudioSource.clip = _gameOverClip;
            _gameOverAudioSource.volume = _gameOverVolume;
            _gameOverAudioSource.loop = true;
            _gameOverAudioSource.Play();
        }

        private void StartLaughter()
        {
            if (_audioLockedForJumpScare || _laughCoroutine != null ||
                _laughClips == null || !_laughClips.Any(clip => clip != null))
            {
                return;
            }

            _laughCoroutine = StartCoroutine(PlayLaughterAlternately());
        }

        private IEnumerator PlayLaughterAlternately()
        {
            int clipIndex = 0;
            while (!_audioLockedForJumpScare)
            {
                AudioClip clip = GetNextLaughClip(ref clipIndex);
                if (clip == null)
                {
                    break;
                }

                _laughAudioSource.clip = clip;
                _laughAudioSource.volume = _laughVolume;
                _laughAudioSource.Play();
                yield return new WaitWhile(() => _laughAudioSource.isPlaying);
            }

            _laughAudioSource.Stop();
            _laughCoroutine = null;
        }

        private AudioClip GetNextLaughClip(ref int clipIndex)
        {
            for (int i = 0; i < _laughClips.Count; i++)
            {
                AudioClip clip = _laughClips[clipIndex % _laughClips.Count];
                clipIndex++;
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private IEnumerator PlayScriptedDreamJumpScare()
        {
            if (_scriptedDreamJumpScareDelay > 0f)
            {
                yield return new WaitForSeconds(_scriptedDreamJumpScareDelay);
            }

            _scriptedDreamJumpScareCoroutine = null;
            PlayJumpScareSound(_scriptedDreamJumpScareDuration);
        }

        private IEnumerator StopJumpScareAfterDuration(float duration)
        {
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            _jumpScareAudioSource.Stop();
            _jumpScareStopCoroutine = null;
        }

        private void StopLoopingAudio()
        {
            if (_laughCoroutine != null)
            {
                StopCoroutine(_laughCoroutine);
                _laughCoroutine = null;
            }

            _laughAudioSource?.Stop();
            _gameOverAudioSource?.Stop();
        }

        private void ResetAudioPlayback()
        {
            if (_scriptedDreamJumpScareCoroutine != null)
            {
                StopCoroutine(_scriptedDreamJumpScareCoroutine);
                _scriptedDreamJumpScareCoroutine = null;
            }

            if (_jumpScareStopCoroutine != null)
            {
                StopCoroutine(_jumpScareStopCoroutine);
                _jumpScareStopCoroutine = null;
            }

            StopLoopingAudio();
            _effectAudioSource?.Stop();
            _jumpScareAudioSource?.Stop();
            _audioLockedForJumpScare = false;

            if (_didPauseAudioListener)
            {
                AudioListener.pause = _audioListenerWasPaused;
                _didPauseAudioListener = false;
            }
        }

        public void QueueResetGame()
        {
            if (_queuedResetCoroutine != null)
            {
                return;
            }

            _queuedResetCoroutine = StartCoroutine(ResetGameAfterCurrentEvent());
        }

        public bool TryPlayGameOverDialogue(Action onCompleted)
        {
            GameInfo gameInfo = GameInfoHolder.GetCurrentGameInfo();
            return gameInfo != null &&
                   gameInfo.TryGetGameOverDialogue(out DialogueData dialogueData) &&
                   _dialogueManager.TryPlayDialogue(dialogueData, onCompleted, false);
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

        private void HandleDefeatEvent(object sender, DefeatEventArgs e)
        {
            if (e.Reason != DefeatReason.DreamRetry)
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
