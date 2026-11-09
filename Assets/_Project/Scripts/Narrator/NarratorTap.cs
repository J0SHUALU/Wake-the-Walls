using UnityEngine;
using WakeTheWalls.Interaction;

namespace WakeTheWalls.Narrator
{
    public class NarratorTap : Interactable
    {
        [SerializeField] Narrator narrator;

        public override void Tap()
        {
            if (!IsInteractable) return;
            if (narrator == null) narrator = GetComponent<Narrator>();
            if (narrator != null) narrator.Tap();
            base.Tap();
        }
    }
}
