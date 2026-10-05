using System;
using System.Collections.Generic;
using Coord = VectorUtils.Vector2Int;

namespace GamePlay
{
    /// <summary>Deterministic lesson progression without scene, singleton or dialogue dependencies.</summary>
    public sealed class TutorialRunner
    {
        private IReadOnlyList<TutorialStep> _steps;
        private int _index;
        private readonly HashSet<(int, int)> _placements = new HashSet<(int, int)>();
        private bool _gateSatisfied;
        public TutorialStep Current => IsActive ? _steps[_index] : null;
        public bool IsActive => _steps != null && _index < _steps.Count;
        public bool IsWaitingForBoard => IsActive && _gateSatisfied;
        public event Action<TutorialStep> StepChanged;
        public event Action Completed;

        public void Start(IReadOnlyList<TutorialStep> steps)
        {
            Cancel();
            _steps = steps ?? throw new ArgumentNullException(nameof(steps));
            EnterStep();
        }

        public void Cancel()
        {
            _steps = null;
            _index = 0;
            _placements.Clear();
            _gateSatisfied = false;
        }

        public bool CanPlace(Coord coord)
        {
            var step = Current;
            return coord != null && step != null && !_gateSatisfied &&
                   step.Completion == TutorialCompletion.PlaceCells &&
                   !step.ExcludedCells.Exists(c => c.Matches(coord)) &&
                   (step.AllowedCells.Count == 0 || step.AllowedCells.Exists(c => c.Matches(coord)));
        }

        public void Next()
        {
            if (Current?.Completion == TutorialCompletion.Next) Advance();
        }

        // Only successful placements count. Duplicate notifications cannot consume a step.
        public void Placed(Coord coord)
        {
            if (!CanPlace(coord) || !_placements.Add((coord.X, coord.Y))) return;
            _gateSatisfied = _placements.Count >= Current.PlacementCount;
        }

        public void EndTurn()
        {
            if (Current?.Completion == TutorialCompletion.EndTurn) _gateSatisfied = true;
        }

        // Wait for the visual transition before presenting the next step.
        public void BoardSettled()
        {
            if (_gateSatisfied) Advance();
        }

        private void Advance() { _index++; EnterStep(); }
        private void EnterStep()
        {
            _placements.Clear();
            _gateSatisfied = false;
            if (IsActive) StepChanged?.Invoke(Current);
            else { _steps = null; Completed?.Invoke(); }
        }
    }
}
