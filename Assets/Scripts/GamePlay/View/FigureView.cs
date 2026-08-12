using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    [RequireComponent(typeof(Image), typeof(Animator))]
    public class FigureView : MonoBehaviour
    {
        [SerializeField] private FigureProfile _fallbackProfile;

        private Image _image;
        private RectTransform _rectTransform;
        private Animator _animator;
        private Coroutine _glitchCoroutine;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            SuspicionManager.Instance.RaiseSuspicionOverflowEvent += HandleSuspicionOverflowEvent;
            SuspicionManager.Instance.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
            TutorialController.Instance.RaiseSetTutorialStateEvent += HandleSetTutorialStateEvent;
            ResetGame();
        }

        public void ResetGame()
        {
            if (_glitchCoroutine != null)
            {
                StopCoroutine(_glitchCoroutine);
            }
            
            GameInfo gameInfo = GetCurrentGameInfo();
            FigureProfile profile = gameInfo != null ? gameInfo.GetFigureProfile() : null;
            Apply(profile != null ? profile : _fallbackProfile);
            
            if (gameInfo.GetMapType() == GameInfo.MapType.Dream3)
            {
                _glitchCoroutine = StartCoroutine(PlayRepetitiveGlitchAnimation());
            }
        }

        public void Apply(FigureProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning("FigureView could not apply a figure because its FigureProfile is missing.", this);
                return;
            }

            _image.sprite = profile.Sprite;
            _rectTransform.anchoredPosition = profile.AnchoredPosition;
            _rectTransform.sizeDelta = profile.SizeDelta;
            _rectTransform.localScale = profile.LocalScale;

            if (profile.AnimatorController == null)
            {
                Debug.LogWarning($"FigureProfile '{profile.name}' has no Animator Controller.", profile);
                return;
            }

            _animator.runtimeAnimatorController = profile.AnimatorController;
            _animator.Rebind();

            if (!string.IsNullOrWhiteSpace(profile.InitialState))
            {
                int stateHash = Animator.StringToHash(profile.InitialState);
                if (_animator.HasState(0, stateHash))
                {
                    _animator.Play(stateHash, 0, 0f);
                }
                else
                {
                    Debug.LogWarning(
                        $"FigureProfile '{profile.name}' does not have state '{profile.InitialState}' on layer 0.",
                        profile);
                }
            }

            // Immediately samples the controller's default or requested initial state.
            _animator.Update(0f);
        }

        private static GameInfo GetCurrentGameInfo()
        {
            var gameInfoList = GameInfoHolder.GetGameInfoList();
            return gameInfoList != null && gameInfoList.Count > 0
                ? GameInfoHolder.GetCurrentGameInfo()
                : null;
        }

        private void HandleSuspicionOverflowEvent(object sender, SetSuspicionEventArgs e)
        {
            if (SuspicionManager.Instance.GetCurrentSuspicion() <= SuspicionManager.Instance.GetMaxSuspicion())
            {
                switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
                {
                    case GameInfo.MapType.Dream2:
                        _animator.SetTrigger("glitch");
                        break;
                }
            }
        }

        private void HandleSetSuspicionEvent(object sender, SetSuspicionEventArgs e)
        {
            
        }

        private IEnumerator PlayRepetitiveGlitchAnimation()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(4f, 5f));
                _animator.SetTrigger("glitch");
            }
        }

        private void HandleSetTutorialStateEvent(object sender, SetTutorialStateEventArgs e)
        {
            if (e.CurrentState == TutorialState.Dream1)
            {
                _animator.SetTrigger("gameover");
            }
        }
    }
}
