using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Assets.Scripts.Generator;
using Assets.Scripts.UI;

namespace Assets.Scripts.VR
{
    /// <summary>
    /// Manages high-level VR input events that affect the entire simulation state.
    /// Handles physical button presses like the "Reset Graph" function.
    /// </summary>
    public class VRInputManager : MonoBehaviour
    {
        private const float RESET_POS_Y = 1.2f;
        private const float RESET_SCALE = 0.1f;
        private const float RESET_DOMAIN_MIN = -10f;
        private const float RESET_DOMAIN_MAX = 10f;
        private const float RESET_PARAM_VALUE = 1f;

        [Header("Input Actions")]
        public InputActionProperty resetGraphButton;
        
        [Header("References")]
        public Generator.Generator generator;
        public UIManager uiManager;
        public HandInteractionHandler handInteractionHandler;

        /// <summary>
        /// Initializes the input manager and connects sub-handlers.
        /// </summary>
        public void Initialize(Generator.Generator gen, UIManager ui)
        {
            generator = gen;
            uiManager = ui;
            if (handInteractionHandler != null) handInteractionHandler.Initialize(gen);
        }

        private void OnEnable()
        {
            resetGraphButton.action.Enable();
            resetGraphButton.action.performed += ResetGraph;
        }

        private void OnDisable()
        {
            resetGraphButton.action.Disable();
            resetGraphButton.action.performed -= ResetGraph;
        }

        /// <summary>
        /// Resets the graph to its default state (position, rotation, scale, and math domain).
        /// </summary>
        public void ResetGraph(InputAction.CallbackContext context)
        {
            XRGrabInteractable graphObject = generator.GetComponentInParent<XRGrabInteractable>();
            if (graphObject != null)
            {
                ResetPhysicalTransform(graphObject);
                ResetMathematicalState();
                
                // Redraw everything
                generator.RefreshGraph();
                
                // Synchronize UI menus to match new values
                if (uiManager != null && uiManager.menuGenerator != null)
                {
                    if (uiManager.menuGenerator.domainHandler != null)
                        uiManager.menuGenerator.domainHandler.ConfigDomainMenu(generator);
                    if (uiManager.menuGenerator.parametrHandler != null)
                        uiManager.menuGenerator.parametrHandler.GenerateSliders(generator);
                }
            }
        }

        private void ResetPhysicalTransform(XRGrabInteractable graphObject)
        {
            // Reset physical transform
            graphObject.transform.position = new Vector3(0f, RESET_POS_Y, 0f);
            graphObject.transform.rotation = Quaternion.identity;
            generator.transform.localScale = Vector3.one * RESET_SCALE;
            
            // Reset physics state
            Rigidbody rb = graphObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void ResetMathematicalState()
        {
            // Reset mathematical domain
            generator.Xmin = RESET_DOMAIN_MIN;
            generator.Xmax = RESET_DOMAIN_MAX;
            generator.Zmin = RESET_DOMAIN_MIN;
            generator.Zmax = RESET_DOMAIN_MAX;
            
            // Reset parameters to default values
            if (generator.parameterValues != null)
            {
                for (int i = 0; i < generator.parameterValues.Length; i++)
                {
                    generator.parameterValues[i] = RESET_PARAM_VALUE;
                }
            }
        }
    }
}
