using System;
using SingletonUtils;
using Coord = VectorUtils.Vector2Int;

namespace GamePlay
{
    /// <summary>Adapts successful game events to the lesson runner. Presentation is handled by its view.</summary>
    public sealed class TutorialController : Singleton<TutorialController>, IDisposable
    {
        private readonly TutorialRunner _runner = new TutorialRunner();
        private DialogueManager _dialogue;
        private TurnManager _turn;
        private BoardController _board;
        private BlockSelectionManager _blocks;
        private SuspicionManager _suspicion;
        private TutorialPresenter _presenter;
        private bool _started;
        private bool _restoring;
        private bool _hasPracticeBoard;
        private int _generation;
        public bool IsActive => _runner.IsActive || _restoring;
        public TutorialStep CurrentStep => _runner.Current;

        public void Initialize(DialogueManager dialogue, TurnManager turn, BoardController board,
            BlockSelectionManager blocks, SuspicionManager suspicion)
        {
            _dialogue = dialogue;
            _turn = turn;
            _board = board;
            _blocks = blocks;
            _suspicion = suspicion;
            _presenter = new TutorialPresenter(board);
            _runner.StepChanged += PresentStep;
            _runner.Completed += Complete;
            _board.RaiseCellPlacementEvent += HandlePlacement;
        }

        public void StartGame()
        {
            if (_started) return;
            _started = true;
            var sequence = GameInfoHolder.GetCurrentGameInfo().GetTutorial();
            if (sequence == null) return;
            sequence.Validate();
            _runner.Start(sequence.Steps);
        }

        public void Tick(float deltaTime)
        {
            // Consume only after every board/turn listener and animation has finished its current event.
            if (_turn.GetTurnState() == TurnState.PlayerIdle) _runner.BoardSettled();
        }

        public bool CanPlaceCellAt(Coord coord) => !IsActive || _runner.CanPlace(coord);
        public bool CanClickEndTurn() => !IsActive ||
            (!_runner.IsWaitingForBoard && CurrentStep?.Completion == TutorialCompletion.EndTurn);
        public bool CanClickEndPlacement() => !IsActive;
        public void NotifyEndTurnClicked() => _runner.EndTurn();

        public void ResetGame()
        {
            _generation++;
            _runner.Cancel();
            _started = false;
            _restoring = false;
            _hasPracticeBoard = false;
            _presenter?.Clear();
            _dialogue.Hide();
        }

        public void Dispose()
        {
            ResetGame();
            _runner.StepChanged -= PresentStep;
            _runner.Completed -= Complete;
            _board.RaiseCellPlacementEvent -= HandlePlacement;
            ReleaseInstance();
        }

        private void HandlePlacement(object sender, CellPlacementEventArgs e) => _runner.Placed(e.GetCoord());

        private void PresentStep(TutorialStep step)
        {
            _presenter.Clear();
            if (step.Board != null && step.Board.Rows.Count > 0)
            {
                _hasPracticeBoard = true;
                _board.SetTutorialBoard(step.Board);
            }
            if (step.ResetPracticeCounters)
            {
                _blocks.ResetGame();
                _suspicion.ResetGame();
            }
            _presenter.Show(step);
            int generation = ++_generation;
            if (string.IsNullOrEmpty(step.Text) && string.IsNullOrEmpty(step.Instruction)) _dialogue.Hide();
            else _dialogue.Show(new DialogueEntry("", step.Text, TutorialState.None),
                step.Completion == TutorialCompletion.Next
                    ? (Action)(() => { if (generation == _generation) _runner.Next(); }) : null,
                step.Completion == TutorialCompletion.Next, instruction: step.Instruction, presentation: DialoguePresentation.Tutorial);
        }

        private void Complete()
        {
            _restoring = true;
            _generation++;
            _dialogue.Hide();
            _presenter.Clear();
            if (_hasPracticeBoard)
            {
                _board.ResetGame();
                _blocks.ResetGame();
                _suspicion.ResetGame();
                if (BoardView.Instance is BoardView view) view.ResetGame();
                _turn.ResetGame();
                GameStateView.Instance?.ResetGame();
            }
            _hasPracticeBoard = false;
            _restoring = false;
        }
    }
}
