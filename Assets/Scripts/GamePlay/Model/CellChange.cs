using System;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    /// <summary>
    /// Describes one board mutation and the two cells that caused it.
    /// The origin coordinates are also used to choose the view animation direction.
    /// </summary>
    public sealed class CellChange
    {
        public Vector2Int Coord { get; }
        public Type CellType { get; }
        public Vector2Int OriginalCellCoord { get; }
        public Vector2Int OtherCellCoord { get; }

        public CellChange(
            Vector2Int coord,
            Type cellType,
            Vector2Int originalCellCoord,
            Vector2Int otherCellCoord)
        {
            Coord = coord;
            CellType = cellType;
            OriginalCellCoord = originalCellCoord;
            OtherCellCoord = otherCellCoord;
        }

        public CellChange(Vector2Int coord, Type cellType)
            : this(coord, cellType, coord, coord)
        {
        }

        // Kept for existing view code. New gameplay code should prefer the properties above.
        public Vector2Int GetCoord() => Coord;
        public Type GetCellType() => CellType;
        public Vector2Int GetOriginalCellCoord() => OriginalCellCoord;
        public Vector2Int GetOtherCellCoord() => OtherCellCoord;
    }

    public sealed class CellPlacementEventArgs : EventArgs
    {
        public CellChange Change { get; }
        public Vector2Int Coord => Change.Coord;
        public Type CellType => Change.CellType;

        public CellPlacementEventArgs(CellChange change)
        {
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public CellChange GetCellChange() => Change;
        public Vector2Int GetCoord() => Coord;
        public Type GetCellType() => CellType;
    }
}
