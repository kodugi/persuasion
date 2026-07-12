using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    public static class GameInfoSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public static string SerializeGameInfo(GameInfo gameInfo)
        {
            if (gameInfo == null)
            {
                throw new ArgumentNullException(nameof(gameInfo));
            }

            SerializableGameInfo serializableGameInfo = SerializableGameInfo.FromGameInfo(gameInfo);
            return JsonConvert.SerializeObject(serializableGameInfo, Formatting.Indented, Settings);
        }

        public static GameInfo DeserializeGameInfo(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON is empty.", nameof(json));
            }

            SerializableGameInfo serializableGameInfo = JsonConvert.DeserializeObject<SerializableGameInfo>(json, Settings);
            if (serializableGameInfo == null)
            {
                throw new JsonSerializationException("Could not deserialize GameInfo JSON.");
            }

            return serializableGameInfo.ToGameInfo();
        }
    }

    [Serializable]
    public class SerializableGameInfo
    {
        public int Version = 1;
        public int Width;
        public int Height;
        public int MaxTurns;
        public int TargetNumber;
        public List<RowData> BoardRows = new List<RowData>();
        public List<GameInfo.DialogueTriggerData> DialogueTriggers = new List<GameInfo.DialogueTriggerData>();

        public SerializableGameInfo()
        {
        }

        public static SerializableGameInfo FromGameInfo(GameInfo gameInfo)
        {
            Cell[,] board = gameInfo.GetBoard();
            SerializableGameInfo serializableGameInfo = new SerializableGameInfo
            {
                Version = 1,
                Width = gameInfo.GetWidth(),
                Height = gameInfo.GetHeight(),
                MaxTurns = gameInfo.GetMaxTurns(),
                TargetNumber = gameInfo.GetTargetNumber(),
                BoardRows = CreateRows(board),
                DialogueTriggers = CreateDialogueTriggers(gameInfo.GetDialogueDataDict())
            };

            return serializableGameInfo;
        }

        public GameInfo ToGameInfo()
        {
            int width = Math.Max(1, Width);
            int height = Math.Max(1, Height);
            Cell[,] board = CreateBoard(width, height);
            Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueData = CreateDialogueDataDict();

            GameInfo gameInfo = ScriptableObject.CreateInstance<GameInfo>();
            gameInfo.Initialize(width, height, board, MaxTurns, TargetNumber, dialogueData);
            return gameInfo;
        }

        private static List<RowData> CreateRows(Cell[,] board)
        {
            List<RowData> rows = new List<RowData>();
            if (board == null)
            {
                return rows;
            }

            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                List<CellKind> cellKinds = new List<CellKind>();
                for (int x = 0; x < width; x++)
                {
                    cellKinds.Add(board[x, y] == null ? CellKind.Empty : board[x, y].CellKind);
                }

                rows.Add(new RowData(cellKinds));
            }

            return rows;
        }

        private static List<GameInfo.DialogueTriggerData> CreateDialogueTriggers(
            Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueData)
        {
            List<GameInfo.DialogueTriggerData> dialogueTriggers = new List<GameInfo.DialogueTriggerData>();
            if (dialogueData == null)
            {
                return dialogueTriggers;
            }

            foreach (KeyValuePair<int, Dictionary<TurnState, DialogueData>> turnPair in dialogueData)
            {
                if (turnPair.Value == null)
                {
                    continue;
                }

                foreach (KeyValuePair<TurnState, DialogueData> statePair in turnPair.Value)
                {
                    GameInfo.DialogueTriggerData trigger = GameInfo.DialogueTriggerData.FromDialogueData(
                        turnPair.Key,
                        statePair.Key,
                        statePair.Value);

                    if (trigger != null)
                    {
                        dialogueTriggers.Add(trigger);
                    }
                }
            }

            return dialogueTriggers;
        }

        private Cell[,] CreateBoard(int width, int height)
        {
            Cell[,] board = new Cell[width, height];
            bool isLegacyColumnData = IsLegacyColumnData(width);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CellKind cellKind = isLegacyColumnData
                        ? GetCellKind(x, y)
                        : GetCellKind(y, x);

                    board[x, y] = CreateCell(cellKind, x, y);
                }
            }

            return board;
        }

        private Dictionary<int, Dictionary<TurnState, DialogueData>> CreateDialogueDataDict()
        {
            Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueDataDict =
                new Dictionary<int, Dictionary<TurnState, DialogueData>>();

            if (DialogueTriggers == null)
            {
                return dialogueDataDict;
            }

            foreach (GameInfo.DialogueTriggerData trigger in DialogueTriggers)
            {
                if (trigger == null || !trigger.TryCreateDialogueData(out DialogueData dialogueData))
                {
                    continue;
                }

                if (!dialogueDataDict.TryGetValue(trigger.Turn, out Dictionary<TurnState, DialogueData> dialogueDataByState))
                {
                    dialogueDataByState = new Dictionary<TurnState, DialogueData>();
                    dialogueDataDict.Add(trigger.Turn, dialogueDataByState);
                }

                dialogueDataByState[trigger.TurnState] = dialogueData;
            }

            return dialogueDataDict;
        }

        private bool IsLegacyColumnData(int width)
        {
            return BoardRows != null &&
                   BoardRows.Count == width &&
                   BoardRows.Exists(row => row != null && row.HasLegacyCells());
        }

        private CellKind GetCellKind(int rowIndex, int cellIndex)
        {
            if (BoardRows == null || rowIndex < 0 || rowIndex >= BoardRows.Count)
            {
                return CellKind.Empty;
            }

            List<CellKind> cells = BoardRows[rowIndex]?.GetCells();
            if (cells == null || cellIndex < 0 || cellIndex >= cells.Count)
            {
                return CellKind.Empty;
            }

            return cells[cellIndex];
        }

        private static Cell CreateCell(CellKind cellKind, int x, int y)
        {
            Type cellType = CellUtils.CellKindToType(cellKind);
            if (cellType == null)
            {
                return new EmptyCell(0, new Vector2Int(x, y));
            }

            Cell cell = Activator.CreateInstance(cellType, 0, new Vector2Int(x, y)) as Cell;
            return cell ?? new EmptyCell(0, new Vector2Int(x, y));
        }
    }

    [Serializable]
    public class RowData
    {
        public List<CellKind> Cells = new List<CellKind>();
        [JsonProperty("_cellKinds", NullValueHandling = NullValueHandling.Ignore)]
        public List<CellKind> LegacyCellKinds;

        public RowData()
        {
        }

        public RowData(List<CellKind> cellKinds)
        {
            Cells = cellKinds ?? new List<CellKind>();
        }

        public bool HasLegacyCells()
        {
            return (Cells == null || Cells.Count == 0) && LegacyCellKinds != null;
        }

        public List<CellKind> GetCells()
        {
            if (Cells != null && Cells.Count > 0)
            {
                return Cells;
            }

            return LegacyCellKinds;
        }
    }
}
