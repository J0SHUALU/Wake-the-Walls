using UnityEngine;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    public static class MuralView
    {
        public static bool IsInView(MuralExperience experience)
        {
            var cam = Camera.main;
            if (cam == null || experience == null || experience.Data == null) return false;
            Transform mural = experience.transform;
            Vector2 half = experience.Data.PhysicalSizeMeters * 0.5f;

            var wall = new Plane(mural.forward, mural.position);
            var look = new Ray(cam.transform.position, cam.transform.forward);
            if (wall.Raycast(look, out float hit))
            {
                Vector3 local = mural.InverseTransformPoint(look.GetPoint(hit));
                if (Mathf.Abs(local.x) <= half.x * 1.2f && Mathf.Abs(local.y) <= half.y * 1.2f) return true;
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 point = i == 4 ? Vector3.zero : new Vector3(i % 2 == 0 ? -half.x : half.x, i < 2 ? -half.y : half.y, 0f);
                Vector3 view = cam.WorldToViewportPoint(mural.TransformPoint(point));
                if (view.z > 0f && view.x > -0.05f && view.x < 1.05f && view.y > -0.05f && view.y < 1.05f) return true;
            }
            return false;
        }
    }
}
