using System;

namespace GamePlay
{
    public class Cell : ICloneable
    {
        protected int _placedTurn;
        protected Vector2Int _coord;
        public int GetPlacedTurn()
        {
            return _placedTurn;
        }

        public Cell(int placedTurn, Vector2Int coord)
        {
            _placedTurn = placedTurn;
            _coord = coord;
        }

        public Cell(Vector2Int coord)
        {
            _placedTurn = 0;
            _coord = coord;
        }

        public virtual object Clone()
        {
            Cell clone = (Cell)MemberwiseClone();
            clone._coord = _coord == null ? null : new Vector2Int(_coord);
            return clone;
        }
    }
}
