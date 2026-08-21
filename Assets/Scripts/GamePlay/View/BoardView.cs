using GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using AnimationUtilsNameSpace;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

public class BoardView : BoardViewBase
{
    [SerializeField] private GameObject _boardCellSuspicionPrefab;
    
    private BlockSelectionManager _subscribedBlockSelectionManager;
    private TutorialController _tutorialController;
    private SuspicionManager _suspicionManager;
    private TurnManager _turnManager;

    private BoardCellMarker _allowedMarkers;

    private SuspicionPrefabView[,] _spawnedBoardCellSuspicionViewsByCoord;
    private bool _isPlayingPreGameOverAnimation;
    private List<Vector2Int> _spawnedPreGameOverAnimationCoroutineCoords;
    private bool  _isPlayingGameOverAnimation;
    private List<Vector2Int> _spawnedGameOverAnimationCoroutineCoords;

    private TurnState _turnStateAfterTransition = TurnState.None;

    protected override bool InitializeCore()
    {
        if (!base.InitializeCore())
        {
            return false;
        }
        if (SuspicionManager.Instance == null)
        {
            return false;
        }

        if (TurnManager.Instance == null)
        {
            return false;
        }
        
        SubscribeToBlockSelectionEvents();
        _allowedMarkers = _gameInfo.GetAllowedMarkers();
        _tutorialController = TutorialController.Instance;
        _tutorialController.RaiseSetTutorialStateEvent += HandleSetTutorialStateEvent;
        _suspicionManager = SuspicionManager.Instance;
        _suspicionManager.RaiseSetSuspicionPreviewEvent += HandleSetSuspicionPreviewEvent;
        _turnManager = TurnManager.Instance;
        _turnManager.RaiseSetTurnStateEvent += HandleSetTurnStateEvent;
        GameStateManager.Instance.RaiseSetGameStateEvent += HandleSetGameStateEvent;
        _spawnedBoardCellSuspicionViewsByCoord = new SuspicionPrefabView[GetGameInfo().GetWidth(), GetGameInfo().GetHeight()];
        SpawnBoardCellSuspicionViews();
        return true;
    }

    protected override GameInfo GetGameInfo()
    {
        return GameInfoHolder.GetCurrentGameInfo();
    }

    public override void Refresh()
    {
        base.Refresh();
        SubscribeToBlockSelectionEvents();
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        StopPreGameOverAnimation();
        StopGameOverAnimation();
        ClearBoardCellSuspicionViews();

        _allowedMarkers = _gameInfo.GetAllowedMarkers();
        _turnStateAfterTransition = TurnState.None;
        base.Refresh();

        _spawnedBoardCellSuspicionViewsByCoord = new SuspicionPrefabView[GetGameInfo().GetWidth(), GetGameInfo().GetHeight()];
        SpawnBoardCellSuspicionViews();
    }

