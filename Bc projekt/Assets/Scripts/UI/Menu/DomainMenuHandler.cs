using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Assets.Scripts.Generator;

namespace Assets.Scripts.UI.Menu
{
    /// <summary>
    /// Manages UI Steppers for adjusting the function's domain (Xmin, Xmax, Zmin, Zmax).
    /// Includes a "Uniform" setting for symmetrical scaling.
    /// </summary>
    public class DomainMenuHandler : MonoBehaviour
    {
        [Header("UI References")]
        public Transform container;
        public GameObject stepperPrefab;
        
        private static readonly float[] steps = { 0.1f, 0.5f, 1f, 2f, 5f, 10f, 20f, 100f };
        private byte currentStepIndex = 2;
        
        /// <summary>Current increment value for the steppers.</summary>
        public float currentStep = 1f;

        /// <summary>
        /// Configures the domain adjustment menu.
        /// </summary>
        public void ConfigDomainMenu(Generator.Generator generator)
        {
            if (container == null || stepperPrefab == null || generator == null) return;

            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            // Step setting (controls how much other steppers increment)
            CreateStepper("Krok", () => currentStep, (val) => {
                if (val > steps[currentStepIndex] && currentStepIndex < steps.Length - 1) currentStepIndex++;
                else if (val < steps[currentStepIndex] && currentStepIndex > 0) currentStepIndex--;
                currentStep = steps[currentStepIndex];
            }, 1f);

            // Axis domain limits
            CreateStepper("X min", () => generator.Xmin, (val) => {
                if (val < generator.Xmax) { generator.Xmin = val; generator.RefreshGraph(); }
            });
            CreateStepper("X max", () => generator.Xmax, (val) => {
                if (val > generator.Xmin) { generator.Xmax = val; generator.RefreshGraph(); }
            });
            CreateStepper("Z min", () => generator.Zmin, (val) => {
                if (val < generator.Zmax) { generator.Zmin = val; generator.RefreshGraph(); }
            });
            CreateStepper("Z max", () => generator.Zmax, (val) => {
                if (val > generator.Zmin) { generator.Zmax = val; generator.RefreshGraph(); }
            });

            // Uniform scale (adjusts all axes at once)
            CreateStepper("Uniform", () => generator.Xmax, (val) => {
                float newSize = Mathf.Max(1f, val);
                generator.Xmax = newSize; generator.Xmin = -newSize;
                generator.Zmax = newSize; generator.Zmin = -newSize;
                generator.RefreshGraph();
                ConfigDomainMenu(generator); // Rebuild to update all numbers
            });
        }

        /// <summary>
        /// Internal helper to create a +/- button stepper.
        /// </summary>
        private void CreateStepper(string labelText, Func<float> getValue, Action<float> setValue, float? customStep = null)
        {
            GameObject stepperObj = Instantiate(stepperPrefab, container);
            stepperObj.SetActive(true);

            TMP_Text nameLabel = stepperObj.transform.Find("AxisLabel")?.GetComponent<TMP_Text>();
            TMP_Text valueLabel = stepperObj.transform.Find("Value")?.GetComponent<TMP_Text>();
            Button minusBtn = stepperObj.transform.Find("MinusButton")?.GetComponent<Button>();
            Button plusBtn = stepperObj.transform.Find("PlusButton")?.GetComponent<Button>();

            if (nameLabel != null) nameLabel.text = labelText;

            Action updateText = () => { if (valueLabel != null) valueLabel.text = getValue().ToString("F1"); };
            updateText();

            if (minusBtn != null)
            {
                minusBtn.onClick.AddListener(() =>
                {
                    float step = customStep ?? currentStep;
                    setValue(getValue() - step);
                    updateText();
                });
            }

            if (plusBtn != null)
            {
                plusBtn.onClick.AddListener(() =>
                {
                    float step = customStep ?? currentStep;
                    setValue(getValue() + step);
                    updateText();
                });
            }
        }
    }
}
