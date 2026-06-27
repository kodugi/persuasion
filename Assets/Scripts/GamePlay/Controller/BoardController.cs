using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class BoardController
    {
        public static BoardController Instance { get; private set; }
        public BoardController()
        {
            if (Instance != null)
            {
                throw new System.Exception("BoardController instance already exists!");
            }
            Instance = this;
        }

        private GameInfoManager _gameInfoManager;
        private Board _board;
        private TurnManager _turnManager;
        private BlockSelectionManager _blockSelectionManager;
        public void Initialize()
        {
            _gameInfoManager = GameInfoManager.Instance;
            _blockSelectionManager = BlockSelectionManager.Instance;
            _board = new Board(_gameInfoManager.GetGameInfo().GetBoard());
            _turnManager = TurnManager.Instance;
        }

        public void HandleCellPlacementInput(Vector2Int coord)
        {
            if (_turnManager.GetTurnState() == TurnState.PlayerIdle)
            {
                // 추후 로직 추가
            }
            else if (_turnManager.GetTurnState() == TurnState.PlayerPlacingContinue)
            {
                // 추후 로직 추가
            }
            else
            {
                return;
            }

            CellPlacementResult placementResult = _blockSelectionManager.GetSelectedBlock().TryPlacement(_board.GetBoard(), coord);
            if (placementResult.GetSuccess())
            {
                PlayerPlaceCell(coord, _blockSelectionManager.GetSelectedBlock().GetCellType());
            }
        }

        private void PlayerPlaceCell(Vector2Int coord, Type cellType)
        {
            Queue<(Vector2Int, Type)> toFlipQueue = new Queue<(Vector2Int, Type)> ();
            toFlipQueue.Enqueue((coord, cellType));

            while(toFlipQueue.Count > 0)
            {
                (Vector2Int curCoord, Type curCellType) = toFlipQueue.Dequeue();
                SetCell(curCoord, curCellType);
                Debug.Log("flipped" + curCoord.ToString());
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
                        toFlipCoordsAndTypes.Add((origin + dir, ((IFlipperCell)originCell).TryFlipWeakCell(otherCell)));
                    }
                }
            }

            // 일반 오셀로 규칙에 따른 처리
            toFlipCoordsAndTypes.AddRange(GetToFlipCoordsAndTypes(origin, typeof(BlackCell)));

            foreach((Vector2Int coord, Type type) in toFlipCoordsAndTypes)
            {
                Debug.Log("added " + coord.ToString() + " for " + origin.ToString());
            }

            return toFlipCoordsAndTypes;
        }

        private List<(Vector2Int, Type)> GetToFlipCoordsAndTypes(Vector2Int origin, Type targetType)
        {
            List<(Vector2Int, Type)> toFlipCoordsAndTypes = new List<(Vector2Int, Type)>();
            Cell originCell = _board.GetCell(origin);
            Vector2Int[] dirs = {new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1),
            new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)};
            foreach (Vector2Int dir in dirs)
            {
                Vector2Int otherCellCoord = GetNearestOtherCellCoord(origin, dir, typeof(ConceptCell));
                Debug.Log("nearest other cell from " + origin.ToString() + " in direction " + dir.ToString() + ": " + otherCellCoord?.ToString());
                if (otherCellCoord != null)
                {
                    Cell otherCell = _board.GetCell(otherCellCoord);
                    bool canBeFlipped = true;
                    for (Vector2Int cur = new Vector2Int(origin) + dir; cur != otherCellCoord; cur += dir)
                    {
                        Cell cell = _board.GetCell(cur);
                        if (targetType.IsAssignableFrom(cell.GetType()))
                        {
                            if (!((IFlippableCell)cell).CanBeFlippedBy(originCell, otherCell))
                            {
                                canBeFlipped = false;
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
                        toFlipCoordsAndTypes.Add((cur, ((IFlipperCell)originCell).TryFlip(otherCell, _board.GetCell(cur))));
                    }
                }
            }

            return toFlipCoordsAndTypes;
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

        public void HandleEnemyFlipInput()
        {
            EnemyFlipCells();
        }

        private void EnemyFlipCells()
        {

        }
    }
}
