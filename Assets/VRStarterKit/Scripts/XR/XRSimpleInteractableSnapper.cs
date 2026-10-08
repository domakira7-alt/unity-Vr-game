using CommanTickManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Yudiz.XRStarter
{
    public class XRSimpleInteractableSnapper : MonoBehaviour, ITick
    {
        public HandGrabType grabType;
        private XRSimpleInteractable simpleInteractable;
        private ControllerAvatar currentControllerAvatar;


        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            simpleInteractable.selectEntered.AddListener(OnSelectEntered);
            simpleInteractable.selectExited.AddListener(OnSelectExited);
        }
        private void OnDisable()
        {
            simpleInteractable.selectEntered.RemoveListener(OnSelectEntered);
            simpleInteractable.selectExited.RemoveListener(OnSelectExited);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnSelectEntered(SelectEnterEventArgs arg0)
        {
            if(arg0.interactorObject is XRDirectInteractor)
            {
                ControllerAvatar controllerAvatar = arg0.interactorObject.transform.GetComponent<ControllerAvatar>();
            }
        }

        private void OnSelectExited(SelectExitEventArgs arg0)
        {
            
        }
        #endregion

        public void Tick()
        {
            
        }
    }
}