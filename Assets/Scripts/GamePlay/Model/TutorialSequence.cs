using System;
using System.Collections.Generic;
using UnityEngine;
using Coord = VectorUtils.Vector2Int;

namespace GamePlay
{
    [CreateAssetMenu(fileName = "TutorialSequence", menuName = "GamePlay/Tutorial Sequence")]
    public sealed class TutorialSequence : ScriptableObject
    {
        public List<TutorialStep> Steps = new List<TutorialStep>();

        public void Validate()
        {
            if (Steps == null || Steps.Count == 0)
                throw new InvalidOperationException(name + ": tutorial has no steps.");
            var ids = new HashSet<string>();
            foreach (var step in Steps)
            {
                if (step == null || string.IsNullOrWhiteSpace(step.Id) || !ids.Add(step.Id))
                    throw new InvalidOperationException(name + ": steps need unique, nonempty IDs.");
                if (step.Completion == TutorialCompletion.PlaceCells && step.PlacementCount < 1)
                    throw new InvalidOperationException(step.Id + ": placement count must be positive.");
                if (step.Completion == TutorialCompletion.PlaceCells && step.AllowedCells.Count > 0 &&
                    step.PlacementCount > step.AllowedCells.Count)
                    throw new InvalidOperationException(step.Id + ": not enough allowed cells.");
                if (step.Board != null && step.Board.Rows.Count > 0) step.Board.Validate();
            }
        }
    }

    public enum TutorialCompletion { Next, PlaceCells, EndTurn }
    public enum TutorialFocus { None, Cells, Suspicion, EndTurn, Target, Stage, OriginalCells, WeakNeighbours }

    [Serializable]
    public sealed class TutorialStep
    {
        public string Id;
        [TextArea(2, 6)] public string Text;
        [Tooltip("Short interaction hint, separate from the document's dialogue.")]
        public string Instruction;
        public TutorialCompletion Completion;
        [Min(1)] public int PlacementCount = 1;
        public List<TutorialCell> AllowedCells = new List<TutorialCell>();
        public List<TutorialCell> ExcludedCells = new List<TutorialCell>();
        public TutorialFocus Focus;
        public List<TutorialCell> FocusCells = new List<TutorialCell>();
        public TutorialBoard Board = new TutorialBoard();
        public bool ResetPracticeCounters;
    }

    [Serializable]
    public struct TutorialCell
    {
        public int X;
        public int Y;
        public TutorialCell(int x, int y) { X = x; Y = y; }
        public Coord ToCoord() => new Coord(X, Y);
        public bool Matches(Coord coord) => coord != null && X == coord.X && Y == coord.Y;
    }

    [Serializable]
    public sealed class TutorialBoard
    {
        [Tooltip("Rows in board coordinate order. . empty, B black, P purple, W weak/gray.")]
        public List<string> Rows = new List<string>();
        public List<TutorialCell> OriginalCells = new List<TutorialCell>();
        [Min(1)] public int Target = 1;

        public void Validate()
        {
            if (Rows == null || Rows.Count == 0 || string.IsNullOrEmpty(Rows[0]))
                throw new InvalidOperationException("Tutorial board must have rows.");
            foreach (var row in Rows)
            {
                if (row == null || row.Length != Rows[0].Length)
                    throw new InvalidOperationException("Tutorial board rows must be rectangular.");
                foreach (char cell in row)
                    if (".BPW".IndexOf(cell) < 0)
                        throw new InvalidOperationException("Unknown tutorial cell: " + cell);
            }
            foreach (var cell in OriginalCells)
                if (cell.X < 0 || cell.Y < 0 || cell.Y >= Rows.Count || cell.X >= Rows[0].Length)
                    throw new InvalidOperationException("Original tutorial cell is outside the board.");
        }

        public Cell[,] CreateCells()
        {
            Validate();
            var cells = new Cell[Rows[0].Length, Rows.Count];
            for (int y = 0; y < Rows.Count; y++)
            for (int x = 0; x < Rows[y].Length; x++)
            {
                var coord = new Coord(x, y);
                switch (Rows[y][x])
                {
                    case 'B': cells[x, y] = new BlackCell(coord); break;
                    case 'P': cells[x, y] = new ConceptCell(0, coord); break;
                    case 'W': cells[x, y] = new WeakBlackCell(coord); break;
                    default: cells[x, y] = new EmptyCell(coord); break;
                }
            }
            return cells;
        }
    }
}
