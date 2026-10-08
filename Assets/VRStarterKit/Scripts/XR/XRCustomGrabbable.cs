using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Yudiz.VRAwarenessExperience.Interactions;
using Yudiz.XRStarter;
using Yudiz.XRStarter.Interactions;


namespace Yudiz.VRAwarenessExperience.XR
{
    public class XRCustomGrabbable : XRGrabInteractable
    {
        [HorizontalLine(color: EColor.Green)]
        [Header("Grab Properties")]
        public Transform leftAnchorTransform;
        public Transform rightAnchorTransform;
        public Transform secondaryLeftAnchorTransform;
        public Transform secondaryRightAnchorTransform;
        public HandGrabType grabType;
        public bool shouldResetOnRelease;

        [HorizontalLine(color: EColor.Blue)]
        [Header("Snap Properties")]
        public Transform snappableTransform;

        [HorizontalLine(color: EColor.Yellow)]
        [Header("Puzzle Piece")]
        [SerializeField] private PuzzlePiece puzzlePiece;

        [HorizontalLine(color: EColor.Red)]
        [Header("Events")]
        public UnityEvent<XRCustomGrabbable> OnGrabbed;
        public UnityEvent<XRCustomGrabbable> OnReleased;
        public UnityEvent<XRCustomGrabbable> OnGrabbedSecondary;
        public UnityEvent<XRCustomGrabbable> OnReleasedSecondary;

        public delegate void OnSnapDelegate(bool isSnapped);
        public static event OnSnapDelegate OnSnap;

        public delegate void OnFinalSnapDelegate(SnapAttach snapAttach);
        public static event OnFinalSnapDelegate OnFinalSnap;

        public delegate void OnWrongSnapDelegate(SnapAttach snapAttach);
        public static event OnWrongSnapDelegate OnWrongSnap;


        private Vector3 grabbedPosition;
        private Quaternion grabbedRotation;

        private Yudiz.XRStarter.Interactions.SnapZone socketInteractor;
        protected Collider interactableCollider;

        private Transform primaryInteractor;
        private Transform secondaryInteractor;
        
        // Snap state tracking
        private bool isSnapped = false;

        #region UNITY_CALLBACKS
        protected override void Awake()
        {
            base.Awake();

            interactableCollider = GetComponent<Collider>();
            if (interactableCollider == null)
            {
                interactableCollider = GetComponentInChildren<Collider>();
            }
            grabbedPosition = transform.position;
            grabbedRotation = transform.rotation;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(OnItemGrabbed);
            selectExited.AddListener(OnItemReleased);
            SnapAttach.OnSnappAttaching += OnSnapAttachingEvent;
            SnapAttach.OnSnapAttached += OnSnapAttachedEvent;
            SnapAttach.OnSnappDetaching += OnSnapDetachedEvent;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            ForceReset();
            selectEntered.RemoveListener(OnItemGrabbed);
            selectExited.RemoveListener(OnItemReleased);
            
            SnapAttach.OnSnappAttaching -= OnSnapAttachingEvent;
            SnapAttach.OnSnapAttached -= OnSnapAttachedEvent;
            SnapAttach.OnSnappDetaching -= OnSnapDetachedEvent;
        }
        #endregion

        #region METHOD_OVERRIDES
        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (leftAnchorTransform == null || rightAnchorTransform == null)
            {
                base.OnHoverEntered(args);
                return;
            }
            if (interactorsSelecting.Count == 0)
            {
                base.OnHoverEntered(args);
                return;
            }
            Debug.Log("OnHoverEntered " + interactorsSelecting.Count + " " + interactorsHovering.Count);

            if (args.interactorObject.transform.TryGetComponent(out ControllerAvatar controllerAvatar))
            {
                if (controllerAvatar.HandSide == HandSide.Left)
                {
                    secondaryAttachTransform = secondaryLeftAnchorTransform;
                }
                else
                {
                    secondaryAttachTransform = secondaryRightAnchorTransform;
                }
                if(secondaryInteractor == null)
                {
                    secondaryInteractor = args.interactorObject.transform;
                }
            }
            base.OnHoverEntered(args);
        }
        // protected override void OnHoverExited(HoverExitEventArgs args)
        // {
        //     if (leftAnchorTransform == null || rightAnchorTransform == null)
        //     {
        //         base.OnHoverExited(args);
        //         return;
        //     }

