using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class BoardController : Singleton<BoardController>
    {
        private GameInfoManager _gameInfoManager;
        private Board _board;
        private TurnManager _turnManager;
        private BlockSelectionManager _blockSelectionManager;

        public event EventHandler RaiseCellPlacementEvent;
        public void Initialize()
        {
            _gameInfoManager = GameInfoManager.Instance;
            _blockSelectionManager = BlockSelectionManager.Instance;
            _board = new Board(_gameInfoManager.GetGameInfo().GetBoard());
            _turnManager = TurnManager.Instance;
            _turnManager.RaiseSetTurnEvent += HandleSetTurnEvent;
        }

        public void HandleCellPlacementInput(Vector2Int coord)
        {
            TurnState turnState = _turnManager.GetTurnState();
            if (turnState != TurnState.PlayerIdle && turnState != TurnState.PlayerPlacingContinue)
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
                        PlayerPlaceCell(coord, selectedBlock.GetCellType());
                        _blockSelectionManager.PlaceSelectedBlock(coord);
                        _turnManager.SetTurnState(selectedBlock is IMultipleBlock ? TurnState.PlayerPlacingContinue : TurnState.PlayerIdle);
                        RaiseCellPlacementEvent.Invoke(this, null);
                    }
                }
                return;
            }

            if (selectedBlock is IMultipleBlock multipleBlock)
            {
                CellPlacementResult placementResult = multipleBlock.TryContinuedPlacement(_board.GetBoard(), coord);
                if (placementResult.GetSuccess())
                {
                    PlayerPlaceCell(coord, multipleBlock.GetCellType());
                    _blockSelectionManager.PlaceContinuedBlock(coord);
                    _turnManager.SetTurnState((multipleBlock.InputState == MultipleBlockInputState.AwaitingContinuedPlacement) ? TurnState.PlayerPlacingContinue : TurnState.PlayerIdle);
                    RaiseCellPlacementEvent.Invoke(this, null);
                }
                return;
            }

            Debug.LogError("Selected block does not support continued placement!");
        }

        private void PlayerPlaceCell(Vector2Int coord, Type cellType)
        {
            Queue<(Vector2Int, Type)> toFlipQueue = new Queue<(Vector2Int, Type)>();
            toFlipQueue.Enqueue((coord, cellType));

            while (toFlipQueue.Count > 0)
            {
                (Vector2Int curCoord, Type curCellType) = toFlipQueue.Dequeue();
                SetCell(curCoord, curCellType);
                List<(Vector2Int, Type)> toFlipCoordsAndTypes = PlayerGetToFlipCoordsAndTypes(coord);
                IFlipperCell curCell = (IFlipperCell)_board.GetCell(curCoord);
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

        private Cell CreateCell(Vector2Int coord, Type cellType)
        {
            Cell cell = (Cell)Activator.CreateInstance(cellType, _turnManager.GetCurrentTurn(), coord);
            return cell;
        }

        private List<(Vector2Int, Type)> PlayerGetToFlipCoordsAndTypes(Vector2Int origin)
        {
            Cell originCell = _board.GetCell(origin);
            List<(Vector2Int, Type)> toFlipCoordsAndTypes = new List<(Vector2Int, Type)>();
            Vector2Int[] dirs = {new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1),
            new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)};

            // 약한 생각 처리
            foreach(Vector2Int dir in dirs)
            {
                Vector2Int otherCoord = origin + dir;
                if (_board.IsWithinBound(otherCoord))
                {
                    Cell otherCell = _board.GetCell(origin + dir);
                    if (otherCell is WeakBlackCell)
                    {
                        toFlipCoordsAndTypes.Add((origin + dir, ((IWeakFlipperCell)originCell).TryFlipWeakCell(otherCell)));
                    }
                }
            }

            // 일반 오셀로 규칙에 따른 처리
            toFlipCoordsAndTypes.AddRange(GetToFlipCoordsAndTypes(origin, typeof(BlackCell), typeof(ConceptCell)));

            return toFlipCoordsAndTypes;
        }

        private List<(Vector2Int, Type)> GetToFlipCoordsAndTypes(Vector2Int origin, Type targetType, Type otherType)
        {
            List<(Vector2Int, Type)> toFlipCoordsAndTypes = new List<(Vector2Int, Type)>();
            Cell originCell = _board.GetCell(origin);
            Vector2Int[] dirs = {new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1),
            new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)};
            foreach (Vector2Int dir in dirs)
            {
                Vector2Int otherCellCoord = GetNearestOtherCellCoord(origin, dir, otherType);
                if (otherCellCoord != null)
                {
                    Cell otherCell = _board.GetCell(otherCellCoord);
                    bool canBeFlipped = true;
                    for (Vector2Int cur = new Vector2Int(origin) + dir; cur != otherCellCoord; cur += dir)
                    {
                        Cell cell = _board.GetCell(cur);
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
                        toFlipCoordsAndTypes.Add((cur, GetFlippedCellType(originCell, otherCell, _board.GetCell(cur))));
                    }
                }
            }

            return toFlipCoordsAndTypes;
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

        private Vector2Int GetNearestOtherCellCoord(Vector2Int origin, Vector2Int dir, Type cellType) // ConceptCell 또는 BlackCell
        {
            Vector2Int current = origin + dir;
            while(_board.IsWithinBound(current))
            {
                Cell currentCell = _board.GetCell(current);
                if (cellType.IsAssignableFrom(currentCell.GetType())) {
                    return current;
                }
                current += dir;
            }
            return null;
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

        private void HandleSetTurnEvent(object sender, SetTurnEventArgs e)
        {
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
            Cell[,] originalBoard = _gameInfoManager.GetGameInfo().GetBoard();
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
    }
}
