using UnityEngine;

namespace StarterKit.StateMachine
{
    public enum CharacterState
    {
        Idle,
        Walking,
        Attacking,
        Defending
    }

    public class StateMachineExample : MonoBehaviour
    {
        private StateMachine<CharacterState> _stateMachine;

        private void Start()
        {
            // Create the state machine
            _stateMachine = new StateMachine<CharacterState>();

            // Add states with their respective actions
            _stateMachine.AddState(
                CharacterState.Idle,
                onEnter: () => Debug.Log("Entering Idle state"),
                onUpdate: () => Debug.Log("Idle state update"),
                onExit: () => Debug.Log("Exiting Idle state")
            );

            _stateMachine.AddState(
                CharacterState.Walking,
                onEnter: () => Debug.Log("Entering Walking state"),
                onUpdate: () => Debug.Log("Walking state update"),
                onExit: () => Debug.Log("Exiting Walking state")
            );

            _stateMachine.AddState(
                CharacterState.Attacking,
                onEnter: () => Debug.Log("Entering Attacking state"),
                onUpdate: () => Debug.Log("Attacking state update"),
                onExit: () => Debug.Log("Exiting Attacking state")
            );

            _stateMachine.AddState(
                CharacterState.Defending,
                onEnter: () => Debug.Log("Entering Defending state"),
                onUpdate: () => Debug.Log("Defending state update"),
                onExit: () => Debug.Log("Exiting Defending state")
            );

            // Initialize the state machine with the initial state
            _stateMachine.Initialize(CharacterState.Idle);
        }

        private void Update()
        {
            // Update the current state
            _stateMachine.Update();

            // Example state transitions based on input
            if (Input.GetKeyDown(KeyCode.W))
            {
                _stateMachine.ChangeState(CharacterState.Walking);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                _stateMachine.ChangeState(CharacterState.Attacking);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                _stateMachine.ChangeState(CharacterState.Defending);
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                _stateMachine.ChangeState(CharacterState.Idle);
            }
        }
    }
} 