    private void SpawnBoardCellSuspicionViews()
    {
        int width = GetGameInfo().GetWidth();
        int height = GetGameInfo().GetHeight();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Transform parent = GetRenderRoot(_cellRoot);
                GameObject boardCellSuspicionView = Instantiate(_boardCellSuspicionPrefab, parent);
                _spawnedBoardCellSuspicionViewsByCoord[x, y] = boardCellSuspicionView.AddComponent<SuspicionPrefabView>();
                boardCellSuspicionView.name = "board cell suspicion view" + " (" + x + ", " + y + ")";
                _spawnedBoardCellSuspicionViewsByCoord[x, y].Initialize();
                if (!TryGetTopSpriteSorting(_spawnedCellsByCoord[x, y], out int sortingLayerID, out int sortingOrder))
                {
                    // Might need a better logic such as placing SuspicionPrefabView under Cell
                    return;
                }
                _spawnedBoardCellSuspicionViewsByCoord[x, y].SetRendererSorting(sortingLayerID, sortingOrder);
                boardCellSuspicionView.transform.localPosition = GetCellLocalPosition(x, y, width, height);
            }
        }
    }

    private void ClearBoardCellSuspicionViews()
    {
        if (_spawnedBoardCellSuspicionViewsByCoord == null)
        {
            return;
        }

        int width = _spawnedBoardCellSuspicionViewsByCoord.GetLength(0);
        int height = _spawnedBoardCellSuspicionViewsByCoord.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SuspicionPrefabView suspicionPrefabView = _spawnedBoardCellSuspicionViewsByCoord[x, y];
                if (suspicionPrefabView != null)
                {
                    Destroy(suspicionPrefabView.gameObject);
                }
            }
        }

        _spawnedBoardCellSuspicionViewsByCoord = null;
    }

    private void HandleSetSuspicionPreviewEvent(object sender, SetSuspicionEventArgs e)
    {
        if (_suspicionManager.GetCurrentSuspicionPreview() > _suspicionManager.GetMaxSuspicion() && _suspicionManager.GetCurrentSuspicion() <= _suspicionManager.GetMaxSuspicion())
        {
            if (_isPlayingPreGameOverAnimation)
            {
                return;
            }

            switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
            {
                case GameInfo.MapType.Dream1:
                case GameInfo.MapType.Dream2:
                case GameInfo.MapType.Dream3:
                case GameInfo.MapType.Dream4:
                    break;
                default:
                    PlayPreGameOverAnimation();
                    break;
            }
        }
        else
        {
            StopPreGameOverAnimation();
        }
    }

    private void HandleSetGameStateEvent(object sender, SetGameStateEventArgs e)
    {
        if (e.gameState == GameState.Lost)
        {
            if (WinConditionManager.Instance.GetLastDefeatReason() == DefeatReason.TurnLimitExceeded)
            {
                StopPreGameOverAnimation();
                StopGameOverAnimation();
                return;
            }

            if (_isPlayingGameOverAnimation)
            {
                return;
            }

            StartCoroutine(PlayGameOverAnimation());
        }
        else
        {
            StopGameOverAnimation();
        }
    }

    private void PlayPreGameOverAnimation()
    {
        _isPlayingPreGameOverAnimation = true;
        GameManager.Instance?.PlayEyeSound();
        _spawnedPreGameOverAnimationCoroutineCoords = new List<Vector2Int>();
        List<Vector2Int> coords = BoardController.Instance.GetRandomBlackCellCoords(2);
        ResetSpawnedPreGameOverAnimationCoroutineCoords();
        foreach (Vector2Int coord in coords)
        {
            _spawnedBoardCellSuspicionViewsByCoord[coord.X, coord.Y].PlayPreGameOverAnimation();
            _spawnedPreGameOverAnimationCoroutineCoords.Add(coord);
        }
    }

    private void ResetSpawnedPreGameOverAnimationCoroutineCoords()
    {
        foreach (Vector2Int coord in _spawnedPreGameOverAnimationCoroutineCoords)
        {
            _spawnedBoardCellSuspicionViewsByCoord[coord.X, coord.Y].StopPreGameOverAnimation();
        }
        _spawnedPreGameOverAnimationCoroutineCoords = new List<Vector2Int>();
    }

    private void StopPreGameOverAnimation()
    {
        if (_isPlayingPreGameOverAnimation)
        {
            _isPlayingPreGameOverAnimation = false;
            GameManager.Instance?.PlayEyeSound();
            foreach (Vector2Int coord in _spawnedPreGameOverAnimationCoroutineCoords)
            {
                _spawnedBoardCellSuspicionViewsByCoord[coord.X, coord.Y].StopPreGameOverAnimation();
            }
        }
    }
    
    private IEnumerator PlayGameOverAnimation()
    {
        StopPreGameOverAnimation();
        _isPlayingGameOverAnimation = true;
        _spawnedGameOverAnimationCoroutineCoords = new List<Vector2Int>();
        
        List<Vector2Int> coords = BoardController.Instance.GetRandomBlackCellCoords(25);

        _spawnedPreGameOverAnimationCoroutineCoords = new List<Vector2Int>();

        StartCoroutine(AnimationUtils.ExecuteAccordingToCountsPreset(coords, (coord) =>
        {
            _spawnedBoardCellSuspicionViewsByCoord[coord.X, coord.Y].PlayGameOverAnimation();
            _spawnedGameOverAnimationCoroutineCoords.Add(coord);
        }));
        yield return null;
    }
    
    private void StopGameOverAnimation()
    {
        if (_isPlayingGameOverAnimation)
        {
            _isPlayingGameOverAnimation = false;
            foreach (Vector2Int coord in _spawnedGameOverAnimationCoroutineCoords)
            {
                _spawnedBoardCellSuspicionViewsByCoord[coord.X, coord.Y].StopGameOverAnimation();
            }
        }
    }

    protected override bool IsCellPlacementAllowed(Vector2Int coord)
    {
        IBlock selectedBlock = BlockSelectionManager.Instance.GetSelectedBlock();
        if (selectedBlock == null)
        {
            return false;
        }
        return BoardController.Instance.CanPlaceBlock(selectedBlock, coord);
    }

    protected override Type GetCellType()
    {
        IBlock selectedBlock = BlockSelectionManager.Instance.GetSelectedBlock();
        return selectedBlock.GetCellType();
    }

    public void SetTurnStateAfterTransition(TurnState turnState)
    {
        _turnStateAfterTransition = turnState;
    }

    protected override IEnumerator BeforeCellPlacement()
    {
        _turnManager.SetTurnState(TurnState.FlippingTransition);
        yield return null;
    }

    protected override IEnumerator AfterCellPlacement()
    {
        // this is probably the worst way to code ever imaginable
        if (_turnStateAfterTransition == TurnState.None)
        {
            Debug.LogError("Turn state is None");
        }
        _turnManager.SetTurnState(_turnStateAfterTransition);
        yield return null;
    }

    public override void HandleCellClick(Vector2Int coord)
    {
        ClearBlockPreview();
        BoardController.Instance.HandleCellPlacementInput(coord);
    }

    public override void RefreshCellMarkers()
    {
        base.RefreshCellMarkers();
        
        Cell[,] originalBoard = _gameInfo.GetBoard();
        if (_allowedMarkers == BoardCellMarker.None)
        {
            _allowedMarkers = _gameInfo.GetAllowedMarkers();
        }
        BoardCellMarker allowedMarkers = _allowedMarkers;
        if (originalBoard == null)
        {
            return;
        }
        
        bool hasReachableCells = TryGetReachableCells(out bool[,] reachableCells);

        int width = _baseMarkersByCoord.GetLength(0);
        int height = _baseMarkersByCoord.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            if (x >= originalBoard.GetLength(0))
            {
                continue;
            }

            for (int y = 0; y < height; y++)
            {
                if (y >= originalBoard.GetLength(1))
                {
                    continue;
                }

                Type originalCellType = originalBoard[x, y] == null ? null : originalBoard[x, y].GetType();
                SetBaseMarker(x, y, GetMarkerForOriginalCell(originalCellType, x, y, hasReachableCells, reachableCells, allowedMarkers));
            }
        }
    }

    private bool TryGetReachableCells(out bool[,] reachableCells)
    {
        reachableCells = null;

        if (BoardController.Instance == null)
        {
            return false;
        }

        reachableCells = BoardController.Instance.CanBeReached();
        return reachableCells != null;
    }

    private BoardCellMarker GetMarkerForOriginalCell(Type originalCellType, int x, int y, bool hasReachableCells, bool[,] reachableCells, BoardCellMarker allowedMarkers)
    {
        BoardCellMarker marker = GetInitialMarker(originalCellType);
        if (!marker.HasFlag(BoardCellMarker.OriginalBlack))
        {
            return marker;
        }
        marker &= allowedMarkers;

        if (hasReachableCells && IsInReachableCells(reachableCells, x, y) && !reachableCells[x, y])
        {
            marker |= BoardCellMarker.Locked & allowedMarkers;
        }

        return marker;
    }

    private static bool IsInReachableCells(bool[,] reachableCells, int x, int y)
    {
        return reachableCells != null
            && x >= 0
            && x < reachableCells.GetLength(0)
            && y >= 0
            && y < reachableCells.GetLength(1);
    }

    private void SubscribeToBlockSelectionEvents()
    {
        BlockSelectionManager blockSelectionManager = BlockSelectionManager.Instance;
        if (blockSelectionManager == null || ReferenceEquals(_subscribedBlockSelectionManager, blockSelectionManager))
        {
            return;
        }

        UnsubscribeFromBlockSelectionEvents();
        _subscribedBlockSelectionManager = blockSelectionManager;
        _subscribedBlockSelectionManager.RaiseSelectBlockEvent += HandleSelectBlockEvent;
    }

    private void UnsubscribeFromBlockSelectionEvents()
    {
        if (_subscribedBlockSelectionManager == null)
        {
            return;
        }

        _subscribedBlockSelectionManager.RaiseSelectBlockEvent -= HandleSelectBlockEvent;
        _subscribedBlockSelectionManager = null;
    }

    private void HandleSelectBlockEvent(object sender, SelectBlockEventArgs e)
    {
        RefreshCellMarkers();
    }

    protected override void OnDestroy()
    {
        UnsubscribeFromBlockSelectionEvents();
        base.OnDestroy();
    }
    
    public void ShowTutorialHint(Vector2Int coord, Type cellType)
    {
        if (coord == null || cellType == null)
        {
            return;
        }

        if (_spawnedCellsByCoord == null)
        {
            EnsureInitialized();
        }

        if (!IsInRenderedBoard(coord))
        {
            return;
        }

        if (!TryGetPreviewSprite(cellType, out Sprite previewSprite))
        {
            Debug.LogWarning("BoardView could not show tutorial hint because there is no preview sprite for " + cellType.Name + ".", this);
            return;
        }

        EnsureTutorialHintGrid(_spawnedCellsByCoord.GetLength(0), _spawnedCellsByCoord.GetLength(1));
        _tutorialHintSpritesByCoord[coord.X, coord.Y] = previewSprite;
        ApplyMarkerForCoord(coord);
    }

    protected override (BoardCellMarker, Sprite) GetMarkerAndSprite(Vector2Int coord)
    {
        BoardCellMarker marker = GetBaseMarker(coord);
        Sprite previewSprite = null;

        if (_previewedCoord != null && _previewedCoord == coord && _previewedSprite != null)
        {
            marker |= BoardCellMarker.Preview;
            previewSprite = _previewedSprite;
        }
        else if (TryGetTutorialHintSprite(coord, out Sprite tutorialHintSprite))
        {
            marker |= BoardCellMarker.Preview;
            previewSprite = tutorialHintSprite;
        }
        return (marker, previewSprite);
    }

    public bool TryGetCellObject(Vector2Int coord, out GameObject cellObject)
    {
        cellObject = null;
        if (coord == null)
        {
            return false;
        }

        if (_spawnedCellsByCoord == null)
        {
            EnsureInitialized();
        }

        if (!IsInRenderedBoard(coord))
        {
            return false;
        }

        cellObject = _spawnedCellsByCoord[coord.X, coord.Y];
        return cellObject != null;
    }

    public void ClearTutorialHint(Vector2Int coord)
    {
        if (coord == null || _tutorialHintSpritesByCoord == null || !IsInRenderedBoard(coord))
        {
            return;
        }

        _tutorialHintSpritesByCoord[coord.X, coord.Y] = null;
        ApplyMarkerForCoord(coord);
    }

    public void ClearTutorialHints()
    {
        if (_tutorialHintSpritesByCoord == null)
        {
            return;
        }

        int width = _tutorialHintSpritesByCoord.GetLength(0);
        int height = _tutorialHintSpritesByCoord.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _tutorialHintSpritesByCoord[x, y] = null;
                ApplyMarkerForCoord(new Vector2Int(x, y));
            }
        }
    }

    private void EnsureTutorialHintGrid(int width, int height)
    {
        if (_tutorialHintSpritesByCoord != null
            && _tutorialHintSpritesByCoord.GetLength(0) == width
            && _tutorialHintSpritesByCoord.GetLength(1) == height)
        {
            return;
        }

        _tutorialHintSpritesByCoord = new Sprite[width, height];
    }

    private bool TryGetTutorialHintSprite(Vector2Int coord, out Sprite sprite)
    {
        sprite = null;
        if (coord == null
            || _tutorialHintSpritesByCoord == null
            || coord.X < 0
            || coord.X >= _tutorialHintSpritesByCoord.GetLength(0)
            || coord.Y < 0
            || coord.Y >= _tutorialHintSpritesByCoord.GetLength(1))
        {
            return false;
        }

        sprite = _tutorialHintSpritesByCoord[coord.X, coord.Y];
        return sprite != null;
    }

    private void HandleSetTutorialStateEvent(object sender, SetTutorialStateEventArgs e)
    {
        switch (e.CurrentState)
        {
            case TutorialState.ExplainLock:
                _allowedMarkers |= BoardCellMarker.Locked;
                RefreshCellMarkers();
                break;
        }
    }

    private void HandleSetTurnStateEvent(object sender, SetTurnStateEventArgs e)
    {
        RefreshCellMarkers();
    }
}
