// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;

namespace FiveSQD.WebVerse.VR.Comfort
{
    /// <summary>
    /// World transition fade controller that renders a solid black overlay.
    /// Camera-attached screen-space quad with custom unlit shader.
    /// Supports FadeOut (with callback) and FadeIn for smooth world transitions.
    /// Renders above the vignette quad (higher sort order).
    /// </summary>
    public class FadeController : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private float _fadeInDuration = 0.5f;

        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Material _material;
        private int _fadeAlphaId;
        private float _currentAlpha;
        private float _targetAlpha;
        private float _fadeSpeed;
        private Action _onComplete;
        private bool _isFading;

        /// <summary>
        /// Whether a fade animation is currently in progress.
        /// </summary>
        public bool IsFading => _isFading;

        /// <summary>
        /// Current fade alpha (0 = transparent, 1 = fully opaque black).
        /// </summary>
        public float CurrentAlpha => _currentAlpha;

        /// <summary>
        /// Whether the fade mesh is currently being rendered.
        /// </summary>
        public bool IsRendering => _meshRenderer != null && _meshRenderer.enabled;

        /// <summary>
        /// Attach the fade quad as a child of the given camera's transform.
        /// </summary>
        public void SetCamera(Camera camera)
        {
            if (camera == null) return;
            transform.SetParent(camera.transform, false);
            float distance = camera.nearClipPlane + 0.01f;
            transform.localPosition = new Vector3(0f, 0f, distance);
            transform.localRotation = Quaternion.identity;

            float halfHeight = distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * camera.aspect;
            transform.localScale = new Vector3(halfWidth, halfHeight, 1f);
        }

        /// <summary>
        /// Fade to fully opaque black. Invokes onComplete when fade finishes.
        /// </summary>
        public void FadeOut(Action onComplete)
        {
            _onComplete = onComplete;
            _targetAlpha = 1f;
            _fadeSpeed = _fadeOutDuration > 0f ? 1f / _fadeOutDuration : float.MaxValue;
            _isFading = true;
            if (_meshRenderer != null)
                _meshRenderer.enabled = true;
        }

        /// <summary>
        /// Fade from opaque black to fully transparent. Disables renderer on completion.
        /// </summary>
        public void FadeIn()
        {
            FadeIn(null);
        }

        /// <summary>
        /// Fade from opaque black to fully transparent with completion callback.
        /// </summary>
        public void FadeIn(Action onComplete)
        {
            _onComplete = onComplete;
            _targetAlpha = 0f;
            _fadeSpeed = _fadeInDuration > 0f ? 1f / _fadeInDuration : float.MaxValue;
            _isFading = true;
        }

        private void Awake()
        {
            _fadeAlphaId = Shader.PropertyToID("_FadeAlpha");
            CreateQuadMesh();
            CreateMaterial();
            if (_meshRenderer != null)
                _meshRenderer.enabled = false;
        }

        private void OnDisable()
        {
            _currentAlpha = 0f;
            _isFading = false;
            _onComplete = null;
            if (_material != null)
                _material.SetFloat(_fadeAlphaId, 0f);
            if (_meshRenderer != null)
                _meshRenderer.enabled = false;
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }
        }

        private void Update()
        {
            if (!_isFading) return;

            _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, _fadeSpeed * Time.deltaTime);

            if (_material != null)
            {
                _material.SetFloat(_fadeAlphaId, _currentAlpha);
            }

            if (Mathf.Approximately(_currentAlpha, _targetAlpha))
            {
                _isFading = false;
                if (_targetAlpha <= 0f && _meshRenderer != null)
                    _meshRenderer.enabled = false;

                var callback = _onComplete;
                _onComplete = null;
                callback?.Invoke();
            }
        }

        private void CreateQuadMesh()
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();

            var mesh = new Mesh { name = "FadeQuad" };

            mesh.vertices = new Vector3[]
            {
                new Vector3(-1f, -1f, 0f),
                new Vector3( 1f, -1f, 0f),
                new Vector3( 1f,  1f, 0f),
                new Vector3(-1f,  1f, 0f)
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };

            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();

            _meshFilter.mesh = mesh;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }

        private void CreateMaterial()
        {
            var shader = Shader.Find("FiveSQD/ComfortFade");
            if (shader == null)
            {
                Debug.LogWarning("[VRInterface] ComfortFade shader not found. Fade disabled.");
                enabled = false;
                return;
            }

            _material = new Material(shader);
            _material.SetFloat(_fadeAlphaId, 0f);
            _meshRenderer.material = _material;
        }
    }
}
