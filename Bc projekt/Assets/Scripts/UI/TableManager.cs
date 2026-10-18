using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Generator;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Manages the physical 3D board showing the list of available functions.
    /// Generates interactive buttons/toggles for each function.
    /// </summary>
    public class TableManager : MonoBehaviour
    {
        [Header("References")]
        public Transform tableMenu;
        public GameObject cellPrefab;
        public Generator.Generator generator;

        /// <summary>
        /// Clears the board and generates new interactive toggles for each function string.
        /// </summary>
        /// <param name="availableFunctions">List of strings to display.</param>
        public void RefreshBoardUI(List<string> availableFunctions)
        {
            if (tableMenu == null || cellPrefab == null || generator == null) return;

            // Clear old UI elements
            foreach (Transform child in tableMenu)
            {
                Destroy(child.gameObject);
            }

            UnityEngine.UI.ToggleGroup toggleGroup = tableMenu.GetComponent<UnityEngine.UI.ToggleGroup>();

            foreach (string func in availableFunctions)
            {
                string localFunc = func;
                GameObject btnObj = Instantiate(cellPrefab, tableMenu);

                // Pre-compile to check for syntax errors
                bool isValid = generator.CompileFunction(localFunc);

                TMPro.TMP_Text label = btnObj.GetComponentInChildren<TMPro.TMP_Text>();
                if (label != null)
                {
                    label.text = "f(x,y) = " + localFunc;
                    if (!isValid) label.color = Color.red; // Visual feedback for invalid functions
                }

                UnityEngine.UI.Toggle toggle = btnObj.GetComponent<UnityEngine.UI.Toggle>();
                if (toggle != null)
                {
                    if (!isValid) toggle.interactable = false;
                    if (toggleGroup != null) toggle.group = toggleGroup;
                    
                    // Clicking the toggle sets the active function
                    toggle.onValueChanged.AddListener((isOn) => {
                        if (isOn) generator.SetFunction(localFunc);
                    });
                }
            }
        }
    }
}