        //     if (secondaryInteractor != null && args.interactorObject.transform == secondaryInteractor)
        //     {
        //         secondaryAttachTransform = null;
        //         secondaryInteractor = null;
        //         Debug.Log("Secondary Interactor Reset");
        //     }
        //     else if (primaryInteractor != null && args.interactorObject.transform == primaryInteractor)
        //     {
        //         if (secondaryInteractor != null)
        //         {
        //             attachTransform = secondaryAttachTransform;
        //             secondaryAttachTransform = null;
        //             primaryInteractor = secondaryInteractor;
        //             secondaryInteractor = null;
        //             Debug.Log("Secondary Interactor Reset and Primary Interactor Replaced");
        //         }
        //         else
        //         {
        //             attachTransform = null;
        //             primaryInteractor = null;
        //             Debug.Log("Primary Interactor Reset");
        //         }
        //     }

        //     base.OnHoverExited(args);
        // }
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            if (leftAnchorTransform == null || rightAnchorTransform == null)
            {
                base.OnSelectEntering(args);
                return;
            }
            if (interactorsSelecting.Count > 0)
            {
                base.OnSelectEntering(args);
                return;
            }
            Debug.Log("OnSelectEntering " + interactorsSelecting.Count + " " + interactorsHovering.Count);

            if (args.interactorObject.transform.TryGetComponent(out ControllerAvatar controllerAvatar))
            {
                if (controllerAvatar.HandSide == HandSide.Left)
                {
                    attachTransform = leftAnchorTransform;
                }
                else
                {
                    attachTransform = rightAnchorTransform;
                }
                if (primaryInteractor == null)
                {
                    primaryInteractor = args.interactorObject.transform;
                }
            }

