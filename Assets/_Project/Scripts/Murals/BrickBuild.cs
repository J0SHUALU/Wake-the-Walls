using System.Collections;
using UnityEngine;
using WakeTheWalls.Rig;

namespace WakeTheWalls.Murals
{
    // Rebuilds a wall layer brick by brick when the mural wakes, then hands over to a light sweep along it.
    public class BrickBuild : MonoBehaviour
    {
        static readonly int ProgressId = Shader.PropertyToID("_Progress");

        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("The wall layer from the layers manifest.")]
        [SerializeField] string element = "wall";
        [Tooltip("Material using the WakeTheWalls/BrickBuild shader.")]
        [SerializeField] Material brickMaterial;
        [SerializeField, Min(0f)] float startDelay = 1.5f;
        [SerializeField, Range(2f, 15f)] float buildSeconds = 6f;
        [Tooltip("Plays once the wall stands, for example a light running along it.")]
        [SerializeField] MaskSweep afterBuild;

        MuralLayer layer;
        MaterialPropertyBlock block;
        Coroutine build;
        float progress;
        bool built, wasRunning;

        void Update()
        {
            if (experience == null || experience.Rig == null || !experience.Rig.IsBuilt) return;
            if (layer == null && !Find()) return;

            bool running = experience.IsRunning;
            if (running && !wasRunning && !built) Rebuild(startDelay);
            if (!running && wasRunning && build != null) { StopCoroutine(build); build = null; }
            wasRunning = running;
        }

        // Knocks the wall down and lays it again. Also used by a tap.
        public void Rebuild() => Rebuild(0f);

        public void ResetWall()
        {
            if (build != null) StopCoroutine(build);
            build = null;
            built = false;
            SetProgress(0f);
        }

        void Rebuild(float delay)
        {
            if (layer == null || !experience.IsRunning) return;
            if (build != null) StopCoroutine(build);
            build = StartCoroutine(Build(delay));
        }

        IEnumerator Build(float delay)
        {
            built = false;
            SetProgress(0f);
            if (delay > 0f) yield return new WaitForSeconds(delay);
            for (float t = 0f; t < buildSeconds; t += Time.deltaTime)
            {
                SetProgress(t / buildSeconds);
                yield return null;
            }
            SetProgress(1f);
            built = true;
            build = null;
            if (afterBuild != null) afterBuild.Play();
        }

        bool Find()
        {
            if (!experience.Rig.TryGetLayer(element, out layer) || layer.Renderer == null) { layer = null; return false; }
            if (brickMaterial != null) layer.Renderer.sharedMaterial = brickMaterial;
            block = new MaterialPropertyBlock();
            SetProgress(0f);
            return true;
        }

        void SetProgress(float value)
        {
            progress = value;
            if (layer == null) return;
            layer.Renderer.GetPropertyBlock(block);
            block.SetFloat(ProgressId, progress);
            layer.Renderer.SetPropertyBlock(block);
        }
    }
}
