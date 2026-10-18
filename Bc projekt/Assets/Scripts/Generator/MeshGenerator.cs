using UnityEngine;
using System;

namespace Assets.Scripts.Generator
{
    /// <summary>
    /// Static utility for generating 3D meshes based on mathematical functions.
    /// Includes algorithms for boundary detection and asymptote cleanup.
    /// </summary>
    public class MeshGenerator
    {
        private const int BINARY_SEARCH_ITERATIONS = 10;
        private const float INVALID_HEIGHT_MARKER = 999f;
        private const int INVALID_DIRECTION_MARKER = 99;
        private const int TRIANGLES_PER_QUAD = 6;
        private const float ASYMPTOTE_THRESHOLD = 0.1f;
        private const float HEIGHT_VALIDATION_MULTIPLIER = 1.5f;

        /// <summary>
        /// Orchestrates the entire mesh generation process.
        /// </summary>
        public static void DrawGraph(Mesh mesh, Func<float, float, float[], float> f, float[] parameterValues, 
            float paramA, float paramB, float paramC, int resolution, 
            float Xmin, float Xmax, float Zmin, float Zmax, 
            float maxHeight, float maxDifference, float scale,
            out float MathMidX, out float MathMidZ, out float finalScale)
        {
            int rowLength = resolution + 1;
            float limitY = maxHeight / 2f;

            // Define lambda for Y calculation used in multiple stages
            Func<float, float, float> GetY = (x, z) => f(x * paramA, z * paramB, parameterValues) + paramC;

            // 1. Calculate raw vertices
            Vector3[] rawVertices = GenerateVertexGrid(resolution, Xmin, Xmax, Zmin, Zmax, GetY);

            // 2. Refine boundaries and handle asymptotes
            int[] dirX, dirZ;
            Vector3[] vertices = RefineBoundaries(resolution, rawVertices, limitY, GetY, out dirX, out dirZ);

            // 3. Create triangle connectivity
            int triangleCount;
            int[] triangles = TriangulateGrid(resolution, vertices, dirX, dirZ, limitY, maxDifference, GetY, out triangleCount);

            // 4. Final normalization and centering
            NormalizeAndFinalize(mesh, vertices, triangles, triangleCount, Xmin, Xmax, Zmin, Zmax, scale, out MathMidX, out MathMidZ, out finalScale);
        }

        private static Vector3[] GenerateVertexGrid(int resolution, float Xmin, float Xmax, float Zmin, float Zmax, Func<float, float, float> GetY)
        {
            int rowLength = resolution + 1;
            Vector3[] rawVertices = new Vector3[rowLength * rowLength];
            float stepX = (Xmax - Xmin) / resolution;
            float stepZ = (Zmax - Zmin) / resolution;

            for (int z = 0; z <= resolution; z++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    int index = z * rowLength + x;
                    float realX = Xmin + (x * stepX);
                    float realZ = Zmin + (z * stepZ);
                    rawVertices[index] = new Vector3(realX, GetY(realX, realZ), realZ);
                }
            }
            return rawVertices;
        }

