namespace GamePlay
{
    public class CellPlacementResult
    {
        private bool _success;
        private CellPlacementResultType _cellPlacementResultType;

        public CellPlacementResult(bool success, CellPlacementResultType cellPlacementResultType)
        {
            _success = success;
            _cellPlacementResultType = cellPlacementResultType;
        }

        public bool GetSuccess()
        {
            return _success;
        }

        public CellPlacementResultType GetCellPlacementResultType()
        {
            return _cellPlacementResultType;
        }
    }

    public enum CellPlacementResultType
    {
        SUCCESS,
        STATE_NOT_ALLOWED,
        OCCUPIED,
        LIE_NOT_ADJACENT,
        THREAT_NO_ADJACENT_CELL_EMPTY,
        THREAT_NOT_ADJACENT
    }
}