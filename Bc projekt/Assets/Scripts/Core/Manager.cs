using UnityEngine;
using Assets.Scripts.Generator;
using Assets.Scripts.VR;
using Assets.Scripts.UI;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Central orchestrator of the project. 
    /// Connects and initializes all main managers and ensures communication between them.
    /// </summary>
    public class Manager : MonoBehaviour
    {
        [Header("System References")]
        public Generator.Generator generator;
        public DataManager dataManager;
        public VRInputManager vrInputManager;
        public UIManager uiManager;
        public ErrorHandler errorHandler;

        void Start()
        {
            // Initialize Data Management
            if (dataManager != null)
            {
                dataManager.CreateShortcut();
                
                // Subscribe to function load events to update UI and Generator
                dataManager.OnFunctionsLoaded += (funcs) => {
                    if (uiManager != null && uiManager.tableManager != null)
                        uiManager.tableManager.RefreshBoardUI(funcs);
                    
                    if (funcs.Count > 0 && generator != null)
                        generator.SetFunction(funcs[0]);
                };
                
                // Trigger initial data load
                dataManager.LoadFunctions();
            }

            // Initialize VR Input systems
            if (vrInputManager != null)
            {
                vrInputManager.Initialize(generator, uiManager);
            }
        }
    }
}
