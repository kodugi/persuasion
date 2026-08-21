using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    public enum GamePlaySoundId
    {
        JumpScare,
        GameOver,
        Laughter,
        SoulPlace,
        Eye,
        BigEye,
        Glitch,
        ButtonClick
    }

    public sealed class GamePlaySoundManager : MonoBehaviour
    {
        public static GamePlaySoundManager Instance { get; private set; }

        [Header("Background Music")]
        [SerializeField] private AudioClip _mainBGM;
        [SerializeField] private AudioClip _dreamBGM;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip _jumpScareSound;
        [SerializeField] private AudioClip _gameOverSound;
        [SerializeField] private AudioClip[] _laughterSounds = new AudioClip[3];
        [SerializeField] private AudioClip _soulPlaceSound;
        [SerializeField] private AudioClip _eyeSound;
        [SerializeField] private AudioClip _bigEyeSound;
        [SerializeField] private AudioClip _glitchSound;
        [SerializeField] private AudioClip _buttonClickSound;
        [SerializeField, Range(0f, 1f)] private float _soundEffectVolume = 0.75f;
        [SerializeField, Range(0f, 1f)] private float _eyeVolume = 0.9f;
        [SerializeField, Range(0f, 1.2f)] private float _jumpScareVolume = 1.1f;
        [SerializeField, Min(0f)] private float _jumpScareDuration = 3f;

        private AudioSource _bgmAudioSource;
        private AudioSource _effectAudioSource;
        private AudioSource _loopingEffectAudioSource;
        private Coroutine _laughterCoroutine;
        private Coroutine _jumpScareStopCoroutine;
        private bool _audioLockedForJumpScare;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            _bgmAudioSource = CreateAudioSource(true);
            _effectAudioSource = CreateAudioSource(false);
            _loopingEffectAudioSource = CreateAudioSource(true);
        }

        private void Start()
        {
            StopExternalBGM();
            PlayStageBGM();
            RegisterButtonClickSounds();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Play(GamePlaySoundId id, float maximumDuration = -1f)
        {
            if (_audioLockedForJumpScare && id != GamePlaySoundId.JumpScare)
            {
                return;
            }

            switch (id)
            {
                case GamePlaySoundId.JumpScare:
                    PlayJumpScare(maximumDuration);
                    break;
                case GamePlaySoundId.GameOver:
                    PlayLoopingEffect(_gameOverSound);
                    break;
                case GamePlaySoundId.Laughter:
                    PlayLaughterLoop();
                    break;
                case GamePlaySoundId.SoulPlace:
                    PlayOneShot(_soulPlaceSound);
                    break;
                case GamePlaySoundId.Eye:
                    PlayOneShot(_eyeSound, _eyeVolume);
                    break;
                case GamePlaySoundId.BigEye:
                    PlayOneShot(_bigEyeSound);
                    break;
                case GamePlaySoundId.Glitch:
                    PlayOneShot(_glitchSound);
                    break;
                case GamePlaySoundId.ButtonClick:
                    PlayOneShot(_buttonClickSound);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }

        private void RegisterButtonClickSounds()
        {
            foreach (Button button in FindObjectsByType<Button>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                button.onClick.AddListener(PlayButtonClickSound);
            }
        }

        private void PlayButtonClickSound()
        {
            Play(GamePlaySoundId.ButtonClick);
        }

        public void ResetAfterGameOver()
        {
            _audioLockedForJumpScare = false;

            if (_jumpScareStopCoroutine != null)
            {
                StopCoroutine(_jumpScareStopCoroutine);
                _jumpScareStopCoroutine = null;
            }

            StopOngoingEffects();
            _effectAudioSource.Stop();
            _effectAudioSource.resource = null;
            _effectAudioSource.volume = _soundEffectVolume;
            PlayStageBGM();
        }

        private void PlayStageBGM()
        {
            AudioClip clip = IsDreamStage() ? _dreamBGM : _mainBGM;
            if (clip == null)
            {
                Debug.LogWarning("GamePlayScene background music clip is missing.", this);
                return;
            }

            if (_bgmAudioSource.resource == clip && _bgmAudioSource.isPlaying)
            {
                return;
            }

            _bgmAudioSource.Stop();
            _bgmAudioSource.resource = clip;
            _bgmAudioSource.volume = 1f;
            _bgmAudioSource.Play();
        }

        private void StopExternalBGM()
        {
            AudioSource[] audioSources = FindObjectsByType<AudioSource>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (AudioSource source in audioSources)
            {
                if (source != _bgmAudioSource &&
                    source != _effectAudioSource &&
                    source != _loopingEffectAudioSource &&
                    source.loop)
                {
                    source.Stop();
                }
            }
        }

        private static bool IsDreamStage()
        {
            if (GameInfoHolder.GetGameInfoList() == null || GameInfoHolder.GetGameInfoList().Count == 0)
            {
                return false;
            }

            GameInfo.MapType mapType = GameInfoHolder.GetCurrentGameInfo().GetMapType();
            return mapType == GameInfo.MapType.Dream1 ||
                   mapType == GameInfo.MapType.Dream2 ||
                   mapType == GameInfo.MapType.Dream3 ||
                   mapType == GameInfo.MapType.Dream4;
        }

        private void PlayOneShot(AudioClip clip, float volume = -1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("GamePlayScene sound effect clip is missing.", this);
                return;
            }

            _effectAudioSource.PlayOneShot(clip, volume >= 0f ? volume : _soundEffectVolume);
        }

        private void PlayLoopingEffect(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("GamePlayScene looping sound effect clip is missing.", this);
                return;
            }

            StopLaughterLoop();
            if (_loopingEffectAudioSource.resource == clip && _loopingEffectAudioSource.isPlaying)
            {
                return;
            }

            _loopingEffectAudioSource.Stop();
            _loopingEffectAudioSource.resource = clip;
            _loopingEffectAudioSource.loop = true;
            _loopingEffectAudioSource.volume = _soundEffectVolume;
            _loopingEffectAudioSource.Play();
        }

        private void PlayLaughterLoop()
        {
            if (_laughterCoroutine != null)
            {
                return;
            }

            AudioClip[] availableClips = _laughterSounds == null
                ? Array.Empty<AudioClip>()
                : _laughterSounds.Where(clip => clip != null).ToArray();
            if (availableClips.Length == 0)
            {
                Debug.LogWarning("GamePlayScene laughter clips are missing.", this);
                return;
            }

            _loopingEffectAudioSource.Stop();
            _loopingEffectAudioSource.loop = false;
            _laughterCoroutine = StartCoroutine(PlayLaughterLoopCore(availableClips));
        }

        private IEnumerator PlayLaughterLoopCore(AudioClip[] clips)
        {
            int clipIndex = 0;
            while (!_audioLockedForJumpScare)
            {
                AudioClip clip = clips[clipIndex];
                _loopingEffectAudioSource.resource = clip;
                _loopingEffectAudioSource.volume = _soundEffectVolume;
                _loopingEffectAudioSource.Play();
                yield return new WaitForSeconds(clip.length);
                clipIndex = (clipIndex + 1) % clips.Length;
            }

            _laughterCoroutine = null;
        }

        private void StopLaughterLoop()
        {
            if (_laughterCoroutine == null)
            {
                return;
            }

            StopCoroutine(_laughterCoroutine);
            _laughterCoroutine = null;
        }

        private void StopOngoingEffects()
        {
            StopLaughterLoop();
            _loopingEffectAudioSource.Stop();
            _loopingEffectAudioSource.resource = null;
        }

        private void PlayJumpScare(float maximumDuration)
        {
            if (_jumpScareSound == null)
            {
                Debug.LogWarning("GamePlayScene jump-scare clip is missing.", this);
                return;
            }

            StopLaughterLoop();
            AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (AudioSource source in allAudioSources)
            {
                source.Stop();
            }

            _audioLockedForJumpScare = true;
            _effectAudioSource.resource = _jumpScareSound;
            _effectAudioSource.loop = false;
            _effectAudioSource.volume = _jumpScareVolume;
            _effectAudioSource.Play();

            float requestedDuration = maximumDuration > 0f ? maximumDuration : _jumpScareDuration;
            float playbackDuration = requestedDuration > 0f
                ? Mathf.Min(requestedDuration, _jumpScareSound.length)
                : _jumpScareSound.length;
            _jumpScareStopCoroutine = StartCoroutine(StopJumpScareAfter(playbackDuration));
        }

        private IEnumerator StopJumpScareAfter(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            _effectAudioSource.Stop();
            _effectAudioSource.resource = null;
            _effectAudioSource.volume = _soundEffectVolume;
            _jumpScareStopCoroutine = null;
        }

        private AudioSource CreateAudioSource(bool loop)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
