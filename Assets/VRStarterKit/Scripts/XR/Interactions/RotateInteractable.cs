using CommanTickManager;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yudiz.XRStarter.Interactions;

public class RotateInteractable : MonoBehaviour, ITick
{

    [Header("Rotation Settings")]
    public RotationAxis rotationAxis;
    public RotationAxis interactorReferenceDirection;
    public RotationDirection rotationDirection;
    public RotationDirection reverseRotationDirection;
    public float rotationStep;
    public float rotationClamp;
    public float handRotationFrequency = 2f;

    public SnapAttach snapAttach;
    
    [Foldout("Events")] public UnityEvent<RotateInteractable> onObjectRotated;
    [Foldout("Events")] public UnityEvent<RotateInteractable, float> onObjectFullRotated;
    [Foldout("Events")] public UnityEvent<RotateInteractable> OnObjectRotationMaxReached;
    [Foldout("Events")] public UnityEvent<RotateInteractable> OnObjectRotationMinReached;
    [Foldout("Events")] public UnityEvent<RotateInteractable> onObjectClockwiseRotated;
    [Foldout("Events")] public UnityEvent<RotateInteractable> onObjectAntiClockwiseRotated;

    private Transform interactingController;
    private float overallRotation = 0;
    private Vector3 controllerPreviousUp;
    private Vector3 localRotationAxis;
    private Coroutine rotationChangeCheckCR;

    private Transform rotationAnchor;

    private Vector3 initialPos;
    private Quaternion initialRotation;


    BoxCollider colider;
    #region UNITY_CALLBACKS
    private void Awake()
    {
        initialPos = transform.position;
        initialRotation = transform.rotation;
    }

    private void OnEnable()
    {
        SnapAttach.OnSnapAttached += OnSnapAttached;
        SnapAttach.OnSnapDetached += OnSnapDetached;
    }
    private void OnDisable()
    {
        SnapAttach.OnSnapAttached -= OnSnapAttached;
        SnapAttach.OnSnapDetached -= OnSnapDetached;
    }

    private void Start()
    {
        overallRotation = 0;
        if (rotationAnchor == null)
        {
            rotationAnchor = transform;
        }
    }
    #endregion

    #region PUBLIC_METHODS
    public void Tick()
    {
        //Rotate
        localRotationAxis = GetAxisVector(rotationAxis, rotationAnchor);
        Vector3 interactionDirection = GetAxisVector(interactorReferenceDirection, interactingController);
        float signedAngle = Vector3.SignedAngle(controllerPreviousUp, interactionDirection, localRotationAxis) * handRotationFrequency;
        //Debug.Log($"Controller Rotation {controllerPreviousUp} New Direction{interactionDirection}");
        float newRawRotation;
        float rotationChange = 0;
        newRawRotation = overallRotation + signedAngle;

        bool isMaxLimitReached = false;
        bool isMinLimitReached = false;

        switch (rotationDirection)
        {
            case RotationDirection.Clockwise:
                newRawRotation = Mathf.Clamp(newRawRotation, 0, rotationClamp);
                rotationChange = newRawRotation - overallRotation;
                if(newRawRotation == 0 )
                {
                    isMinLimitReached = true;
                }
                else if(newRawRotation == rotationClamp)
                {
                    isMaxLimitReached = true;
                }
                break;

            case RotationDirection.CounterClockwise:
                newRawRotation = Mathf.Clamp(newRawRotation, -rotationClamp, 0);
                rotationChange = newRawRotation - overallRotation;
                if (newRawRotation == -rotationClamp)
                {
                    isMinLimitReached = true;
                }
                else if (newRawRotation == 0)
                {
                    isMaxLimitReached = true;
                }
                break;

            case RotationDirection.Both:
                newRawRotation = Mathf.Clamp(newRawRotation, -rotationClamp, rotationClamp);
                rotationChange = newRawRotation - overallRotation;
                if (newRawRotation == -rotationClamp)
                {
                    isMinLimitReached = true;
                }
                else if (newRawRotation == rotationClamp)
                {
                    isMaxLimitReached = true;
                }
                break;
        }

        if (Mathf.Abs(rotationChange) > rotationStep)
        {
            rotationAnchor.Rotate(localRotationAxis, rotationChange);
            overallRotation = newRawRotation;
            OnObjectRotated(rotationChange);
            controllerPreviousUp = interactionDirection;

            if(isMaxLimitReached)
            {
                OnObjectRotationMaxReached?.Invoke(this);
            }
            if(isMinLimitReached)
            {
                OnObjectRotationMinReached?.Invoke(this);
            }
        }
    }

    public virtual void OnObjectRotated(float rotationChange)
    {
        if (rotationChange > 0)
        {
            onObjectClockwiseRotated?.Invoke(this);
        }
        else if (rotationChange < 0)
        {
            onObjectAntiClockwiseRotated?.Invoke(this);
        }
        onObjectRotated?.Invoke(this);
        onObjectFullRotated?.Invoke(this, MathF.Abs(overallRotation) / rotationClamp);
    }
    #endregion

    #region PRIVATE_METHODS
    private Vector3 GetAxisVector(RotationAxis axis, Transform reference)
    {
        Vector3 localAxis;
        switch (axis)
        {
            case RotationAxis.Up:
                localAxis = reference.up;
                break;
            case RotationAxis.Down:
                localAxis = -reference.up;
                break;
            case RotationAxis.Left:
                localAxis = -reference.right;
                break;
            case RotationAxis.Right:
                localAxis = reference.right;
                break;
            case RotationAxis.Forward:
                localAxis = reference.forward;
                break;
            case RotationAxis.Backward:
                localAxis = -reference.forward;
                break;
            default:
                localAxis = reference.up;
                break;
        }
        return localAxis;
    }
    private void OnSnapAttached(SnapAttach snapAttach, Transform attachedObject, Transform correctPuzzlePieceTransform)
    {
        interactingController = snapAttach.GetAttachedReferenceTransform();
        controllerPreviousUp = GetAxisVector(interactorReferenceDirection, interactingController);
        ProcessingUpdate.Instance.Add(this);
    }
    private void OnSnapDetached(SnapAttach snapAttach, Transform attachedObject)
    {
        ProcessingUpdate.Instance.Remove(this);
    }
    #endregion

    #region ENUMS
    public enum RotationAxis
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward
    }
    public enum RotationDirection
    {
        Clockwise,
        CounterClockwise,
        Both
    }
    #endregion
}
