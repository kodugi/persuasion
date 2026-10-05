using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using GamePlay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Coord = VectorUtils.Vector2Int;

/// <summary>Batch play-mode check of the actual scene, its views, and animated transitions.</summary>
[InitializeOnLoad]
public static class TutorialSceneSmokeTest
{
    private const string RunningKey = "TutorialSceneSmokeTest.Running";
    private static double _nextAction;
    private static double _deadline;
    private static TutorialStep _previous;
    private static int _placed;
    private static int _steps;
    private static bool _stopping;

    static TutorialSceneSmokeTest()
    {
        if (SessionState.GetBool(RunningKey, false)) EditorApplication.update += Update;
    }

    public static void Run()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Scene smoke test is batch-only; it exits its editor process.");
        TutorialRegressionTests.RunAll();
        EditorSceneManager.OpenScene("Assets/Scenes/GamePlayScene.unity");
        var manager = UnityEngine.Object.FindAnyObjectByType<GamePlay.GameManager>();
        var serialized = new SerializedObject(manager);
        var infos = serialized.FindProperty("_gameInfoList");
        infos.arraySize = 1;
        infos.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameInfo>("Assets/GameInfos/Guard1.asset");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Directory.CreateDirectory("Logs/TutorialVerification");
        SessionState.SetBool(RunningKey, true);
        EditorApplication.EnterPlaymode();
    }

    private static void Update()
    {
        if (_stopping || !EditorApplication.isPlaying) return;
        if (_deadline == 0) { _deadline = EditorApplication.timeSinceStartup + 110; _nextAction = EditorApplication.timeSinceStartup + 3; }
        if (EditorApplication.timeSinceStartup > _deadline) { Finish(false, "timed out at " + TutorialController.Instance?.CurrentStep?.Id); return; }
        if (EditorApplication.timeSinceStartup < _nextAction) return;
        _nextAction = EditorApplication.timeSinceStartup + 1.5;
        try
        {
            var tutorial = TutorialController.Instance;
            if (tutorial == null) throw new Exception("scene did not initialize tutorial service");
            if (!tutorial.IsActive)
            {
                if (_steps != 20) throw new Exception("expected 20 steps, visited " + _steps);
                if (TurnManager.Instance.GetCurrentTurn() != 0 || SuspicionManager.Instance.GetCurrentSuspicion() != 0)
                    throw new Exception("practice counters leaked into real puzzle");
                if (UnityEngine.Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Length == 0)
                    throw new Exception("scene UI disappeared");
                foreach (var image in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
                    if (image.name.StartsWith("Tutorial White") || image.name.StartsWith("Tutorial Overlay"))
                        throw new Exception("tutorial presentation remained visible after completion: " + image.name);
                VerifyNarrativeRestored();
                Finish(true, "20 steps, live views and animations, restored puzzle");
                return;
            }
            var step = tutorial.CurrentStep;
            if (step != _previous)
            {
                _previous = step; _placed = 0; _steps++;
                var field = typeof(DialogueView).GetField("_dialogueText", BindingFlags.Instance | BindingFlags.NonPublic);
                var label = (TMP_Text)field.GetValue(DialogueView.Instance);
                label.ForceMeshUpdate();
                if (label.isTextOverflowing) throw new Exception("dialogue overflows at " + step.Id);
                if (!string.IsNullOrEmpty(step.Text) && !label.text.Contains(step.Text))
                    throw new Exception("wrong dialogue displayed at " + step.Id);
                VerifyCallout(step.Id, label);
                Debug.Log("TUTORIAL_SCENE_STEP: " + step.Id);
                if (step.Id == "first-placement" || step.Id == "suspicion" || step.Id == "gap" || step.Id == "weak-neighbours" || step.Id == "free-practice" || step.Id == "end-turn")
                {
                    Capture(step.Id);
                    Capture(step.Id + "-4x3", 1280, 960);
                }
                return;
            }
            if (TurnManager.Instance.GetTurnState() == TurnState.FlippingTransition) return;
            switch (step.Completion)
            {
                case TutorialCompletion.Next:
                    var next = (Button)typeof(DialogueView).GetField("_nextButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DialogueView.Instance);
                    next.onClick.Invoke();
                    break;
                case TutorialCompletion.PlaceCells:
                    if (_placed >= step.PlacementCount) throw new Exception("placement gate stalled at " + step.Id);
                    var coord = step.AllowedCells.Count > 0 ? step.AllowedCells[_placed].ToCoord() : new Coord(_placed * 4, _placed * 4);
                    if (!(BoardView.Instance is BoardView boardView) || !boardView.TryGetCellObject(coord, out var cell))
                        throw new Exception("missing clickable board cell");
                    var pointer = (Vector2)Camera.main.WorldToScreenPoint(cell.transform.position);
                    if (DialogueView.Instance.ContainsScreenPoint(pointer)) throw new Exception("dialogue covers required cell at " + step.Id);
                    cell.GetComponent<BoardCellClickView>().HandlePointerDown(pointer);
                    _placed++;
                    break;
                case TutorialCompletion.EndTurn:
                    var endTarget = ButtonUIView.Instance.GetEndTurnButtonTarget();
                    var endButton = endTarget.GetComponent<Button>();
                    var endCanvas = endTarget.GetComponentInParent<Canvas>();
                    var endPoint = RectTransformUtility.WorldToScreenPoint(endCanvas.worldCamera, endTarget.position);
                    var hits = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = endPoint }, hits);
                    if (hits.Count == 0 || hits[0].gameObject.GetComponentInParent<Button>() != endButton)
                        throw new Exception("end-turn button is blocked by " + (hits.Count == 0 ? "no UI" : hits[0].gameObject.name));
                    endButton.onClick.Invoke();
                    break;
            }
        }
        catch (Exception exception) { Finish(false, exception.ToString()); }
    }

    private static void VerifyCallout(string step, TMP_Text label)
    {
        if (label.color != Color.white) throw new Exception("tutorial caption is not white at " + step);
        var panel = (GameObject)typeof(DialogueView).GetField("_dialoguePanel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DialogueView.Instance);
        if (panel.GetComponent<Image>().enabled) throw new Exception("old dialogue background is visible at " + step);
        var panelRect = (RectTransform)panel.transform;
        var root = (RectTransform)panelRect.parent;
        var targets = new List<Rect>();
        TutorialOverlayView.Instance.GetFocusRects(root, targets);
        var caption = new Rect(panelRect.anchoredPosition + root.rect.min, panelRect.rect.size);
        if (!root.rect.Contains(caption.min) || !root.rect.Contains(caption.max))
            throw new Exception("caption leaves viewport at " + step);
        foreach (var target in targets)
            if (caption.Overlaps(target)) throw new Exception("caption covers highlighted target at " + step);
        bool outlined = false;
        foreach (var image in TutorialOverlayView.Instance.GetComponentsInChildren<Image>())
            if (image.name.StartsWith("Tutorial White Outline"))
            {
                outlined = true;
                if (image.raycastTarget) throw new Exception("outline intercepts clicks at " + step);
            }
        if (targets.Count > 0 && !outlined) throw new Exception("target has no white outline at " + step);
    }

    private static void Finish(bool success, string message)
    {
        _stopping = true;
        SessionState.SetBool(RunningKey, false);
        EditorApplication.update -= Update;
        if (success) Debug.Log("TUTORIAL_SCENE_PASSED: " + message);
        else Debug.LogError("TUTORIAL_SCENE_FAILED: " + message);
        EditorApplication.Exit(success ? 0 : 1);
    }

    private static void VerifyNarrativeRestored()
    {
        DialogueManager.Instance.Show(new DialogueEntry("", "일반 대사 표시 확인", TutorialState.None), () => { }, true);
        var panel = (GameObject)typeof(DialogueView).GetField("_dialoguePanel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DialogueView.Instance);
        var label = (TMP_Text)typeof(DialogueView).GetField("_dialogueText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DialogueView.Instance);
        if (!panel.GetComponent<Image>().enabled || label.color != Color.black || !panel.GetComponent<AspectRatioFitter>().enabled)
            throw new Exception("normal dialogue presentation was not restored");
        DialogueManager.Instance.Hide();
    }

    private static void RefreshCallout()
    {
        Canvas.ForceUpdateCanvases();
        typeof(TutorialOverlayView).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(TutorialOverlayView.Instance, null);
        typeof(DialogueView).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(DialogueView.Instance, null);
    }

    private static void Capture(string name, int width = 1920, int height = 1080)
    {
        // Batchmode has no Game View backbuffer. Render the camera and camera-space canvases.
        var camera = Camera.main;
        var previousTarget = camera.targetTexture;
        var previousRect = camera.rect;
        var previousActive = RenderTexture.active;
        var target = new RenderTexture(width, height, 24);
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.rect = new Rect(0, 0, 1, 1);
            RefreshCallout();
            camera.Render();
            RefreshCallout();
            var label = (TMP_Text)typeof(DialogueView).GetField("_dialogueText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DialogueView.Instance);
            VerifyCallout(name, label);
            if (label.isTextOverflowing) throw new Exception("caption overflows at " + name);
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes("Logs/TutorialVerification/" + name + ".png", image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            camera.rect = previousRect;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
            RefreshCallout();
        }
    }
}
