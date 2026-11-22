using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>The ship on each launch pad: rises with the ascent (1 km = 0.25 cells, so it leaves the screen), with a
    /// flame sized by the throttle, and a flight readout while a launch is in progress.</summary>
    public sealed class SpaceView : MonoBehaviour
    {
        GameSession session;
        readonly Dictionary<int, (SpriteRenderer ship, SpriteRenderer flame)> views = new Dictionary<int, (SpriteRenderer, SpriteRenderer)>();
        GUIStyle box;

        public void Init(GameSession s) { session = s; }

        void LateUpdate()
        {
            var seen = new HashSet<int>();
            foreach (var b in session.Grid.Buildings)
            {
                if (!(b.State is LaunchPadState lp)) continue;
                seen.Add(b.InstanceId);
                if (!views.TryGetValue(b.InstanceId, out var v))
                {
                    var ship = new GameObject("Ship").AddComponent<SpriteRenderer>();
                    ship.sprite = PixelArtFactory.Ship(); ship.sortingOrder = 6;
                    var flame = new GameObject("Flame").AddComponent<SpriteRenderer>();
                    flame.sprite = PixelArtFactory.White(); flame.sortingOrder = 5; flame.color = new Color(1f, 0.6f, 0.15f, 0.9f);
                    views[b.InstanceId] = v = (ship, flame);
                }
                bool inOrbit = lp.Phase == "orbit";
                float alt = lp.Flight != null ? lp.Flight.Altitude : 0f;
                v.ship.enabled = !inOrbit;   // in orbit the ship is far above
                v.ship.transform.position = new Vector3(b.Origin.X + 1.5f, b.Origin.Y + 1.5f + alt * 0.25f, 0);
                float thr = lp.Phase == "ascent" && lp.Flight != null && lp.Flight.Fuel > 0 ? lp.Flight.Throttle : 0f;
                v.flame.enabled = thr > 0.01f && !inOrbit;
                v.flame.transform.position = v.ship.transform.position + new Vector3(-0.25f, -1.0f - thr * 1.2f, 0);
                v.flame.transform.localScale = new Vector3(2f, (0.3f + thr * 1.2f) * 4f, 1f) * (Settings.ReduceEffects ? 1f : 1f + 0.1f * Mathf.Sin(Time.time * 40f));
            }
            foreach (var id in new List<int>(views.Keys))
                if (!seen.Contains(id)) { Destroy(views[id].ship.gameObject); Destroy(views[id].flame.gameObject); views.Remove(id); }
        }

        void OnGUI()
        {
            LaunchPadState lp = null, orbit = null;
            foreach (var b in session.Grid.Buildings)
                if (b.State is LaunchPadState l && l.Flight != null) { if (l.Phase == "ascent") { lp = l; break; } if (l.Phase == "orbit") orbit = l; }
            if (lp == null && orbit == null) return;
            UiGui.Begin();
            if (box == null)
            {
                var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.06f, 0.92f)); bg.Apply();
                box = new GUIStyle(GUI.skin.box) { normal = { background = bg, textColor = new Color(0.8f, 0.95f, 1f) }, fontSize = UiGui.Font(14), richText = true, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(12, 12, 8, 8) };
            }
            if (lp == null)   // in orbit: where, fuel, cargo, story progress
            {
                var st = session.State;
                int cargo = 0; for (int i = 0; i < orbit.Cargo.SlotCount; i++) cargo += orbit.Cargo[i].Count;
                string where = StarSystem.Get(st.HomeSeed, st.PlanetId)?.Name ?? st.PlanetId;
                GUI.Box(new Rect(UiGui.W / 2 - 230, 8, 460, 52),
                    "<b>" + Loc.T("orbit.title", "IN ORBIT above {0}", where) + "</b>" + (orbit.Docked != null ? "  ·  " + Loc.T("orbit.docked", "docked at {0}", Relays.Name(orbit.Docked)) : "") + "\n" +
                    Loc.T("orbit.line", "fuel {0}   cargo {1} items   key fragments {2}/{3}", orbit.Flight.Fuel.ToString("0"), cargo, st.Story.Fragments.Count, Relays.Total), box);
                return;
            }
            var f = lp.Flight;
            string heatCol = f.Heat > 75 ? Settings.Bad : f.Heat > 50 ? "#ffd43b" : Settings.Good;
            GUI.Box(new Rect(UiGui.W / 2 - 230, 8, 460, 52),
                $"<b>ASCENT</b>  {f.Time:0}s   alt <b>{f.Altitude:0.0}</b>/{Ascent.TargetAltitude:0} km   v <b>{f.Velocity:0.00}</b>/{Ascent.TargetVelocity} km/s\n" +
                $"throttle {f.Throttle * 100:0}%   fuel {f.Fuel:0}   heat <color={heatCol}><b>{f.Heat:0}%</b></color>", box);
        }

        void OnDestroy() { foreach (var v in views.Values) { if (v.ship != null) Destroy(v.ship.gameObject); if (v.flame != null) Destroy(v.flame.gameObject); } }
    }
}
