using System;
using System.Collections.Generic;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    /// <summary>
    /// Calculates flanking and weak-cell conversions without owning turn or view state.
    /// </summary>
    internal sealed class BoardFlipResolver
    {
        private static readonly Vector2Int[] Directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1)
        };

        public List<CellChange> FindPlayerFlips(Board board, Vector2Int origin)
        {
            return FindFlips(board, origin, typeof(BlackCell), typeof(ConceptCell));
        }

        public List<CellChange> FindEnemyFlips(Board board, Vector2Int origin)
        {
            return FindFlips(board, origin, typeof(ConceptCell), typeof(BlackCell));
        }

        public List<CellChange> FindAdjacentWeakCellFlips(Board board, Vector2Int origin)
        {
            List<CellChange> changes = new List<CellChange>();
            if (!(board.GetCell(origin) is IWeakFlipperCell weakFlipper))
            {
                return changes;
            }

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int targetCoord = origin + direction;
                if (!board.IsWithinBound(targetCoord) || !(board.GetCell(targetCoord) is WeakBlackCell weakCell))
                {
                    continue;
                }

                Type flippedType = weakFlipper.TryFlipWeakCell(weakCell);
                if (flippedType != null)
                {
                    changes.Add(new CellChange(targetCoord, flippedType, origin, targetCoord));
                }
            }

            return changes;
        }

        private static List<CellChange> FindFlips(
            Board board,
            Vector2Int origin,
            Type targetType,
            Type flankingType)
        {
            List<CellChange> changes = new List<CellChange>();
            Cell originCell = board.GetCell(origin);

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int otherCoord = FindNearestCell(board, origin, direction, flankingType);
                if (otherCoord == null || !CanFlipLine(board, origin, otherCoord, direction, targetType))
                {
                    continue;
                }

                Cell otherCell = board.GetCell(otherCoord);
                for (Vector2Int current = origin + direction; current != otherCoord; current += direction)
                {
                    Type flippedType = ResolveFlippedType(originCell, otherCell, board.GetCell(current));
                    if (flippedType != null)
                    {
                        changes.Add(new CellChange(current, flippedType, origin, otherCoord));
                    }
                }
            }

            return changes;
        }

        private static bool CanFlipLine(
            Board board,
            Vector2Int origin,
            Vector2Int otherCoord,
            Vector2Int direction,
            Type targetType)
        {
            Cell originCell = board.GetCell(origin);
            Cell otherCell = board.GetCell(otherCoord);

            for (Vector2Int current = origin + direction; current != otherCoord; current += direction)
            {
                Cell cell = board.GetCell(current);
                if (!targetType.IsAssignableFrom(cell.GetType()) ||
                    !(cell is IFlippableCell flippableCell) ||
                    flippableCell.TryBeFlipped(originCell, otherCell) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static Vector2Int FindNearestCell(
            Board board,
            Vector2Int origin,
            Vector2Int direction,
            Type cellType)
        {
            Vector2Int current = origin + direction;
            while (board.IsWithinBound(current))
            {
                if (cellType.IsAssignableFrom(board.GetCell(current).GetType()))
                {
                    return current;
                }

                current += direction;
            }

            return null;
        }

        private static Type ResolveFlippedType(Cell first, Cell second, Cell cellToFlip)
        {
            if (!(first is IFlipperCell firstFlipper) ||
                !(second is IFlipperCell secondFlipper) ||
                !(cellToFlip is IFlippableCell flippableCell))
            {
                Debug.LogError("A board flip requires two flippers and one flippable cell.");
                return null;
            }

            Type firstType = firstFlipper.TryFlip(second, cellToFlip);
            Type secondType = secondFlipper.TryFlip(first, cellToFlip);
            Type cellType = flippableCell.TryBeFlipped(first, second);

            int highestPrecedence = Math.Max(
                firstFlipper.FlipperPrecedence,
                Math.Max(secondFlipper.FlipperPrecedence, flippableCell.FlippedPrecedence));

            bool firstWins = firstFlipper.FlipperPrecedence == highestPrecedence;
            bool secondWins = secondFlipper.FlipperPrecedence == highestPrecedence;
            bool cellWins = flippableCell.FlippedPrecedence == highestPrecedence;

            if ((firstWins && secondWins && firstType != secondType) ||
                (firstWins && cellWins && firstType != cellType) ||
                (secondWins && cellWins && secondType != cellType))
            {
                Debug.LogWarning(
                    "Board flip rules produced different cell types at the same precedence. " +
                    "Resolving in first, second, target order.");
            }

            if (firstWins)
            {
                return firstType;
            }

            return secondWins ? secondType : cellType;
        }
    }
}