        private static Vector3[] RefineBoundaries(int resolution, Vector3[] rawVertices, float limitY, Func<float, float, float> GetY, out int[] dirX, out int[] dirZ)
        {
            int rowLength = resolution + 1;
            Vector3[] vertices = new Vector3[rowLength * rowLength];
            dirX = new int[rowLength * rowLength];
            dirZ = new int[rowLength * rowLength];

            Func<float, float, bool> IsValid = (y, limit) => !float.IsNaN(y) && !float.IsInfinity(y) && Mathf.Abs(y) <= limit;

            for (int z = 0; z <= resolution; z++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    int current = z * rowLength + x;
                    Vector3 pt = rawVertices[current];
                    bool isCurrentInvalid = !IsValid(pt.y, limitY);

                    if (isCurrentInvalid)
                    {
                        Vector3? goodNeighbor = null;
                        int neighborDirX = 0, neighborDirZ = 0;

                        int left = current - 1, right = current + 1;
                        int back = current - rowLength, fwd = current + rowLength;

                        if (x > 0 && IsValid(rawVertices[left].y, limitY)) { goodNeighbor = rawVertices[left]; neighborDirX = -1; }
                        else if (x < resolution && IsValid(rawVertices[right].y, limitY)) { goodNeighbor = rawVertices[right]; neighborDirX = 1; }
                        else if (z > 0 && IsValid(rawVertices[back].y, limitY)) { goodNeighbor = rawVertices[back]; neighborDirZ = -1; }
                        else if (z < resolution && IsValid(rawVertices[fwd].y, limitY)) { goodNeighbor = rawVertices[fwd]; neighborDirZ = 1; }

                        if (goodNeighbor != null)
                        {
                            Vector2 good2D = new Vector2(goodNeighbor.Value.x, goodNeighbor.Value.z);
                            Vector2 bad2D = new Vector2(pt.x, pt.z);
                            float prevY = goodNeighbor.Value.y;

                            for (int i = 0; i < BINARY_SEARCH_ITERATIONS; i++)
                            {
                                Vector2 mid = (good2D + bad2D) / 2f;
                                float midY = GetY(mid.x, mid.y);
                                if (IsValid(midY, limitY) && (prevY * midY >= 0)) good2D = mid;
                                else bad2D = mid;
                            }
                            pt.x = good2D.x; pt.z = good2D.y; pt.y = GetY(pt.x, pt.z);
                            dirX[current] = neighborDirX; dirZ[current] = neighborDirZ;
                        }
                        else dirX[current] = INVALID_DIRECTION_MARKER;
                    }

                    if (float.IsNaN(pt.y) || float.IsInfinity(pt.y)) vertices[current] = new Vector3(pt.x, INVALID_HEIGHT_MARKER, pt.z);
                    else vertices[current] = new Vector3(pt.x, Mathf.Clamp(pt.y, -limitY, limitY), pt.z);
                }
            }
            return vertices;
        }

        private static int[] TriangulateGrid(int resolution, Vector3[] vertices, int[] dirX, int[] dirZ, float limitY, float maxDifference, Func<float, float, float> GetY, out int triangleCount)
        {
            int rowLength = resolution + 1;
            int[] triangles = new int[resolution * resolution * TRIANGLES_PER_QUAD];
            int t = 0;

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int current = z * rowLength + x;
                    int right = current + 1, up = current + rowLength, upRight = up + 1;

                    float y0 = vertices[current].y, y1 = vertices[right].y;
                    float y2 = vertices[up].y, y3 = vertices[upRight].y;

                    if (y0 == INVALID_HEIGHT_MARKER || y1 == INVALID_HEIGHT_MARKER || y2 == INVALID_HEIGHT_MARKER || y3 == INVALID_HEIGHT_MARKER) continue;
                    if (dirX[current] == INVALID_DIRECTION_MARKER || dirX[right] == INVALID_DIRECTION_MARKER || dirX[up] == INVALID_DIRECTION_MARKER || dirX[upRight] == INVALID_DIRECTION_MARKER) continue;

                    if (dirX[current] == -1 || dirZ[current] == -1) continue;
                    if (dirX[right] == 1 || dirZ[right] == -1) continue;
                    if (dirX[up] == -1 || dirZ[up] == 1) continue;
                    if (dirX[upRight] == 1 || dirZ[upRight] == 1) continue;

                    float maxH = Mathf.Max(y0, y1, y2, y3), minH = Mathf.Min(y0, y1, y2, y3);
                    if ((maxH - minH) >= maxDifference) continue;
                    if (maxH >= limitY - ASYMPTOTE_THRESHOLD && minH <= -limitY + ASYMPTOTE_THRESHOLD) continue;

                    float midX = (vertices[current].x + vertices[right].x + vertices[up].x + vertices[upRight].x) / 4f;
                    float midZ = (vertices[current].z + vertices[right].z + vertices[up].z + vertices[upRight].z) / 4f;
                    float midY = GetY(midX, midZ);

                    if (float.IsNaN(midY) || float.IsInfinity(midY) || Mathf.Abs(midY) > limitY * HEIGHT_VALIDATION_MULTIPLIER) continue;

                    triangles[t + 0] = current; triangles[t + 1] = up; triangles[t + 2] = right;
                    triangles[t + 3] = right; triangles[t + 4] = up; triangles[t + 5] = upRight;
                    t += TRIANGLES_PER_QUAD;
                }
            }
            triangleCount = t;
            return triangles;
        }

        private static void NormalizeAndFinalize(Mesh mesh, Vector3[] vertices, int[] triangles, int triangleCount, float Xmin, float Xmax, float Zmin, float Zmax, float scale, out float MathMidX, out float MathMidZ, out float finalScale)
        {
            MathMidX = (Xmax + Xmin) / 2f;
            MathMidZ = (Zmax + Zmin) / 2f;
            float maxMathDim = Mathf.Max(Xmax - Xmin, Zmax - Zmin);
            finalScale = scale / maxMathDim;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].y != INVALID_HEIGHT_MARKER)
                {
                    vertices[i] = new Vector3((vertices[i].x - MathMidX) * finalScale, vertices[i].y * finalScale, (vertices[i].z - MathMidZ) * finalScale);
                }
                else vertices[i] = Vector3.zero;
            }

            mesh.Clear();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            int[] realtri = new int[triangleCount];
            Array.Copy(triangles, realtri, triangleCount);
            mesh.triangles = realtri;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
