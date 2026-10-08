using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yudiz.XRStarter;

public class XRControlsHandler : MonoBehaviour
{
    public XRControlsSwitcher leftHandSwitcher;
    public XRControlsSwitcher rightHandSwitcher;

    public void ToggleControl(HandSide handSide)
    {
        if(handSide == HandSide.Left)
        {
            leftHandSwitcher.ToggleInteraction();
        }
        else
        {
            rightHandSwitcher.ToggleInteraction();
        }
    }

    public void ChangeInteraction(HandSide handSide, XRControlsSwitcher.InteractorType interactionType)
    {
        if(handSide == HandSide.Left)
        {
            leftHandSwitcher.ChangeInteraction(interactionType);
        }
        else
        {
            rightHandSwitcher.ChangeInteraction(interactionType);
        }
    }
}
