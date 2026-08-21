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
        [Tooltip("Vertical offset used when FigureView matches the Investigation dialogue layout. " +
                 "The value uses the Investigation canvas' 800-pixel-wide reference space.")]
        [SerializeField] private float _investigationVerticalOffset;

        [Header("Focus")]
        [Tooltip("Normalized head position inside the figure image. (0, 0) is bottom-left and (1, 1) is top-right.")]
        [SerializeField] private Vector2 _headPosition = new Vector2(0.5f, 0.5f);

        [Header("Animation")]
        [Tooltip("Each figure may use a different Animator Controller.")]
        [SerializeField] private RuntimeAnimatorController _animatorController;
        [Tooltip("Optional. Leave empty to use the controller's default state.")]
        [SerializeField] private string _initialState;

        public Sprite Sprite => _sprite;
        public Vector2 AnchoredPosition => _anchoredPosition;
        public Vector2 SizeDelta => _sizeDelta;
        public Vector3 LocalScale => _localScale;
        public float InvestigationVerticalOffset => _investigationVerticalOffset;
        public Vector2 HeadPosition => _headPosition;
        public RuntimeAnimatorController AnimatorController => _animatorController;
        public string InitialState => _initialState;

        private void OnValidate()
        {
            _headPosition = new Vector2(
                Mathf.Clamp01(_headPosition.x),
                Mathf.Clamp01(_headPosition.y));
        }
    }
}
