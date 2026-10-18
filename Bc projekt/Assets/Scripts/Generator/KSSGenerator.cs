using UnityEngine;
using TMPro;

namespace Assets.Scripts.Generator
{
    /// <summary>
    /// Static utility for drawing the Cartesian Coordinate System (KSS).
    /// Generates 3D axes (cylinders), ticks, and text labels (TMP).
    /// </summary>
    public class KSSGenerator
    {
        private const float AXIS_THICKNESS = 0.01f;
        private const int AXIS_SEGMENTS = 10;
        private const float TICK_SIZE_MULTIPLIER = 1.5f;
        private const float TICK_LENGTH_MULTIPLIER = 2f;
        private const float LABEL_FONT_SIZE = 2f;
        private const float LABEL_OFFSET_X_FACTOR = 4f;
        private const float LABEL_OFFSET_Z_FACTOR = 6f;

        /// <summary>
        /// Clears existing axes and draws a new coordinate system.
        /// </summary>
        public static void DrawKSS(Transform parent, Mesh mesh, float Xmin, float Xmax, float Zmin, float Zmax, 
            float maxHeight, float MathMidX, float MathMidZ, float finalScale)
        {
            ClearOldKSS(parent);

            GameObject kssRoot = CreateRoot(parent);

            float mathSizeX = Xmax - Xmin;
            float mathSizeZ = Zmax - Zmin;
            float floorY = mesh.bounds.min.y;
            float crossX = Mathf.Clamp(0f, Xmin, Xmax);
            float crossZ = Mathf.Clamp(0f, Zmin, Zmax);

            DrawMainAxes(kssRoot.transform, mathSizeX, mathSizeZ, maxHeight, floorY, crossX, crossZ, MathMidX, MathMidZ, finalScale);
            DrawTicksAndLabels(kssRoot.transform, Xmin, Xmax, Zmin, Zmax, floorY, crossX, crossZ, MathMidX, MathMidZ, finalScale);
        }

        private static void ClearOldKSS(Transform parent)
        {
            Transform oldKss = parent.Find("KSS_Root");
            if (oldKss != null) Object.Destroy(oldKss.gameObject);
        }

        private static GameObject CreateRoot(Transform parent)
        {
            GameObject kssRoot = new GameObject("KSS_Root");
            kssRoot.transform.SetParent(parent, false);
            return kssRoot;
        }

        private static void DrawMainAxes(Transform root, float mathSizeX, float mathSizeZ, float maxHeight, float floorY, float crossX, float crossZ, float MathMidX, float MathMidZ, float finalScale)
        {
            // X Axis (Red)
            CreateAxis("Axis_X", root, new Vector3(0, floorY, (crossZ - MathMidZ) * finalScale), 
                Quaternion.Euler(0, 0, 90), new Vector3(AXIS_THICKNESS, (mathSizeX * finalScale) / 2f, AXIS_THICKNESS), Color.red);

            // Z Axis (Blue)
            CreateAxis("Axis_Z", root, new Vector3((crossX - MathMidX) * finalScale, floorY, 0), 
                Quaternion.Euler(90, 0, 0), new Vector3(AXIS_THICKNESS, (mathSizeZ * finalScale) / 2f, AXIS_THICKNESS), Color.blue);

            // Y Axis (Green)
            CreateAxis("Axis_Y", root, new Vector3((crossX - MathMidX) * finalScale, 0, (crossZ - MathMidZ) * finalScale), 
                Quaternion.identity, new Vector3(AXIS_THICKNESS, (maxHeight * finalScale) / 2f, AXIS_THICKNESS), Color.green);
        }

        private static void DrawTicksAndLabels(Transform root, float Xmin, float Xmax, float Zmin, float Zmax, float floorY, float crossX, float crossZ, float MathMidX, float MathMidZ, float finalScale)
        {
            // X Ticks and Labels
            for (int i = 0; i <= AXIS_SEGMENTS; i++)
            {
                float t = i / (float)AXIS_SEGMENTS;
                float valX = Mathf.Lerp(Xmin, Xmax, t);
                CreateTickLabel(root, new Vector3((valX - MathMidX) * finalScale, floorY, (crossZ - MathMidZ) * finalScale), 
                    Quaternion.Euler(90, 0, 0), AXIS_THICKNESS, valX.ToString("F1"), true);
            }

            // Z Ticks and Labels
            for (int i = 0; i <= AXIS_SEGMENTS; i++)
            {
                float t = i / (float)AXIS_SEGMENTS;
                float valZ = Mathf.Lerp(Zmin, Zmax, t);
                CreateTickLabel(root, new Vector3((crossX - MathMidX) * finalScale, floorY, (valZ - MathMidZ) * finalScale), 
                    Quaternion.Euler(0, 0, 90), AXIS_THICKNESS, valZ.ToString("F1"), false);
            }
        }

        private static void CreateAxis(string name, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, Color color)
        {
            GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            axis.name = name;
            axis.transform.SetParent(parent, false);
            axis.transform.localPosition = pos;
            axis.transform.localRotation = rot;
            axis.transform.localScale = scale;
            axis.GetComponent<Renderer>().material.color = color;
            Object.Destroy(axis.GetComponent<Collider>());
        }

        private static void CreateTickLabel(Transform parent, Vector3 pos, Quaternion tickRot, float thickness, string text, bool isX)
        {
            GameObject tick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tick.transform.SetParent(parent, false);
            tick.transform.localPosition = pos;
            tick.transform.localRotation = tickRot;
            tick.transform.localScale = new Vector3(thickness * TICK_SIZE_MULTIPLIER, thickness * TICK_LENGTH_MULTIPLIER, thickness * TICK_SIZE_MULTIPLIER);
            tick.GetComponent<Renderer>().material.color = Color.white;
            Object.Destroy(tick.GetComponent<Collider>());

            GameObject txtObj = new GameObject($"Label_{(isX ? "X" : "Z")}_{text}");
            txtObj.transform.SetParent(parent, false);
            
            Vector3 offset = isX ? new Vector3(0, 0, -thickness * LABEL_OFFSET_X_FACTOR) : new Vector3(-thickness * LABEL_OFFSET_Z_FACTOR, 0, 0);
            txtObj.transform.localPosition = pos + offset;
            txtObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = LABEL_FONT_SIZE;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
    }
}
