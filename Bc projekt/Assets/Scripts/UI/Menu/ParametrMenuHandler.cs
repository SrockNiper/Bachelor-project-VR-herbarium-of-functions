using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Generator;

namespace Assets.Scripts.UI.Menu
{
    /// <summary>
    /// Generates and manages UI Sliders for mathematical parameters (a, b, c...).
    /// Automatically adapts to the number of variables found in the compiled function.
    /// </summary>
    public class ParametrMenuHandler : MonoBehaviour
    {
        [Header("UI References")]
        public Transform container;
        public GameObject sliderPrefab;
        
        [Header("Slider Settings")]
        public float defaultMin = -5f;
        public float defaultMax = 5f;

        /// <summary>
        /// Instantiates sliders for each unique parameter in the current function.
        /// </summary>
        public void GenerateSliders(Generator.Generator generator)
        {
            if (container == null || sliderPrefab == null) return;

            // Clear old sliders
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            // Create a slider for each variable (parameter)
            for (int i = 0; i < generator.parameterNames.Count; i++)
            {
                int localIndex = i;
                GameObject sliderObj = Instantiate(sliderPrefab, container);
                sliderObj.SetActive(true);

                TMP_Text label = sliderObj.GetComponentInChildren<TMP_Text>();
                Slider sliderComponent = sliderObj.GetComponentInChildren<Slider>();

                if (label != null)
                {
                    label.text = $"{generator.parameterNames[i]} = {generator.parameterValues[i]:F2}";
                }

                if (sliderComponent != null)
                {
                    sliderComponent.minValue = defaultMin;
                    sliderComponent.maxValue = defaultMax;
                    sliderComponent.value = generator.parameterValues[i];

                    // Real-time update of graph when sliding
                    sliderComponent.onValueChanged.AddListener((value) =>
                    {
                        generator.parameterValues[localIndex] = value;
                        if (label != null)
                        {
                            label.text = $"{generator.parameterNames[localIndex]} = {value:F2}";
                        }
                        generator.RefreshGraph();
                    });
                }
            }
        }
    }
}
