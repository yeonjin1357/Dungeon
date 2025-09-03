using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Dungeon.Managers;

namespace Dungeon.Combat
{
    public class TurnManager : MonoBehaviour
    {
        #region Singleton
        private static TurnManager _instance;
        public static TurnManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TurnManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TurnManager");
                        _instance = go.AddComponent<TurnManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion
        
        #region Fields
        [Header("Turn Settings")]
        [SerializeField] private float _turnDelay = 0.2f;
        [SerializeField] private bool _isProcessingTurn = false;
        
        private List<ITurnBased> _turnParticipants = new List<ITurnBased>();
        private Queue<ITurnBased> _turnQueue = new Queue<ITurnBased>();
        private ITurnBased _currentTurnEntity;
        
        public delegate void TurnEventDelegate(ITurnBased entity);
        public event TurnEventDelegate OnTurnStart;
        public event TurnEventDelegate OnTurnEnd;
        
        public delegate void RoundEventDelegate(int roundNumber);
        public event RoundEventDelegate OnRoundStart;
        public event RoundEventDelegate OnRoundEnd;
        
        private int _currentRound = 0;
        #endregion
        
        #region Properties
        public bool IsProcessingTurn => _isProcessingTurn;
        public ITurnBased CurrentTurnEntity => _currentTurnEntity;
        public int CurrentRound => _currentRound;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeTurnSystem();
        }
        #endregion
        
        #region Public Methods
        public void RegisterEntity(ITurnBased entity)
        {
            if (!_turnParticipants.Contains(entity))
            {
                _turnParticipants.Add(entity);
                Debug.Log($"Entity registered to turn system: {entity}");
            }
        }
        
        public void UnregisterEntity(ITurnBased entity)
        {
            if (_turnParticipants.Contains(entity))
            {
                _turnParticipants.Remove(entity);
                Debug.Log($"Entity unregistered from turn system: {entity}");
            }
        }
        
        public void StartNewRound()
        {
            _currentRound++;
            Debug.Log($"Starting Round {_currentRound}");
            
            OnRoundStart?.Invoke(_currentRound);
            
            // Sort entities by priority and rebuild queue
            var sortedEntities = _turnParticipants
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.TurnPriority)
                .ToList();
            
            _turnQueue.Clear();
            
            // Check if there are any active entities
            if (sortedEntities.Count == 0)
            {
                Debug.LogWarning("No active entities found. Stopping turn processing.");
                _isProcessingTurn = false;
                return;
            }
            
            foreach (var entity in sortedEntities)
            {
                _turnQueue.Enqueue(entity);
            }
            
            ProcessNextTurn();
        }
        
        public void ProcessNextTurn()
        {
            if (_isProcessingTurn)
                return;
            
            if (_turnQueue.Count == 0)
            {
                // Check if we have any active participants before starting new round
                bool hasActiveParticipants = _turnParticipants.Any(e => e.IsActive);
                
                if (!hasActiveParticipants)
                {
                    Debug.LogWarning("No active participants. Stopping turn processing.");
                    _isProcessingTurn = false;
                    return;
                }
                
                OnRoundEnd?.Invoke(_currentRound);
                StartNewRound();
                return;
            }
            
            _isProcessingTurn = true;
            _currentTurnEntity = _turnQueue.Dequeue();
            
            if (!_currentTurnEntity.IsActive)
            {
                _isProcessingTurn = false;
                ProcessNextTurn();
                return;
            }
            
            StartCoroutine(ExecuteTurnCoroutine());
        }
        
        public void EndCurrentTurn()
        {
            if (_currentTurnEntity != null)
            {
                _currentTurnEntity.OnTurnEnd();
                OnTurnEnd?.Invoke(_currentTurnEntity);
            }
            
            _isProcessingTurn = false;
            
            // Add delay before next turn
            Invoke(nameof(ProcessNextTurn), _turnDelay);
        }
        
        public void ForceEndTurn()
        {
            if (_isProcessingTurn)
            {
                StopAllCoroutines();
                EndCurrentTurn();
            }
        }
        
        public List<ITurnBased> GetActiveEntities()
        {
            return _turnParticipants.Where(e => e.IsActive).ToList();
        }
        
        public void ClearAllEntities()
        {
            _turnParticipants.Clear();
            _turnQueue.Clear();
            _currentTurnEntity = null;
            _isProcessingTurn = false;
        }
        #endregion
        
        #region Private Methods
        private void InitializeTurnSystem()
        {
            _turnParticipants.Clear();
            _turnQueue.Clear();
            _currentRound = 0;
            _isProcessingTurn = false;
        }
        
        private System.Collections.IEnumerator ExecuteTurnCoroutine()
        {
            // Notify turn start
            _currentTurnEntity.OnTurnStart();
            OnTurnStart?.Invoke(_currentTurnEntity);
            
            yield return new WaitForSeconds(0.1f);
            
            // Execute the entity's turn
            _currentTurnEntity.ExecuteTurn();
            
            // Turn will end when entity calls EndCurrentTurn()
            // or automatically after timeout
            yield return new WaitForSeconds(2f);
            
            // Force end turn if still processing
            if (_isProcessingTurn)
            {
                EndCurrentTurn();
            }
        }
        #endregion
    }
}