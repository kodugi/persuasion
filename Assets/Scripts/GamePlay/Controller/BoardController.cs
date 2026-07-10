using System;
using System.Collections.Generic;
using UnityEngine;
using SingletonUtils;

namespace GamePlay
{
    public class BoardController : Singleton<BoardController>
    {
        private Board _board;
        private TurnManager _turnManager;
        private BlockSelectionManager _blockSelectionManager;

        public event EventHandler<CellPlacementEventArgs> RaiseCellPlacementEvent;
        public void Initialize()
        {
            _blockSelectionManager = BlockSelectionManager.Instance;
            _board = new Board(GameInfoManager.GetGameInfo().GetBoard());
            _turnManager = TurnManager.Instance;
            _turnManager.RaiseSetTurnStateEvent += HandleSetTurnStateEvent;
        }

        public void HandleCellPlacementInput(Vector2Int coord)
        {
            TurnState turnState = _turnManager.GetTurnState();
            if (turnState != TurnState.PlayerIdle && turnState != TurnState.PlayerPlacingContinue)
            {
                return;
            }

            if (TutorialController.Instance != null && !TutorialController.Instance.CanPlaceCellAt(coord))
            {
                return;
            }

            IBlock selectedBlock = _blockSelectionManager.GetSelectedBlock();

            if (turnState == TurnState.PlayerIdle)
            {
                CellPlacementResult placementResult = selectedBlock.TryPlacement(_board.GetBoard(), coord);
                if (placementResult.GetSuccess())
                {
                    if (_blockSelectionManager.IsSelectedBlockAvailable())
                    {
                        Type cellType =  selectedBlock.GetCellType();
                        PlayerPlaceCell(coord, cellType);
                        _blockSelectionManager.PlaceSelectedBlock(coord);
                        _turnManager.SetTurnState(selectedBlock is IMultipleBlock ? TurnState.PlayerPlacingContinue : TurnState.PlayerIdle);
                        RaiseCellPlacementEvent.Invoke(this, new CellPlacementEventArgs(coord, cellType));
                    }
                }
                return;
            }

            if (selectedBlock is IMultipleBlock multipleBlock)
            {
                CellPlacementResult placementResult = multipleBlock.TryContinuedPlacement(_board.GetBoard(), coord);
                if (placementResult.GetSuccess())
                {
                    Type cellType =  multipleBlock.GetCellType();
                    PlayerPlaceCell(coord, cellType);
                    _blockSelectionManager.PlaceContinuedBlock(coord);
                    _turnManager.SetTurnState((multipleBlock.InputState == MultipleBlockInputState.AwaitingContinuedPlacement) ? TurnState.PlayerPlacingContinue : TurnState.PlayerIdle);
                    RaiseCellPlacementEvent.Invoke(this, new CellPlacementEventArgs(coord, cellType));
                }
                return;
            }

            Debug.LogError("Selected block does not support continued placement!");
        }

        public bool CanPlaceBlock(IBlock block, Vector2Int coord)
        {
            if (_turnManager.GetTurnState() == TurnState.PlayerIdle)
            {
                return block.TryPlacement(_board.GetBoard(), coord).GetSuccess();
            }
            
            if (_turnManager.GetTurnState() == TurnState.PlayerPlacingContinue &&
                     block is IMultipleBlock multipleBlock)
            {
                return multipleBlock.TryContinuedPlacement(_board.GetBoard(), coord).GetSuccess();
            }

            return false;
        }

        private void PlayerPlaceCell(Vector2Int coord, Type cellType)
        {
            Queue<(Vector2Int, Type)> toFlipQueue = new Queue<(Vector2Int, Type)>();
            toFlipQueue.Enqueue((coord, cellType));
            SetCell(coord, cellType);
            
            Vector2Int[] dirs = {new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1),
                new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)};
            
            Cell originCell = _board.GetCell(coord);
            
            // 약한 생각 처리
            foreach(Vector2Int dir in dirs)
            {
                Vector2Int otherCoord = coord + dir;
                if ( _board.IsWithinBound(otherCoord))
                {
                    Cell otherCell = _board.GetCell(coord + dir);
                    if (otherCell is WeakBlackCell)
                    {
                        toFlipQueue.Enqueue((coord + dir, ((IWeakFlipperCell)originCell).TryFlipWeakCell(otherCell)));
                    }
                }
            }

