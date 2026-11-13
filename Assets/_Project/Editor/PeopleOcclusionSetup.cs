using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace WakeTheWalls.EditorTools
{
    // Adds the PeopleCut full screen pass to the URP renderer.
    public static class PeopleOcclusionSetup
    {
        const string RendererPath = "Assets/Settings/URP-Performant-Renderer.asset";
        const string MaterialPath = "Assets/_Project/Shaders/PeopleCut.mat";
        const string FeatureName = "PeopleCut";

        [MenuItem("Wake the Walls/Set Up People Occlusion")]
        public static void Run()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Hidden/WakeTheWalls/PeopleCut")) { name = "PeopleCut" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            FullScreenPassRendererFeature feature = null;
            foreach (ScriptableRendererFeature existing in data.rendererFeatures)
                if (existing != null && existing.name == FeatureName) feature = existing as FullScreenPassRendererFeature;

            if (feature == null)
            {
                feature = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
                feature.name = FeatureName;
                AssetDatabase.AddObjectToAsset(feature, data);
                data.rendererFeatures.Add(feature);
            }

            feature.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;
            feature.fetchColorBuffer = true;
            feature.requirements = ScriptableRenderPassInput.None;
            feature.passMaterial = material;
            feature.passIndex = 0;
            feature.SetActive(true);

            EditorUtility.SetDirty(feature);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(RendererPath);
            Debug.Log("People occlusion pass is on the renderer.");
        }
    }
}
