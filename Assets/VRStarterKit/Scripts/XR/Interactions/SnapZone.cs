using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Yudiz.VRAwarenessExperience.XR;
using Yudiz.XRStarter;

namespace Yudiz.XRStarter.Interactions
{
    public class SnapZone : XRSocketInteractor
    {
        // XRSnapInteractor is an XRSocketInteractor that filters socketable items by tag
        [Header("Filters")]
        [SerializeField] protected List<XRCustomGrabbable> Dropables;

        [Header("Events")]
        public UnityEvent<SnapZone, GameObject> OnDropped;
        public UnityEvent<SnapZone, GameObject> OnUnDropped;
        public UnityEvent<SnapZone, GameObject> OnDropZoneEnter;
        public UnityEvent<SnapZone, GameObject> OnDropZoneExit;

        InteractionLayerMask interactionMask;

        protected override void OnEnable()
        {
            base.OnEnable();
            RegisterSnapEvents();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            DeregisterSnapEvents();
        }
        protected override void Start()
        {
            base.Start();
            interactionMask = interactionLayers;
        }

        public override bool CanHover(IXRHoverInteractable interactable)
        {
            bool canBaseHover = base.CanHover(interactable);
            return canBaseHover && Dropables.Exists(x => x.gameObject == interactable.transform.gameObject);
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            bool canBaseHover = base.CanSelect(interactable);
            return canBaseHover && Dropables.Exists(x => x.gameObject == interactable.transform.gameObject);
        }

        public void ToggleInteractable(bool enabled)
        {
            if (!hasSelection)
            {
                allowHover = enabled;
                allowSelect = enabled;
            }
        }
        public void ToggleInteractable(bool enabled, bool forceUpdate)
        {
            if (forceUpdate)
            {
                allowHover = enabled;
                allowSelect = enabled;
            }
        }

        public void RegisterSnapEvents()
        {
            foreach (XRCustomGrabbable grabbable in Dropables)
            {
                selectEntered.AddListener(grabbable.OnSocketAttached);
                selectExited.AddListener(grabbable.OnSocketDetached);
            }


            selectEntered.AddListener(OnItemDropped);
            selectExited.AddListener(OnItemUndropped);
            hoverEntered.AddListener(OnItemHoverEntered);
            hoverExited.AddListener(OnItemHoverExited);

        }
        public void DeregisterSnapEvents()
        {
            foreach (XRCustomGrabbable grabbable in Dropables)
            {
                selectEntered.RemoveListener(grabbable.OnSocketAttached);
                selectExited.RemoveListener(grabbable.OnSocketDetached);
            }

            selectEntered.RemoveListener(OnItemDropped);
            selectExited.RemoveListener(OnItemUndropped);
            hoverEntered.RemoveListener(OnItemHoverEntered);
            hoverExited.RemoveListener(OnItemHoverExited);
        }

        public void ResetInteraction()
        {
            XRPlayer.instance.interactionManager.CancelInteractorSelection((IXRSelectInteractor)this);
        }

        public void OnItemDropped(SelectEnterEventArgs args)
        {
            OnDropped?.Invoke(this, args.interactableObject.transform.gameObject);

            this.Execute(() =>
            {
                XRCustomGrabbable grabbable = args.interactableObject.transform.GetComponent<XRCustomGrabbable>();
                if (grabbable != null)
                {
                    grabbable.ToggleInteractableCollider(false);
                }
            }, 1000);
        }
        public void OnItemUndropped(SelectExitEventArgs args)
        {
            OnUnDropped?.Invoke(this, args.interactableObject.transform.gameObject);
        }
        public void OnItemHoverEntered(HoverEnterEventArgs args)
        {
            OnDropZoneEnter?.Invoke(this, args.interactableObject.transform.gameObject);
        }
        public void OnItemHoverExited(HoverExitEventArgs args)
        {
            OnDropZoneExit?.Invoke(this, args.interactableObject.transform.gameObject);
        }

        public bool Verify()
        {
            return hasSelection;
        }
    }
}