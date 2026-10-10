using UnityEngine;
using WakeTheWalls.Interaction;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Makes every floating bloom of a <see cref="Mural02Experience"/> tappable: tap one while it hovers
    /// and it flies back into the wall where it grew from.
    /// </summary>
    public class BloomTapInteraction : MonoBehaviour
    {
        [SerializeField] Mural02Experience experience;

        void Start()
        {
            foreach (FloatingBloom bloom in experience.Blooms)
            {
                var hit = bloom.gameObject.AddComponent<SphereCollider>();
                hit.isTrigger = true;
                hit.radius = 0.6f;
                FloatingBloom target = bloom;
                bloom.gameObject.AddComponent<Interactable>().Tapped.AddListener(() => SendHome(target));
            }
        }

        void SendHome(FloatingBloom bloom)
        {
            if (experience.IsRunning && bloom.IsOut) bloom.GoHome();
        }
    }
}
