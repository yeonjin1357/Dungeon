using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Dungeon.UI
{
    public class DamageNumberUI : MonoBehaviour
    {
        #region Fields
        [Header("UI Components")]
        [SerializeField] private Text _damageText;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);
        
        private float _duration;
        private UnityEngine.Camera _mainCamera;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            if (_damageText == null)
                _damageText = GetComponentInChildren<Text>();
            
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
                
            _mainCamera = UnityEngine.Camera.main;
        }
        #endregion
        
        #region Public Methods
        public void Initialize(int damage, float duration, bool isCritical = false)
        {
            _duration = duration;
            
            // Set damage text
            _damageText.text = damage.ToString();
            
            // Set color based on critical
            if (isCritical)
            {
                _damageText.color = Color.yellow;
                _damageText.fontSize = 48;
            }
            else
            {
                _damageText.color = Color.white;
                _damageText.fontSize = 36;
            }
            
            // Start animation
            StartCoroutine(AnimateNumber());
        }
        
        public void SetWorldPosition(Vector3 worldPosition)
        {
            // Convert world position to screen position
            if (_mainCamera != null)
            {
                Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPosition);
                transform.position = screenPos;
            }
        }
        #endregion
        
        #region Private Methods
        private IEnumerator AnimateNumber()
        {
            float elapsedTime = 0f;
            Vector3 startPosition = transform.position;
            
            while (elapsedTime < _duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / _duration;
                
                // Float upward
                Vector3 newPosition = startPosition;
                newPosition.y += _floatSpeed * elapsedTime * 50f; // 50 pixels per second
                transform.position = newPosition;
                
                // Scale animation
                float scale = _scaleCurve.Evaluate(normalizedTime);
                transform.localScale = Vector3.one * scale;
                
                // Fade out
                if (normalizedTime > 0.5f) // Start fading after half duration
                {
                    float fadeProgress = (normalizedTime - 0.5f) * 2f; // 0 to 1 in second half
                    _canvasGroup.alpha = 1f - fadeProgress;
                }
                
                yield return null;
            }
            
            // Destroy when animation is complete
            Destroy(gameObject);
        }
        #endregion
    }
}