            base.OnSelectEntering(args);
        }
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            if (leftAnchorTransform == null || rightAnchorTransform == null)
            {
                base.OnSelectExited(args);
                return;
            }
            Debug.Log("OnSelectExited " + interactorsSelecting.Count + " " + interactorsHovering.Count);

            if (secondaryInteractor != null && args.interactorObject.transform == secondaryInteractor)
            {
                secondaryAttachTransform = null;
                secondaryInteractor = null;
                Debug.Log("Secondary Interactor Reset");
            }
            else if (primaryInteractor != null && args.interactorObject.transform == primaryInteractor)
            {
                if (secondaryInteractor != null)
                {
                    attachTransform = secondaryAttachTransform;
                    secondaryAttachTransform = null;
                    primaryInteractor = secondaryInteractor;
                    secondaryInteractor = null;
                    Debug.Log("Secondary Interactor Reset and Primary Interactor Replaced");
                }
                else
                {
                    attachTransform = null;
                    primaryInteractor = null;
                    Debug.Log("Primary Interactor Reset");
                }
            }

            base.OnSelectExited(args);
        }
        #endregion

        #region PRIVATE_METHODS
        

        private void OnSnapAttachingEvent(SnapAttach snapAttach, Transform attachedTransform, Transform correctPuzzlePieceTransform)
        {
            if (attachedTransform == snappableTransform && correctPuzzlePieceTransform == puzzlePiece.transform)
            {
                isSnapped = true;
                Debug.Log("OnSnapAttachingEvent -> isSnapped :" + isSnapped);
            }
        }
        private void OnSnapAttachedEvent(SnapAttach snapAttach, Transform attachedTransform, Transform correctPuzzlePieceTransform)
        {
            Debug.Log($"{this.gameObject.name} -> OnSnapAttachedEvent -> PuzzlePiece Transform : {puzzlePiece.name} correctPuzzlePieceTransform : {correctPuzzlePieceTransform.name}");
            Debug.Log( this.gameObject.name + " -> OnSnapAttachedEvent -> Snap Valid Check :  " + (attachedTransform == snappableTransform));
            if (attachedTransform == snappableTransform)
            {
                if (correctPuzzlePieceTransform == puzzlePiece.transform)
                {
                    Debug.Log(this.gameObject.name + " -> OnSnapAttachedEvent -> PuzzlePiece Snapped -> Attached Transform : " + attachedTransform.name + " Correct Puzzle Piece Transform : " + correctPuzzlePieceTransform.name);
                    puzzlePiece.StopMovement();
                    puzzlePiece.SetAttached(true);
                    transform.position = puzzlePiece.transform.position;
                    transform.rotation = puzzlePiece.transform.rotation;
                    puzzlePiece.transform.SetParent(transform);
                    puzzlePiece.transform.localPosition = Vector3.zero;
                    puzzlePiece.transform.localRotation = Quaternion.identity;
                    enabled = false;
                    OnSnap?.Invoke(true);
                    OnFinalSnap?.Invoke(snapAttach);
                }
                else if (correctPuzzlePieceTransform != puzzlePiece.transform)
                {
                    Debug.Log("OnSnapAttachedEvent -> PuzzlePiece Not Snapped -> Attached Transform : " + attachedTransform.name + " Correct Puzzle Piece Transform : " + correctPuzzlePieceTransform.name);
                    transform.position = grabbedPosition;
                    transform.rotation = grabbedRotation;

                    puzzlePiece.StopMovement();
                    puzzlePiece.transform.SetParent(transform);
                    puzzlePiece.transform.localPosition = Vector3.zero;
                    puzzlePiece.transform.localRotation = Quaternion.identity;
                    enabled = true;
                    OnSnap?.Invoke(false);
                    OnWrongSnap?.Invoke(snapAttach);
                }
            }
        }
        
        private void OnSnapDetachedEvent(SnapAttach snapAttach, Transform detachedTransform)
        {
            // Check if this grabbable is the one being detached
            if (detachedTransform == snappableTransform)
            {
                isSnapped = false;
                Debug.Log("OnSnapDetachedEvent -> isSnapped :" + isSnapped);

                transform.position = grabbedPosition;
                transform.rotation = grabbedRotation;

                puzzlePiece.StopMovement();
                puzzlePiece.transform.SetParent(transform);
                puzzlePiece.transform.localPosition = Vector3.zero;
                puzzlePiece.transform.localRotation = Quaternion.identity;
                enabled = true;
            }
        }
        
        private void OnItemGrabbed(SelectEnterEventArgs arg0)
        {
            if (shouldResetOnRelease)
            {
                Debug.Log("OnItemGrabbed and Deparenting Puzzle Piece");
                puzzlePiece.transform.parent = null;
                puzzlePiece.StartMovement();
            }

            if (arg0.interactorObject.transform == primaryInteractor)
            {
                OnGrabbed?.Invoke(this);
            }
            else if (arg0.interactorObject.transform == secondaryInteractor)
            {
                OnGrabbedSecondary?.Invoke(this);
            }
           
        }

        private void OnItemReleased(SelectExitEventArgs arg0)
        {
            OnReleased?.Invoke(this);

            if (shouldResetOnRelease)
            {
                Debug.Log("OnItemReleased -> isSnapped :" + isSnapped);
                if (!isSnapped)
                {
                    transform.position = grabbedPosition;
                    transform.rotation = grabbedRotation;

                    puzzlePiece.StopMovement();
                    puzzlePiece.transform.SetParent(transform);
                    puzzlePiece.transform.localPosition = Vector3.zero;
                    puzzlePiece.transform.localRotation = Quaternion.identity;
                    enabled = true;
                }
            }
        }
        #endregion

        #region PUBLIC_METHODS
        public void ToggleInteractableCollider(bool enabled)
        {
            if (interactableCollider == null)
                return;
            interactableCollider.enabled = enabled;
        }
    
        
        public void ForceReset()
        {
            if (shouldResetOnRelease)
            {
                // Stop Movement of the puzzle piece and then parent and then reset the transform 
                puzzlePiece.StopMovement();
                puzzlePiece.transform.SetParent(transform);
                puzzlePiece.transform.position = Vector3.zero;
                puzzlePiece.transform.rotation = Quaternion.identity;

                transform.position = grabbedPosition;
                transform.rotation = grabbedRotation;
            }
        }

        #endregion

#if UNITY_EDITOR
        #region EDITOR_TOOLS_METHODS
        [ContextMenu("AddGrabAdjuster")]
        public void AddGrabAdjuster()
        {
            gameObject.AddComponent<GrabAnchorAdjuster>();
        }

        [ContextMenu("RemoveGrabAdjuster")]
        public void RemoveGrabAdjuster()
        {
            DestroyImmediate(gameObject.GetComponent<GrabAnchorAdjuster>());
        }

        #endregion
#endif

        #region SNAPPING
        public void OnSocketAttached(SelectEnterEventArgs args)
        {
            socketInteractor = args.interactorObject as Yudiz.XRStarter.Interactions.SnapZone;
        }
        public void OnSocketDetached(SelectExitEventArgs args)
        {
            socketInteractor = null;
        }
        #endregion
    }
}