using UnityEngine;

namespace WakeTheWalls.Murals
{
    public static class TapArea
    {
        public static void Fit(BoxCollider box, Rect area, Vector2 muralSize)
        {
            box.isTrigger = true;
            box.center = Centre(area, muralSize);
            box.size = new Vector3(area.width * muralSize.x, area.height * muralSize.y, 0.06f);
        }

        public static Vector3 Centre(Rect area, Vector2 muralSize) =>
            new Vector3((area.center.x - 0.5f) * muralSize.x, (area.center.y - 0.5f) * muralSize.y, -0.03f);
    }
}
