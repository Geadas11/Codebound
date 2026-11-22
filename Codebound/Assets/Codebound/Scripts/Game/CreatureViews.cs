using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Draws creatures (they fade out when defeated — no gore), damage tint on buildings and turret shots.</summary>
    public sealed class CreatureViews : MonoBehaviour
    {
        sealed class View { public SpriteRenderer Body; public SpriteRenderer Bar; public float Fade = -1; }

        GameSession session;
        readonly Dictionary<int, View> views = new Dictionary<int, View>();
        readonly List<int> scratch = new List<int>();
        Transform root;
        LineRenderer shot;
        float shotUntil;
        int lastShots;

        public void Init(GameSession s)
        {
            session = s;
            root = new GameObject("Creatures").transform;
            shot = new GameObject("TurretShot").AddComponent<LineRenderer>();
            shot.positionCount = 2; shot.startWidth = shot.endWidth = 0.06f; shot.sortingOrder = 12; shot.enabled = false;
            var player = s.Player.GetComponent<SpriteRenderer>();
            if (player != null) shot.sharedMaterial = player.sharedMaterial;
            shot.startColor = shot.endColor = new Color(1f, 0.9f, 0.4f, 0.9f);
            s.State.Creatures.Died += c => { if (views.TryGetValue(c.Id, out var v)) v.Fade = 0.6f; };
        }

        View Create(Creature c)
        {
            var v = new View();
            v.Body = new GameObject($"{c.Type}#{c.Id}").AddComponent<SpriteRenderer>();
            v.Body.transform.SetParent(root, false);
            v.Body.sprite = PixelArtFactory.Creature(c.Type);
            v.Body.sortingOrder = 9;
            v.Bar = new GameObject("hp").AddComponent<SpriteRenderer>();
            v.Bar.transform.SetParent(v.Body.transform, false);
            v.Bar.sprite = PixelArtFactory.White();
            v.Bar.sortingOrder = 11;
            v.Bar.color = new Color(1f, 0.35f, 0.35f, 0.9f);
            return v;
        }

        void LateUpdate()
        {
            var creatures = session.State.Creatures;
            foreach (var c in creatures.All)
            {
                if (!views.TryGetValue(c.Id, out var v)) views[c.Id] = v = Create(c);
                v.Body.transform.position = new Vector3((float)c.X, (float)c.Y, 0);
                float bob = c.Type == CreatureType.Drone ? Mathf.Sin(Time.time * 6 + c.Id) * 0.08f : 0;
                v.Body.transform.position += new Vector3(0, bob, 0);
                if (v.Fade < 0) v.Body.sprite = PixelArtFactory.Creature(c.Type, Settings.ReduceEffects ? 0 : (int)(Time.time * (c.Type == CreatureType.Drone ? 12 : 5) + c.Id) % 2);
                bool hurt = c.Health < c.MaxHealth;
                v.Bar.enabled = hurt;
                v.Bar.transform.localPosition = new Vector3(-0.4f, 0.55f, 0);
                v.Bar.transform.localScale = new Vector3(3.2f * c.Health / c.MaxHealth, 0.35f, 1);
            }
            // defeated creatures dissolve
            scratch.Clear();
            foreach (var kv in views)
            {
                var v = kv.Value;
                if (v.Fade < 0) { if (creatures.ById(kv.Key) == null) v.Fade = 0.6f; else continue; }
                v.Fade -= Time.deltaTime;
                var col = v.Body.color; col.a = Mathf.Clamp01(v.Fade / 0.6f); v.Body.color = col;
                v.Body.transform.localScale = Vector3.one * (0.6f + 0.4f * col.a);
                v.Bar.enabled = false;
                if (v.Fade <= 0) { Destroy(v.Body.gameObject); scratch.Add(kv.Key); }
            }
            foreach (var id in scratch) views.Remove(id);

            // turret shot line (skipped with reduced effects)
            int shots = 0; PlacedBuilding last = null; TurretState lastState = null;
            foreach (var b in session.Grid.Buildings)
                if (b.State is TurretState ts) { shots += ts.Shots; if (lastState == null || ts.LastShotTime > lastState.LastShotTime) { lastState = ts; last = b; } }
            if (shots != lastShots && last != null && !ClimateView.ReduceEffects)
            {
                var target = creatures.ById(lastState.LastTarget);
                if (target != null || views.ContainsKey(lastState.LastTarget))
                {
                    var tp = target != null ? new Vector3((float)target.X, (float)target.Y, 0) : views[lastState.LastTarget].Body.transform.position;
                    shot.SetPosition(0, new Vector3((float)last.CenterX, (float)last.CenterY, 0)); shot.SetPosition(1, tp);
                    shot.enabled = true; shotUntil = Time.time + 0.08f;
                }
            }
            lastShots = shots;
            if (shot.enabled && Time.time > shotUntil) shot.enabled = false;
        }
    }
}
