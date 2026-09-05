using System;
using System.Collections.Generic;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    /// <summary>
    /// Simulates placements on a cloned board to find every cell the player can reach.
    /// </summary>
    internal sealed class BoardReachabilityAnalyzer
    {
        private readonly BoardFlipResolver _flipResolver = new BoardFlipResolver();

        public bool[,] Analyze(Cell[,] initialCells, Type selectedBlockType, int currentTurn)
        {
            Board board = new Board(initialCells);
            bool[,] reachable = new bool[board.GetWidth(), board.GetHeight()];
            bool[,] processedOrigins = new bool[board.GetWidth(), board.GetHeight()];
            Queue<ReachableCellCandidate> candidates = new Queue<ReachableCellCandidate>();

            if (!(Activator.CreateInstance(selectedBlockType) is IBlock simulatedBlock))
            {
                Debug.LogError("The selected block type could not be simulated: " + selectedBlockType);
                return reachable;
            }

            SeedCandidates(board, simulatedBlock, candidates, reachable);

            while (candidates.Count > 0)
            {
                simulatedBlock.Reset();
                ReachableCellCandidate candidate = candidates.Dequeue();
                Vector2Int coord = candidate.Change.Coord;
                if (processedOrigins[coord.X, coord.Y])
                {
                    continue;
                }

                processedOrigins[coord.X, coord.Y] = true;
                if (candidate.ShouldSetCell)
                {
                    board.SetCell(
                        coord,
                        BoardCellFactory.Create(candidate.Change.CellType, currentTurn, coord));
                }

                reachable[coord.X, coord.Y] = true;
                foreach (CellChange flip in _flipResolver.FindPlayerFlips(board, coord))
                {
                    Enqueue(candidates, reachable, flip, true);

                    if (simulatedBlock is IMultipleBlock multipleBlock)
                    {
                        multipleBlock.RegisterPlacement(flip.Coord);
                        EnqueueContinuedPlacements(board, simulatedBlock, multipleBlock, candidates, reachable);
                    }
                }
            }

            return reachable;
        }

        private static void SeedCandidates(
            Board board,
            IBlock simulatedBlock,
            Queue<ReachableCellCandidate> candidates,
            bool[,] reachable)
        {
            for (int x = 0; x < board.GetWidth(); x++)
            {
                for (int y = 0; y < board.GetHeight(); y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    Cell currentCell = board.GetCell(coord);
                    simulatedBlock.Reset();

                    if (currentCell is ConceptCell)
                    {
                        Enqueue(candidates, reachable, new CellChange(coord, currentCell.GetType()), false);
                        if (simulatedBlock is IMultipleBlock multipleFromConcept)
                        {
                            multipleFromConcept.RegisterPlacement(coord);
                            EnqueueContinuedPlacements(
                                board,
                                simulatedBlock,
                                multipleFromConcept,
                                candidates,
                                reachable);
                        }

                        continue;
                    }

                    if (simulatedBlock.TryPlacement(board.GetBoard(), coord).GetSuccess())
                    {
                        Enqueue(
                            candidates,
                            reachable,
                            new CellChange(coord, simulatedBlock.GetCellType()),
                            true);

                        if (simulatedBlock is IMultipleBlock multipleBlock)
                        {
                            multipleBlock.RegisterPlacement(coord);
                            EnqueueContinuedPlacements(
                                board,
                                simulatedBlock,
                                multipleBlock,
                                candidates,
                                reachable);
                        }
                    }

                    if (currentCell is EmptyCell)
                    {
                        reachable[x, y] = true;
                    }
                }
            }
        }

        private static void EnqueueContinuedPlacements(
            Board board,
            IBlock simulatedBlock,
            IMultipleBlock multipleBlock,
            Queue<ReachableCellCandidate> candidates,
            bool[,] reachable)
        {
            for (int x = 0; x < board.GetWidth(); x++)
            {
                for (int y = 0; y < board.GetHeight(); y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    if (multipleBlock.TryContinuedPlacement(board.GetBoard(), coord).GetSuccess())
                    {
                        Enqueue(
                            candidates,
                            reachable,
                            new CellChange(coord, simulatedBlock.GetCellType()),
                            true);
                    }
                }
            }
        }

        private static void Enqueue(
            Queue<ReachableCellCandidate> candidates,
            bool[,] reachable,
            CellChange change,
            bool shouldSetCell)
        {
            candidates.Enqueue(new ReachableCellCandidate(change, shouldSetCell));
            reachable[change.Coord.X, change.Coord.Y] = true;
        }

        private sealed class ReachableCellCandidate
        {
            public CellChange Change { get; }
            public bool ShouldSetCell { get; }

            public ReachableCellCandidate(CellChange change, bool shouldSetCell)
            {
                Change = change;
                ShouldSetCell = shouldSetCell;
            }
        }
    }
}
