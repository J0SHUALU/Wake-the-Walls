using UnityEngine;

namespace WakeTheWalls.VFX
{
    /// <summary>
    /// Builds a ring (torus) mesh lying flat in the XZ plane, centred on the origin, in metres.
    /// Used for 3D rings that leave a mural, drawn with the Painterly material.
    /// </summary>
    public static class RingMesh
    {
        /// <summary>Creates a ring mesh.</summary>
        /// <param name="radius">Distance from the centre to the middle of the band, in metres.</param>
        /// <param name="thickness">Radius of the band itself, in metres.</param>
        /// <param name="segments">Steps around the ring.</param>
        /// <param name="sides">Steps around the band.</param>
        public static Mesh Build(float radius, float thickness, int segments = 64, int sides = 10)
        {
            var vertices = new Vector3[(segments + 1) * (sides + 1)];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            for (int i = 0; i <= segments; i++)
            {
                float u = (float)i / segments;
                float a = u * Mathf.PI * 2f;
                var around = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                for (int j = 0; j <= sides; j++)
                {
                    float v = (float)j / sides;
                    float b = v * Mathf.PI * 2f;
                    Vector3 normal = around * Mathf.Cos(b) + Vector3.up * Mathf.Sin(b);
                    int k = i * (sides + 1) + j;
                    vertices[k] = around * radius + normal * thickness;
                    normals[k] = normal;
                    uvs[k] = new Vector2(u, v);
                }
            }

            var triangles = new int[segments * sides * 6];
            int t = 0;
            for (int i = 0; i < segments; i++)
                for (int j = 0; j < sides; j++)
                {
                    int a = i * (sides + 1) + j, b = a + sides + 1;
                    triangles[t++] = a; triangles[t++] = a + 1; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = a + 1; triangles[t++] = b + 1;
                }

            var mesh = new Mesh { name = "Ring", vertices = vertices, normals = normals, uv = uvs, triangles = triangles };
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
