using UnityEngine;
using TMPro;
using System.Collections;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Displays error messages directly on the 2D computer monitor (Screen Space UI).
    /// Used for debugging or notifying users about syntax errors in real-time.
    /// </summary>
    public class DesktopConsole : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The TextMeshPro text component where messages will be displayed.")]
        public TMP_Text errorText;

        [Header("Settings")]
        [Tooltip("How many seconds the error message remains visible on the screen.")]
        public float displayDuration = 5f;

        private void Start()
        {
            // Clear text on startup
            if (errorText != null) 
            {
                errorText.text = "";
            }
        }

        /// <summary>
        /// Triggers the display of an error message on the UI.
        /// </summary>
        /// <param name="message">The specific error text to show.</param>
        public void ShowError(string message)
        {
            if (errorText == null) return;

            // Interrupt any previous countdown and refresh the message
            StopAllCoroutines();
            StartCoroutine(DisplayRoutine(message));
        }

        /// <summary>
        /// Coroutine that handles the visual display and timeout of the message.
        /// </summary>
        /// <param name="message">Message string to display.</param>
        private IEnumerator DisplayRoutine(string message)
        {
            // Set formatted text with red syntax highlight
            errorText.text = $"<color=red>Syntax Error:</color> {message}";
            
            // Wait for the specified duration
            yield return new WaitForSeconds(displayDuration);
            
            // Clear text after timeout
            errorText.text = "";
        }
    }
}
