using CommanTickManager;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yudiz.VRAwarenessExperience.Interactions;
using Yudiz.VRAwarenessExperience.XR;

namespace Yudiz.XRStarter.Interactions
{
    public class SnapAttach : MonoBehaviour, ITick
    {
        [HorizontalLine(color: EColor.Green)]
        [Header("Snapping Settings")]
        [SerializeField] private SnapAttachType snapAttachType;
        [SerializeField] private Transform snapAttachTransform;
        [SerializeField] private float distanceToDetech = 0.2f;
        [SerializeField] private bool lerpSnap;
        [ShowIf("lerpSnap")][SerializeField] private float snapSpeed = 10f;
        [ShowIf("lerpSnap")][SerializeField] private float alignSpeed = 50f;
        [HorizontalLine(color: EColor.Blue)]
        [Header("Validation Settings")]
        [SerializeField] private float distanceThreshold = 0.01f;
        [SerializeField] private float rotationThreshold = 30f;

        [HorizontalLine(color: EColor.Yellow)]
        [Header("Other")]
        [SerializeField] private Transform snappableItem;
        [SerializeField] private Collider snapAttachCollider;

        // Static C# delegate events
        public static event System.Action<SnapAttach, Transform, Transform> OnSnapAttached;
        public static event System.Action<SnapAttach, Transform> OnSnapDetached;
        public static event System.Action<SnapAttach, Transform> OnSnappDetaching;
        public static event System.Action<SnapAttach, Transform, Transform> OnSnappAttaching;

        // Simple static tracking to prevent multiple simultaneous snap attempts

        private SnapState snapState;

        private Transform attachingItemParent;
        private Transform attachingTransform;

        private XRCustomGrabbable xrCustomGrabbable;
        

        #region UNITY_CALLBACKS
        private void Start()
        {
            snapState = SnapState.Idle;
            if (snapAttachTransform == null)
            {
                snapAttachTransform = transform;
            }
            XRCustomGrabbable.OnWrongSnap += OnWrongSnapDetaching;
            XRCustomGrabbable.OnFinalSnap += OnFinalSnap;
        }

        private void OnFinalSnap(SnapAttach snapAttach)
        {
            if (snapAttach == this)
            {
                ChangeSnapState(SnapState.FinalSnap);
            }
        }

