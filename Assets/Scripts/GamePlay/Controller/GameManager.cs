using System.Collections;
using System.Linq;
using UnityEngine;

namespace GamePlay
{
    public class GameManager : MonoBehaviour
    {
        private TurnManager _turnManager;
        private GameInfoManager _gameInfoManager;
        private BlockSelectionManager _blockSelectionManager;
        private BoardController _boardController;
        private void Awake()
        {
            _turnManager = new TurnManager();
            _gameInfoManager = new GameInfoManager();
            _blockSelectionManager = new BlockSelectionManager();
            _boardController = new BoardController();

            IBlock[] blockList = { new BasicBlock() };
            Cell[,] exampleBoard = new Cell[5, 5];
            for(int i = 0; i < 5; i++) {
                for(int j = 0; j < 5; j++)
                {
                    if((i + j) % 2 == 0)
                    {
                        exampleBoard[i, j] = new BlackCell(new Vector2Int(i, j));
                    }
                    else
                    {
                        exampleBoard[i, j] = new EmptyCell(new Vector2Int(i, j));
                    }
                }
            }
            GameInfo exampleGameInfo = new GameInfo(5, 5, exampleBoard, 10, 2);

            _turnManager.Initialize();
            _gameInfoManager.Initialize(exampleGameInfo);
            _blockSelectionManager.Initialize(blockList.ToList());
            _boardController.Initialize();
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
