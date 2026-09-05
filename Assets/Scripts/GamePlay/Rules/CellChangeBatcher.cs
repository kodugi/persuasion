using System;
using System.Collections.Generic;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    internal static class CellChangeBatcher
    {
        public static List<List<CellChange>> ByAnimationDistance(IEnumerable<CellChange> changes)
        {
            SortedDictionary<int, List<CellChange>> batches =
                new SortedDictionary<int, List<CellChange>>();

            foreach (CellChange change in changes)
            {
                int distance = Math.Min(
                    Vector2Int.TaxiDist(change.OriginalCellCoord, change.Coord),
                    Vector2Int.TaxiDist(change.OtherCellCoord, change.Coord));

                if (!batches.TryGetValue(distance, out List<CellChange> batch))
                {
                    batch = new List<CellChange>();
                    batches.Add(distance, batch);
                }

                batch.Add(change);
            }

            return new List<List<CellChange>>(batches.Values);
        }
    }
}
