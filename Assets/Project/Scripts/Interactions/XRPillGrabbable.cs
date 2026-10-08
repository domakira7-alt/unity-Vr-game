using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Yudiz.VRAwarenessExperience.Core;
using Yudiz.XRStarter;

namespace Yudiz.VRAwarenessExperience.XR
{
    public class XRPillGrabbable : XRGrabInteractable
    {
        [SerializeField] private Collider interactableCollider;
        private Vector3 grabbedPosition;
        private Quaternion grabbedRotation;

        public bool isEaten = false;

        public delegate void OnPillGrabbedDelegate();
        public static event OnPillGrabbedDelegate OnPillGrabbed;

        public delegate void OnPillReleasedDelegate();
        public static event OnPillReleasedDelegate OnPillReleased;

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
            ManageEatables.OnEatableEaten += OnEatableEaten;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            selectEntered.RemoveListener(OnItemGrabbed);
            selectExited.RemoveListener(OnItemReleased);
            ManageEatables.OnEatableEaten -= OnEatableEaten;
        }
        private void OnItemGrabbed(SelectEnterEventArgs args)
        {
            Debug.Log("XRPillGrabbable -> OnItemGrabbed");
            HandSide handSide = args.interactorObject.transform.GetComponent<ControllerAvatar>().HandSide;
            XRManager.instance.ToggleHandModel(false, handSide);
            OnPillGrabbed?.Invoke();
        }
        private void OnItemReleased(SelectExitEventArgs args)
        {
            Debug.Log("XRPillGrabbable -> OnItemReleased");
            HandSide handSide = args.interactorObject.transform.GetComponent<ControllerAvatar>().HandSide;
            XRManager.instance.ToggleHandModel(true, handSide);
            Debug.Log("Inside OnItemReleased" +isEaten);
            if (isEaten)
            {
                Debug.Log("pill is grabbed but released without eating" +isEaten);
                return;
            }
            
            transform.position = grabbedPosition;
            transform.rotation = grabbedRotation;
            Debug.Log("OnItemreleased without pill" + isEaten);
            OnPillReleased?.Invoke();
        }

        private void OnEatableEaten(IEatable eatable)
        {
            Debug.Log("XRPillGrabbable -> OnEatableEaten");
            isEaten = true;
            Debug.Log("after eating pill . Inside OnEatableEaten" +isEaten);
            interactionManager.SelectExit(firstInteractorSelecting, this);
            
        }
    }
}
