using System.Collections;
using SingletonUtils;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    [RequireComponent(typeof(Image))]
    public class BlackOutPanelView: SelfInitializingMonoBehaviourSingleton<BlackOutPanelView>
    {
        [Header("Suspicion Overflow Blackout")]
        [SerializeField, Min(0f)] private float _suspicionOverflowFadeDuration = 0.45f;
        [SerializeField, Min(0f)] private float _suspicionOverflowHoldDuration = 0.35f;

        [Header("Scripted Dream Blackout")]
        [SerializeField, Min(0f)] private float _scriptedDreamDelay = 3f;

        private Image _image;
        private Coroutine _scriptedDreamBlackOutCoroutine;

        protected override bool InitializeCore()
        {
            if (GameStateManager.Instance == null)
            {
                return false;
            }

            _image = GetComponent<Image>();
            GameStateManager.Instance.RaiseSetGameStateEvent += HandleSetGameStateEvent;
            ResetGame();
            return true;
        }

        protected override void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.RaiseSetGameStateEvent -= HandleSetGameStateEvent;
            }

            base.OnDestroy();
        }

        public void ResetGame()
        {
            StopAllCoroutines();
            _scriptedDreamBlackOutCoroutine = null;

            if (_image == null)
            {
                _image = GetComponent<Image>();
            }

            _image.color = new Color(0f, 0f, 0f, 0f);
            _image.raycastTarget = false;
        }

        public IEnumerator PlayBlackOut()
        {
            if (!EnsureInitialized())
            {
                yield break;
            }

            _image.raycastTarget = true;
            _image.color = new Color(0f, 0f, 0f, 0f);

            float elapsed = 0f;
            while (elapsed < _suspicionOverflowFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = _suspicionOverflowFadeDuration <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / _suspicionOverflowFadeDuration);
                _image.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }

            _image.color = Color.black;

            if (_suspicionOverflowHoldDuration > 0f)
            {
                yield return new WaitForSeconds(_suspicionOverflowHoldDuration);
            }
        }

        private void HandleSetGameStateEvent(System.Object sender, SetGameStateEventArgs e)
        {
            if (e.gameState == GameState.Lost &&
                WinConditionManager.Instance.GetLastDefeatReason() == DefeatReason.Scripted &&
                GameInfoHolder.GetCurrentGameInfo().GetMapType() == GameInfo.MapType.Dream4)
            {
                _scriptedDreamBlackOutCoroutine = StartCoroutine(PlayScriptedDreamBlackOut());
            }
        }

        private IEnumerator PlayScriptedDreamBlackOut()
        {
            yield return new WaitForSeconds(_scriptedDreamDelay);
            GamePlaySoundManager.Instance?.Play(
                GamePlaySoundId.JumpScare,
                _suspicionOverflowFadeDuration + _suspicionOverflowHoldDuration);
            yield return PlayBlackOut();
            _scriptedDreamBlackOutCoroutine = null;
        }
    }
}
