using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Keeps one sprite GameObject per placed building, driven by BuildGrid events.</summary>
    public sealed class BuildingViews : MonoBehaviour
    {
        GameSession session;
        readonly Dictionary<int, GameObject> views = new Dictionary<int, GameObject>();
        readonly Dictionary<int, SpriteRenderer> bodies = new Dictionary<int, SpriteRenderer>();
        readonly Dictionary<int, int> shownFrame = new Dictionary<int, int>();
        readonly Dictionary<int, SpriteRenderer> leds = new Dictionary<int, SpriteRenderer>();
        Transform root;

        public void Init(GameSession s)
        {
            session = s;
            root = new GameObject("Buildings").transform;
            s.Grid.Placed += OnPlaced;
            s.Grid.Removed += OnRemoved;
            foreach (var b in s.Grid.Buildings) OnPlaced(b);
        }

        /// <summary>World-space centre of a footprint and its z-rotation; shared with the build preview.</summary>
        public static void Pose(BuildingDef def, Cell origin, Rotation rot, Transform t)
        {
            var (w, h) = def.Size(rot);
            t.position = new Vector3(origin.X + w / 2f, origin.Y + h / 2f, 0);
            t.rotation = Quaternion.Euler(0, 0, -90f * (int)rot); // clockwise turns
        }

        void OnPlaced(PlacedBuilding b)
        {
            var go = new GameObject($"{b.Def.Id}#{b.InstanceId}");
            go.transform.SetParent(root, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArtFactory.Building(b.Def);
            sr.sortingOrder = 2;
            Pose(b.Def, b.Origin, b.Rotation, go.transform);
            views[b.InstanceId] = go; bodies[b.InstanceId] = sr;
            if (b.Def.Kind != BuildingKind.Belt && b.Def.Kind != BuildingKind.Conveyor && b.Def.Kind != BuildingKind.Inserter)
            {
                var shadow = new GameObject("shadow").AddComponent<SpriteRenderer>();   // sits the building on the ground
                shadow.sprite = PixelArtFactory.Shadow(); shadow.sortingOrder = 1;
                shadow.transform.SetParent(go.transform, false);
                var (sw, sh) = b.Size;
                shadow.transform.rotation = Quaternion.identity;
                shadow.transform.position = go.transform.position + new Vector3(0.14f, -0.16f, 0);
                shadow.transform.localScale = new Vector3(sw * 2.2f, sh * 2.2f, 1);
            }
            if (b.State == null) return;
            var led = new GameObject("led").AddComponent<SpriteRenderer>();   // status light in the top-right corner
            led.sprite = PixelArtFactory.White(); led.sortingOrder = 3;
            led.transform.SetParent(go.transform, false);
            var (w, h) = b.Size;
            led.transform.rotation = Quaternion.identity;
            led.transform.position = new Vector3(b.Origin.X + w - 0.3f, b.Origin.Y + h - 0.3f, 0);
            led.transform.localScale = new Vector3(0.9f, 0.9f, 1);
            leds[b.InstanceId] = led;
        }

        void LateUpdate()
        {
            // damaged buildings glow red, proportional to the damage taken
            int tick = (int)(Time.time * 8f);   // 8 animation frames per second
            foreach (var b in session.Grid.Buildings)
                if (views.TryGetValue(b.InstanceId, out var go))
                {
                    var body = bodies[b.InstanceId];
                    float k = Mathf.Clamp01(b.Health / b.Def.MaxHealth);
                    body.color = Color.Lerp(new Color(1f, 0.45f, 0.45f), Color.white, k);
                    if (!leds.TryGetValue(b.InstanceId, out var led)) continue;
                    var a = session.Factory.Activity(b);
                    int frames = PixelArtFactory.Frames(b.Def);
                    if (frames > 1)   // working machines move: drills turn, fans spin, belts roll, flames flicker
                    {
                        int f = a == MachineActivity.Working && !Settings.ReduceEffects ? (tick + b.InstanceId) % frames : 0;
                        if (!shownFrame.TryGetValue(b.InstanceId, out var was) || was != f) { body.sprite = PixelArtFactory.Building(b.Def, f); shownFrame[b.InstanceId] = f; }
                    }
                    bool calm = Settings.ReduceEffects;
                    float blink = calm ? 1f : 0.55f + 0.45f * Mathf.Sin(Time.time * 6f);
                    led.enabled = a != MachineActivity.None && a != MachineActivity.Idle;
                    led.color = a == MachineActivity.Working ? Settings.GoodColor
                              : a == MachineActivity.NoPower ? Settings.BadColor * new Color(1, 1, 1, blink)
                              : new Color(1f, 0.85f, 0.2f, blink);   // blocked: yellow
                    // working machines breathe a little
                    float pulse = a == MachineActivity.Working && !calm && b.Def.Kind != BuildingKind.Belt ? 1f + 0.025f * Mathf.Sin(Time.time * 9f + b.InstanceId) : 1f;
                    go.transform.localScale = new Vector3(pulse, pulse, 1);
                }
        }

        void OnRemoved(PlacedBuilding b)
        {
            if (views.TryGetValue(b.InstanceId, out var go)) { Destroy(go); views.Remove(b.InstanceId); }
            leds.Remove(b.InstanceId); bodies.Remove(b.InstanceId); shownFrame.Remove(b.InstanceId);
        }

        void OnDestroy()
        {
            if (session == null) return;
            session.Grid.Placed -= OnPlaced;
            session.Grid.Removed -= OnRemoved;
        }
    }
}
