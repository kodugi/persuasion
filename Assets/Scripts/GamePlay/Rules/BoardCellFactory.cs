using System;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    internal static class BoardCellFactory
    {
        public static Cell Create(Type cellType, int placedTurn, Vector2Int coord)
        {
            if (cellType == null)
            {
                throw new ArgumentNullException(nameof(cellType));
            }

            if (coord == null)
            {
                throw new ArgumentNullException(nameof(coord));
            }

            return (Cell)Activator.CreateInstance(cellType, placedTurn, coord);
        }
    }
}
