using UnityEngine;

namespace WakeTheWalls.Murals
{
    // A few birds that circle a point on the mural, for example above a tower.
    public class CirclingBirds : MonoBehaviour
    {
        [SerializeField] LayeredMuralExperience experience;
        [Tooltip("Centre of the circle, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Vector2 centre = new Vector2(0.85f, 0.6f);
        [Tooltip("Width and height of the circle as a share of the mural width.")]
        [SerializeField] Vector2 radius = new Vector2(0.08f, 0.025f);
        [SerializeField, Range(1, 12)] int count = 5;
        [Tooltip("Laps per second.")]
        [SerializeField] float speed = 0.14f;
        [SerializeField] float wingspan = 0.07f;
        [SerializeField] float flapsPerSecond = 2.2f;
        [SerializeField] Color colour = new Color(0.12f, 0.12f, 0.14f, 1f);
        [SerializeField] float startDelay = 2.5f;

        Transform[] birds;
        Transform[,] wings;
        SpriteRenderer[] sprites;
        float time, weight, runningFor;

        void Update()
        {
            if (experience == null || experience.Rig == null || !experience.Rig.IsBuilt) return;
            if (birds == null) Build();

            runningFor = experience.IsRunning ? runningFor + Time.deltaTime : 0f;
            float target = experience.IsRunning && runningFor > startDelay ? 1f : 0f;
            weight = Mathf.MoveTowards(weight, target, Time.deltaTime);
            if (weight <= 0f && target <= 0f) { SetAlpha(0f); return; }
            time += Time.deltaTime;

            Vector2 mural = experience.Rig.MuralSize;
            Vector2 c = new Vector2((centre.x - 0.5f) * mural.x, (centre.y - 0.5f) * mural.y);
            for (int i = 0; i < birds.Length; i++)
            {
                float lap = time * speed * Mathf.PI * 2f + i * Mathf.PI * 2f / birds.Length;
                float r = 1f + 0.12f * Mathf.Sin(i * 2.1f);
                birds[i].localPosition = new Vector3(c.x + Mathf.Cos(lap) * radius.x * mural.x * r,
                    c.y + Mathf.Sin(lap) * radius.y * mural.x * r + i * 0.012f, -0.04f);
                float flap = Mathf.Sin(time * flapsPerSecond * Mathf.PI * 2f + i * 1.7f) * 35f;
                wings[i, 0].localRotation = Quaternion.Euler(0f, 0f, flap);
                wings[i, 1].localRotation = Quaternion.Euler(0f, 0f, -flap);
            }
            SetAlpha(weight);
        }

        void Build()
        {
            birds = new Transform[count];
            wings = new Transform[count, 2];
            sprites = new SpriteRenderer[count * 2];
            Sprite wing = ShapeSprites.Get(ShapeSprites.Shape.BirdWing);
            for (int i = 0; i < count; i++)
            {
                birds[i] = new GameObject("Bird").transform;
                birds[i].SetParent(experience.transform, false);
                birds[i].localScale = Vector3.one * wingspan * (0.85f + 0.3f * ((i * 0.37f) % 1f));
                for (int side = 0; side < 2; side++)
                {
                    var go = new GameObject(side == 0 ? "WingRight" : "WingLeft");
                    go.transform.SetParent(birds[i], false);
                    go.transform.localScale = new Vector3(side == 0 ? 1f : -1f, 1f, 1f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = wing;
                    sr.sortingOrder = 515;
                    wings[i, side] = go.transform;
                    sprites[i * 2 + side] = sr;
                }
            }
            SetAlpha(0f);
        }

        void SetAlpha(float a)
        {
            if (sprites == null) return;
            foreach (SpriteRenderer sr in sprites) sr.color = new Color(colour.r, colour.g, colour.b, colour.a * a);
        }
    }
}
