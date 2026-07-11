using GamePlay;
using System;
using System.Collections.Generic;
using UnityEngine;
using SingletonUtils;
using Vector2Int = GamePlay.Vector2Int;

public abstract class BoardViewBase : SelfInitializingMonoBehaviourSingleton<BoardViewBase>
{
    [Flags]
    public enum BoardCellMarker
    {
        None = 0,
        OriginalBlack = 1,
        Locked = 2,
        Preview = 4
    }

    [Serializable]
    protected class CellPrefabEntry
    {
        public CellKind CellKind;
        public GameObject Prefab;
    }

    [Serializable]
    protected class CellPreviewSpriteEntry
    {
        public CellKind CellKind;
        public Sprite Sprite;
    }

    [SerializeField] protected Transform _cellRoot;
    [SerializeField] protected Transform _markerRoot;
    [SerializeField] protected CellPrefabEntry[] _cellPrefabs = new CellPrefabEntry[0];
    [SerializeField] protected CellPreviewSpriteEntry[] _cellPreviewSprites = new CellPreviewSpriteEntry[0];
    [SerializeField] protected BoardCellMarkerView _markerPrefab;
    [SerializeField] protected Color _previewColor = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] protected float _cellSize = 1f;
    [SerializeField] protected Vector2 _origin;
    [SerializeField] protected bool _centerBoard = true;

    protected GameInfo _gameInfo;
    protected readonly Dictionary<Type, GameObject> _prefabsByCellType = new Dictionary<Type, GameObject>();
    protected readonly Dictionary<Type, Sprite> _previewSpritesByCellType = new Dictionary<Type, Sprite>();
    protected readonly List<GameObject> _spawnedCells = new List<GameObject>();
    protected readonly List<BoardCellMarkerView> _spawnedMarkers = new List<BoardCellMarkerView>();
    protected GameObject[,] _spawnedCellsByCoord;
    protected BoardCellMarkerView[,] _spawnedMarkersByCoord;
    protected BoardCellMarker[,] _baseMarkersByCoord;
    protected Sprite[,] _tutorialHintSpritesByCoord;
    protected GamePlay.Vector2Int _previewedCoord;
    protected Sprite _previewedSprite;

    protected void OnValidate()
    {
        if (_cellSize <= 0f)
        {
            _cellSize = 0.01f;
        }
    }

    protected override bool InitializeCore()
    {
        _gameInfo = GetGameInfo();
        if (_gameInfo == null)
        {
            Debug.LogWarning("BoardView could not initialize because GameInfo is null.", this);
            return false;
        }

        BuildPrefabMap();
        BuildPreviewSpriteMap();
        RenderBoard();
        return _spawnedCellsByCoord != null;
    }

    protected abstract GameInfo GetGameInfo();

    public virtual void Refresh()
    {
        if (_gameInfo == null)
        {
            EnsureInitialized();
            return;
        }

        BuildPrefabMap();
        BuildPreviewSpriteMap();
        RenderBoard();
        SetInitialized(_spawnedCellsByCoord != null);
    }

    public virtual void SetCell(GamePlay.Vector2Int coord, Cell cell)
    {
        if (coord == null)
        {
            Debug.LogWarning("BoardView could not update a cell because coord is null.", this);
            return;
        }

        if (_spawnedCellsByCoord == null)
        {
            EnsureInitialized();
        }

        if (_spawnedCellsByCoord == null)
        {
            Debug.LogWarning("BoardView could not update cell " + coord + " because the board has not been rendered.", this);
            return;
        }

        if (!IsInRenderedBoard(coord))
        {
            Debug.LogWarning("BoardView could not update cell " + coord + " because it is outside the rendered board.", this);
            return;
        }
        
        ReplaceCellObject(coord.X, coord.Y, cell);
        RefreshCellMarkers();
    }

    public abstract void HandleCellClick(GamePlay.Vector2Int coord);

    protected void BuildPrefabMap()
    {
        _prefabsByCellType.Clear();

        foreach (CellPrefabEntry entry in _cellPrefabs)
        {
            if (entry == null || entry.Prefab == null)
            {
                continue;
            }

            Type cellType = GetCellType(entry.CellKind);
            if (_prefabsByCellType.ContainsKey(cellType))
            {
                Debug.LogWarning("Duplicate prefab mapping for " + cellType.Name + ". The later entry will be used.", this);
            }

            _prefabsByCellType[cellType] = entry.Prefab;
        }
    }

    protected void BuildPreviewSpriteMap()
    {
        _previewSpritesByCellType.Clear();

        foreach (CellPreviewSpriteEntry entry in _cellPreviewSprites)
        {
            if (entry == null || entry.Sprite == null)
            {
                continue;
            }

            Type cellType = GetCellType(entry.CellKind);
            if (_previewSpritesByCellType.ContainsKey(cellType))
            {
                Debug.LogWarning("Duplicate preview sprite mapping for " + cellType.Name + ". The later entry will be used.", this);
            }

            _previewSpritesByCellType[cellType] = entry.Sprite;
        }
    }

    protected void RenderBoard()
    {
        Cell[,] board = _gameInfo.GetBoard();
        if (board == null)
        {
            Debug.LogWarning("BoardView could not render because GameInfo board is null.", this);
            return;
        }

        int width = _gameInfo.GetWidth();
        int height = _gameInfo.GetHeight();
        if (width <= 0 || height <= 0)
        {
            if (!TryGetBoardDimensions(board, out width, out height))
            {
                Debug.LogWarning("BoardView could not render because board dimensions are invalid.", this);
                return;
            }
        }

        ClearRenderedBoard();
        _spawnedCellsByCoord = new GameObject[width, height];
        _spawnedMarkersByCoord = new BoardCellMarkerView[width, height];
        _baseMarkersByCoord = new BoardCellMarker[width, height];
        _previewedCoord = null;
        _previewedSprite = null;

        for (int x = 0; x < width; x++)
        {
            if (x >= board.GetLength(0))
            {
                Debug.LogWarning("BoardView skipped column " + x + " because the board data is missing.", this);
                continue;
            }

            for (int y = 0; y < height; y++)
            {
                if (y >= board.GetLength(1))
                {
                    Debug.LogWarning("BoardView skipped cell (" + x + ", " + y + ") because the board data is missing.", this);
                    continue;
                }

                SpawnMarker(x, y, width, height);
                SetBaseMarker(x, y, GetInitialMarker(board[x, y]));
                SpawnCell(board[x, y], x, y, width, height);
            }
        }

        RefreshCellMarkers();
    }

    public virtual void RefreshCellMarkers()
    {
        if (_gameInfo == null || _baseMarkersByCoord == null)
        {
            return;
        }
        ClearBlockPreview();
    }

    protected void SpawnCell(Cell cell, int x, int y, int width, int height)
    {
        if (cell == null)
        {
            return;
        }

        if (!TryGetPrefab(cell.GetType(), out GameObject prefab))
        {
            Debug.LogWarning("BoardView has no prefab mapping for " + cell.GetType().Name + ".", this);
            return;
        }

        Transform parent = GetRenderRoot(_cellRoot);
        GameObject cellObject = Instantiate(prefab, parent);
        cellObject.name = cell.GetType().Name + " (" + x + ", " + y + ")";
        cellObject.transform.localPosition = GetCellLocalPosition(x, y, width, height);
        ConfigureCellClick(cellObject, x, y);
        _spawnedCells.Add(cellObject);
        _spawnedCellsByCoord[x, y] = cellObject;
        ConfigureMarkerSorting(x, y, cellObject);
    }

    protected void SpawnMarker(int x, int y, int width, int height)
    {
        if (_markerPrefab == null)
        {
            return;
        }

        Transform parent = GetRenderRoot(_markerRoot);
        BoardCellMarkerView markerView = Instantiate(_markerPrefab, parent);
        markerView.name = "Cell Marker (" + x + ", " + y + ")";
        markerView.transform.localPosition = GetCellLocalPosition(x, y, width, height);
        markerView.NormalizeLargeRendererOffsets(_cellSize * 2f);
        ApplyMarker(markerView, BoardCellMarker.None);
        _spawnedMarkers.Add(markerView);
        _spawnedMarkersByCoord[x, y] = markerView;
    }

    protected void ConfigureCellClick(GameObject cellObject, int x, int y)
    {
        GamePlay.Vector2Int coord = new GamePlay.Vector2Int(x, y);
        BoxCollider2D[] clickColliders = cellObject.GetComponentsInChildren<BoxCollider2D>(true);
        if (clickColliders.Length == 0)
        {
            Debug.LogWarning("BoardView could not configure click handling for " + cellObject.name + " because it has no BoxCollider2D.", this);
            return;
        }

        foreach (BoxCollider2D clickCollider in clickColliders)
        {
            ConfigureClickHandler(clickCollider.gameObject, coord);
        }
    }

    protected void ConfigureClickHandler(GameObject target, GamePlay.Vector2Int coord)
    {
        BoardCellView cellView = target.GetComponent<BoardCellView>();
        if (cellView == null)
        {
            cellView = target.AddComponent<BoardCellView>();
        }

        cellView.Initialize(this, coord);
    }

    protected Transform GetRenderRoot(Transform preferredRoot)
    {
        if (preferredRoot != null)
        {
            return preferredRoot;
        }

        return _cellRoot == null ? transform : _cellRoot;
    }

    protected void ConfigureMarkerSorting(int x, int y, GameObject cellObject)
    {
        if (_spawnedMarkersByCoord == null || _spawnedMarkersByCoord[x, y] == null)
        {
            return;
        }

        if (!TryGetTopSpriteSorting(cellObject, out int sortingLayerID, out int sortingOrder))
        {
            return;
        }

        _spawnedMarkersByCoord[x, y].SetSorting(sortingLayerID, sortingOrder);
    }

    protected static bool TryGetTopSpriteSorting(GameObject target, out int sortingLayerID, out int sortingOrder)
    {
        SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        if (spriteRenderers.Length == 0)
        {
            sortingLayerID = 0;
            sortingOrder = 0;
            return false;
        }

        SpriteRenderer topRenderer = spriteRenderers[0];
        int topLayerValue = SortingLayer.GetLayerValueFromID(topRenderer.sortingLayerID);
        int topSortingOrder = topRenderer.sortingOrder;

        for (int i = 1; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            int layerValue = SortingLayer.GetLayerValueFromID(spriteRenderer.sortingLayerID);
            if (layerValue > topLayerValue || (layerValue == topLayerValue && spriteRenderer.sortingOrder > topSortingOrder))
            {
                topRenderer = spriteRenderer;
                topLayerValue = layerValue;
                topSortingOrder = spriteRenderer.sortingOrder;
            }
        }

        sortingLayerID = topRenderer.sortingLayerID;
        sortingOrder = topRenderer.sortingOrder;
        return true;
    }

    protected void ReplaceCellObject(int x, int y, Cell cell)
    {
        GameObject previousCellObject = _spawnedCellsByCoord[x, y];
        if (previousCellObject != null)
        {
            _spawnedCells.Remove(previousCellObject);
            DestroyCellObject(previousCellObject);
            _spawnedCellsByCoord[x, y] = null;
        }

        if (cell == null)
        {
            return;
        }

        SpawnCell(cell, x, y, _spawnedCellsByCoord.GetLength(0), _spawnedCellsByCoord.GetLength(1));
    }

    protected bool TryGetPrefab(Type cellType, out GameObject prefab)
    {
        if (_prefabsByCellType.TryGetValue(cellType, out prefab))
        {
            return prefab != null;
        }

        foreach (KeyValuePair<Type, GameObject> entry in _prefabsByCellType)
        {
            if (entry.Value != null && entry.Key.IsAssignableFrom(cellType))
            {
                prefab = entry.Value;
                return true;
            }
        }

        prefab = null;
        return false;
    }

    protected bool TryGetPreviewSprite(Type cellType, out Sprite sprite)
    {
        if (_previewSpritesByCellType.TryGetValue(cellType, out sprite))
        {
            return sprite != null;
        }

        foreach (KeyValuePair<Type, Sprite> entry in _previewSpritesByCellType)
        {
            if (entry.Value != null && entry.Key.IsAssignableFrom(cellType))
            {
                sprite = entry.Value;
                return true;
            }
        }

        sprite = null;
        return false;
    }

    protected Vector3 GetCellLocalPosition(int x, int y, int width, int height)
    {
        float centerOffsetX = _centerBoard ? (width - 1) * _cellSize * 0.5f : 0f;
        float centerOffsetY = _centerBoard ? (height - 1) * _cellSize * 0.5f : 0f;

        return new Vector3(
            _origin.x + x * _cellSize - centerOffsetX,
            _origin.y + y * _cellSize - centerOffsetY,
            0f
        );
    }

    protected void ClearRenderedBoard()
    {
        ClearSpawnedCells();
        ClearSpawnedMarkers();
    }

    protected void ClearSpawnedCells()
    {
        foreach (GameObject spawnedCell in _spawnedCells)
        {
            DestroyCellObject(spawnedCell);
        }

        _spawnedCells.Clear();
        _spawnedCellsByCoord = null;
    }

    protected void ClearSpawnedMarkers()
    {
        foreach (BoardCellMarkerView spawnedMarker in _spawnedMarkers)
        {
            if (spawnedMarker != null)
            {
                DestroyCellObject(spawnedMarker.gameObject);
            }
        }

        _spawnedMarkers.Clear();
        _spawnedMarkersByCoord = null;
        _baseMarkersByCoord = null;
        _previewedCoord = null;
        _previewedSprite = null;
    }

    protected void DestroyCellObject(GameObject cellObject)
    {
        if (cellObject == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(cellObject);
        }
        else
        {
            DestroyImmediate(cellObject);
        }
    }

    protected bool IsInRenderedBoard(GamePlay.Vector2Int coord)
    {
        return _spawnedCellsByCoord != null
            && coord.X >= 0
            && coord.X < _spawnedCellsByCoord.GetLength(0)
            && coord.Y >= 0
            && coord.Y < _spawnedCellsByCoord.GetLength(1);
    }

    protected static bool TryGetBoardDimensions(Cell[,] board, out int width, out int height)
    {
        width = board.GetLength(0);
        height = board.GetLength(1);
        return width > 0 && height > 0;
    }

    protected static Type GetCellType(CellKind kind)
    {
        switch (kind)
        {
            case CellKind.Empty:
                return typeof(EmptyCell);
            case CellKind.Black:
                return typeof(BlackCell);
            case CellKind.WeakBlack:
                return typeof(WeakBlackCell);
            case CellKind.Concept:
                return typeof(ConceptCell);
            case CellKind.Lie:
                return typeof(LieCell);
            case CellKind.Threat:
                return typeof(ThreatCell);
            case CellKind.Disdain:
                return typeof(DisdainCell);
            case CellKind.Religious:
                return typeof(ReligiousCell);
            default:
                throw new ArgumentOutOfRangeException("kind", kind, null);
        }
    }

    public void HandleCellEnter(GamePlay.Vector2Int coord)
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

        RenderBlockPreview(coord);
    }

    public void HandleCellExit(GamePlay.Vector2Int coord)
    {
        if (coord == null || _previewedCoord == null || coord != _previewedCoord)
        {
            return;
        }

        ClearBlockPreview();
    }

    protected void RenderBlockPreview(GamePlay.Vector2Int coord)
    {
        ClearBlockPreview();

        if (!IsCellPlacementAllowed(coord))
        {
            return;
        }
        if (!TryGetPreviewSprite(GetCellType(), out Sprite previewSprite))
        {
            return;
        }

        _previewedCoord = new GamePlay.Vector2Int(coord);
        _previewedSprite = previewSprite;
        ApplyMarkerForCoord(coord);
    }

    protected abstract Type GetCellType();

    protected abstract bool IsCellPlacementAllowed(Vector2Int coord);

    protected void ClearBlockPreview()
    {
        if (_previewedCoord == null)
        {
            return;
        }

        GamePlay.Vector2Int coord = _previewedCoord;
        _previewedCoord = null;
        _previewedSprite = null;

        if (IsInRenderedBoard(coord))
        {
            ApplyMarkerForCoord(coord);
        }
    }

    public void SetCellMarker(GamePlay.Vector2Int coord, BoardCellMarker marker, Sprite previewSprite)
    {
        if (coord == null || !IsInRenderedBoard(coord))
        {
            return;
        }

        if (_spawnedMarkersByCoord == null)
        {
            return;
        }

        BoardCellMarkerView boardCellMarkerView = _spawnedMarkersByCoord[coord.X, coord.Y];
        if (boardCellMarkerView == null)
        {
            return;
        }

        ApplyMarker(boardCellMarkerView, marker, previewSprite);
    }

    protected void SetBaseMarker(int x, int y, BoardCellMarker marker)
    {
        if (_baseMarkersByCoord == null)
        {
            return;
        }

        _baseMarkersByCoord[x, y] = marker & ~BoardCellMarker.Preview;
        if (_spawnedMarkersByCoord != null && _spawnedMarkersByCoord[x, y] != null)
        {
            ApplyMarkerForCoord(new GamePlay.Vector2Int(x, y));
        }
    }

    protected BoardCellMarker GetBaseMarker(GamePlay.Vector2Int coord)
    {
        if (coord == null || _baseMarkersByCoord == null || !IsInRenderedBoard(coord))
        {
            return BoardCellMarker.None;
        }

        return _baseMarkersByCoord[coord.X, coord.Y];
    }

    protected virtual BoardCellMarker GetInitialMarker(Cell originalCell)
    {
        return BoardCellMarker.None;
    }

    protected void ApplyMarker(BoardCellMarkerView markerView, BoardCellMarker marker)
    {
        ApplyMarker(markerView, marker, null);
    }

    protected void ApplyMarker(BoardCellMarkerView markerView, BoardCellMarker marker, Sprite previewSprite)
    {
        markerView.SetTargetBorderVisible(marker.HasFlag(BoardCellMarker.OriginalBlack));
        markerView.SetLockedVisible(marker.HasFlag(BoardCellMarker.Locked));

        if (marker.HasFlag(BoardCellMarker.Preview))
        {
            markerView.SetPreview(previewSprite, _previewColor);
        }
        else
        {
            markerView.ClearPreview();
        }
    }

    protected void ApplyMarkerForCoord(GamePlay.Vector2Int coord)
    {
        if (coord == null || !IsInRenderedBoard(coord) || _spawnedMarkersByCoord == null)
        {
            return;
        }

        BoardCellMarkerView markerView = _spawnedMarkersByCoord[coord.X, coord.Y];
        if (markerView == null)
        {
            return;
        }

        (BoardCellMarker marker, Sprite previewSprite) = GetMarkerAndSprite(coord);
        ApplyMarker(markerView, marker, previewSprite);
    }

    protected virtual (BoardCellMarker, Sprite) GetMarkerAndSprite(Vector2Int coord)
    {
        BoardCellMarker marker = GetBaseMarker(coord);
        Sprite previewSprite = null;
        
        if (_previewedCoord != null && _previewedCoord == coord && _previewedSprite != null)
        {
            marker |= BoardCellMarker.Preview;
            previewSprite = _previewedSprite;
        }
        
        return (marker, previewSprite);
    }
}
