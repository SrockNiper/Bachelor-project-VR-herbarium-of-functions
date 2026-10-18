using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Assets.Scripts.VR
{
    /// <summary>
    /// Implements complex hand-based interactions, specifically the two-handed scaling gesture.
    /// Measures distance changes between controllers to scale the graph object.
    /// </summary>
    public class HandInteractionHandler : MonoBehaviour
    {
        [Header("Input Settings")]
        public InputActionProperty leftPrimary;
        public InputActionProperty rightPrimary;
        
        [Header("Controller References")]
        public Transform leftControllerTransform;
        public Transform rightControllerTransform;
        
        [Header("Target References")]
        public Generator.Generator generator;
        private XRGrabInteractable graphObject;

        // Scaling state
        private bool isScaling = false;
        private float initialHandDistance;
        private Vector3 initialGraphScale;
        private bool isGraphHovered = false;

        /// <summary>
        /// Connects the handler to the target Generator and its interactable component.
        /// </summary>
        public void Initialize(Generator.Generator gen)
        {
            generator = gen;
            graphObject = generator.GetComponentInParent<XRGrabInteractable>();
            if (graphObject != null)
            {
                // Track if user is currently pointing at the graph
                graphObject.hoverEntered.AddListener((args) => isGraphHovered = true);
                graphObject.hoverExited.AddListener((args) => isGraphHovered = false);
            }
        }

        private void OnEnable()
        {
            leftPrimary.action.Enable();
            rightPrimary.action.Enable();
        }

        private void OnDisable()
        {
            leftPrimary.action.Disable();
            rightPrimary.action.Disable();
        }

        private void Update()
        {
            // Only scale if both buttons are pressed and hand is near the graph
            bool bothPressed = leftPrimary.action.IsPressed() && rightPrimary.action.IsPressed();
            
            if (bothPressed)
            {
                if (!isScaling && isGraphHovered)
                {
                    StartScaling();
                }
                else if (isScaling)
                {
                    UpdateScaling();
                }
            }
            else
            {
                isScaling = false;
            }
        }

        /// <summary>
        /// Captures the initial state when the scaling gesture starts.
        /// </summary>
        private void StartScaling()
        {
            initialHandDistance = Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);
            if (initialHandDistance < 0.05f) initialHandDistance = 0.05f; // Prevent division by zero
            
            initialGraphScale = generator.transform.localScale;
            isScaling = true;
        }

        /// <summary>
        /// Calculates the current scale factor and applies it to the graph.
        /// </summary>
        private void UpdateScaling()
        {
            float currentHandDistance = Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);
            float scaleFactor = currentHandDistance / initialHandDistance;
            Vector3 targetScale = initialGraphScale * scaleFactor;

            // Clamping to prevent extreme sizes
            targetScale.x = Mathf.Clamp(targetScale.x, 0.1f, 5f);
            targetScale.y = Mathf.Clamp(targetScale.y, 0.1f, 5f);
            targetScale.z = Mathf.Clamp(targetScale.z, 0.1f, 5f);
            
            generator.transform.localScale = targetScale;
        }
    }
}
