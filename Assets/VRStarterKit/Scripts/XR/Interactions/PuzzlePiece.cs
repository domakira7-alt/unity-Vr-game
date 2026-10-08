using CommanTickManager;
using UnityEngine;
using NaughtyAttributes;
using Yudiz.VRAwarenessExperience.XR;
using Yudiz.VRAwarenessExperience.Data;
using Yudiz.VRAwarenessExperience.Manager;
using System.Collections.Generic;
using System;

namespace Yudiz.VRAwarenessExperience.Interactions
{
    public class PuzzlePiece : MonoBehaviour, ITick
    {
        [Header("Target Settings")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private Transform targetTransform;

        [Header("Snap Settings")]
        [HorizontalLine(color: EColor.Blue)]
        public Transform snapAttachTransform;

        [Header("Puzzle Piece Settings")]
        [SerializeField] private SnappableID puzzlePieceID;
        [SerializeField] private Collider puzzlePieceCollider;
        [SerializeField] private List<SnapZone> nearbySnapZones = new List<SnapZone>();
        
        private MovementData movementData;
        
        // Private variables
        private Vector3 velocity;
        private Vector3 acceleration;
        private Vector3 lastTargetPosition;
        private bool hasOvershot = false;
        private float delayTimer;
        private bool isMoving = false;
        private bool isAttached = false;
        
        public PuzzleMovementType MovementType => movementData.movementType;
        public bool IsMoving => isMoving;
        public bool IsAttached => isAttached;

        public delegate void OnSnapCompleteDelegate(bool isCorrect);
        public static event OnSnapCompleteDelegate OnSnapComplete;

        public SnappableID PuzzlePieceID => puzzlePieceID;

        private void Start()
        {
            isAttached = false;
            isMoving = false;
        }

        public void InitializeMovementData(MovementData movementData)
        {
            this.movementData = movementData;
        }

        private void OnTriggerEnter(Collider other)
        {
            SnapZone snapZone = other.GetComponent<SnapZone>();
            if (snapZone != null)
            {
                nearbySnapZones.Add(snapZone);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            SnapZone snapZone = other.GetComponent<SnapZone>();
            if (snapZone != null)
            {
                nearbySnapZones.Remove(snapZone);
            }
        }

        public void ValidateSnap(Action OnCorrectSnap, Action OnWrongSnap)
        {
            Debug.Log("PuzzlePiece -> ValidateSnap");
            if (isAttached) return;

            if (nearbySnapZones.Count <= 0)
            {
                OnWrongSnap?.Invoke(); 
                return;
            } 

            SnapZone correctSnapZone = nearbySnapZones.Find(snapZone => snapZone.ZoneID == puzzlePieceID);
            if (correctSnapZone != null)
            {
                bool isValid = correctSnapZone.ValidateSnap(this);
                if (isValid)
                {
                    Debug.Log("PuzzlePiece -> ValidateSnap -> Correct Snap");
                    StopMovement();
                    correctSnapZone.SnapItem(this);
                    SetAttached(true);
                    puzzlePieceCollider.enabled = false;
                    OnCorrectSnap?.Invoke();
                    OnSnapComplete?.Invoke(true);
                }
                else
                {
                    OnWrongSnap?.Invoke();
                }
            }
            else
            {
                Debug.Log("PuzzlePiece -> ValidateSnap -> Wrong Snap");
                OnWrongSnap?.Invoke();
                OnSnapComplete?.Invoke(false);
            }
        }
        
        public void Tick()
        {
            if (!isMoving) return;
            
            switch (movementData.movementType)
            {
                case PuzzleMovementType.Fast:
                    UpdateFastMovement();
                    break;
                case PuzzleMovementType.Slow:
                    UpdateSlowMovement();
                    break;
                case PuzzleMovementType.Normal:
                    UpdateNormalMovement();
                    break;

            }
        }
        
        public void StartMovement()
        {
            if (isMoving) return;
            switch (movementData.movementType)
            {
                case PuzzleMovementType.Fast:
                    SetupFastMovement();
                    break;
                case PuzzleMovementType.Slow:
                    SetupSlowMovement();
                    break;
                case PuzzleMovementType.Normal:
                    SetupNormalMovement();
                    break;
            }
            
            isMoving = true;
            ProcessingUpdate.Instance.Add(this);
        }
        
        public void StopMovement()
        {
            if (!isMoving) return;
            
            isMoving = false;
            ProcessingUpdate.Instance.Remove(this);
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
            velocity = Vector3.zero;
        }

        public void SetAttached(bool value)
        {
            isAttached = value;
        }
        
        private void SetupFastMovement()
        {
            // Initialize harmonic motion variables
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
            lastTargetPosition = targetTransform.position;
            hasOvershot = false;
        }
        
        private void UpdateFastMovement()
        {
            FastMovementData fastMovementData = movementData as FastMovementData;

            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = targetTransform.position;
            
            // Calculate displacement from target
            Vector3 displacement = currentPosition - targetPosition;
            float distance = displacement.magnitude;
            
            // Check if target has moved significantly
            bool targetMoved = Vector3.Distance(targetPosition, lastTargetPosition) > 0.01f;
            if (targetMoved)
            {
                // Reset overshoot when target moves
                hasOvershot = false;
                lastTargetPosition = targetPosition;
            }
            
            // Spring-mass system: F = -kx - cv
            // Where k = spring constant, c = damping coefficient, x = displacement, v = velocity
            
            // Spring force (restoring force towards target)
            Vector3 springForce = -fastMovementData.springConstant * displacement;
            
            // Damping force (opposes velocity)
            Vector3 dampingForce = -fastMovementData.dampingRatio * velocity;
            
            // Calculate acceleration
            acceleration = springForce + dampingForce;
            
            // Apply overshoot effect on first movement towards target
            if (!hasOvershot && distance > 0.1f)
            {
                // Add extra force to create overshoot
                Vector3 overshootDirection = (targetPosition - currentPosition).normalized;
                Vector3 overshootForce = overshootDirection * fastMovementData.springConstant * fastMovementData.overshootFactor;
                acceleration += overshootForce;
            }
            
            // Update velocity and position
            velocity += acceleration * Time.deltaTime;
            
            // Limit maximum velocity
            if (velocity.magnitude > fastMovementData.maxVelocity)
            {
                velocity = velocity.normalized * fastMovementData.maxVelocity;
            }
            
            // Apply movement
            transform.position += velocity * Time.deltaTime;
            
            // Check for overshoot (when we pass the target)
            Vector3 newDisplacement = transform.position - targetPosition;
            if (!hasOvershot && Vector3.Dot(displacement, newDisplacement) < 0)
            {
                hasOvershot = true;
            }
            
            // Smoothly rotate towards target
            transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, fastMovementData.springConstant * 0.1f * Time.deltaTime);
            
            // Check if close enough to target and velocity is low (settled)
            if (distance < 0.01f && velocity.magnitude < 0.1f)
            {
                transform.position = targetPosition;
                transform.rotation = targetTransform.rotation;
                velocity = Vector3.zero;
            }
        }
        
        private void SetupSlowMovement()
        {
            SlowMovementData slowMovementData = movementData as SlowMovementData;
            delayTimer = slowMovementData.delay;
        }

        private void SetupNormalMovement()
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
        
        private void UpdateSlowMovement()
        {
            SlowMovementData slowMovementData = movementData as SlowMovementData;
            // Apply delay
            if (delayTimer > 0)
            {
                delayTimer -= Time.deltaTime;
                return;
            }
            
            // Interpolate position and rotation
            transform.position = Vector3.Lerp(transform.position, targetTransform.position, slowMovementData.lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, slowMovementData.lerpSpeed * Time.deltaTime);
            
            // Check if close enough to target
            float distance = Vector3.Distance(transform.position, targetTransform.position);
            if (distance < 0.01f)
            {
                transform.position = targetTransform.position;
                transform.rotation = targetTransform.rotation;
            }
        }

        private void UpdateNormalMovement()
        {
            // Debug.Log($"UpdateNormalMovement -> target position : {targetTransform.position} Target rotation : {targetTransform.rotation}");
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
        
        public XRCustomGrabbable GetXRCustomGrabbable()
        {
            return targetTransform.GetComponent<XRCustomGrabbable>();
        }
    }
    
    public enum PuzzleMovementType
    {
        Fast, 
        Slow,   
        Normal,   
    }  
}
