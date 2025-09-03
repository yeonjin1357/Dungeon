using UnityEngine;
using System.Collections.Generic;

namespace Dungeon.Enemies
{
    /// <summary>
    /// 적 AI 상태 머신
    /// </summary>
    public class EnemyStateMachine
    {
        private IEnemyState _currentState;
        private Dictionary<EnemyState, IEnemyState> _states;
        private EnemyBase _enemy;

        public EnemyState CurrentStateType { get; private set; }

        public EnemyStateMachine(EnemyBase enemy)
        {
            _enemy = enemy;
            _states = new Dictionary<EnemyState, IEnemyState>();
        }

        /// <summary>
        /// 상태 등록
        /// </summary>
        public void RegisterState(EnemyState stateType, IEnemyState state)
        {
            if (!_states.ContainsKey(stateType))
            {
                _states.Add(stateType, state);
            }
        }

        /// <summary>
        /// 상태 변경
        /// </summary>
        public void ChangeState(EnemyState newStateType)
        {
            if (!_states.ContainsKey(newStateType))
            {
                Debug.LogWarning($"State {newStateType} not registered!");
                return;
            }

            // 이전 상태 종료
            _currentState?.Exit(_enemy);

            // 새 상태로 전환
            CurrentStateType = newStateType;
            _currentState = _states[newStateType];
            _currentState.Enter(_enemy);

            Debug.Log($"{_enemy.name} changed state to {newStateType}");
        }

        /// <summary>
        /// 현재 상태 업데이트
        /// </summary>
        public void UpdateState()
        {
            _currentState?.Execute(_enemy);
        }

        /// <summary>
        /// 초기 상태 설정
        /// </summary>
        public void Initialize(EnemyState initialState)
        {
            if (_states.ContainsKey(initialState))
            {
                CurrentStateType = initialState;
                _currentState = _states[initialState];
                _currentState.Enter(_enemy);
            }
        }
    }
}