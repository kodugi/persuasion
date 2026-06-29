using GamePlay;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardView : SelfInitializingMonoBehaviourSingleton<BoardView>
{
    private enum CellPrefabKind
    {
        Empty,
        Black,
        WeakBlack,
        Concept,
        Lie,
        Threat
    }

    [Serializable]
    private class CellPrefabEntry
    {
        public CellPrefabKind CellKind;
        public GameObject Prefab;
    }

    [SerializeField] private Transform _cellRoot;
    [SerializeField] private CellPrefabEntry[] _cellPrefabs = new CellPrefabEntry[0];
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private Vector2 _origin;
    [SerializeField] private bool _centerBoard = true;

    private GameInfo _gameInfo;
    private readonly Dictionary<Type, GameObject> _prefabsByCellType = new Dictionary<Type, GameObject>();
    private readonly List<GameObject> _spawnedCells = new List<GameObject>();
    private GameObject[,] _spawnedCellsByCoord;
    private void OnValidate()
    {
        if (_cellSize <= 0f)
        {
            _cellSize = 0.01f;
        }
    }

    protected override bool InitializeCore()
    {
        if (GameInfoManager.Instance == null)
        {
            Debug.LogWarning("BoardView could not initialize because GameInfoManager.Instance is null.", this);
            return false;
        }

        _gameInfo = GameInfoManager.Instance.GetGameInfo();
        if (_gameInfo == null)
        {
            Debug.LogWarning("BoardView could not initialize because GameInfo is null.", this);
            return false;
        }

        BuildPrefabMap();
        RenderBoard();
        return _spawnedCellsByCoord != null;
    }

    public void Refresh()
    {
        if (_gameInfo == null)
        {
            EnsureInitialized();
            return;
        }

        BuildPrefabMap();
        RenderBoard();
        SetInitialized(_spawnedCellsByCoord != null);
    }

    public void SetCell(GamePlay.Vector2Int coord, Cell cell)
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
    }

    public void HandleCellClick(GamePlay.Vector2Int coord)
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

        BoardController.Instance.HandleCellPlacementInput(coord);
    }

    private void BuildPrefabMap()
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

    private void RenderBoard()
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

        ClearSpawnedCells();
        _spawnedCellsByCoord = new GameObject[width, height];

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

                SpawnCell(board[x, y], x, y, width, height);
            }
        }
    }

    private void SpawnCell(Cell cell, int x, int y, int width, int height)
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

        Transform parent = _cellRoot == null ? transform : _cellRoot;
        GameObject cellObject = Instantiate(prefab, parent);
        cellObject.name = cell.GetType().Name + " (" + x + ", " + y + ")";
        cellObject.transform.localPosition = GetCellLocalPosition(x, y, width, height);
        ConfigureCellClick(cellObject, x, y);
        _spawnedCells.Add(cellObject);
        _spawnedCellsByCoord[x, y] = cellObject;
    }

    private void ConfigureCellClick(GameObject cellObject, int x, int y)
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

    private void ConfigureClickHandler(GameObject target, GamePlay.Vector2Int coord)
    {
        BoardCellView cellView = target.GetComponent<BoardCellView>();
        if (cellView == null)
        {
            cellView = target.AddComponent<BoardCellView>();
        }

        cellView.Initialize(this, coord);
    }

    private void ReplaceCellObject(int x, int y, Cell cell)
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

    private bool TryGetPrefab(Type cellType, out GameObject prefab)
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

    private Vector3 GetCellLocalPosition(int x, int y, int width, int height)
    {
        float centerOffsetX = _centerBoard ? (width - 1) * _cellSize * 0.5f : 0f;
        float centerOffsetY = _centerBoard ? (height - 1) * _cellSize * 0.5f : 0f;

        return new Vector3(
            _origin.x + x * _cellSize - centerOffsetX,
            _origin.y + y * _cellSize - centerOffsetY,
            0f
        );
    }

    private void ClearSpawnedCells()
    {
        foreach (GameObject spawnedCell in _spawnedCells)
        {
            DestroyCellObject(spawnedCell);
        }

        _spawnedCells.Clear();
        _spawnedCellsByCoord = null;
    }

    private void DestroyCellObject(GameObject cellObject)
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

    private bool IsInRenderedBoard(GamePlay.Vector2Int coord)
    {
        return _spawnedCellsByCoord != null
            && coord.X >= 0
            && coord.X < _spawnedCellsByCoord.GetLength(0)
            && coord.Y >= 0
            && coord.Y < _spawnedCellsByCoord.GetLength(1);
    }

    private static bool TryGetBoardDimensions(Cell[,] board, out int width, out int height)
    {
        width = board.GetLength(0);
        height = board.GetLength(1);
        return width > 0 && height > 0;
    }

    private static Type GetCellType(CellPrefabKind kind)
    {
        switch (kind)
        {
            case CellPrefabKind.Empty:
                return typeof(EmptyCell);
            case CellPrefabKind.Black:
                return typeof(BlackCell);
            case CellPrefabKind.WeakBlack:
                return typeof(WeakBlackCell);
            case CellPrefabKind.Concept:
                return typeof(ConceptCell);
            case CellPrefabKind.Lie:
                return typeof(LieCell);
            case CellPrefabKind.Threat:
                return typeof(ThreatCell);
            default:
                throw new ArgumentOutOfRangeException("kind", kind, null);
        }
    }
}