            while (toFlipQueue.Count > 0)
            {
                (Vector2Int curCoord, Type curCellType) = toFlipQueue.Dequeue();
                SetCell(curCoord, curCellType);
                List<(Vector2Int, Type)> toFlipCoordsAndTypes = PlayerGetToFlipCoordsAndTypes(curCoord);
                foreach ((Vector2Int toFlipCoord, Type toFlipType) in toFlipCoordsAndTypes)
                {
                    toFlipQueue.Enqueue((toFlipCoord, toFlipType));
                }
            }
        }

        private void SetCell(Vector2Int coord, Type cellType)
        {
            Cell cell = CreateCell(coord, cellType);
            _board.SetCell(coord, cell);

            if (BoardView.Instance != null)
            {
                BoardView.Instance.SetCell(coord, cell);
            }
        }

        private void PseudoSetCell(Vector2Int coord, Type cellType, Board board)
        {
            Cell cell = CreateCell(coord, cellType);
            board.SetCell(coord, cell);
        }

        private Cell CreateCell(Vector2Int coord, Type cellType)
        {
            Cell cell = (Cell)Activator.CreateInstance(cellType, _turnManager.GetCurrentTurn(), coord);
            return cell;
        }

        private List<(Vector2Int, Type)> PlayerGetToFlipCoordsAndTypes(Vector2Int origin, Board board)
        {
            // 일반 오셀로 규칙에 따른 처리
            return GetToFlipCoordsAndTypes(origin, typeof(BlackCell), typeof(ConceptCell), board);
        }

        private List<(Vector2Int, Type)> PlayerGetToFlipCoordsAndTypes(Vector2Int origin)
        {
            return PlayerGetToFlipCoordsAndTypes(origin, _board);
        }
        
        private List<(Vector2Int, Type)> GetToFlipCoordsAndTypes(Vector2Int origin, Type targetType, Type otherType, Board board)
        {
            List<(Vector2Int, Type)> toFlipCoordsAndTypes = new List<(Vector2Int, Type)>();
            Cell originCell = board.GetCell(origin);
            Vector2Int[] dirs = {new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1),
            new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)};
            foreach (Vector2Int dir in dirs)
            {
                Vector2Int otherCellCoord = GetNearestOtherCellCoord(origin, dir, otherType, board);
                if (otherCellCoord != null)
                {
                    Cell otherCell = board.GetCell(otherCellCoord);
                    bool canBeFlipped = true;
                    for (Vector2Int cur = new Vector2Int(origin) + dir; cur != otherCellCoord; cur += dir)
                    {
                        Cell cell = board.GetCell(cur);
                        if (targetType.IsAssignableFrom(cell.GetType()))
                        {
                            if (((IFlippableCell)cell).TryBeFlipped(originCell, otherCell) == null)
                            {
                                canBeFlipped = false;
                                break;
                            }
                        }
                        else
                        {
                            canBeFlipped = false;
                            break;
                        }
                    }

                    if (!canBeFlipped)
                    {
                        continue;
                    }

                    for (Vector2Int cur = new Vector2Int(origin) + dir; cur != otherCellCoord; cur += dir)
                    {
                        toFlipCoordsAndTypes.Add((cur, GetFlippedCellType(originCell, otherCell, board.GetCell(cur))));
                    }
                }
            }

            return toFlipCoordsAndTypes;
        }

        private List<(Vector2Int, Type)> GetToFlipCoordsAndTypes(Vector2Int origin, Type targetType, Type otherType)
        {
            return GetToFlipCoordsAndTypes(origin, targetType, otherType, _board);
        }

        private Type GetFlippedCellType(Cell first, Cell second, Cell cellToFlip)
        {
            if(!(first is IFlipperCell && second is IFlipperCell && cellToFlip is IFlippableCell))
            {
                Debug.LogError("input type for GetFlippedCellType not correct");
                return null;
            }

            Type firstType = ((IFlipperCell)first).TryFlip(second, cellToFlip);
            Type secondType = ((IFlipperCell)second).TryFlip(first, cellToFlip);
            Type cellToFlipType = ((IFlippableCell)cellToFlip).TryBeFlipped(first, second);

            int firstPrecedence = ((IFlipperCell)first).FlipperPrecedence;
            int secondPrecedence = ((IFlipperCell)second).FlipperPrecedence;
            int cellToFlipPrecedence = ((IFlippableCell)cellToFlip).FlippedPrecedence;

            int highestPrecedence = Math.Max(firstPrecedence, Math.Max(secondPrecedence, cellToFlipPrecedence));
            bool firstHasHighestPrecedence = firstPrecedence == highestPrecedence;
            bool secondHasHighestPrecedence = secondPrecedence == highestPrecedence;
            bool cellToFlipHasHighestPrecedence = cellToFlipPrecedence == highestPrecedence;

            bool hasDifferentTypeTie =
                (firstHasHighestPrecedence && secondHasHighestPrecedence && firstType != secondType) ||
                (firstHasHighestPrecedence && cellToFlipHasHighestPrecedence && firstType != cellToFlipType) ||
                (secondHasHighestPrecedence && cellToFlipHasHighestPrecedence && secondType != cellToFlipType);

            if (hasDifferentTypeTie)
            {
                Debug.LogWarning("GetFlippedCellType found different flip types with the same precedence. Resolving by first > second > cellToFlip.");
            }

            if (firstHasHighestPrecedence)
            {
                return firstType;
            }

            if (secondHasHighestPrecedence)
            {
                return secondType;
            }

            return cellToFlipType;
        }

        private Vector2Int GetNearestOtherCellCoord(Vector2Int origin, Vector2Int dir, Type cellType,Board board) // ConceptCell 또는 BlackCell
        {
            Vector2Int current = origin + dir;
            while(board.IsWithinBound(current))
            {
                Cell currentCell = board.GetCell(current);
                if (cellType.IsAssignableFrom(currentCell.GetType())) {
                    return current;
                }
                current += dir;
            }
            return null;
        }
        
        private Vector2Int GetNearestOtherCellCoord(Vector2Int origin, Vector2Int dir, Type cellType) // ConceptCell 또는 BlackCell
        {
            return GetNearestOtherCellCoord(origin, dir, cellType, _board);
        }

        public void HandleEnemyTurn()
        {
            EnemyFlipCells();
            _turnManager.SetTurnState(TurnState.End);
        }

        private void EnemyFlipCells()
        {
            List<(Vector2Int, Type)> toFlipCoordsAndTypes = new List<(Vector2Int, Type)> ();
            for(int i = 0; i < _board.GetWidth(); i++) {
                for(int j = 0; j < _board.GetHeight(); j++)
                {
                    if(_board.GetCell(new Vector2Int(i, j)) is BlackCell)
                    {
                        toFlipCoordsAndTypes.AddRange(GetToFlipCoordsAndTypes(new Vector2Int(i, j), typeof(ConceptCell), typeof(BlackCell)));
                    }
                }
            }
            foreach ((Vector2Int coord, Type cellType) in toFlipCoordsAndTypes)
            {
                SetCell(coord, cellType);
            }
        }

        public void HandlePlayerPlacingEnd()
        {
            // TODO: 애니메이션 재생 구현
            _turnManager.SetTurnState(TurnState.PlayerIdle);
        }

        private void HandleSetTurnStateEvent(object sender, SetTurnStateEventArgs e)
        {
            if (_turnManager.GetTurnState() != e.turnState)
            {
                return;
            }

            switch (e.turnState)
            {
                case TurnState.PlayerPlacingEnd:
                    HandlePlayerPlacingEnd();
                    break;
                case TurnState.EnemyIdle:
                    HandleEnemyTurn();
                    break;
                default:
                    break;
            }
        }

        public int GetConvertedBlackCellCount()
        {
            Cell[,] originalBoard = GameInfoManager.GetGameInfo().GetBoard();
            int cnt = 0;
            for(int i = 0; i < _board.GetWidth(); i++)
            {
                for(int j = 0; j < _board.GetHeight(); j++)
                {
                    if (originalBoard[i, j] is BlackCell && _board.GetCell(new Vector2Int(i, j)) is ConceptCell)
                    {
                        cnt++;
                    }
                }
            }
            return cnt;
        }
        
        public bool[,] CanBeReached()
        {
            Board pseudoBoard = new Board(GameInfoManager.GetGameInfo().GetBoard());
            Queue<(Vector2Int, Type, bool)> toFlipQueue = new Queue<(Vector2Int, Type, bool)>();
            IBlock selectedBlock = Activator.CreateInstance(_blockSelectionManager.GetSelectedBlock().GetType()) as IBlock;
            bool[,] canBeReached = new bool[pseudoBoard.GetWidth(), pseudoBoard.GetHeight()];
            bool[,] processedReachableOrigins = new bool[pseudoBoard.GetWidth(), pseudoBoard.GetHeight()];
            
            for (int i = 0; i < pseudoBoard.GetWidth(); i++)
            {
                for (int j = 0; j < pseudoBoard.GetHeight(); j++)
                {
                    Vector2Int coord = new Vector2Int(i, j);
                    Cell currentCell = pseudoBoard.GetCell(coord);

                    selectedBlock.Reset();

                    if (currentCell is ConceptCell)
                    {
                        EnqueueReachableCell(toFlipQueue, canBeReached, coord, currentCell.GetType(), false);
                        
                        if (selectedBlock is IMultipleBlock multipleBlockFromConcept)
                        {
                            multipleBlockFromConcept.RegisterPlacement(coord);
                            EnqueueContinuedPlacementCandidates(pseudoBoard, selectedBlock, multipleBlockFromConcept, toFlipQueue, canBeReached);
                        }
                        
                        continue;
                    }

                    CellPlacementResult cellPlacementResult =
                        selectedBlock.TryPlacement(pseudoBoard.GetBoard(), coord);
                    if (cellPlacementResult.GetSuccess())
                    {
                        EnqueueReachableCell(toFlipQueue, canBeReached, coord, selectedBlock.GetCellType(), true);
                        
                        if (selectedBlock is IMultipleBlock multipleBlock)
                        {
                            multipleBlock.RegisterPlacement(coord);
                            EnqueueContinuedPlacementCandidates(pseudoBoard, selectedBlock, multipleBlock, toFlipQueue, canBeReached);
                        }
                    }

                    if (currentCell is EmptyCell)
                    {
                        canBeReached[i, j] = true;
                    }
                }
            }

            while (toFlipQueue.Count > 0)
            {
                selectedBlock.Reset();
                (Vector2Int curCoord, Type curCellType, bool shouldSetCell) = toFlipQueue.Dequeue();
                if (processedReachableOrigins[curCoord.X, curCoord.Y])
                {
                    continue;
                }

                processedReachableOrigins[curCoord.X, curCoord.Y] = true;
                
                if (shouldSetCell)
                {
                    PseudoSetCell(curCoord, curCellType, pseudoBoard);
                }

                canBeReached[curCoord.X, curCoord.Y] = true;
                
                List<(Vector2Int, Type)> toFlipCoordsAndTypes = PlayerGetToFlipCoordsAndTypes(curCoord, pseudoBoard);
                foreach ((Vector2Int toFlipCoord, Type toFlipType) in toFlipCoordsAndTypes)
                {
                    EnqueueReachableCell(toFlipQueue, canBeReached, toFlipCoord, toFlipType, true);
                    
                    if (selectedBlock is IMultipleBlock multipleBlock)
                    {
                        multipleBlock.RegisterPlacement(toFlipCoord);
                        EnqueueContinuedPlacementCandidates(pseudoBoard, selectedBlock, multipleBlock, toFlipQueue, canBeReached);
                    }
                }
            }

            return canBeReached;
        }

        private void EnqueueReachableCell(
            Queue<(Vector2Int, Type, bool)> queue,
            bool[,] canBeReached,
            Vector2Int coord,
            Type cellType,
            bool shouldSetCell)
        {
            queue.Enqueue((coord, cellType, shouldSetCell));
            canBeReached[coord.X, coord.Y] = true;
        }

        private void EnqueueContinuedPlacementCandidates(
            Board pseudoBoard,
            IBlock selectedBlock,
            IMultipleBlock multipleBlock,
            Queue<(Vector2Int, Type, bool)> queue,
            bool[,] canBeReached)
        {
            for (int k = 0; k < pseudoBoard.GetWidth(); k++)
            {
                for (int l = 0; l < pseudoBoard.GetHeight(); l++)
                {
                    Vector2Int coord = new Vector2Int(k, l);
                    CellPlacementResult multipleCellPlacementResult = multipleBlock.TryContinuedPlacement(pseudoBoard.GetBoard(), coord);
                    if (multipleCellPlacementResult.GetSuccess())
                    {
                        EnqueueReachableCell(queue, canBeReached, coord, selectedBlock.GetCellType(), true);
                    }
                }
            }
        }
    }
    
    public class CellPlacementEventArgs: EventArgs{
        private Vector2Int _coord;
        private Type _cellType;
        public CellPlacementEventArgs(Vector2Int coord, Type cellType)
        {
            _coord = coord;
            _cellType = cellType;
        }

        public Vector2Int GetCoord()
        {
            return _coord;
        }

        public Type GetCellType()
        {
            return _cellType;
        }
    }
}
