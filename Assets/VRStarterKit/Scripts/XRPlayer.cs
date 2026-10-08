using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRPlayer : MonoBehaviour
{
    public static XRPlayer instance;
    public XRInteractionManager interactionManager;

    private void Awake()
    {
        instance = this;
    }

    public void TeleportPlayer(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
