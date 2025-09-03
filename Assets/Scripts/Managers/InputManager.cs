using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Dungeon.Managers
{
    public class InputManager : MonoBehaviour
    {
        #region Singleton
        private static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InputManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InputManager");
                        _instance = go.AddComponent<InputManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        public delegate void OnTouchDelegate(Vector2 position);
        public event OnTouchDelegate OnTouchStart;
        public event OnTouchDelegate OnTouchEnd;
        public event OnTouchDelegate OnTouchHold;
        
        public delegate void OnSwipeDelegate(Vector2 direction);
        public event OnSwipeDelegate OnSwipe;
        #endregion

        #region Fields
        [Header("Touch Settings")]
        [SerializeField] private float _swipeThreshold = 50f;
        [SerializeField] private float _holdDuration = 0.5f;
        
        private Vector2 _touchStartPosition;
        private float _touchStartTime;
        private bool _isTouching = false;
        private bool _isHolding = false;
        
        private const float TAP_TIME_THRESHOLD = 0.2f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeInput();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            ProcessTouchInput();
        }
        #endregion

        #region Public Methods
        public Vector2 GetTouchPosition()
        {
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
            {
                return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
            }
            
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
            
            return Vector2.zero;
        }

        public bool IsTouching()
        {
            return _isTouching;
        }

        public Vector2 WorldToScreenPosition(Vector3 worldPosition)
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                return mainCamera.WorldToScreenPoint(worldPosition);
            }
            return Vector2.zero;
        }

        public Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
                worldPos.z = 0;
                return worldPos;
            }
            return Vector3.zero;
        }
        #endregion

        #region Private Methods
        private void InitializeInput()
        {
            EnhancedTouchSupport.Enable();
        }

        private void ProcessTouchInput()
        {
            bool currentlyTouching = false;
            Vector2 currentPosition = Vector2.zero;

            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
            {
                currentlyTouching = true;
                currentPosition = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
            }
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                currentlyTouching = true;
                currentPosition = Mouse.current.position.ReadValue();
            }

            if (currentlyTouching && !_isTouching)
            {
                HandleTouchStart(currentPosition);
            }
            else if (!currentlyTouching && _isTouching)
            {
                HandleTouchEnd(currentPosition);
            }
            else if (currentlyTouching && _isTouching)
            {
                HandleTouchHold(currentPosition);
            }

            _isTouching = currentlyTouching;
        }

        private void HandleTouchStart(Vector2 position)
        {
            _touchStartPosition = position;
            _touchStartTime = Time.time;
            _isHolding = false;
            
            OnTouchStart?.Invoke(position);
        }

        private void HandleTouchEnd(Vector2 position)
        {
            float touchDuration = Time.time - _touchStartTime;
            Vector2 swipeVector = position - _touchStartPosition;
            
            if (swipeVector.magnitude > _swipeThreshold)
            {
                Vector2 swipeDirection = swipeVector.normalized;
                OnSwipe?.Invoke(swipeDirection);
            }
            else if (touchDuration < TAP_TIME_THRESHOLD)
            {
                OnTouchEnd?.Invoke(position);
            }
            
            _isHolding = false;
        }

        private void HandleTouchHold(Vector2 position)
        {
            float touchDuration = Time.time - _touchStartTime;
            
            if (!_isHolding && touchDuration >= _holdDuration)
            {
                _isHolding = true;
                OnTouchHold?.Invoke(position);
            }
        }
        #endregion
    }
}
