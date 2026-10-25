using System;
using System.Collections.Generic;
using GamePlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Coord = VectorUtils.Vector2Int;

/// <summary>Runs the actual runtime without views. Available from the editor menu and batchmode.</summary>
public static class TutorialRegressionTests
{
    private static int _assertions;

    [MenuItem("Tools/Tests/Tutorial Regression Tests")]
    public static void RunAll()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Run regression tests outside Play Mode.");
        if (Application.isBatchMode) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        _assertions = 0;
        CheckRunnerGates();
        CheckContentAndFullLessons();
        CheckResetDuringLesson();
        CheckDialogueSessions();
        CheckDreamNarrative();
        CheckSuspicionBoundary();
        CheckNormalEnemyFlipKeepsWeak();
        CheckSerializedTutorial();
        Debug.Log("TUTORIAL_TESTS_PASSED: " + _assertions + " assertions");
    }

    private static void Assert(bool condition, string message)
    {
        _assertions++;
        if (!condition) throw new InvalidOperationException("Tutorial regression: " + message);
    }

    private static void CheckRunnerGates()
    {
        var runner = new TutorialRunner();
        var placement = new TutorialStep { Id = "place", Completion = TutorialCompletion.PlaceCells,
            PlacementCount = 2, AllowedCells = new List<TutorialCell> { new TutorialCell(1, 1), new TutorialCell(2, 2) } };
        var end = new TutorialStep { Id = "end", Completion = TutorialCompletion.EndTurn };
        int completed = 0;
        runner.Completed += () => completed++;
        runner.Start(new[] { placement, end, new TutorialStep { Id = "next" } });
        runner.Next(); runner.EndTurn(); runner.Placed(new Coord(0, 0)); runner.BoardSettled();
        Assert(runner.Current == placement, "unrelated inputs must not skip a placement");
        runner.Placed(new Coord(1, 1)); runner.Placed(new Coord(1, 1)); runner.BoardSettled();
        Assert(runner.Current == placement, "duplicate placement is counted once");
        runner.Placed(new Coord(2, 2));
        Assert(runner.IsWaitingForBoard && !runner.CanPlace(new Coord(1, 1)), "input closes while animation settles");
        runner.BoardSettled();
        Assert(runner.Current == end, "settlement releases the gate");
        runner.Next(); runner.BoardSettled();
        Assert(runner.Current == end, "Next does not consume end turn");
        runner.EndTurn(); runner.BoardSettled(); runner.Next(); runner.BoardSettled(); runner.Next();
        Assert(completed == 1 && !runner.IsActive, "completion occurs once");
        runner.Start(new[] { placement }); runner.Placed(new Coord(1, 1)); runner.Cancel(); runner.BoardSettled();
        Assert(!runner.IsActive && completed == 1, "cancel discards pending events without completing");
    }

    private static GameInfo Load(string name) => AssetDatabase.LoadAssetAtPath<GameInfo>("Assets/GameInfos/" + name + ".asset");

    private static GamePlayRuntime CreateRuntime(GameInfo info)
    {
        GameInfoHolder.SetGameInfo(info);
        var runtime = new GamePlayRuntime();
        runtime.Initialize(new List<IBlock> { new BasicBlock() }, 100, 38);
        runtime.StartGame(null);
        return runtime;
    }

    private static void CheckContentAndFullLessons()
    {
        foreach (string name in new[] { "Guard1", "Guard2", "OldLady1", "ExampleGameInfo3" })
        {
            var info = Load(name);
            Assert(info != null && info.GetTutorial() != null, name + " uses the shared sequence");
            info.GetTutorial().Validate();
            foreach (var step in info.GetTutorial().Steps)
            {
                Assert(string.IsNullOrEmpty(step.Text) || !step.Text.Contains("회색"),
                    name + " tutorial does not explain gray cells");
                if (step.Board != null)
                    foreach (string row in step.Board.Rows)
                        Assert(!row.Contains("W"), name + " tutorial example contains no gray cell");
            }
            Assert(info.GetDialogueDataDict(false).Count == 0 && info.GetDialogueDataDict(true).Count == 0,
                name + " cannot select old mode-specific tutorials");
            using (var runtime = CreateRuntime(info))
            {
                PlayLesson(runtime);
                Assert(!runtime.Tutorial.IsActive && !runtime.Dialogue.HasCurrentDialogueData(), name + " releases input and dialogue");
                Assert(runtime.Turn.GetCurrentTurn() == 0 && runtime.Suspicion.GetCurrentSuspicion() == 0,
                    name + " restores turn and suspicion after practice");
                Assert(runtime.State.GetGameState() == GameState.Playing, name + " does not win/lose from examples");
                var original = info.GetBoard(); var restored = runtime.Board.GetCurrentBoard();
                for (int x = 0; x < original.GetLength(0); x++)
                for (int y = 0; y < original.GetLength(1); y++)
                    Assert(original[x, y].GetType() == restored[x, y].GetType(), name + " restores original board");
                Assert(runtime.Tutorial.CanClickEndTurn(), name + " releases end turn");
            }
        }
    }

    private static void PlayLesson(GamePlayRuntime runtime)
    {
        int guard = 0;
        while (runtime.Tutorial.IsActive && guard++ < 80)
        {
            var step = runtime.Tutorial.CurrentStep;
            Assert(step != null, "active lesson has a step");
            if (step.Completion == TutorialCompletion.Next)
            {
                Assert(!runtime.Tutorial.CanPlaceCellAt(new Coord(0, 0)), "narration blocks placement");
                Assert(!runtime.Tutorial.CanClickEndTurn(), "narration blocks end turn");
                runtime.Dialogue.ToNextEntry();
            }
            else if (step.Completion == TutorialCompletion.PlaceCells)
            {
                runtime.Dialogue.ToNextEntry();
                Assert(runtime.Tutorial.CurrentStep == step, "Next cannot skip practice");
                for (int i = 0; i < step.PlacementCount; i++)
                {
                    var coord = step.AllowedCells.Count > 0 ? step.AllowedCells[i].ToCoord() : new Coord(i * 4, i * 4);
                    Assert(runtime.Tutorial.CanPlaceCellAt(coord), "authored placement is permitted");
                    runtime.Board.HandleCellPlacementInput(coord);
                }
                runtime.Tutorial.Tick(0.02f);
            }
            else
            {
                Assert(runtime.Tutorial.CanClickEndTurn(), "end turn gate permits the button");
                runtime.Tutorial.NotifyEndTurnClicked();
                runtime.Turn.SetTurnState(TurnState.EnemyIdle);
                Assert(runtime.Board.GetCurrentBoard()[2, 2].GetType() == typeof(BlackCell),
                    "tutorial enemy flanks produce ordinary black");
                runtime.Tutorial.Tick(0.02f);
            }
            Assert(runtime.State.GetGameState() == GameState.Playing, "lesson cannot trigger a stage transition");
        }
        Assert(guard < 80, "lesson terminates");
    }

    private static void CheckResetDuringLesson()
    {
        using (var runtime = CreateRuntime(Load("Guard1")))
        {
            runtime.Board.HandleCellPlacementInput(new Coord(2, 3));
            runtime.BeginReset(); runtime.EndReset(); runtime.Tutorial.Tick(2f);
            Assert(runtime.Tutorial.CurrentStep.Id == "first-placement", "reset replays incomplete tutorial from start");
            Assert(runtime.Suspicion.GetCurrentSuspicion() == 0, "reset clears placement cost");
            GameInfoHolder.SetGameInfo(Load("Sister1"));
            runtime.BeginReset(); runtime.EndReset();
            Assert(!runtime.Tutorial.IsActive, "stage change cancels old tutorial");
            Assert(runtime.Dialogue.HasCurrentDialogueData(), "story trigger works after tutorial reset");
        }
    }

    private static void CheckDialogueSessions()
    {
        using (var dialogue = new DialogueManager())
        {
            int clicks = 0;
            dialogue.Show(new DialogueEntry("", "first", TutorialState.None), () =>
            {
                clicks++;
                dialogue.Show(new DialogueEntry("", "second", TutorialState.None), () => clicks++, true);
            }, true);
            dialogue.ToNextEntry();
            Assert(dialogue.CanAdvance && dialogue.GetCurrentDialogueEntry().DialogueText == "second", "nested Show retains its new callback");
            dialogue.ToNextEntry(); dialogue.ToNextEntry();
            Assert(clicks == 2, "callback is consumed once");
            dialogue.ResetGame();
            Assert(!dialogue.CanAdvance && !dialogue.ShouldBlockInteractionOutsideDialogue(), "reset releases presentation lock");
        }
    }

    private static void CheckDreamNarrative()
    {
        using (var runtime = CreateRuntime(Load("Sister4")))
        {
            int reveals = 0;
            runtime.Dialogue.Cue += cue => { if (cue == DialogueCue.DreamReveal) reveals++; };
            runtime.Dialogue.ToNextEntry(); runtime.Dialogue.ToNextEntry();
            Assert(reveals == 1 && !runtime.Dialogue.HasCurrentDialogueData(), "dream reveal runs after the second line");
            runtime.Narrative.Tick(0.5f);
            Assert(!runtime.Dialogue.HasCurrentDialogueData(), "dream reveal delay is respected");
            runtime.Narrative.Tick(0.6f);
            Assert(runtime.Dialogue.HasCurrentDialogueData(), "dream final line appears after delay");
            runtime.Dialogue.ToNextEntry();
            Assert(runtime.State.GetGameState() == GameState.Lost, "dream scripted defeat is preserved");
        }
    }

    private static void CheckSuspicionBoundary()
    {
        var info = ScriptableObject.CreateInstance<GameInfo>();
        info.Initialize(new TutorialBoard { Rows = new List<string> { "B..", "...", "..." } }.CreateCells(), 10, 1);
        try
        {
            using (var runtime = CreateRuntime(info))
            {
                runtime.Suspicion.IncrementSuspicion(99);
                Assert(runtime.State.GetGameState() == GameState.Playing, "99 suspicion remains playable");
                runtime.Suspicion.IncrementSuspicion(1);
                Assert(runtime.State.GetGameState() == GameState.Lost, "100 suspicion matches documented game over");
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(info); }
    }
    private static void CheckNormalEnemyFlipKeepsWeak()
    {
        var info = ScriptableObject.CreateInstance<GameInfo>();
        info.Initialize(new TutorialBoard { Rows = new List<string> { "...", "BPB", "..." } }.CreateCells(), 10, 1);
        try
        {
            using (var runtime = CreateRuntime(info))
            {
                runtime.Turn.SetTurnState(TurnState.EnemyIdle);
                Assert(runtime.Board.GetCurrentBoard()[1, 1] is WeakBlackCell,
                    "normal gameplay still produces a weak black cell");
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(info); }
    }
    private static void CheckSerializedTutorial()
    {
        var original = Load("Guard1");
        var copy = GameInfoSerializer.DeserializeGameInfo(GameInfoSerializer.SerializeGameInfo(original));
        try
        {
            Assert(copy.GetTutorial() != null && copy.GetTutorial().Steps.Count == 17, "JSON preserves all lesson steps");
            for (int i = 0; i < original.GetTutorial().Steps.Count; i++)
            {
                Assert(copy.GetTutorial().Steps[i].Text == original.GetTutorial().Steps[i].Text, "JSON preserves Korean dialogue");
                Assert(copy.GetTutorial().Steps[i].Completion == original.GetTutorial().Steps[i].Completion, "JSON preserves completion gates");
                var expectedLines = original.GetTutorial().Steps[i].Lines ?? new List<TutorialLine>();
                var actualLines = copy.GetTutorial().Steps[i].Lines ?? new List<TutorialLine>();
                Assert(actualLines.Count == expectedLines.Count, "JSON preserves tutorial connection count");
                for (int j = 0; j < expectedLines.Count; j++)
                    Assert(actualLines[j].Start.X == expectedLines[j].Start.X &&
                           actualLines[j].Start.Y == expectedLines[j].Start.Y &&
                           actualLines[j].End.X == expectedLines[j].End.X &&
                           actualLines[j].End.Y == expectedLines[j].End.Y &&
                           actualLines[j].EndInset == expectedLines[j].EndInset,
                        "JSON preserves tutorial connection endpoints");
            }
            using (var runtime = CreateRuntime(copy)) PlayLesson(runtime);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(copy.GetTutorial());
            UnityEngine.Object.DestroyImmediate(copy);
        }
    }
}
