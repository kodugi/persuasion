using GamePlay;
using Newtonsoft.Json.Linq;
namespace MapEditor
{
    public class EditorUtils
    {
        /*public GameInfo ParseGameInfo(string json)
        {
            JObject jObject = JObject.Parse(json);
            int width = (int)jObject["cols"];
            int height = (int)jObject["rows"];
            Cell[,] cells = new Cell[width, height];
            JArray tiles = (JArray)jObject["tiles"];
            for (int row = 0; row < height; row++)
            {
                JArray tileRow = (JArray)tiles[row];
                for (int col = 0; col < width; col++)
                {
                    string cellString = (string)tileRow[col];
                    cells[col, row] = ParseCellString(cellString, col, row);
                }
            }
            int maxTurns = (int)jObject["settings"]["maxTurns"];
            int targetNumber = (int)jObject["settings"]["goal"];

            return new GameInfo(width, height, cells, maxTurns, targetNumber);
        }

        private Cell ParseCellString(string cellString, int col, int row)
        {
            switch (cellString)
            {
                case "empty":
                    return new EmptyCell(new Vector2Int(col, row));
                case "black":
                    return new BlackCell(new Vector2Int(col, row));
                case "scorn":
                    return new DisdainCell(new Vector2Int(col, row));
                default:
                    return new EmptyCell(new Vector2Int(col, row));
            }
        }

        public string SerializeGameInfo(GameInfo gameInfo)
        {
            JArray tiles = new JArray();
            Cell[,] board = gameInfo.GetBoard();

            for (int row = 0; row < gameInfo.GetHeight(); row++)
            {
                JArray tileRow = new JArray();
                for (int col = 0; col < gameInfo.GetWidth(); col++)
                {
                    tileRow.Add(SerializeCell(board[col, row]));
                }
                tiles.Add(tileRow);
            }

            JObject jObject = new JObject
            {
                ["version"] = 1,
                ["cols"] = gameInfo.GetWidth(),
                ["rows"] = gameInfo.GetHeight(),
                ["tiles"] = tiles,
                ["settings"] = new JObject
                {
                    ["goal"] = gameInfo.GetTargetNumber(),
                    ["maxTurns"] = gameInfo.GetMaxTurns(),
                    ["religion"] = false
                }
            };

            return jObject.ToString();
        }

        private string SerializeCell(Cell cell)
        {
            if (cell is DisdainCell)
            {
                return "scorn";
            }

            if (cell is BlackCell)
            {
                return "black";
            }

            return "empty";
        }*/
    }
}
