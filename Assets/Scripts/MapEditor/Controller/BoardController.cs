using System;
using System.Collections.Generic;
using SingletonUtils;
using GamePlay;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

namespace MapEditor
{
    public class BoardController: Singleton<BoardController>
    {
        private Board _board;
        private CellSelectionManager _cellSelectionManager;
        
        public void Initialize(int width, int height)
        {
            CreateBlankBoard(width, height);
            
            _cellSelectionManager = CellSelectionManager.Instance;
        }

        private void CreateBlankBoard(int width, int height)
        {
            _board = new Board(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    _board.SetCell(new Vector2Int(i, j), new EmptyCell(0, new Vector2Int(i, j)));
                }
            }
        }

        public void HandleCellPlacementInput(Vector2Int coord)
        {
            SetCell(coord, _cellSelectionManager.GetCurrentCellKind());
        }

        private void SetCell(Vector2Int coord, CellKind cellKind)
        {
            Type cellType = CellUtils.CellKindToType(cellKind);
            Cell cell = (Cell)Activator.CreateInstance(cellType, 0, coord);
            _board.SetCell(coord, cell);
            List<List<CellChange>> cellChanges = new List<List<CellChange>>();
            cellChanges.Add(new List<CellChange>() {new CellChange(coord, cellType, coord, coord)});
        }

        public Board GetBoard()
        {
            return _board;
        }

        public void RefreshBoard(Cell[,] board)
        {
            _board = new Board(board);
            BoardView.Instance.Refresh();
        }
    }
}
