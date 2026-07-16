using GamePlay;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

public class BoardView : BoardViewBase
{
    private BlockSelectionManager _subscribedBlockSelectionManager;

    protected override bool InitializeCore()
    {
        SubscribeToBlockSelectionEvents();
        return base.InitializeCore();
    }

    protected override GameInfo GetGameInfo()
    {
        return GameInfoHolder.GetGameInfo();
    }

    public override void Refresh()
    {
        base.Refresh();
        SubscribeToBlockSelectionEvents();
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

    public override void SetCell(Vector2Int coord, Cell cell)
    {
        base.SetCell(coord, cell);
        ClearTutorialHint(coord);
    }

    public override void HandleCellClick(Vector2Int coord)
    {
        if (coord == null)
        {
            Debug.LogWarning("BoardView ignored a cell click because coord is null.", this);
            return;
        }

        if (!IsInRenderedBoard(coord))
        {
            Debug.LogWarning("BoardView ignored cell click " + coord + " because it is outside the rendered board.", this);
            return;
        }

        if (BoardController.Instance == null)
        {
            Debug.LogWarning("BoardView could not send cell click " + coord + " because BoardController.Instance is null.", this);
            return;
        }

        ClearBlockPreview();
        BoardController.Instance.HandleCellPlacementInput(coord);
    }

    public override void RefreshCellMarkers()
    {
        base.RefreshCellMarkers();
        
        Cell[,] originalBoard = _gameInfo.GetBoard();
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

                SetBaseMarker(x, y, GetMarkerForOriginalCell(originalBoard[x, y], x, y, hasReachableCells, reachableCells));
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

    protected override BoardCellMarker GetInitialMarker(Cell originalCell)
    {
        if (originalCell is BlackCell)
        {
            return BoardCellMarker.OriginalBlack;
        }

        return BoardCellMarker.None;
    }

    private BoardCellMarker GetMarkerForOriginalCell(Cell originalCell, int x, int y, bool hasReachableCells, bool[,] reachableCells)
    {
        BoardCellMarker marker = GetInitialMarker(originalCell);
        if (!marker.HasFlag(BoardCellMarker.OriginalBlack))
        {
            return marker;
        }

        if (hasReachableCells && IsInReachableCells(reachableCells, x, y) && !reachableCells[x, y])
        {
            marker |= BoardCellMarker.Locked;
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
}
