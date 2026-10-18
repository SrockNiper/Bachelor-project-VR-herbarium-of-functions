using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Handles toggling of VR menus (Parameters and Domain) via input actions.
    /// Ensures only one menu is active at a time to prevent UI clutter.
    /// </summary>
    public class HandMenuHandler : MonoBehaviour
    {
        [Header("Input Actions")]
        public InputActionReference paramsMenuAction;
        public InputActionReference domainMenuAction;

        [Header("Menu Panels")]
        public GameObject parametersPanel;
        public GameObject domainPanel;

        private void Start()
        {
            // Ensure menus are hidden on startup
            if (parametersPanel != null) parametersPanel.SetActive(false);
            if (domainPanel != null) domainPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (paramsMenuAction != null) paramsMenuAction.action.performed += ToggleParamsMenu;
            if (domainMenuAction != null) domainMenuAction.action.performed += ToggleDomainMenu;
        }

        private void OnDisable()
        {
            if (paramsMenuAction != null) paramsMenuAction.action.performed -= ToggleParamsMenu;
            if (domainMenuAction != null) domainMenuAction.action.performed -= ToggleDomainMenu;
        }

        /// <summary>
        /// Toggles the Parameter menu visibility.
        /// </summary>
        private void ToggleParamsMenu(InputAction.CallbackContext context)
        {
            if (parametersPanel != null) parametersPanel.SetActive(!parametersPanel.activeSelf);
            // Hide the other menu if it was open
            if (domainPanel != null && domainPanel.activeSelf) domainPanel.SetActive(false);
        }

        /// <summary>
        /// Toggles the Domain menu visibility.
        /// </summary>
        private void ToggleDomainMenu(InputAction.CallbackContext context)
        {
            if (domainPanel != null) domainPanel.SetActive(!domainPanel.activeSelf);
            // Hide the other menu if it was open
            if (parametersPanel != null && parametersPanel.activeSelf) parametersPanel.SetActive(false);
        }
    }
}
