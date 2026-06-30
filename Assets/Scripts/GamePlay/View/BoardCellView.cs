using GamePlay;
using UnityEngine;

public class BoardCellView : MonoBehaviour
{
    private BoardView _boardView;
    private GamePlay.Vector2Int _coord;

    public void Initialize(BoardView boardView, GamePlay.Vector2Int coord)
    {
        _boardView = boardView;
        _coord = coord;
    }

    private void OnMouseDown()
    {
        if (_boardView == null)
        {
            return;
        }

        _boardView.HandleCellClick(_coord);
    }
}
