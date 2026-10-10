using UnityEngine;

namespace WakeTheWalls.VFX
{
    /// <summary>
    /// Builds a simple 3D flower out of curved petals around a round centre, so blooms can leave
    /// the wall without needing a modelled asset. Pair it with the Painterly material.
    /// </summary>
    /// <remarks>
    /// The flower faces -Z (towards the viewer). Each petal sits on its own pivot, so opening and
    /// closing is a rotation of those pivots. Size is 1 unit across when fully open.
    /// </remarks>
    public static class BloomBuilder
    {
        static Mesh petalMesh;

        /// <summary>Creates the petals and centre under <paramref name="parent"/>.</summary>
        /// <param name="parent">The bloom's root transform.</param>
        /// <param name="petals">Number of petals, 5 to 8 looks right.</param>
        /// <param name="material">Usually the Painterly material.</param>
        /// <param name="petalPivots">One pivot per petal, rotate around local X to open or close.</param>
        /// <param name="centre">Renderer of the round centre.</param>
        /// <returns>Renderers of the petals.</returns>
        public static Renderer[] Build(Transform parent, int petals, Material material, out Transform[] petalPivots, out Renderer centre)
        {
            petals = Mathf.Max(3, petals);
            petalPivots = new Transform[petals];
            var renderers = new Renderer[petals];
            for (int i = 0; i < petals; i++)
            {
                var pivot = new GameObject("Petal" + i).transform;
                pivot.SetParent(parent, false);
                pivot.localRotation = Quaternion.Euler(0f, 0f, i * 360f / petals);
                var petal = new GameObject("Mesh");
                petal.transform.SetParent(pivot, false);
                petal.transform.localScale = new Vector3(1f, 0.5f, 1f);
                petal.AddComponent<MeshFilter>().sharedMesh = GetPetalMesh();
                renderers[i] = petal.AddComponent<MeshRenderer>();
                renderers[i].sharedMaterial = material;
                petalPivots[i] = pivot;
            }

            var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(dot.GetComponent<Collider>());
            dot.name = "Centre";
            dot.transform.SetParent(parent, false);
            dot.transform.localPosition = new Vector3(0f, 0f, -0.03f);
            dot.transform.localScale = new Vector3(0.24f, 0.24f, 0.14f);
            centre = dot.GetComponent<Renderer>();
            centre.sharedMaterial = material;
            return renderers;
        }

        /// <summary>Tilts every petal: 0 lies flat and fully open, about 75 is a closed bud.</summary>
        public static void SetOpening(Transform[] petalPivots, float closedDegrees)
        {
            for (int i = 0; i < petalPivots.Length; i++)
            {
                float around = i * 360f / petalPivots.Length;
                petalPivots[i].localRotation = Quaternion.Euler(0f, 0f, around) * Quaternion.Euler(-closedDegrees, 0f, 0f);
            }
        }

        // One petal along +Y from the centre: rounded, slightly cupped, with faces on both sides.
        static Mesh GetPetalMesh()
        {
            if (petalMesh != null) return petalMesh;
            const int Segments = 6;
            var vertices = new Vector3[(Segments + 1) * 3 * 2];
            var triangles = new int[Segments * 4 * 3 * 2];
            int half = (Segments + 1) * 3;
            for (int s = 0; s <= Segments; s++)
            {
                float t = s / (float)Segments;
                float width = 0.42f * Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Mathf.PI * Mathf.Lerp(0.08f, 0.97f, t))), 0.7f);
                float y = 0.06f + t * 0.44f;
                float cup = -0.05f * Mathf.Sin(Mathf.PI * t);
                for (int c = 0; c < 3; c++)
                {
                    float x = (c - 1) * width;
                    float z = cup + (c == 1 ? -0.03f : 0f);
                    vertices[s * 3 + c] = new Vector3(x, y, z);
                    vertices[half + s * 3 + c] = new Vector3(x, y, z);
                }
            }
            int k = 0;
            for (int s = 0; s < Segments; s++)
            {
                for (int c = 0; c < 2; c++)
                {
                    int a = s * 3 + c, b = a + 1, d = a + 3, e = d + 1;
                    triangles[k++] = a; triangles[k++] = d; triangles[k++] = b;
                    triangles[k++] = b; triangles[k++] = d; triangles[k++] = e;
                    triangles[k++] = half + a; triangles[k++] = half + b; triangles[k++] = half + d;
                    triangles[k++] = half + b; triangles[k++] = half + e; triangles[k++] = half + d;
                }
            }
            petalMesh = new Mesh { name = "BloomPetal", vertices = vertices, triangles = triangles };
            petalMesh.RecalculateNormals();
            petalMesh.RecalculateBounds();
            return petalMesh;
        }
    }
}
