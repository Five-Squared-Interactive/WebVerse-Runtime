// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.VR.Comfort
{
    /// <summary>
    /// Locomotion comfort vignette that darkens peripheral vision during movement.
    /// Camera-attached screen-space quad with custom unlit shader.
    /// Intensity is proportional to velocity, with configurable threshold and gradual release.
    /// </summary>
    public class VignetteController : MonoBehaviour
    {
        [Header("Vignette Settings")]
        [SerializeField] private float _velocityThreshold = 0.1f;
        [SerializeField] private float _maxIntensity = 0.6f;
        [SerializeField] private float _releaseTime = 0.25f;
        [SerializeField] private float _innerRadius = 0.5f;
        [SerializeField] private float _outerRadius = 1.0f;

        private VelocityTracker _velocityTracker;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Material _material;
        private int _vignetteIntensityId;
        private int _innerRadiusId;
        private int _outerRadiusId;
        private float _currentIntensity;

        /// <summary>
        /// Assign the velocity source for this vignette.
        /// </summary>
        public void SetVelocityTracker(VelocityTracker tracker)
        {
            _velocityTracker = tracker;
        }

        /// <summary>
        /// Attach the vignette quad as a child of the given camera's transform.
        /// </summary>
        public void SetCamera(Camera camera)
        {
            if (camera == null) return;
            transform.SetParent(camera.transform, false);
            float distance = camera.nearClipPlane + 0.01f;
            transform.localPosition = new Vector3(0f, 0f, distance);
            transform.localRotation = Quaternion.identity;

            // Scale quad to fill the camera's viewport at the near clip distance.
            // Quad vertices span -1 to +1, so localScale maps directly to half-extents.
            float halfHeight = distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * camera.aspect;
            transform.localScale = new Vector3(halfWidth, halfHeight, 1f);
        }

        /// <summary>
        /// Current vignette intensity (0 = off, up to maxIntensity).
        /// </summary>
        public float CurrentIntensity => _currentIntensity;

        /// <summary>
        /// Whether the vignette mesh is currently being rendered.
        /// </summary>
        public bool IsRendering => _meshRenderer != null && _meshRenderer.enabled;

        private void Awake()
        {
            _vignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
            _innerRadiusId = Shader.PropertyToID("_InnerRadius");
            _outerRadiusId = Shader.PropertyToID("_OuterRadius");
            CreateQuadMesh();
            CreateMaterial();
            _meshRenderer.enabled = false;
        }

        private void OnDisable()
        {
            _currentIntensity = 0f;
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

        private void LateUpdate()
        {
            if (_velocityTracker == null) return;

            float velocity = _velocityTracker.GetVelocity();

            if (velocity > _velocityThreshold)
            {
                // Activate — proportional intensity, clamped to _maxIntensity
                float t = Mathf.InverseLerp(_velocityThreshold, _velocityThreshold * 10f, velocity);
                _currentIntensity = Mathf.Lerp(0f, _maxIntensity, t);
                _meshRenderer.enabled = true;
            }
            else if (_currentIntensity > 0f)
            {
                // Release — lerp toward 0 over _releaseTime
                float releaseRate = _releaseTime > 0f
                    ? _maxIntensity / _releaseTime * Time.deltaTime
                    : _maxIntensity;
                _currentIntensity = Mathf.MoveTowards(_currentIntensity, 0f, releaseRate);
                if (_currentIntensity <= 0f)
                {
                    _currentIntensity = 0f;
                    _meshRenderer.enabled = false;
                }
            }

            if (_material != null)
            {
                _material.SetFloat(_vignetteIntensityId, _currentIntensity);
            }
        }

        private void CreateQuadMesh()
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();

            var mesh = new Mesh { name = "VignetteQuad" };

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
            var shader = Shader.Find("FiveSQD/ComfortVignette");
            if (shader == null)
            {
                Debug.LogWarning("[VRInterface] ComfortVignette shader not found. Vignette disabled.");
                enabled = false;
                return;
            }

            _material = new Material(shader);
            _material.SetFloat(_vignetteIntensityId, 0f);
            _material.SetFloat(_innerRadiusId, _innerRadius);
            _material.SetFloat(_outerRadiusId, _outerRadius);
            _meshRenderer.material = _material;
        }
    }
}
