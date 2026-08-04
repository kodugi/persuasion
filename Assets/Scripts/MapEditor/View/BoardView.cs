using System;
using GamePlay;
using UnityEngine;
using SingletonUtils;
using Vector2Int = VectorUtils.Vector2Int;

namespace MapEditor
{
    public class BoardView: BoardViewBase
    {
        protected override GameInfo GetGameInfo()
        {
            // TODO: board size subject to change; also implement changing the board size
            return GameInfoController.Instance.AssembleGameInfo();
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