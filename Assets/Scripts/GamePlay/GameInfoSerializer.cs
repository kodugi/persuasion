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
        public int Version = 2;
        public int Width;
        public int Height;
        public int MaxTurns;
        public int TargetNumber;
        public List<RowData> BoardRows = new List<RowData>();
        public List<SerializableDialogueTriggerData> DialogueTriggers = new List<SerializableDialogueTriggerData>();
        public SerializableDialogueData GameOverDialogue;

        public SerializableGameInfo()
        {
        }

        public static SerializableGameInfo FromGameInfo(GameInfo gameInfo)
        {
            Cell[,] board = gameInfo.GetBoard();
            SerializableGameInfo serializableGameInfo = new SerializableGameInfo
            {
                Version = 2,
                Width = gameInfo.GetWidth(),
                Height = gameInfo.GetHeight(),
                MaxTurns = gameInfo.GetMaxTurns(),
                TargetNumber = gameInfo.GetTargetNumber(),
                BoardRows = CreateRows(board),
                DialogueTriggers = CreateDialogueTriggers(gameInfo.GetDialogueDataDict()),
                GameOverDialogue = CreateGameOverDialogue(gameInfo)
            };

            return serializableGameInfo;
        }

        public GameInfo ToGameInfo()
        {
            int width = Math.Max(1, Width);
            int height = Math.Max(1, Height);
            Cell[,] board = CreateBoard(width, height);
            Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueData = CreateDialogueDataDict();
            DialogueData gameOverDialogue = null;
            GameOverDialogue?.TryCreateDialogueData(out gameOverDialogue);

            GameInfo gameInfo = ScriptableObject.CreateInstance<GameInfo>();
            gameInfo.Initialize(width, height, board, MaxTurns, TargetNumber, dialogueData, gameOverDialogue);
            return gameInfo;
        }

        private static SerializableDialogueData CreateGameOverDialogue(GameInfo gameInfo)
        {
            return gameInfo.TryGetGameOverDialogue(out DialogueData dialogueData)
                ? SerializableDialogueData.FromDialogueData(dialogueData)
                : null;
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

        private static List<SerializableDialogueTriggerData> CreateDialogueTriggers(
            Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueData)
        {
            List<SerializableDialogueTriggerData> dialogueTriggers = new List<SerializableDialogueTriggerData>();
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
                    SerializableDialogueTriggerData trigger = SerializableDialogueTriggerData.FromDialogueData(
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

            foreach (SerializableDialogueTriggerData trigger in DialogueTriggers)
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
    public class SerializableDialogueData
    {
        public List<DialoguePageData> Pages = new List<DialoguePageData>();

        public bool TryCreateDialogueData(out DialogueData dialogueData)
        {
            dialogueData = null;
            List<List<DialogueEntry>> dialogueList = new List<List<DialogueEntry>>();

            if (Pages != null)
            {
                foreach (DialoguePageData page in Pages)
                {
                    if (page == null || !page.TryCreateDialogueEntries(out List<DialogueEntry> entries))
                    {
                        continue;
                    }

                    dialogueList.Add(entries);
                }
            }

            if (dialogueList.Count == 0)
            {
                return false;
            }

            dialogueData = new DialogueData(dialogueList);
            return true;
        }

        public static SerializableDialogueData FromDialogueData(DialogueData dialogueData)
        {
            if (dialogueData == null || dialogueData.DialogueList == null)
            {
                return null;
            }

            SerializableDialogueData data = new SerializableDialogueData();
            foreach (List<DialogueEntry> dialoguePage in dialogueData.DialogueList)
            {
                DialoguePageData page = DialoguePageData.FromDialogueEntries(dialoguePage);
                if (page != null)
                {
                    data.Pages.Add(page);
                }
            }

            return data.Pages.Count == 0 ? null : data;
        }
    }

    [Serializable]
    public class SerializableDialogueTriggerData
    {
        [Min(0)] public int Turn;
        public TurnState TurnState;
        public List<DialoguePageData> Pages = new List<DialoguePageData>();

        public bool TryCreateDialogueData(out DialogueData dialogueData)
        {
            dialogueData = null;
            List<List<DialogueEntry>> dialogueList = new List<List<DialogueEntry>>();

            if (Pages != null)
            {
                foreach (DialoguePageData page in Pages)
                {
                    if (page == null || !page.TryCreateDialogueEntries(out List<DialogueEntry> entries))
                    {
                        continue;
                    }

                    dialogueList.Add(entries);
                }
            }

            if (dialogueList.Count == 0)
            {
                return false;
            }

            dialogueData = new DialogueData(dialogueList);
            return true;
        }

        public static SerializableDialogueTriggerData FromDialogueData(
            int turn,
            TurnState turnState,
            DialogueData dialogueData)
        {
            if (dialogueData == null || dialogueData.DialogueList == null)
            {
                return null;
            }

            SerializableDialogueTriggerData trigger = new SerializableDialogueTriggerData
            {
                Turn = Math.Max(0, turn),
                TurnState = turnState
            };

            foreach (List<DialogueEntry> dialoguePage in dialogueData.DialogueList)
            {
                DialoguePageData page = DialoguePageData.FromDialogueEntries(dialoguePage);
                if (page != null)
                {
                    trigger.Pages.Add(page);
                }
            }

            return trigger.Pages.Count == 0 ? null : trigger;
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
