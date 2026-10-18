using Assets.Scripts.UI;
using System;
using UnityEngine;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Central point for handling and logging errors across the application.
    /// Manages internal Unity logging and triggers UI notifications.
    /// </summary>
    public class ErrorHandler : MonoBehaviour
    {
        [Header("UI Reference")]
        [Tooltip("Reference to the DesktopConsole component responsible for displaying messages on the 2D monitor.")]
        public DesktopConsole desktopConsole;

        /// <summary>
        /// Logs an error message to the Unity Console and displays it on the desktop UI.
        /// </summary>
        /// <param name="message">Context-specific message or prefix.</param>
        /// <param name="e">Optional Exception details providing the specific error cause.</param>
        public void LogError(string message, Exception e = null)
        {
            // Format the internal debug message
            string fullMessage = e != null ? $"{message}: {e.Message}" : message;

            // Standard Unity console logging
            Debug.LogError(fullMessage);

            // Notify the desktop UI system
            if (desktopConsole != null)
            {
                // Prioritize the exception message for the UI if available, otherwise use the prefix
                string uiMessage = e != null ? e.Message : message;
                desktopConsole.ShowError(uiMessage);
            }
        }
    }
}
