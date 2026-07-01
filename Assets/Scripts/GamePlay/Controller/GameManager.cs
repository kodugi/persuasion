using System.Collections;
using System.Linq;
using UnityEngine;

namespace GamePlay
{
    public class GameManager : MonoBehaviour
    {
        private TurnManager _turnManager;
        private BlockSelectionManager _blockSelectionManager;
        private BoardController _boardController;
        private SuspicionManager _suspicionManager;
        private WinConditionManager _winConditionManager;
        private void Awake()
        {
            _turnManager = new TurnManager();
            _blockSelectionManager = new BlockSelectionManager();
            _boardController = new BoardController();
            _suspicionManager = new SuspicionManager();
            _winConditionManager = new WinConditionManager();
            // TODO: 하드코딩을 실제 값으로 대체
            IBlock[] blockList = { new BasicBlock(), new LieBlock(), new ThreatBlock(), new ReligiousBlock() };
            Cell[,] exampleBoard = new Cell[5, 5];
            for(int i = 0; i < 5; i++) {
                for(int j = 0; j < 5; j++)
                {
                    if(i == 1 && j == 2)
                    {
                        exampleBoard[i, j] = new DisdainCell(new Vector2Int(i, j));
                        continue;
                    }
                    if((i + j) % 3 == 0)
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
            if(GameInfoManager.GetGameInfo() == null)
            {
                GameInfoManager.SetGameInfo(exampleGameInfo);
            }
            _blockSelectionManager.Initialize(blockList.ToList());
            _boardController.Initialize();
            _suspicionManager.Initialize(100,38);
            _winConditionManager.Initialize();
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
