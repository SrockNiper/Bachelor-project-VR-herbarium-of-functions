using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Generator
{
    /// <summary>
    /// Main component for function visualization. 
    /// Acts as a Facade that manages the current mathematical state and coordinates mesh/KSS generation.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Generator : MonoBehaviour
    {
        [Header("System References")]
        [Tooltip("Reference to the centralized ErrorHandler.")]
        public Assets.Scripts.Core.ErrorHandler errorHandler;

        [Header("Resolution settings")]
        [Tooltip("Number of vertices per axis (resolution^2 total).")]
        [Range(10, 1000)] public int resolution = 400;
        
        [Tooltip("Absolute maximum height allowed for vertices.")]
        public float maxHeight = 100f;
        
        [Tooltip("Max vertical difference between adjacent vertices before a triangle is discarded (prevents asymptote lines).")]
        public float maxDifference = 5f;
        
        private Mesh mesh;
        private Func<float, float, float[], float> f;
        
        [Tooltip("Global scale of the generated graph.")]
        public float scale = 1f;

        [Header("Domain of a function")]
        public float Xmin = -5f;
        public float Xmax = 5f;
        public float Zmin = -5f;
        public float Zmax = 5f;

        [Header("Live Parameters")]
        /// <summary>Names of variables detected in the function string.</summary>
        public List<string> parameterNames = new List<string>();
        /// <summary>Current values of live parameters.</summary>
        public float[] parameterValues = new float[5];

        [Header("Fallback Parameters")]
        [Range(-5f, 5f)] public float paramA = 1f;
        [Range(-5f, 5f)] public float paramB = 1f;
        [Range(-5f, 5f)] public float paramC = 1f;
        
        private struct CompiledData
        {
            public Func<float, float, float[], float> function;
            public List<string> paramNames;
        }
        
        private Dictionary<string, CompiledData> compiledCache;
        
        /// <summary>Event fired when a new function is successfully loaded and compiled.</summary>
        public event Action<string> OnFunctionLoaded;

        private void Start()
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            compiledCache = new Dictionary<string, CompiledData>();
        }

        /// <summary>
        /// Compiles and draws a new mathematical function.
        /// </summary>
        /// <param name="func">Text representation of the function (e.g. "sin(x*a)").</param>
        public void SetFunction(string func)
        {
            if (CompileFunction(func))
            {
                DrawGraph();
                OnFunctionLoaded?.Invoke(func);
            }
        }

        /// <summary>
        /// Forces a redraw of the current graph (e.g. after parameter or domain change).
        /// </summary>
        public void RefreshGraph()
        {
            DrawGraph();
        }

        /// <summary>
        /// Compiles a function string into an executable delegate.
        /// Uses caching to improve performance for repeated functions.
        /// </summary>
        /// <param name="func">The function string.</param>
        /// <returns>True if compilation was successful.</returns>
        public bool CompileFunction(string func)
        {
            try
            {
                if (compiledCache.TryGetValue(func, out CompiledData data))
                {
                    f = data.function;
                    parameterNames = data.paramNames;
                }
                else
                {
                    var result = MathPreprocess.Compile(func);
                    f = result.Item1;
                    parameterNames = result.Item2;

                    compiledCache[func] = new CompiledData
                    {
                        function = f,
                        paramNames = parameterNames
                    };
                }

                // Reset parameter values if the signature changed
                if (parameterValues == null || parameterValues.Length != parameterNames.Count)
                {
                    parameterValues = new float[parameterNames.Count];
                    for (int i = 0; i < parameterValues.Length; i++)
                    {
                        parameterValues[i] = 1f;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                // Route the error through the ErrorHandler if available
                if (errorHandler != null)
                {
                    errorHandler.LogError("Error compiling function", e);
                }
                else
                {
                    Debug.LogError($"Error compiling function: {e.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Triggers the actual mesh and coordinate system generation.
        /// </summary>
        private void DrawGraph()
        {
            if (f == null) return;

            MeshGenerator.DrawGraph(mesh, f, parameterValues, paramA, paramB, paramC, resolution, 
                Xmin, Xmax, Zmin, Zmax, maxHeight, maxDifference, scale, 
                out float mathMidX, out float mathMidZ, out float finalScale);

            // Update collider to match the new mesh bounds
            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.center = mesh.bounds.center;
                collider.size = mesh.bounds.size; 
            }

            transform.localPosition = Vector3.zero;
            
            // Draw axes and labels
            KSSGenerator.DrawKSS(transform, mesh, Xmin, Xmax, Zmin, Zmax, maxHeight, mathMidX, mathMidZ, finalScale);
        }
    }
}
