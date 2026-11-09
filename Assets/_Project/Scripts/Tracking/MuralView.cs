using UnityEngine;
using WakeTheWalls.Murals;

namespace WakeTheWalls.Tracking
{
    public static class MuralView
    {
        const int Grid = 5;
        const float MinShareOnScreen = 0.2f;

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
                if (Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y) return true;
            }

            int onScreen = 0;
            for (int ix = 0; ix < Grid; ix++)
            {
                for (int iy = 0; iy < Grid; iy++)
                {
                    var point = new Vector3(Mathf.Lerp(-half.x, half.x, ix / (Grid - 1f)), Mathf.Lerp(-half.y, half.y, iy / (Grid - 1f)), 0f);
                    Vector3 view = cam.WorldToViewportPoint(mural.TransformPoint(point));
                    if (view.z > 0f && view.x >= 0f && view.x <= 1f && view.y >= 0f && view.y <= 1f) onScreen++;
                }
            }
            return onScreen >= MinShareOnScreen * Grid * Grid;
        }
    }
}
