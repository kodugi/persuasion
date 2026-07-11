using System;
using GamePlay;
using UnityEngine;
using SingletonUtils;
using Vector2Int = GamePlay.Vector2Int;

namespace MapEditor
{
    public class BoardView: BoardViewBase
    {
        protected override GameInfo GetGameInfo()
        {
            // TODO: board size subject to change; also implement changing the board size
            return CreateInitialGameInfo(5, 5);
        }

        private GameInfo CreateInitialGameInfo(int width, int height)
        {
            Cell[,] board = new Cell[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    board[i, j] = new EmptyCell(new Vector2Int(i, j));
                }
            }
            GameInfo gameInfo = ScriptableObject.CreateInstance<GameInfo>();
            gameInfo.Initialize(width, height, board, 0, 0);
            return gameInfo;
        }

        public override void HandleCellClick(Vector2Int coord)
        {
            BoardController.Instance.HandleCellPlacementInput(coord);
        }

        protected override Type GetCellType()
        {
            return CellUtils.CellKindToType(CellSelectionManager.Instance.GetCurrentCellKind());
        }

        protected override bool IsCellPlacementAllowed(Vector2Int coord)
        {
            return true;
        }
    }
}