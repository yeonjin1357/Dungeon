using System.Collections;
using UnityEngine;

namespace Dungeon.Camera
{
    public class CameraController : MonoBehaviour
    {
        #region Fields
        [Header("Follow Settings")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10f);
        [SerializeField] private bool _enableFollow = true;
        
        [Header("Camera Bounds")]
        [SerializeField] private bool _useBounds = true;
        [SerializeField] private Vector2 _minBounds = new Vector2(-50, -50);
        [SerializeField] private Vector2 _maxBounds = new Vector2(50, 50);
        
        [Header("Zoom Settings")]
        [SerializeField] private float _defaultZoom = 5f;
        [SerializeField] private float _minZoom = 3f;
        [SerializeField] private float _maxZoom = 10f;
        [SerializeField] private float _zoomSpeed = 2f;
        
        [Header("Shake Settings")]
        private float _shakeIntensity;
        private float _shakeDuration;
        private Vector3 _originalPosition;
        private Coroutine _shakeCoroutine;
        
        private UnityEngine.Camera _camera;
        private float _currentZoom;
        #endregion

        #region Properties
        public Transform Target
        {
            get => _target;
            set => _target = value;
        }
        
        public bool EnableFollow
        {
            get => _enableFollow;
            set => _enableFollow = value;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            if (_camera == null)
            {
                _camera = gameObject.AddComponent<UnityEngine.Camera>();
            }
            
            _currentZoom = _defaultZoom;
            _camera.orthographicSize = _currentZoom;
            
            // Auto-find player if no target set
            if (_target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _target = player.transform;
                }
            }
        }

        private void Start()
        {
            if (_target != null)
            {
                // Snap to target initially
                Vector3 targetPosition = _target.position + _offset;
                transform.position = ClampToBounds(targetPosition);
            }
        }

        private void LateUpdate()
        {
            if (_enableFollow && _target != null)
            {
                FollowTarget();
            }
            
            HandleZoomInput();
        }
        #endregion

        #region Camera Movement
        private void FollowTarget()
        {
            Vector3 desiredPosition = _target.position + _offset;
            desiredPosition = ClampToBounds(desiredPosition);
            
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
        
        private Vector3 ClampToBounds(Vector3 position)
        {
            if (!_useBounds) return position;
            
            // Calculate camera bounds based on orthographic size
            float verticalExtent = _camera.orthographicSize;
            float horizontalExtent = verticalExtent * _camera.aspect;
            
            // Clamp position considering camera view
            position.x = Mathf.Clamp(position.x, _minBounds.x + horizontalExtent, _maxBounds.x - horizontalExtent);
            position.y = Mathf.Clamp(position.y, _minBounds.y + verticalExtent, _maxBounds.y - verticalExtent);
            
            return position;
        }
        
        public void SetBounds(Vector2 min, Vector2 max)
        {
            _minBounds = min;
            _maxBounds = max;
        }
        
        public void UpdateBoundsFromTilemap()
        {
            GameObject gridObject = GameObject.Find("Grid");
            if (gridObject != null)
            {
                // Find all tilemaps and calculate combined bounds
                var tilemaps = gridObject.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>();
                if (tilemaps.Length > 0)
                {
                    Bounds combinedBounds = tilemaps[0].localBounds;
                    foreach (var tilemap in tilemaps)
                    {
                        tilemap.CompressBounds();
                        combinedBounds.Encapsulate(tilemap.localBounds);
                    }
                    
                    _minBounds = new Vector2(combinedBounds.min.x, combinedBounds.min.y);
                    _maxBounds = new Vector2(combinedBounds.max.x, combinedBounds.max.y);
                    
                    Debug.Log($"Camera bounds updated: Min {_minBounds}, Max {_maxBounds}");
                }
            }
        }
        #endregion

        #region Camera Zoom
        private void HandleZoomInput()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            
            if (scrollInput != 0)
            {
                SetZoom(_currentZoom - scrollInput * _zoomSpeed);
            }
            
            // Mobile pinch zoom support
            if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);
                
                Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                
                float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
                float currentMagnitude = (touch0.position - touch1.position).magnitude;
                
                float difference = currentMagnitude - prevMagnitude;
                SetZoom(_currentZoom - difference * 0.01f);
            }
        }
        
        public void SetZoom(float zoomLevel)
        {
            _currentZoom = Mathf.Clamp(zoomLevel, _minZoom, _maxZoom);
            _camera.orthographicSize = _currentZoom;
        }
        
        public void ZoomIn()
        {
            SetZoom(_currentZoom - 1f);
        }
        
        public void ZoomOut()
        {
            SetZoom(_currentZoom + 1f);
        }
        #endregion

        #region Camera Shake
        public void Shake(float intensity, float duration)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }
            
            _shakeIntensity = intensity;
            _shakeDuration = duration;
            _shakeCoroutine = StartCoroutine(ShakeCoroutine());
        }
        
        private IEnumerator ShakeCoroutine()
        {
            _originalPosition = transform.position;
            float elapsed = 0f;
            
            while (elapsed < _shakeDuration)
            {
                float percentComplete = elapsed / _shakeDuration;
                float damper = 1f - Mathf.Clamp01(percentComplete);
                
                float offsetX = Random.Range(-1f, 1f) * _shakeIntensity * damper;
                float offsetY = Random.Range(-1f, 1f) * _shakeIntensity * damper;
                
                transform.position = _originalPosition + new Vector3(offsetX, offsetY, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = _originalPosition;
            _shakeCoroutine = null;
        }
        
        public void ShakeOnDamage()
        {
            Shake(0.3f, 0.2f);
        }
        
        public void ShakeOnExplosion()
        {
            Shake(0.6f, 0.4f);
        }
        #endregion

        #region Public Methods
        public void SnapToTarget()
        {
            if (_target != null)
            {
                Vector3 targetPosition = _target.position + _offset;
                transform.position = ClampToBounds(targetPosition);
            }
        }
        
        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
            if (_target != null)
            {
                SnapToTarget();
            }
        }
        #endregion
    }
}