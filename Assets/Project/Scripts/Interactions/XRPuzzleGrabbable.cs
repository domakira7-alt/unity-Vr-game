using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Yudiz.VRAwarenessExperience.XR;
using Yudiz.XRStarter;

namespace Yudiz.VRAwarenessExperience.Interactions
{
    public class XRPuzzleGrabbable : XRGrabInteractable
    {
        [SerializeField] private Collider interactableCollider;
        [SerializeField] private PuzzlePiece puzzlePiece;
        private Vector3 grabbedPosition;
        private Quaternion grabbedRotation;

        protected override void Awake()
        {
            base.Awake();
            grabbedPosition = transform.position;
            grabbedRotation = transform.rotation;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(OnItemGrabbed);
            selectExited.AddListener(OnItemReleased);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            selectEntered.RemoveListener(OnItemGrabbed);
            selectExited.RemoveListener(OnItemReleased);
        }
        private void OnItemGrabbed(SelectEnterEventArgs args)
        {
            Debug.Log("XRPuzzleGrabbable -> OnItemGrabbed");
            HandSide handSide = args.interactorObject.transform.GetComponent<ControllerAvatar>().HandSide;
            XRManager.instance.ToggleHandModel(false, handSide);
            puzzlePiece.transform.parent = null;
            puzzlePiece.StartMovement();
        }
        private void OnItemReleased(SelectExitEventArgs args)
        {
            Debug.Log("XRPuzzleGrabbable -> OnItemReleased");
            HandSide handSide = args.interactorObject.transform.GetComponent<ControllerAvatar>().HandSide;
            XRManager.instance.ToggleHandModel(true, handSide);
            puzzlePiece.ValidateSnap(OnCorrectSnap, OnWrongSnap);
            puzzlePiece.StopMovement();
        }

        private void OnCorrectSnap()
        {
            transform.position = puzzlePiece.transform.position;
            transform.rotation = puzzlePiece.transform.rotation;
            puzzlePiece.transform.SetParent(transform);
            puzzlePiece.transform.localPosition = Vector3.zero;
            puzzlePiece.transform.localRotation = Quaternion.identity;
            interactableCollider.enabled = false;
        }
        private void OnWrongSnap()
        {
            transform.position = grabbedPosition;
            transform.rotation = grabbedRotation;

            puzzlePiece.transform.SetParent(transform);
            puzzlePiece.transform.localPosition = Vector3.zero;
            puzzlePiece.transform.localRotation = Quaternion.identity;
        }
    }
}

