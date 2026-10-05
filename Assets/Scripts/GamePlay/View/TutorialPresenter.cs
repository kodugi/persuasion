using System.Collections.Generic;
using UnityEngine;
using Coord = VectorUtils.Vector2Int;

namespace GamePlay
{
    /// <summary>Resolves typed focus targets against the currently rendered board and HUD.</summary>
    public sealed class TutorialPresenter
    {
        private readonly BoardController _board;
        public TutorialPresenter(BoardController board) { _board = board; }

        public void Clear()
        {
            TutorialOverlayView.Instance?.Hide();
            if (BoardView.Instance is BoardView view) view.ClearTutorialHints();
        }

        public void Show(TutorialStep step)
        {
            var ui = new List<RectTransform>();
            var world = new List<GameObject>();
            var view = BoardView.Instance as BoardView;
            RectTransform target = null;
            switch (step.Focus)
            {
                case TutorialFocus.Suspicion: target = SuspicionView.Instance?.GetFocusTarget(); break;
                case TutorialFocus.EndTurn: target = ButtonUIView.Instance?.GetEndTurnButtonTarget(); break;
                case TutorialFocus.Target: target = GameStateView.Instance?.GetTargetFocusTarget(); break;
                case TutorialFocus.Stage: target = GameStateView.Instance?.GetStageFocusTarget(); break;
            }
            if (target != null) ui.Add(target);
            if (view != null)
            {
                var coords = new HashSet<(int, int)>();
                foreach (var cell in step.FocusCells) coords.Add((cell.X, cell.Y));
                foreach (var cell in step.AllowedCells)
                {
                    coords.Add((cell.X, cell.Y));
                    if (step.Completion == TutorialCompletion.PlaceCells)
                        view.ShowTutorialHint(cell.ToCoord(), typeof(ConceptCell));
                }
                var cells = _board.GetCurrentBoard();
                var originals = _board.GetOriginalBoard();
                for (int x = 0; x < cells.GetLength(0); x++)
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    // Free placement still dims the surroundings while keeping the whole board visible.
                    if (step.Completion == TutorialCompletion.PlaceCells && step.AllowedCells.Count == 0)
                        coords.Add((x, y));
                    if (step.Focus == TutorialFocus.OriginalCells && originals[x, y] is BlackCell)
                        coords.Add((x, y));
                    if (step.Focus != TutorialFocus.WeakNeighbours || !(cells[x, y] is WeakBlackCell)) continue;
                    for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (x + dx >= 0 && y + dy >= 0 && x + dx < cells.GetLength(0) && y + dy < cells.GetLength(1))
                            coords.Add((x + dx, y + dy));
                }
                foreach (var coord in coords)
                    if (view.TryGetCellObject(new Coord(coord.Item1, coord.Item2), out var obj)) world.Add(obj);
            }
            // The runner enforces board input; raycast holes only guide the user's attention.
            TutorialOverlayView.Instance?.Focus(ui, world, step.Completion != TutorialCompletion.Next, null);
        }
    }
}
