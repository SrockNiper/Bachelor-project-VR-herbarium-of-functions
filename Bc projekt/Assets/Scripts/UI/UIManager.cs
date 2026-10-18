using UnityEngine;
using TMPro;
using Assets.Scripts.Generator;
using Assets.Scripts.UI.Menu;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Global manager for the user interface. 
    /// Coordinates high-level UI elements like the function title and sub-managers.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("References")]
        public Generator.Generator generator;
        public MenuGenerator menuGenerator;
        public HandMenuHandler handMenuHandler;
        public TableManager tableManager;
        
        [Header("Common UI Elements")]
        public TMP_Text functionTitleText;

        private void Start()
        {
            if (generator != null)
            {
                // Initialize spatial menus
                if (menuGenerator != null) menuGenerator.Initialize(generator);
                
                // Subscribe to title updates
                generator.OnFunctionLoaded += UpdateTitle;
            }
        }

        /// <summary>
        /// Updates the 3D text showing the current active function.
        /// </summary>
        /// <param name="functionString">The formatted math string.</param>
        private void UpdateTitle(string functionString)
        {
            if (functionTitleText != null)
            {
                functionTitleText.text = "f(x,y) = " + functionString;
            }
        }

        private void OnDestroy()
        {
            if (generator != null)
            {
                generator.OnFunctionLoaded -= UpdateTitle;
            }
        }
    }
}
