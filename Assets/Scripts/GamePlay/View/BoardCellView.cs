using GamePlay;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

public class BoardCellView : MonoBehaviour
{
    private BoardViewBase _boardView;
    private Vector2Int _coord;

    public void Initialize(BoardViewBase boardView, Vector2Int coord)
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

    private void OnMouseEnter()
    {
        if(_boardView == null)
        {
            return;
        }

        _boardView.HandleCellEnter(_coord);
    }

    private void OnMouseExit()
    {
        if (_boardView == null)
        {
            return;
        }

        _boardView.HandleCellExit(_coord);
    }
}