        private void OnWrongSnapDetaching(SnapAttach snapAttach)
        {
            if (snapAttach == this)
            {
                ChangeSnapState(SnapState.Detaching);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("SnapTriggerEnter");
            if (snapState == SnapState.Idle)
            {
                switch (snapAttachType)
                {
                    case SnapAttachType.Hand:
                        if (other.TryGetComponent(out ControllerAvatar controllerAvatar))
                        {
                            attachingItemParent = controllerAvatar.transform;
                            attachingTransform = controllerAvatar.AvatarTransform;
                            // ChangeSnapState(SnapState.Snapping);
                        }
                        break;
                    case SnapAttachType.Object:
                        if (other.TryGetComponent(out XRCustomGrabbable grabbable))
                        {
                            if (grabbable.snappableTransform == snappableItem)
                            {
                                attachingItemParent = grabbable.transform;
                                attachingTransform = grabbable.snappableTransform;
                                // ChangeSnapState(SnapState.Snapping);
                            }
                        }
                        break;

                    case SnapAttachType.PuzzlePiece:
                        Debug.Log("SnapAttach -> PuzzlePiece -> OnTriggerEnter -> PuzzlePiece : " + other.gameObject.name);
                        if (other.TryGetComponent(out PuzzlePiece puzzlePiece))
                        {
                            attachingItemParent = puzzlePiece.transform;
                            attachingTransform = puzzlePiece.snapAttachTransform;
                            this.xrCustomGrabbable = puzzlePiece.GetXRCustomGrabbable();
                            xrCustomGrabbable.OnReleased.AddListener(OnPuzzlePieceReleased);
                        }
                        
                        break;
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (snapState != SnapState.None)
            {
                switch (snapAttachType)
                {
                    case SnapAttachType.Hand:
                        if (other.TryGetComponent(out ControllerAvatar controllerAvatar))
                        {
                            if (controllerAvatar.AvatarTransform == attachingTransform)
                            {
                                ChangeSnapState(SnapState.Detaching);
                            }
                        }
                        break;
                    case SnapAttachType.Object:
                        if (other.TryGetComponent(out XRCustomGrabbable grabbable))
                        {
                            if (grabbable.snappableTransform == attachingTransform)
                            {
                                ChangeSnapState(SnapState.Detaching);
                            }
                        }
                        break;

                    case SnapAttachType.PuzzlePiece:
                        if (other.TryGetComponent(out PuzzlePiece puzzlePiece))
                        {
                            Debug.Log("SnapAttach -> PuzzlePiece -> OnTriggerExit -> Remove Release Listener");
                            xrCustomGrabbable?.OnReleased.RemoveListener(OnPuzzlePieceReleased);
                            xrCustomGrabbable = null;
                        }
                        break;
                }
            }
        }
        #endregion

        #region PRIVATE_METHODS

        private void OnPuzzlePieceReleased(XRCustomGrabbable xrCustomGrabbable)
        {
            ChangeSnapState(SnapState.Snapping);
        }

        private void SnapItem(Transform snappingItem, Transform attachTransform, bool isSnapping)
        {
            if (lerpSnap && isSnapping)
            {
                snappingItem.position = Vector3.MoveTowards(snappingItem.position, attachTransform.position, Time.deltaTime * snapSpeed);
                snappingItem.rotation = Quaternion.RotateTowards(snappingItem.rotation, attachTransform.rotation, Time.deltaTime * alignSpeed);
            }
            else
            {
                snappingItem.position = attachTransform.position;
                snappingItem.rotation = attachTransform.rotation;
            }
        }
        private void DetachItem(Transform snappingItem, Transform attachTransform)
        {
            if (lerpSnap)
            {
                snappingItem.localPosition = Vector3.MoveTowards(snappingItem.localPosition, Vector3.zero, Time.deltaTime * snapSpeed);
                snappingItem.localRotation = Quaternion.RotateTowards(snappingItem.localRotation, Quaternion.identity, Time.deltaTime * alignSpeed);
            }
            else
            {
                snappingItem.localPosition = Vector3.zero;
                snappingItem.localRotation = Quaternion.identity;
            }
        }
        private void CheckForDetech(Transform snappingItem, Transform itemParent)
        {
            if (Vector3.Distance(snappingItem.position, itemParent.position) > distanceToDetech)
            {
                ChangeSnapState(SnapState.Detaching);
            }
        }
        
        private bool ValidateRotation(Transform snappingItem)
        {
            float forwardAngle = Vector3.Angle(snappingItem.forward, snapAttachTransform.forward);
            return forwardAngle <= rotationThreshold;
        }
        private void ChangeSnapState(SnapState newState)
        {
            if (newState == snapState) { return; }

            ExitState(snapState);
            snapState = newState;
            EnterState(snapState);

            Debug.Log($" {this.gameObject.name} -> SnapAttach State Changed: {snapState} -> {newState}");

        }
        private void ExitState(SnapState state)
        {
            switch (snapState)
            {
                case SnapState.Idle:
                    break;
                case SnapState.Snapping:
                    break;
                case SnapState.Snapped:
                    break;
                case SnapState.Detaching:
                    break;
                case SnapState.Detached:
                    attachingTransform = null;
                    attachingItemParent = null;
                    break;
            }
        }
        private void EnterState(SnapState state)
        {
            switch (snapState)
            {
                case SnapState.Idle:
                    ProcessingUpdate.Instance.Remove(this);
                    attachingTransform = null;
                    attachingItemParent = null;
                    break;
                case SnapState.Snapping:
                    ProcessingUpdate.Instance.Add(this);
                    OnSnappAttaching?.Invoke(this, attachingTransform, snappableItem);
                    break;
                case SnapState.Snapped:
                    ProcessingUpdate.Instance.Remove(this);
                    OnSnapAttached?.Invoke(this, attachingTransform, snappableItem);
                    break;
                case SnapState.FinalSnap:
                    snapAttachCollider.enabled = false;
                    this.enabled = false;
                    xrCustomGrabbable?.OnReleased?.RemoveListener(OnPuzzlePieceReleased);
                    xrCustomGrabbable = null;
                    break;
                case SnapState.Detaching:
                    OnSnappDetaching?.Invoke(this, attachingTransform);
                    break;
                case SnapState.Detached:
                    OnSnapDetached?.Invoke(this, attachingTransform);
                    ChangeSnapState(SnapState.Idle);
                    break;
            }
        }
        #endregion

        #region PUBLIC_METHODS
        public void ForceDetach()
        {
            ChangeSnapState(SnapState.Detaching);
        }
        public void Tick()
        {
            switch (snapState)
            {
                case SnapState.Snapping:
                    if (Vector3.Distance(attachingTransform.position, snapAttachTransform.position) < distanceThreshold)
                    {
                        if (ValidateRotation(attachingTransform) )
                        {
                            SnapItem(attachingTransform, snapAttachTransform, true);
                            ChangeSnapState(SnapState.Snapped);
                        }
                        else
                        {
                            ChangeSnapState(SnapState.Detaching);
                            Debug.Log("SnapAttach -> Snapping -> ValidateRotation Failed");
                            xrCustomGrabbable?.OnReleased.RemoveListener(OnPuzzlePieceReleased);
                            xrCustomGrabbable = null;
                        }
                    }
                    else
                    {
                        ChangeSnapState(SnapState.Detaching);
                        xrCustomGrabbable?.OnReleased.RemoveListener(OnPuzzlePieceReleased);
                        xrCustomGrabbable = null;
                    }
                    CheckForDetech(attachingTransform, attachingItemParent);
                    break;
                    
                case SnapState.Detaching:
                    DetachItem(attachingTransform, snapAttachTransform);
                    if (Vector3.Distance(attachingTransform.localPosition, Vector3.zero) < 0.1f)
                    {
                        attachingTransform.localPosition = Vector3.zero;
                        attachingTransform.localRotation = Quaternion.identity;
                        ChangeSnapState(SnapState.Detached);
                    }
                    break;
            }
        }
        public Transform GetAttachedReferenceTransform()
        {
            return attachingItemParent;
        }
        #endregion
    }

    public enum SnapAttachType
    {
        None,
        Hand,
        Object,
        PuzzlePiece
    }
    public enum SnapState
    {
        None,
        Idle,
        Snapping,
        Snapped,
        FinalSnap,
        Detaching,
        Detached
    }
}