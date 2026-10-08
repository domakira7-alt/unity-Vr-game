using System.Collections;
using System.Collections.Generic;
using StarterKit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Yudiz.XRStarter;

namespace Yudiz.VRAwarenessExperience.XR
{
    public class HapticManager : Singleton<HapticManager>
    {
        [Header("Controller References")]
        [SerializeField] private XRBaseController leftController;
        [SerializeField] private XRBaseController rightController;
        
        [Header("Haptic Settings")]
        [SerializeField] private bool enableHaptics = true;
        [SerializeField] private float maxDuration = 2f;

        public override void OnAwake()
        {
            // Find controllers if not assigned
            if (leftController == null || rightController == null)
            {
                FindControllers();
            }
        }

        /// <summary>
        /// Triggers haptic feedback on the specified hand
        /// </summary>
        /// <param name="handSide">Which hand to trigger haptics on</param>
        /// <param name="intensity">Haptic intensity (0.0 to 1.0)</param>
        /// <param name="duration">Haptic duration in seconds</param>
        public void TriggerHaptic(HandSide handSide, float intensity, float duration)
        {
            if (!enableHaptics) return;

            // Validate parameters
            intensity = Mathf.Clamp01(intensity);
            duration = Mathf.Clamp(duration, 0f, maxDuration);

            XRBaseController controller = GetController(handSide);
            if (controller != null)
            {
                controller.SendHapticImpulse(intensity, duration);
                Debug.Log($"HapticManager -> Haptic triggered on {handSide} hand - Intensity: {intensity}, Duration: {duration}");
            }
            else
            {
                Debug.LogWarning($"HapticManager -> Controller not found for {handSide} hand");
            }
        }

        /// <summary>
        /// Triggers haptic feedback on both hands
        /// </summary>
        /// <param name="intensity">Haptic intensity (0.0 to 1.0)</param>
        /// <param name="duration">Haptic duration in seconds</param>
        public void TriggerHapticBothHands(float intensity, float duration)
        {
            TriggerHaptic(HandSide.Left, intensity, duration);
            TriggerHaptic(HandSide.Right, intensity, duration);
        }

        /// <summary>
        /// Triggers a quick haptic pulse (commonly used for UI interactions)
        /// </summary>
        /// <param name="handSide">Which hand to trigger haptics on</param>
        /// <param name="intensity">Haptic intensity (0.0 to 1.0)</param>
        public void TriggerQuickHaptic(HandSide handSide, float intensity = 0.5f)
        {
            TriggerHaptic(handSide, intensity, 0.1f);
        }

        /// <summary>
        /// Triggers a strong haptic pulse (commonly used for important events)
        /// </summary>
        /// <param name="handSide">Which hand to trigger haptics on</param>
        /// <param name="intensity">Haptic intensity (0.0 to 1.0)</param>
        public void TriggerStrongHaptic(HandSide handSide, float intensity = 0.8f)
        {
            TriggerHaptic(handSide, intensity, 0.3f);
        }

        /// <summary>
        /// Triggers a continuous haptic pattern
        /// </summary>
        /// <param name="handSide">Which hand to trigger haptics on</param>
        /// <param name="intensity">Haptic intensity (0.0 to 1.0)</param>
        /// <param name="duration">Total duration of the haptic pattern</param>
        /// <param name="pulseInterval">Interval between pulses in seconds</param>
        public void TriggerHapticPattern(HandSide handSide, float intensity, float duration, float pulseInterval = 0.1f)
        {
            if (!enableHaptics) return;

            StartCoroutine(HapticPatternCoroutine(handSide, intensity, duration, pulseInterval));
        }

        /// <summary>
        /// Enables or disables haptic feedback
        /// </summary>
        /// <param name="enabled">Whether haptics should be enabled</param>
        public void SetHapticsEnabled(bool enabled)
        {
            enableHaptics = enabled;
        }

        /// <summary>
        /// Gets the current haptic enabled state
        /// </summary>
        /// <returns>True if haptics are enabled</returns>
        public bool IsHapticsEnabled()
        {
            return enableHaptics;
        }

        private XRBaseController GetController(HandSide handSide)
        {
            return handSide == HandSide.Left ? leftController : rightController;
        }

        private void FindControllers()
        {
            // Find controllers in the scene
            XRBaseController[] controllers = FindObjectsOfType<XRBaseController>();
            
            foreach (XRBaseController controller in controllers)
            {
                // Try to determine which hand based on the controller's name or position
                string controllerName = controller.name.ToLower();
                
                if (controllerName.Contains("left") && leftController == null)
                {
                    leftController = controller;
                }
                else if (controllerName.Contains("right") && rightController == null)
                {
                    rightController = controller;
                }
            }

            // If still not found, try to find by XRDirectInteractor components
            if (leftController == null || rightController == null)
            {
                XRDirectInteractor[] interactors = FindObjectsOfType<XRDirectInteractor>();
                
                foreach (XRDirectInteractor interactor in interactors)
                {
                    ControllerAvatar avatar = interactor.GetComponent<ControllerAvatar>();
                    if (avatar != null)
                    {
                        XRBaseController controller = interactor.GetComponent<XRBaseController>();
                        if (controller != null)
                        {
                            if (avatar.HandSide == HandSide.Left && leftController == null)
                            {
                                leftController = controller;
                            }
                            else if (avatar.HandSide == HandSide.Right && rightController == null)
                            {
                                rightController = controller;
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator HapticPatternCoroutine(HandSide handSide, float intensity, float duration, float pulseInterval)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                TriggerHaptic(handSide, intensity, pulseInterval);
                yield return new WaitForSeconds(pulseInterval);
                elapsedTime += pulseInterval;
            }
        }

        #region Context Menu Methods
        [ContextMenu("Test Left Hand Haptic")]
        private void TestLeftHandHaptic()
        {
            TriggerHaptic(HandSide.Left, 0.5f, 0.2f);
        }

        [ContextMenu("Test Right Hand Haptic")]
        private void TestRightHandHaptic()
        {
            TriggerHaptic(HandSide.Right, 0.5f, 0.2f);
        }

        [ContextMenu("Test Both Hands Haptic")]
        private void TestBothHandsHaptic()
        {
            TriggerHapticBothHands(0.5f, 0.2f);
        }
        #endregion
    }
}

