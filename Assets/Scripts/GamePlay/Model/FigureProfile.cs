using UnityEngine;

namespace GamePlay
{
    [CreateAssetMenu(fileName = "FigureProfile", menuName = "GamePlay/Figure Profile")]
    public class FigureProfile : ScriptableObject
    {
        [Header("Visual")]
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Vector2 _anchoredPosition;
        [SerializeField] private Vector2 _sizeDelta = new Vector2(700f, 2000f);
        [SerializeField] private Vector3 _localScale = Vector3.one;

        [Header("Animation")]
        [Tooltip("Each figure may use a different Animator Controller.")]
        [SerializeField] private RuntimeAnimatorController _animatorController;
        [Tooltip("Optional. Leave empty to use the controller's default state.")]
        [SerializeField] private string _initialState;

        public Sprite Sprite => _sprite;
        public Vector2 AnchoredPosition => _anchoredPosition;
        public Vector2 SizeDelta => _sizeDelta;
        public Vector3 LocalScale => _localScale;
        public RuntimeAnimatorController AnimatorController => _animatorController;
        public string InitialState => _initialState;
    }
}
