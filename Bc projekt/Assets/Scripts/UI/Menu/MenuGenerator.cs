using UnityEngine;
using Assets.Scripts.Generator;

namespace Assets.Scripts.UI.Menu
{
    /// <summary>
    /// Orchestrator for dynamic 3D menu panels.
    /// Coordinates parameter sliders and domain adjustment menus.
    /// </summary>
    public class MenuGenerator : MonoBehaviour
    {
        public ParametrMenuHandler parametrHandler;
        public DomainMenuHandler domainHandler;

        /// <summary>
        /// Initializes the menus with a reference to the central Generator.
        /// </summary>
        public void Initialize(Generator.Generator generator)
        {
            if (parametrHandler != null)
            {
                // Subscribe to function changes to re-generate sliders
                generator.OnFunctionLoaded += (func) => parametrHandler.GenerateSliders(generator);
            }
            if (domainHandler != null)
            {
                // Initial domain config
                domainHandler.ConfigDomainMenu(generator);
            }
        }
    }
}
