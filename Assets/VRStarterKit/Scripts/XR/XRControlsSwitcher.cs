using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XRControlsSwitcher : MonoBehaviour
{
    public struct InteractorController
    {
        public GameObject m_GO;
        public ActionBasedController m_XRController;
        public XRInteractorLineVisual m_LineRenderer;
        public XRBaseInteractor m_Interactor;

        public void Attach(GameObject gameObject)
        {
            m_GO = gameObject;
            if (m_GO != null)
            {
                m_XRController = m_GO.GetComponent<ActionBasedController>();
                m_LineRenderer = m_GO.GetComponent<XRInteractorLineVisual>();
                m_Interactor = m_GO.GetComponent<XRBaseInteractor>();
            }
        }

        public void Activate()
        {
            if (m_LineRenderer)
            {
                m_LineRenderer.enabled = true;
            }
            if (m_XRController)
            {
                m_XRController.enableInputActions = true;
            }
            if (m_Interactor)
            {
                m_Interactor.enabled = true;
            }
        }

        public void Deactivate()
        {
            if (m_LineRenderer)
            {
                m_LineRenderer.enabled = false;
            }
            if (m_XRController)
            {
                m_XRController.enableInputActions = false;
            }
            if (m_Interactor)
            {
                m_Interactor.enabled = false;
            }
        }
    }

    [SerializeField] private GameObject normalInteractor;
    [SerializeField] private GameObject activableInteractor;
    [SerializeField] private InputActionReference activationInputReference;
    [SerializeField] private bool useAutoChange = false;

    InteractorController normalInteraction;
    InteractorController activableInteraction;

    bool isActivated;

    void OnEnable()
    {
        if (useAutoChange)
        {
            activationInputReference.action.performed += OnTogglePressed;
            activationInputReference.action.canceled += OnToggleReleased;
        }
    }

    void OnDisable()
    {
        if (useAutoChange)
        {
            activationInputReference.action.performed -= OnTogglePressed;
            activationInputReference.action.canceled -= OnToggleReleased;
        }
    }

    private void Start()
    {
        normalInteraction.Attach(normalInteractor);
        activableInteraction.Attach(activableInteractor);

        // normalInteraction.Activate();
        // activableInteraction.Deactivate();
    }

    private void OnTogglePressed(InputAction.CallbackContext obj)
    {
        normalInteraction.Deactivate();
        activableInteraction.Activate();
    }

    private void OnToggleReleased(InputAction.CallbackContext obj)
    {
        this.Execute(() =>
        {
            normalInteraction.Activate();
            activableInteraction.Deactivate();
        }, 0.1f);
    }

    public void ToggleInteraction(bool activate)
    {
        if (activate == isActivated)
            return;
        if (activate)
        {
            normalInteraction.Deactivate();
            activableInteraction.Activate();
        }
        else
        {
            normalInteraction.Activate();
            activableInteraction.Deactivate();
        }
        isActivated = activate;
    }

    public void ChangeInteraction(InteractorType interactionType)
    {
        if (interactionType == InteractorType.DirectInteraction)
        {
            normalInteraction.Activate();
            activableInteraction.Deactivate();
        }
        else 
        {
            normalInteraction.Deactivate();
            activableInteraction.Activate();
        }
    }
    public void ToggleInteraction()
    {
        ToggleInteraction(!isActivated);
    }

    public enum InteractorType
    {
        DirectInteraction,
        RayInteraction,
    }
}
