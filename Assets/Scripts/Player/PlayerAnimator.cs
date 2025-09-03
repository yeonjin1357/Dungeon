using UnityEngine;
using System.Collections;

namespace Dungeon.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerAnimator : MonoBehaviour
    {
        #region Fields
        [Header("Animation Settings")]
        [SerializeField] private float _walkAnimationSpeed = 0.3f;
        [SerializeField] private float _attackAnimationSpeed = 0.2f;
        [SerializeField] private float _hurtAnimationSpeed = 0.1f;
        
        [Header("Sprite Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _movingColor = new Color(0.8f, 0.8f, 1f);
        [SerializeField] private Color _attackingColor = new Color(1f, 0.8f, 0.8f);
        [SerializeField] private Color _hurtColor = new Color(1f, 0.5f, 0.5f);
        
        private SpriteRenderer _spriteRenderer;
        private Coroutine _currentAnimation;
        private Vector3 _originalScale;
        
        public enum AnimationState
        {
            Idle,
            Moving,
            Attacking,
            Hurt,
            Death
        }
        
        private AnimationState _currentState = AnimationState.Idle;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalScale = transform.localScale;
        }
        #endregion
        
        #region Public Methods
        public void PlayMoveAnimation(Vector2 direction)
        {
            StopCurrentAnimation();
            _currentState = AnimationState.Moving;
            _currentAnimation = StartCoroutine(MoveAnimationCoroutine(direction));
        }
        
        public void PlayAttackAnimation(Vector2 direction)
        {
            StopCurrentAnimation();
            _currentState = AnimationState.Attacking;
            _currentAnimation = StartCoroutine(AttackAnimationCoroutine(direction));
        }
        
        public void PlayHurtAnimation()
        {
            StopCurrentAnimation();
            _currentState = AnimationState.Hurt;
            _currentAnimation = StartCoroutine(HurtAnimationCoroutine());
        }
        
        public void PlayDeathAnimation()
        {
            StopCurrentAnimation();
            _currentState = AnimationState.Death;
            _currentAnimation = StartCoroutine(DeathAnimationCoroutine());
        }
        
        public void PlayIdleAnimation()
        {
            StopCurrentAnimation();
            _currentState = AnimationState.Idle;
            _currentAnimation = StartCoroutine(IdleAnimationCoroutine());
        }
        
        public void SetFacingDirection(Vector2 direction)
        {
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
            }
            else if (direction.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
            }
        }
        #endregion
        
        #region Private Methods
        private void StopCurrentAnimation()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }
            
            ResetAnimationState();
        }
        
        private void ResetAnimationState()
        {
            _spriteRenderer.color = _normalColor;
            transform.localScale = _originalScale;
            transform.rotation = Quaternion.identity;
        }
        
        private IEnumerator IdleAnimationCoroutine()
        {
            _spriteRenderer.color = _normalColor;
            
            float time = 0;
            while (_currentState == AnimationState.Idle)
            {
                time += Time.deltaTime;
                float scale = 1f + Mathf.Sin(time * 2f) * 0.02f;
                transform.localScale = new Vector3(_originalScale.x * scale, _originalScale.y * scale, _originalScale.z);
                yield return null;
            }
        }
        
        private IEnumerator MoveAnimationCoroutine(Vector2 direction)
        {
            SetFacingDirection(direction);
            _spriteRenderer.color = _movingColor;
            
            float animationTime = _walkAnimationSpeed;
            float elapsedTime = 0;
            
            Vector3 startScale = transform.localScale;
            
            while (elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationTime;
                
                float bounce = Mathf.Sin(t * Mathf.PI);
                float squash = 1f - bounce * 0.1f;
                float stretch = 1f + bounce * 0.1f;
                
                transform.localScale = new Vector3(
                    startScale.x * squash,
                    startScale.y * stretch,
                    startScale.z
                );
                
                yield return null;
            }
            
            transform.localScale = startScale;
            _spriteRenderer.color = _normalColor;
            
            PlayIdleAnimation();
        }
        
        private IEnumerator AttackAnimationCoroutine(Vector2 direction)
        {
            SetFacingDirection(direction);
            _spriteRenderer.color = _attackingColor;
            
            float animationTime = _attackAnimationSpeed;
            float elapsedTime = 0;
            
            Vector3 originalPosition = transform.localPosition;
            Vector3 attackOffset = new Vector3(direction.x, direction.y, 0) * 0.3f;
            
            while (elapsedTime < animationTime * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (animationTime * 0.5f);
                
                transform.localPosition = Vector3.Lerp(originalPosition, originalPosition + attackOffset, t);
                
                float scale = 1f + t * 0.2f;
                transform.localScale = new Vector3(
                    _originalScale.x * scale,
                    _originalScale.y * scale,
                    _originalScale.z
                );
                
                yield return null;
            }
            
            elapsedTime = 0;
            while (elapsedTime < animationTime * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (animationTime * 0.5f);
                
                transform.localPosition = Vector3.Lerp(originalPosition + attackOffset, originalPosition, t);
                
                float scale = 1.2f - t * 0.2f;
                transform.localScale = new Vector3(
                    _originalScale.x * scale,
                    _originalScale.y * scale,
                    _originalScale.z
                );
                
                yield return null;
            }
            
            transform.localPosition = originalPosition;
            transform.localScale = _originalScale;
            _spriteRenderer.color = _normalColor;
            
            PlayIdleAnimation();
        }
        
        private IEnumerator HurtAnimationCoroutine()
        {
            _spriteRenderer.color = _hurtColor;
            
            float animationTime = _hurtAnimationSpeed;
            float elapsedTime = 0;
            
            Vector3 originalPosition = transform.localPosition;
            
            int shakeCount = 0;
            while (elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                
                if (shakeCount % 2 == 0)
                {
                    transform.localPosition = originalPosition + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
                }
                else
                {
                    transform.localPosition = originalPosition;
                }
                
                shakeCount++;
                
                _spriteRenderer.color = Color.Lerp(_hurtColor, _normalColor, elapsedTime / animationTime);
                
                yield return new WaitForSeconds(0.02f);
            }
            
            transform.localPosition = originalPosition;
            _spriteRenderer.color = _normalColor;
            
            PlayIdleAnimation();
        }
        
        private IEnumerator DeathAnimationCoroutine()
        {
            _spriteRenderer.color = _hurtColor;
            
            float animationTime = 1f;
            float elapsedTime = 0;
            
            while (elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationTime;
                
                float rotation = t * 90f;
                transform.rotation = Quaternion.Euler(0, 0, rotation);
                
                float scale = 1f - t * 0.5f;
                transform.localScale = new Vector3(
                    _originalScale.x * scale,
                    _originalScale.y * scale,
                    _originalScale.z
                );
                
                Color fadeColor = _hurtColor;
                fadeColor.a = 1f - t;
                _spriteRenderer.color = fadeColor;
                
                yield return null;
            }
        }
        #endregion
    }
}