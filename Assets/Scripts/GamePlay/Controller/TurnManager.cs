namespace GamePlay
{
    public class TurnManager
    {
        public static TurnManager Instance { get; private set; }
        public TurnManager()
        {
            if (Instance != null)
            {
                throw new System.Exception("TurnManager instance already exists!");
            }
            Instance = this;
        }
        private TurnState _currentState;
        public TurnState GetTurnState()
        {
            return _currentState;
        }
        private int _currentTurn;
        public int GetCurrentTurn()
        {
            return _currentTurn;
        }

        public void Initialize()
        {
            _currentState = TurnState.PlayerIdle;
            _currentTurn = 0;
        }
    }
}