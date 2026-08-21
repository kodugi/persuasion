using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SingletonUtils;
using Vector2Int = VectorUtils.Vector2Int;

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
            _board = new Board(GameInfoHolder.GetCurrentGameInfo().GetBoard());
            _turnManager = TurnManager.Instance;
            _turnManager.RaiseSetTurnStateEvent += HandleSetTurnStateEvent;
        }

        public void ResetGame()
        {
            _board = new Board(GameInfoHolder.GetCurrentGameInfo().GetBoard());
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
                        CellChange cellChange = new CellChange(coord, cellType, coord, coord);
                        PlayerPlaceCell(cellChange, selectedBlock is IMultipleBlock ? TurnState.PlayerPlacingContinue : TurnState.PlayerIdle);
                        _blockSelectionManager.PlaceSelectedBlock(coord);
                        GamePlaySoundManager.Instance?.Play(GamePlaySoundId.SoulPlace);
                        RaiseCellPlacementEvent?.Invoke(this, new CellPlacementEventArgs(cellChange));
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
                    CellChange cellChange = new CellChange(coord, cellType, coord, coord);
                    PlayerPlaceCell(cellChange, (multipleBlock.InputState == MultipleBlockInputState.AwaitingContinuedPlacement) ? TurnState.PlayerPlacingContinue : TurnState.PlayerIdle);
                    _blockSelectionManager.PlaceContinuedBlock(coord);
                    GamePlaySoundManager.Instance?.Play(GamePlaySoundId.SoulPlace);
                    RaiseCellPlacementEvent?.Invoke(this, new CellPlacementEventArgs(cellChange));
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

        private void PlayerPlaceCell(CellChange placedCellChange, TurnState nextTurnState)
        {
            Vector2Int coord = placedCellChange.GetCoord();
            Queue<CellChange> toFlipQueue = new Queue<CellChange>();
            bool[,] visited = new bool[_board.GetWidth(), _board.GetHeight()];
            EnqueueCellChangeIfUnvisited(toFlipQueue, visited, placedCellChange);
            SetCell(placedCellChange);
            
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
                        // TODO: needs more discussion on how to flip weak cells
                        EnqueueCellChangeIfUnvisited(
                            toFlipQueue,
                            visited,
                            new CellChange(coord + dir, ((IWeakFlipperCell)originCell).TryFlipWeakCell(otherCell), coord, coord + dir));
                    }
                }
            }

            List<CellChange> cellChangeList = new List<CellChange>();
            while (toFlipQueue.Count > 0)
            {
                CellChange curCellChange = toFlipQueue.Dequeue();
                Vector2Int curCoord = curCellChange.GetCoord();
                cellChangeList.Add(curCellChange);
                SetCell(curCellChange);
                List<CellChange> toFlipCellChanges = PlayerGetToFlipCellChanges(curCoord);
                foreach (CellChange toFlipCellChange in toFlipCellChanges)
                {
                    EnqueueCellChangeIfUnvisited(toFlipQueue, visited, toFlipCellChange);
                }
            }
            
            SendCellChanges(cellChangeList, nextTurnState);
        }

        private bool EnqueueCellChangeIfUnvisited(Queue<CellChange> queue, bool[,] visited, CellChange cellChange)
        {
            if (cellChange == null)
            {
                return false;
            }

            Vector2Int coord = cellChange.GetCoord();
            if (coord == null
                || coord.X < 0
                || coord.X >= visited.GetLength(0)
                || coord.Y < 0
                || coord.Y >= visited.GetLength(1)
                || visited[coord.X, coord.Y])
            {
                return false;
            }

            visited[coord.X, coord.Y] = true;
            queue.Enqueue(cellChange);
            return true;
        }

        private void SetCell(CellChange cellChange)
        {
            Vector2Int coord = cellChange.GetCoord();
            Cell cell = CreateCell(coord, cellChange.GetCellType());
            _board.SetCell(coord, cell);
        }

        private void SendCellChanges(List<CellChange> cellChangeList, TurnState nextState)
        {
            if (BoardView.Instance != null)
            {
                ((BoardView)BoardView.Instance).SetTurnStateAfterTransition(nextState);
                SortedDictionary<int, List<CellChange>> cellChangeDict = new SortedDictionary<int, List<CellChange>>();
                foreach (CellChange cellChange in cellChangeList)
                {
                    int val = Math.Min(Vector2Int.TaxiDist(cellChange.GetOriginalCellCoord(), cellChange.GetCoord()),
                        Vector2Int.TaxiDist(cellChange.GetOtherCellCoord(), cellChange.GetCoord()));

                    if (!cellChangeDict.ContainsKey(val))
                    {
                        cellChangeDict[val] = new List<CellChange>();
                    }
                    cellChangeDict[val].Add(cellChange);
                }

                List<List<CellChange>> result = new List<List<CellChange>>();
                foreach (var x in cellChangeDict)
                {
                    result.Add(x.Value);
                }
                BoardView.Instance.SetCell(result);
            }
        }

        private void PseudoSetCell(CellChange cellChange, Board board)
        {
            board.SetCell(cellChange.GetCoord(), CreateCell(cellChange.GetCoord(), cellChange.GetCellType()));
        }

        private Cell CreateCell(Vector2Int coord, Type cellType)
        {
            Cell cell = (Cell)Activator.CreateInstance(cellType, _turnManager.GetCurrentTurn(), coord);
            return cell;
        }

        private List<CellChange> PlayerGetToFlipCellChanges(Vector2Int origin, Board board)
        {
            return GetToFlipCellChanges(origin, typeof(BlackCell), typeof(ConceptCell), board);
        }

        private List<CellChange> PlayerGetToFlipCellChanges(Vector2Int origin)
        {
            return PlayerGetToFlipCellChanges(origin, _board);
        }
        
        private List<CellChange> GetToFlipCellChanges(Vector2Int origin, Type targetType, Type otherType, Board board)
        {
            List<CellChange> toFlipCellChanges = new List<CellChange>();
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
                        toFlipCellChanges.Add(new CellChange(cur, GetFlippedCellType(originCell, otherCell, board.GetCell(cur)), origin, otherCellCoord));
                    }
                }
            }

            return toFlipCellChanges;
        }

        private List<CellChange> GetToFlipCellChanges(Vector2Int origin, Type targetType, Type otherType)
        {
            return GetToFlipCellChanges(origin, targetType, otherType, _board);
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
        }

        private void EnemyFlipCells()
        {
            List<CellChange> toFlipCellChanges = new List<CellChange> ();
            for(int i = 0; i < _board.GetWidth(); i++) {
                for(int j = 0; j < _board.GetHeight(); j++)
                {
                    if(_board.GetCell(new Vector2Int(i, j)) is BlackCell)
                    {
                        toFlipCellChanges.AddRange(GetToFlipCellChanges(new Vector2Int(i, j), typeof(ConceptCell), typeof(BlackCell)));
                    }
                }
            }

            List<CellChange> cellChangeList = new List<CellChange>();
            foreach (CellChange toFlipCellChange in toFlipCellChanges)
            {
                Vector2Int coord = toFlipCellChange.GetCoord();
                Type cellType = toFlipCellChange.GetCellType();
                if (_board.GetCell(coord).GetType() != cellType)
                {
                    SetCell(toFlipCellChange);
                    cellChangeList.Add(toFlipCellChange);
                }
            }
            
            SendCellChanges(cellChangeList, TurnState.End);
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
            Cell[,] originalBoard = GameInfoHolder.GetCurrentGameInfo().GetBoard();
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
            Board pseudoBoard = new Board(GameInfoHolder.GetCurrentGameInfo().GetBoard());
            Queue<ReachableCellCandidate> toFlipQueue = new Queue<ReachableCellCandidate>();
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
                        EnqueueReachableCell(toFlipQueue, canBeReached, new CellChange(coord, currentCell.GetType(), coord, coord), false);
                        
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
                        EnqueueReachableCell(toFlipQueue, canBeReached, new CellChange(coord, selectedBlock.GetCellType(), coord, coord), true);
                        
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
                ReachableCellCandidate reachableCellCandidate = toFlipQueue.Dequeue();
                Vector2Int curCoord = reachableCellCandidate.GetCoord();
                if (processedReachableOrigins[curCoord.X, curCoord.Y])
                {
                    continue;
                }

                processedReachableOrigins[curCoord.X, curCoord.Y] = true;
                
                if (reachableCellCandidate.GetShouldSetCell())
                {
                    PseudoSetCell(reachableCellCandidate.GetCellChange(), pseudoBoard);
                }

                canBeReached[curCoord.X, curCoord.Y] = true;
                
                List<CellChange> toFlipCellChanges = PlayerGetToFlipCellChanges(curCoord, pseudoBoard);
                foreach (CellChange toFlipCellChange in toFlipCellChanges)
                {
                    EnqueueReachableCell(toFlipQueue, canBeReached, toFlipCellChange, true);
                    
                    if (selectedBlock is IMultipleBlock multipleBlock)
                    {
                        multipleBlock.RegisterPlacement(toFlipCellChange.GetCoord());
                        EnqueueContinuedPlacementCandidates(pseudoBoard, selectedBlock, multipleBlock, toFlipQueue, canBeReached);
                    }
                }
            }

            return canBeReached;
        }

        private void EnqueueReachableCell(
            Queue<ReachableCellCandidate> queue,
            bool[,] canBeReached,
            CellChange cellChange,
            bool shouldSetCell)
        {
            queue.Enqueue(new ReachableCellCandidate(cellChange, shouldSetCell));
            Vector2Int coord = cellChange.GetCoord();
            canBeReached[coord.X, coord.Y] = true;
        }

        private void EnqueueContinuedPlacementCandidates(
            Board pseudoBoard,
            IBlock selectedBlock,
            IMultipleBlock multipleBlock,
            Queue<ReachableCellCandidate> queue,
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
                        EnqueueReachableCell(queue, canBeReached, new CellChange(coord, selectedBlock.GetCellType(), coord, coord), true);
                    }
                }
            }
        }

        public List<Vector2Int> GetRandomBlackCellCoords(int cnt)
        {
            List<Vector2Int> blackCellCoords = new List<Vector2Int>();
            for (int i = 0; i < _board.GetWidth(); i++)
            {
                for (int j = 0; j < _board.GetHeight(); j++)
                {
                    if (_board.GetCell(new Vector2Int(i, j)).GetType().IsAssignableFrom(typeof(BlackCell)))
                    {
                        blackCellCoords.Add(new Vector2Int(i, j));
                    }
                }
            }
            return blackCellCoords.OrderBy(x => Guid.NewGuid()).Take(Math.Min(blackCellCoords.Count, cnt)).ToList();
        }
    }
    
    public class CellPlacementEventArgs: EventArgs{
        private readonly CellChange _cellChange;

        public CellPlacementEventArgs(CellChange cellChange)
        {
            _cellChange = cellChange;
        }

        public CellChange GetCellChange()
        {
            return _cellChange;
        }

        public Vector2Int GetCoord()
        {
            return _cellChange.GetCoord();
        }

        public Type GetCellType()
        {
            return _cellChange.GetCellType();
        }
    }

    public sealed class CellChange
    {
        private readonly Vector2Int _coord;
        private readonly Type _cellType;
        private readonly Vector2Int _originalCellCoord;
        private readonly Vector2Int _otherCellCoord;

        public CellChange(Vector2Int coord, Type cellType, Vector2Int originalCellCoord, Vector2Int otherCellCoord)
        {
            _coord = coord;
            _cellType = cellType;
            _originalCellCoord = originalCellCoord;
            _otherCellCoord = otherCellCoord;
        }

        public CellChange(Vector2Int coord, Type cellType)
        {
            _coord = coord;
            _cellType = cellType;
            _originalCellCoord = coord;
            _otherCellCoord = coord;
        }

        public Vector2Int GetCoord()
        {
            return _coord;
        }

        public Type GetCellType()
        {
            return _cellType;
        }

        public Vector2Int GetOriginalCellCoord()
        {
            return _originalCellCoord;
        }

        public Vector2Int GetOtherCellCoord()
        {
            return _otherCellCoord;
        }
    }

    public sealed class ReachableCellCandidate
    {
        private readonly CellChange _cellChange;
        private readonly bool _shouldSetCell;

        public ReachableCellCandidate(CellChange cellChange, bool shouldSetCell)
        {
            _cellChange = cellChange;
            _shouldSetCell = shouldSetCell;
        }

        public Vector2Int GetCoord()
        {
            return _cellChange.GetCoord();
        }

        public Type GetCellType()
        {
            return _cellChange.GetCellType();
        }

        public CellChange GetCellChange()
        {
            return _cellChange;
        }

        public bool GetShouldSetCell()
        {
            return _shouldSetCell;
        }
    }
}
