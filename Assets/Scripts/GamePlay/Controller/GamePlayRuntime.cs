using System;
using System.Collections.Generic;

namespace GamePlay
{
    /// <summary>
    /// Owns the plain C# services used during one GamePlayScene lifetime.
    /// </summary>
    internal sealed class GamePlayRuntime : IDisposable
    {
        public TurnManager Turn { get; } = new TurnManager();
        public BlockSelectionManager BlockSelection { get; } = new BlockSelectionManager();
        public BoardController Board { get; } = new BoardController();
        public SuspicionManager Suspicion { get; } = new SuspicionManager();
        public WinConditionManager WinCondition { get; } = new WinConditionManager();
        public GameStateManager State { get; } = new GameStateManager();
        public DialogueManager Dialogue { get; } = new DialogueManager();
        public TutorialController Tutorial { get; } = new TutorialController();

        public bool IsInitialized { get; private set; }
        public bool IsStarted { get; private set; }

        public void Initialize(
            List<IBlock> blocks,
            Dictionary<TutorialState, List<TutorialEntry>> tutorialEntries,
            int maxSuspicion,
            int suspicionDecrementPerTurn)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("GamePlayRuntime has already been initialized.");
            }

            State.Initialize();
            Turn.Initialize(State);
            Dialogue.Initialize(Turn, Tutorial);
            BlockSelection.Initialize(blocks, Turn);
            Board.Initialize(Turn, BlockSelection, Tutorial);
            Suspicion.Initialize(maxSuspicion, suspicionDecrementPerTurn, BlockSelection, Turn);
            Tutorial.Initialize(tutorialEntries, Dialogue, Turn, Board);
            WinCondition.Initialize(Board, Suspicion, State, Turn, Tutorial);

            IsInitialized = true;
        }

        public void StartGame(BlockSelectionView blockSelectionView)
        {
            EnsureInitialized();
            if (IsStarted)
            {
                return;
            }

            BlockSelection.AttachView(blockSelectionView);
            IsStarted = true;
            Turn.SetTurnState(TurnState.Start);
            State.SetGameState(GameState.Playing);
        }

        public void BeginReset()
        {
            EnsureInitialized();
            WinCondition.BeginReset();
            State.ResetGame();
            Dialogue.ResetGame();
            Tutorial.ResetGame();
            Board.ResetGame();
            BlockSelection.ResetGame();
            Suspicion.ResetGame();
        }

        public void EndReset()
        {
            EnsureInitialized();
            Turn.ResetGame();
            WinCondition.EndReset();
        }

        public void Dispose()
        {
            WinCondition.Dispose();
            Tutorial.Dispose();
            Suspicion.Dispose();
            Board.Dispose();
            BlockSelection.Dispose();
            Dialogue.Dispose();
            Turn.Dispose();
            State.Dispose();
            IsInitialized = false;
            IsStarted = false;
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("GamePlayRuntime is not initialized.");
            }
        }
    }
}
