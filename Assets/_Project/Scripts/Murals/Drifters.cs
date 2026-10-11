using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;

namespace WakeTheWalls.Murals
{
    /// <summary>
    /// Lets small shapes drift off a part of the mural and past the viewer: leaves falling from trees,
    /// petals from a flower crown, or sand blown in gusts. They come out steadily, in gusts, or both,
    /// while the mural is in view. The area, shape, colours and motion are set on the prefab.
    /// </summary>
    public class Drifters : MonoBehaviour
    {
        class Piece { public Transform transform; public SpriteRenderer sprite; public Vector3 velocity; public float age, life, spin, phase; }

        [SerializeField] LayeredMuralExperience experience;
        [SerializeField] ShapeSprites.Shape shape = ShapeSprites.Shape.Leaf;
        [Tooltip("Colours to pick from. Uses the mural palette when empty.")]
        [SerializeField] Color[] colours = new Color[0];
        [Tooltip("Where pieces start, 0 to 1 across the mural (0,0 bottom left).")]
        [SerializeField] Rect area = new Rect(0.1f, 0.6f, 0.8f, 0.3f);
        [Tooltip("Size range of a piece, in metres.")]
        [SerializeField] Vector2 size = new Vector2(0.05f, 0.09f);
        [SerializeField] float startDelay = 3f;

        [Header("Amount")]
        [Tooltip("Pieces per second, steadily. 0 for gusts only.")]
        [SerializeField] float perSecond = 1.5f;
        [Tooltip("Seconds between gusts, random between the two. 0,0 for no gusts.")]
        [SerializeField] Vector2 gustEvery = Vector2.zero;
        [SerializeField] int piecesPerGust = 30;
        [SerializeField] int maxPieces = 60;

        [Header("Motion (metres per second, negative Z is towards the viewer)")]
        [SerializeField] Vector3 velocity = new Vector3(0.1f, -0.25f, -0.15f);
        [SerializeField] Vector3 velocityRandom = new Vector3(0.1f, 0.08f, 0.1f);
        [Tooltip("Side to side flutter, in metres.")]
        [SerializeField] float sway = 0.08f;
        [SerializeField] float spinDegrees = 120f;
        [SerializeField] Vector2 lifeSeconds = new Vector2(4f, 6f);

        readonly List<Piece> live = new List<Piece>();
        float nextPiece, nextGust;
        bool running;

        void OnEnable()
        {
            GameEvents.MuralFound += HandleFound;
            GameEvents.MuralLost += HandleLost;
        }

        void OnDisable()
        {
            GameEvents.MuralFound -= HandleFound;
            GameEvents.MuralLost -= HandleLost;
        }

        void HandleFound(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            running = true;
            nextPiece = Time.time + startDelay;
            nextGust = Time.time + startDelay + Random.Range(gustEvery.x, gustEvery.y);
        }

        void HandleLost(MuralData mural)
        {
            if (experience == null || experience.Data != mural) return;
            running = false;
            foreach (var piece in live) if (piece.transform != null) Destroy(piece.transform.gameObject);
            live.Clear();
        }

        void Update()
        {
            if (running && experience.IsRunning)
            {
                if (perSecond > 0f && Time.time >= nextPiece) { Spawn(); nextPiece = Time.time + 1f / perSecond; }
                if (gustEvery.y > 0f && Time.time >= nextGust)
                {
                    for (int i = 0; i < piecesPerGust; i++) Spawn();
                    nextGust = Time.time + Random.Range(gustEvery.x, gustEvery.y);
                }
            }
            Move();
        }

        void Spawn()
        {
            if (live.Count >= maxPieces) return;
            Color[] palette = colours.Length > 0 ? colours : experience.Data.Palette;
            Vector2 mural = experience.Rig.MuralSize;
            var go = new GameObject("Drifter");
            go.transform.SetParent(experience.transform, false);
            go.transform.localPosition = new Vector3((area.x + Random.value * area.width - 0.5f) * mural.x,
                (area.y + Random.value * area.height - 0.5f) * mural.y, -Random.Range(0.02f, 0.1f));
            go.transform.localScale = Vector3.one * Random.Range(size.x, size.y);
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = ShapeSprites.Get(shape);
            sprite.color = palette.Length > 0 ? palette[Random.Range(0, palette.Length)] : Color.white;
            sprite.sortingOrder = 510;
            live.Add(new Piece
            {
                transform = go.transform, sprite = sprite, phase = Random.value * 10f,
                velocity = velocity + Vector3.Scale(velocityRandom, Random.insideUnitSphere),
                life = Random.Range(lifeSeconds.x, lifeSeconds.y),
                spin = spinDegrees * Random.Range(-1f, 1f)
            });
        }

        void Move()
        {
            for (int i = live.Count - 1; i >= 0; i--)
            {
                var piece = live[i];
                piece.age += Time.deltaTime;
                if (piece.transform == null || piece.age >= piece.life)
                {
                    if (piece.transform != null) Destroy(piece.transform.gameObject);
                    live.RemoveAt(i);
                    continue;
                }
                float flutter = Mathf.Sin(piece.phase + piece.age * 2.5f);
                piece.transform.localPosition += (piece.velocity + new Vector3(flutter * sway * 2.5f, 0f, 0f)) * Time.deltaTime;
                piece.transform.localRotation = Quaternion.Euler(35f * flutter, 50f * Mathf.Sin(piece.phase + piece.age * 1.7f), piece.spin * piece.age);
                Color c = piece.sprite.color;
                c.a = Mathf.Min(1f, piece.age / 0.4f, (piece.life - piece.age) / 0.8f);
                piece.sprite.color = c;
            }
        }
    }
}
