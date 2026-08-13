using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class UIGlitchEffect : MonoBehaviour
    {
        private const string ShaderResourcePath = "Shaders/UIGlitch";

        private static readonly int GlitchAmountId = Shader.PropertyToID("_GlitchAmount");
        private static readonly int BlockCountId = Shader.PropertyToID("_BlockCount");
        private static readonly int GlitchSpeedId = Shader.PropertyToID("_GlitchSpeed");
        private static readonly int ChromaticAberrationId = Shader.PropertyToID("_ChromaticAberration");

        [SerializeField, Min(1f)] private float _blockCount = 90f;
        [SerializeField, Min(0f)] private float _glitchSpeed = 24f;
        [SerializeField, Range(0f, 0.05f)] private float _chromaticAberration = 0.012f;

        private Graphic _target;
        private Material _originalMaterial;
        private Material _runtimeMaterial;
        private Coroutine _playCoroutine;
        private bool _initializationFailed;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void Play(float duration, float intensity)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
            }

            _playCoroutine = StartCoroutine(PlayRoutine(duration, intensity));
        }

        public void Stop()
        {
            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
                _playCoroutine = null;
            }

            SetEffectActive(false, 0f);
        }

        private IEnumerator PlayRoutine(float duration, float intensity)
        {
            SetEffectActive(true, intensity);
            yield return new WaitForSeconds(Mathf.Max(0f, duration));
            SetEffectActive(false, 0f);
            _playCoroutine = null;
        }

        private bool EnsureInitialized()
        {
            if (_runtimeMaterial != null)
            {
                return true;
            }

            if (_initializationFailed)
            {
                return false;
            }

            _target = GetComponent<Graphic>();
            Shader glitchShader = Resources.Load<Shader>(ShaderResourcePath);
            if (glitchShader == null)
            {
                _initializationFailed = true;
                Debug.LogError(
                    $"Could not load the UI glitch shader at Resources/{ShaderResourcePath}.",
                    this);
                return false;
            }

            _originalMaterial = _target.material;
            _runtimeMaterial = new Material(glitchShader)
            {
                name = $"{name} UI Glitch (Runtime)",
                hideFlags = HideFlags.HideAndDontSave
            };

            _runtimeMaterial.SetFloat(BlockCountId, _blockCount);
            _runtimeMaterial.SetFloat(GlitchSpeedId, _glitchSpeed);
            _runtimeMaterial.SetFloat(ChromaticAberrationId, _chromaticAberration);
            _runtimeMaterial.SetFloat(GlitchAmountId, 0f);
            return true;
        }

        private void SetEffectActive(bool active, float intensity)
        {
            if (_target == null || _runtimeMaterial == null)
            {
                return;
            }

            _runtimeMaterial.SetFloat(GlitchAmountId, Mathf.Clamp01(intensity));
            _target.material = active ? _runtimeMaterial : _originalMaterial;
            _target.SetMaterialDirty();
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial != null)
            {
                Destroy(_runtimeMaterial);
            }
        }
    }
}
