using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarterKit.StateMachine
{
    /// <summary>
    /// Represents a state in the state machine with its enter, update, and exit actions
    /// </summary>
    public class State<T> where T : Enum
    {
        public T StateType { get; private set; }
        public Action OnEnter { get; private set; }
        public Action OnUpdate { get; private set; }
        public Action OnExit { get; private set; }

        public State(T stateType, Action onEnter = null, Action onUpdate = null, Action onExit = null)
        {
            StateType = stateType;
            OnEnter = onEnter;
            OnUpdate = onUpdate;
            OnExit = onExit;
        }
    }

    /// <summary>
    /// A generic state machine that can work with any enum type
    /// </summary>
    public class StateMachine<T> where T : Enum
    {
        private Dictionary<T, State<T>> _states;
        private State<T> _currentState;
        private bool _isInitialized;

        public T CurrentStateType => _currentState != null ? _currentState.StateType : default;
        public bool IsInitialized => _isInitialized;

        public StateMachine()
        {
            _states = new Dictionary<T, State<T>>();
        }

        /// <summary>
        /// Adds a new state to the state machine
        /// </summary>
        public void AddState(T stateType, Action onEnter = null, Action onUpdate = null, Action onExit = null)
        {
            if (_states.ContainsKey(stateType))
            {
                Debug.LogWarning($"State {stateType} already exists in the state machine");
                return;
            }

            _states[stateType] = new State<T>(stateType, onEnter, onUpdate, onExit);
        }

        /// <summary>
        /// Initializes the state machine with a starting state
        /// </summary>
        public void Initialize(T initialState)
        {
            if (!_states.ContainsKey(initialState))
            {
                Debug.LogError($"Cannot initialize state machine: State {initialState} does not exist");
                return;
            }

            _currentState = _states[initialState];
            _currentState.OnEnter?.Invoke();
            _isInitialized = true;
        }

        /// <summary>
        /// Changes the current state to the specified state
        /// </summary>
        public void ChangeState(T newState)
        {
            if (!_isInitialized)
            {
                Debug.LogError("State machine is not initialized");
                return;
            }

            if (!_states.ContainsKey(newState))
            {
                Debug.LogError($"Cannot change state: State {newState} does not exist");
                return;
            }

            if (_currentState.StateType.Equals(newState))
            {
                return;
            }

            _currentState.OnExit?.Invoke();
            _currentState = _states[newState];
            _currentState.OnEnter?.Invoke();
        }

        /// <summary>
        /// Updates the current state
        /// </summary>
        public void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            _currentState.OnUpdate?.Invoke();
        }

        /// <summary>
        /// Clears all states from the state machine
        /// </summary>
        public void Clear()
        {
            _states.Clear();
            _currentState = null;
            _isInitialized = false;
        }
    }
} 