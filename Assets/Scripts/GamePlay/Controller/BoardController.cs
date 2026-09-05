using System;
using System.Collections.Generic;
using System.Linq;
using SingletonUtils;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    /// <summary>
    /// Coordinates board input, turn state, and board presentation.
    /// Board rule calculations live in the resolver classes under GamePlay/Rules.
    /// </summary>
    public class BoardController : Singleton<BoardController>, IDisposable
    {
        private readonly BoardTransitionResolver _transitionResolver = new BoardTransitionResolver();
        private readonly BoardReachabilityAnalyzer _reachabilityAnalyzer = new BoardReachabilityAnalyzer();

        private Board _board;
        private TurnManager _turnManager;
        private BlockSelectionManager _blockSelectionManager;
        private TutorialController _tutorialController;

        public event EventHandler<CellPlacementEventArgs> RaiseCellPlacementEvent;

        public void Initialize(
            TurnManager turnManager,
            BlockSelectionManager blockSelectionManager,
            TutorialController tutorialController)
        {
            _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
            _blockSelectionManager = blockSelectionManager ??
                                     throw new ArgumentNullException(nameof(blockSelectionManager));
            _tutorialController = tutorialController ?? throw new ArgumentNullException(nameof(tutorialController));
            _board = CreateBoardFromCurrentGameInfo();
            _turnManager.RaiseSetTurnStateEvent += HandleSetTurnStateEvent;
        }

        public void Dispose()
        {
            if (_turnManager != null)
            {
                _turnManager.RaiseSetTurnStateEvent -= HandleSetTurnStateEvent;
            }

            RaiseCellPlacementEvent = null;
            _board = null;
            _turnManager = null;
            _blockSelectionManager = null;
            _tutorialController = null;
            ReleaseInstance();
        }

        public void ResetGame()
        {
            _board = CreateBoardFromCurrentGameInfo();
        }

        public void HandleCellPlacementInput(Vector2Int coord)
        {
            if (!CanHandlePlacementInput(coord))
            {
                return;
            }

            IBlock selectedBlock = _blockSelectionManager.GetSelectedBlock();
            TurnState turnState = _turnManager.GetTurnState();

            if (turnState == TurnState.PlayerIdle)
            {
                TryPlaceInitialCell(selectedBlock, coord);
                return;
            }

            if (selectedBlock is IMultipleBlock multipleBlock)
            {
                TryPlaceContinuedCell(multipleBlock, coord);
                return;
            }

            Debug.LogError("The selected block does not support continued placement.");
        }

        public bool CanPlaceBlock(IBlock block, Vector2Int coord)
        {
            if (block == null || coord == null || !_board.IsWithinBound(coord))
            {
                return false;
            }

            TurnState turnState = _turnManager.GetTurnState();
            if (turnState == TurnState.PlayerIdle)
            {
                return block.TryPlacement(_board.GetBoard(), coord).GetSuccess();
            }

            return turnState == TurnState.PlayerPlacingContinue &&
                   block is IMultipleBlock multipleBlock &&
                   multipleBlock.TryContinuedPlacement(_board.GetBoard(), coord).GetSuccess();
        }

        public int GetConvertedBlackCellCount()
        {
            Cell[,] originalBoard = GameInfoHolder.GetCurrentGameInfo().GetBoard();
            int convertedCount = 0;

            for (int x = 0; x < _board.GetWidth(); x++)
            {
                for (int y = 0; y < _board.GetHeight(); y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    if (originalBoard[x, y] is BlackCell && _board.GetCell(coord) is ConceptCell)
                    {
                        convertedCount++;
                    }
                }
            }

            return convertedCount;
        }

        public bool[,] CanBeReached()
        {
            IBlock selectedBlock = _blockSelectionManager.GetSelectedBlock();
            return _reachabilityAnalyzer.Analyze(
                GameInfoHolder.GetCurrentGameInfo().GetBoard(),
                selectedBlock.GetType(),
                _turnManager.GetCurrentTurn());
        }

        public List<Vector2Int> GetRandomBlackCellCoords(int count)
        {
            List<Vector2Int> blackCellCoords = new List<Vector2Int>();
            for (int x = 0; x < _board.GetWidth(); x++)
            {
                for (int y = 0; y < _board.GetHeight(); y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    if (_board.GetCell(coord) is BlackCell)
                    {
                        blackCellCoords.Add(coord);
                    }
                }
            }

            return blackCellCoords
                .OrderBy(_ => Guid.NewGuid())
                .Take(Math.Min(blackCellCoords.Count, count))
                .ToList();
        }

        private static Board CreateBoardFromCurrentGameInfo()
        {
            GameInfo gameInfo = GameInfoHolder.GetCurrentGameInfo();
            if (gameInfo == null)
            {
                throw new InvalidOperationException("A GameInfo must be selected before initializing the board.");
            }

            return new Board(gameInfo.GetBoard());
        }

        private bool CanHandlePlacementInput(Vector2Int coord)
        {
            TurnState turnState = _turnManager.GetTurnState();
            if (coord == null || !_board.IsWithinBound(coord) ||
                (turnState != TurnState.PlayerIdle && turnState != TurnState.PlayerPlacingContinue))
            {
                return false;
            }

            return _tutorialController.CanPlaceCellAt(coord);
        }

        private void TryPlaceInitialCell(IBlock selectedBlock, Vector2Int coord)
        {
            if (!selectedBlock.TryPlacement(_board.GetBoard(), coord).GetSuccess() ||
                !_blockSelectionManager.IsSelectedBlockAvailable())
            {
                return;
            }

            TurnState nextState = selectedBlock is IMultipleBlock
                ? TurnState.PlayerPlacingContinue
                : TurnState.PlayerIdle;

            PlaceCell(
                new CellChange(coord, selectedBlock.GetCellType()),
                nextState,
                () => _blockSelectionManager.PlaceSelectedBlock(coord));
        }

        private void TryPlaceContinuedCell(IMultipleBlock multipleBlock, Vector2Int coord)
        {
            if (!multipleBlock.TryContinuedPlacement(_board.GetBoard(), coord).GetSuccess())
            {
                return;
            }

            TurnState nextState = multipleBlock.InputState == MultipleBlockInputState.AwaitingContinuedPlacement
                ? TurnState.PlayerPlacingContinue
                : TurnState.PlayerIdle;

            PlaceCell(
                new CellChange(coord, multipleBlock.GetCellType()),
                nextState,
                () => _blockSelectionManager.PlaceContinuedBlock(coord));
        }

        private void PlaceCell(CellChange placement, TurnState nextState, Action registerPlacement)
        {
            List<CellChange> changes = _transitionResolver.ApplyPlayerPlacement(
                _board,
                placement,
                _turnManager.GetCurrentTurn());

            PresentCellChanges(changes, nextState);
            registerPlacement();
            GamePlaySoundManager.Instance?.Play(GamePlaySoundId.SoulPlace);
            RaiseCellPlacementEvent?.Invoke(this, new CellPlacementEventArgs(placement));
        }

        private void HandleEnemyTurn()
        {
            List<CellChange> changes = _transitionResolver.ApplyEnemyTurn(
                _board,
                _turnManager.GetCurrentTurn());
            PresentCellChanges(changes, TurnState.End);
        }

        private static void PresentCellChanges(List<CellChange> changes, TurnState nextState)
        {
            if (!(BoardView.Instance is BoardView boardView))
            {
                return;
            }

            boardView.SetTurnStateAfterTransition(nextState);
            boardView.SetCell(CellChangeBatcher.ByAnimationDistance(changes));
        }

        private void HandleSetTurnStateEvent(object sender, SetTurnStateEventArgs eventArgs)
        {
            if (_turnManager.GetTurnState() != eventArgs.turnState)
            {
                return;
            }

            switch (eventArgs.turnState)
            {
                case TurnState.PlayerPlacingEnd:
                    _turnManager.SetTurnState(TurnState.PlayerIdle);
                    break;
                case TurnState.EnemyIdle:
                    HandleEnemyTurn();
                    break;
            }
        }
    }
}
