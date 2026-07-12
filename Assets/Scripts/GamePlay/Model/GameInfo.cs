using System;
using System.Collections.Generic;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    [CreateAssetMenu(fileName = "GameInfo", menuName = "GamePlay/Game Info")]
    public class GameInfo : ScriptableObject
    {
        [SerializeField, Min(1)] private int _width = 5;
        [SerializeField, Min(1)] private int _height = 5;
        [SerializeField, Min(0)] private int _maxTurns = 10;
        [SerializeField, Min(0)] private int _targetNumber = 5;
        [SerializeField] private List<BoardRowData> _boardRows = new List<BoardRowData>();
        [SerializeField] private List<DialogueTriggerData> _dialogueTriggers = new List<DialogueTriggerData>();

        public int GetWidth()
        {
            return Math.Max(1, _width);
        }

        public int GetHeight()
        {
            return Math.Max(1, _height);
        }

        public Cell[,] GetBoard()
        {
            int width = GetWidth();
            int height = GetHeight();
            Cell[,] board = new Cell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    board[x, y] = CreateCell(GetCellType(x, y), new Vector2Int(x, y));
                }
            }

            return board;
        }

        public int GetMaxTurns()
        {
            return Math.Max(0, _maxTurns);
        }

        public int GetTargetNumber()
        {
            return Math.Max(0, _targetNumber);
        }

        public Dictionary<int, Dictionary<TurnState, DialogueData>> GetDialogueDataDict()
        {
            Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueDataDict =
                new Dictionary<int, Dictionary<TurnState, DialogueData>>();

            if (_dialogueTriggers == null)
            {
                return dialogueDataDict;
            }

            foreach (DialogueTriggerData trigger in _dialogueTriggers)
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

        public void Initialize(
            int width,
            int height,
            Cell[,] board,
            int maxTurns,
            int targetNumber,
            Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueData = null)
        {
            _width = Math.Max(1, width);
            _height = Math.Max(1, height);
            _maxTurns = Math.Max(0, maxTurns);
            _targetNumber = Math.Max(0, targetNumber);

            ResizeBoardRows();
            SetBoardData(board);
            SetDialogueData(dialogueData);
        }

        public void Initialize(
            Cell[,] board,
            int maxTurns,
            int targetNumber
        )
        {
            Initialize(board.GetLength(0), board.GetLength(1), board, maxTurns, targetNumber);
        }

        private void OnEnable()
        {
            ResizeBoardRows();
        }

        private void OnValidate()
        {
            _width = Math.Max(1, _width);
            _height = Math.Max(1, _height);
            _maxTurns = Math.Max(0, _maxTurns);
            _targetNumber = Math.Max(0, _targetNumber);
            ResizeBoardRows();
        }

        private InitialCellType GetCellType(int x, int y)
        {
            if (_boardRows == null ||
                y < 0 ||
                y >= _boardRows.Count ||
                _boardRows[y] == null ||
                _boardRows[y].Cells == null ||
                x < 0 ||
                x >= _boardRows[y].Cells.Count)
            {
                return InitialCellType.Empty;
            }

            return _boardRows[y].Cells[x];
        }

        private void SetBoardData(Cell[,] board)
        {
            if (board == null)
            {
                return;
            }

            int width = Math.Min(GetWidth(), board.GetLength(0));
            int height = Math.Min(GetHeight(), board.GetLength(1));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _boardRows[y].Cells[x] = GetCellType(board[x, y]);
                }
            }
        }

        private void SetDialogueData(Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueData)
        {
            _dialogueTriggers = new List<DialogueTriggerData>();

            if (dialogueData == null)
            {
                return;
            }

            foreach (KeyValuePair<int, Dictionary<TurnState, DialogueData>> turnPair in dialogueData)
            {
                if (turnPair.Value == null)
                {
                    continue;
                }

                foreach (KeyValuePair<TurnState, DialogueData> statePair in turnPair.Value)
                {
                    DialogueTriggerData trigger = DialogueTriggerData.FromDialogueData(
                        turnPair.Key,
                        statePair.Key,
                        statePair.Value);

                    if (trigger != null)
                    {
                        _dialogueTriggers.Add(trigger);
                    }
                }
            }
        }

        private void ResizeBoardRows()
        {
            int width = GetWidth();
            int height = GetHeight();

            if (_boardRows == null)
            {
                _boardRows = new List<BoardRowData>();
            }

            while (_boardRows.Count < height)
            {
                _boardRows.Add(new BoardRowData());
            }

            while (_boardRows.Count > height)
            {
                _boardRows.RemoveAt(_boardRows.Count - 1);
            }

            for (int y = 0; y < _boardRows.Count; y++)
            {
                if (_boardRows[y] == null)
                {
                    _boardRows[y] = new BoardRowData();
                }

                _boardRows[y].Resize(width);
            }
        }

        private static Cell CreateCell(InitialCellType cellType, Vector2Int coord)
        {
            switch (cellType)
            {
                case InitialCellType.Black:
                    return new BlackCell(coord);
                case InitialCellType.WeakBlack:
                    return new WeakBlackCell(coord);
                case InitialCellType.Disdain:
                    return new DisdainCell(coord);
                case InitialCellType.Concept:
                    return new ConceptCell(0, coord);
                case InitialCellType.Lie:
                    return new LieCell(0, coord);
                case InitialCellType.Threat:
                    return new ThreatCell(0, coord);
                case InitialCellType.Religious:
                    return new ReligiousCell(0, coord);
                case InitialCellType.Empty:
                default:
                    return new EmptyCell(coord);
            }
        }

        private static InitialCellType GetCellType(Cell cell)
        {
            if (cell is ReligiousCell)
            {
                return InitialCellType.Religious;
            }

            if (cell is ThreatCell)
            {
                return InitialCellType.Threat;
            }

            if (cell is LieCell)
            {
                return InitialCellType.Lie;
            }

            if (cell is ConceptCell)
            {
                return InitialCellType.Concept;
            }

            if (cell is DisdainCell)
            {
                return InitialCellType.Disdain;
            }

            if (cell is WeakBlackCell)
            {
                return InitialCellType.WeakBlack;
            }

            if (cell is BlackCell)
            {
                return InitialCellType.Black;
            }

            return InitialCellType.Empty;
        }

        public enum InitialCellType
        {
            Empty,
            Black,
            WeakBlack,
            Disdain,
            Concept,
            Lie,
            Threat,
            Religious
        }

        [Serializable]
        public class BoardRowData
        {
            public List<InitialCellType> Cells = new List<InitialCellType>();

            public void Resize(int width)
            {
                if (Cells == null)
                {
                    Cells = new List<InitialCellType>();
                }

                while (Cells.Count < width)
                {
                    Cells.Add(InitialCellType.Empty);
                }

                while (Cells.Count > width)
                {
                    Cells.RemoveAt(Cells.Count - 1);
                }
            }
        }

        [Serializable]
        public class DialogueTriggerData
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

            public static DialogueTriggerData FromDialogueData(int turn, TurnState turnState, DialogueData dialogueData)
            {
                if (dialogueData == null || dialogueData.DialogueList == null)
                {
                    return null;
                }

                DialogueTriggerData trigger = new DialogueTriggerData
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
        public class DialoguePageData
        {
            public List<DialogueEntryData> Entries = new List<DialogueEntryData>();

            public bool TryCreateDialogueEntries(out List<DialogueEntry> entries)
            {
                entries = new List<DialogueEntry>();

                if (Entries != null)
                {
                    foreach (DialogueEntryData entry in Entries)
                    {
                        if (entry == null)
                        {
                            continue;
                        }

                        entries.Add(entry.CreateDialogueEntry());
                    }
                }

                return entries.Count > 0;
            }

            public static DialoguePageData FromDialogueEntries(List<DialogueEntry> dialogueEntries)
            {
                if (dialogueEntries == null)
                {
                    return null;
                }

                DialoguePageData page = new DialoguePageData();

                foreach (DialogueEntry dialogueEntry in dialogueEntries)
                {
                    if (dialogueEntry != null)
                    {
                        page.Entries.Add(DialogueEntryData.FromDialogueEntry(dialogueEntry));
                    }
                }

                return page.Entries.Count == 0 ? null : page;
            }
        }

        [Serializable]
        public class DialogueEntryData
        {
            public string SpeakerName;
            [TextArea(2, 6)] public string DialogueText;
            public TutorialState StateToTrigger = TutorialState.None;

            public DialogueEntry CreateDialogueEntry()
            {
                return new DialogueEntry(SpeakerName, DialogueText, StateToTrigger);
            }

            public static DialogueEntryData FromDialogueEntry(DialogueEntry dialogueEntry)
            {
                if (dialogueEntry == null)
                {
                    return null;
                }

                return new DialogueEntryData
                {
                    SpeakerName = dialogueEntry.SpeakerName,
                    DialogueText = dialogueEntry.DialogueText,
                    StateToTrigger = dialogueEntry.StateToTrigger
                };
            }
        }
    }
}
