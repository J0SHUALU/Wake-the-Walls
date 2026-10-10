using UnityEngine;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Helper for tap areas that are set as a rectangle across the mural (0 to 1, with 0,0 at the
    /// bottom left), so they fit the mural at whatever real size it is built.
    /// </summary>
    public static class TapArea
    {
        /// <summary>Fits a trigger box collider over <paramref name="area"/>, just in front of the wall.</summary>
        public static void Fit(BoxCollider box, Rect area, Vector2 muralSize)
        {
            box.isTrigger = true;
            box.center = Centre(area, muralSize);
            box.size = new Vector3(area.width * muralSize.x, area.height * muralSize.y, 0.06f);
        }

        /// <summary>Centre of the area in mural space, in metres, 3 cm in front of the wall.</summary>
        public static Vector3 Centre(Rect area, Vector2 muralSize) =>
            new Vector3((area.center.x - 0.5f) * muralSize.x, (area.center.y - 0.5f) * muralSize.y, -0.03f);
    }
}
