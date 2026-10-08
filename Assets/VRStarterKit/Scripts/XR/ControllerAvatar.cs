using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Yudiz.VRAwarenessExperience.XR;

namespace Yudiz.XRStarter
{
    [RequireComponent(typeof(XRDirectInteractor))]
    public class ControllerAvatar : MonoBehaviour
    {
        public HandSide HandSide { get => handSide; }
        public HandGrabType defaultGrabType = HandGrabType.Default;
        public Transform HandTransform
        {
            get
            {
                if (handTransform == null)
                    handTransform = AvatarTransform.GetChild(0);
                return handTransform;
            }
        }
        public Transform AvatarTransform
        {
            get
            {
                if (avatarTransform == null)
                    avatarTransform = transform.GetChild(0);
                return avatarTransform;
            }
        }

        [SerializeField] private Animator handAnimator;
        [SerializeField] private HandSide handSide;

        private Transform avatarTransform;
        private Transform handTransform;
        private XRDirectInteractor interactor;
        private HandGrabType handGrabType;

        private const string handTypeParam = "HandState";

        #region UNITY_CALLBACKS
        private void OnValidate()
        {
            avatarTransform = transform.GetChild(0);
            handTransform = avatarTransform.GetChild(0);
        }
        private void Awake()
        {
            interactor = GetComponent<XRDirectInteractor>();
            avatarTransform = transform.GetChild(0);
            handTransform = avatarTransform.GetChild(0);
        }
        private void OnEnable()
        {
            interactor.hoverEntered.AddListener(OnControllerHoverEntered);
            interactor.hoverExited.AddListener(OnControllerHoverExited);
            interactor.selectEntered.AddListener(OnControllerGrabPerformed);
            interactor.selectExited.AddListener(OnControllerReleasePerformed);
        }
        private void OnDisable()
        {
            interactor.hoverEntered.RemoveListener(OnControllerHoverEntered);
            interactor.hoverExited.RemoveListener(OnControllerHoverExited);
            interactor.selectEntered.RemoveListener(OnControllerGrabPerformed);
            interactor.selectExited.RemoveListener(OnControllerReleasePerformed);
        }
        private void Start()
        {
            SetHandPose(defaultGrabType);
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetHandPose(HandGrabType grabType)
        {
            handGrabType = grabType;
            handAnimator.SetInteger(handTypeParam, (int)grabType);
        }
        private void OnControllerHoverEntered(HoverEnterEventArgs arg0)
        {

        }

        private void OnControllerHoverExited(HoverExitEventArgs arg0)
        {

        }

        private void OnControllerGrabPerformed(SelectEnterEventArgs arg0)
        {
            if (arg0.interactableObject.transform.TryGetComponent(out XRCustomGrabbable grabbable))
            {
                SetHandPose(grabbable.grabType);
            }
        }

        private void OnControllerReleasePerformed(SelectExitEventArgs arg0)
        {
            SetHandPose(defaultGrabType);
        }
        #endregion

    }

    public enum HandGrabType
    {
        None = -1,
        Default = 0,
        Pinch,
        Fist,
        indexfingerpoint
    }

    public enum HandSide
    {
        Left,
        Right
    }
}