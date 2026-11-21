using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Codebound.Core;
using Codebound.Core.Hud;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Codebound.Game
{
    /// <summary>
    /// The player-programmable HUD (UI Toolkit): "+" creates HTML/CSS boxes; boxes can be moved (drag the
    /// title), resized (corner), collapsed, edited and closed. {{keys}} show live game data and values that
    /// computer programs publish with hud.set(key, value). Layout is saved with the world.
    /// </summary>
    public sealed class HudLayer : MonoBehaviour
    {
        sealed class View
        {
            public HudPanel Panel;
            public VisualElement Root, Body, Header;
            public Label Title, Warn;
            public string ParsedHtml, ParsedCss, Signature;
            public HtmlNode Dom;
            public List<CssRule> Rules;
            public readonly List<string> ParseProblems = new List<string>();
            public readonly List<string> RenderProblems = new List<string>();
            public readonly HashSet<string> Missing = new HashSet<string>();
        }

        public const float UiScaleDefault = 1f;
        static readonly Color PanelBg = new Color(0.04f, 0.05f, 0.08f, 0.86f), HeaderBg = new Color(0.09f, 0.13f, 0.17f, 0.95f),
            Accent = new Color(0.37f, 0.94f, 0.63f), TextCol = new Color(0.85f, 0.95f, 0.9f), Border = new Color(0.17f, 0.25f, 0.3f);

        GameSession session;
        ProgramManager programs;
        UIDocument doc;
        VisualElement root, toastBox;
        readonly Dictionary<int, View> views = new Dictionary<int, View>();
        float refreshTimer, startTime, fps;
        public HudPanel EditRequest { get; set; }   // read by HudEditor
        public bool PointerOverHud { get; private set; }
        int appliedSettings = -1;

        void ApplySettings()
        {
            appliedSettings = Settings.Version;
            doc.panelSettings.scale = Settings.UiScale;
            root.style.fontSize = 13 * Settings.TextScale;
            root.style.unityFontDefinition = FontDefinition.FromFont(Settings.UiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            root.style.color = Settings.HighContrast ? Color.white : TextCol;
            foreach (var v in views.Values)
            {
                v.Root.style.backgroundColor = Settings.HighContrast ? Color.black : PanelBg;
                v.Signature = null;   // re-render with the new sizes
            }
        }

        public void Init(GameSession s, ProgramManager pm)
        {
            session = s; programs = pm; startTime = Time.time;
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.name = "HUD";
            settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            settings.scale = UiScaleDefault;
            settings.sortingOrder = 10;
            settings.themeStyleSheet = ScriptableObject.CreateInstance<ThemeStyleSheet>(); // no default theme at runtime: we style everything ourselves
            var go = new GameObject("HUD");
            go.SetActive(false);
            doc = go.AddComponent<UIDocument>();
            doc.panelSettings = settings;
            go.SetActive(true);

            root = doc.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            root.style.position = Position.Absolute; root.style.left = 0; root.style.top = 0; root.style.right = 0; root.style.bottom = 0;
            root.style.unityFontDefinition = FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
            root.style.color = TextCol;
            root.style.fontSize = 13;

            var plus = MakeButton("+", () => { var p = session.State.Hud.Add(HudLayout.Starter()); if (p != null) { p.Title = "Panel " + p.Id; p.X = 360 + 20 * (p.Id % 8); p.Y = 20 + 20 * (p.Id % 8); EditRequest = p; } });
            plus.tooltip = "New HUD box (HTML + CSS)";
            plus.style.position = Position.Absolute; plus.style.right = 252; plus.style.top = 10;
            plus.style.width = 28; plus.style.height = 28; plus.style.fontSize = 20;
            plus.style.backgroundColor = new Color(0.12f, 0.35f, 0.25f, 0.95f);
            root.Add(plus);

            toastBox = new VisualElement { pickingMode = PickingMode.Ignore };
            toastBox.style.position = Position.Absolute; toastBox.style.top = 90; toastBox.style.left = Length.Percent(30); toastBox.style.width = Length.Percent(40);
            toastBox.style.alignItems = Align.Center;
            root.Add(toastBox);

            session.State.Hud.Changed += Sync;
            Sync();
        }

        void OnDestroy()
        {
            if (session != null) session.State.Hud.Changed -= Sync;
            if (doc != null) Destroy(doc.gameObject);
        }

        Button MakeButton(string text, System.Action onClick)
        {
            var b = new Button(onClick) { text = text };
            b.style.backgroundColor = HeaderBg; b.style.color = TextCol;
            b.style.borderTopWidth = 1; b.style.borderBottomWidth = 1; b.style.borderLeftWidth = 1; b.style.borderRightWidth = 1;
            b.style.borderTopColor = Border; b.style.borderBottomColor = Border; b.style.borderLeftColor = Border; b.style.borderRightColor = Border;
            b.style.borderTopLeftRadius = 3; b.style.borderTopRightRadius = 3; b.style.borderBottomLeftRadius = 3; b.style.borderBottomRightRadius = 3;
            b.style.paddingLeft = 4; b.style.paddingRight = 4; b.style.marginLeft = 2; b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.RegisterCallback<MouseEnterEvent>(_ => b.style.backgroundColor = new Color(0.16f, 0.24f, 0.3f));
            b.RegisterCallback<MouseLeaveEvent>(_ => b.style.backgroundColor = HeaderBg);
            return b;
        }

        // ---------------------------------------------------------------- panels
        void Sync()
        {
            var alive = new HashSet<int>();
            foreach (var p in session.State.Hud.Panels) { alive.Add(p.Id); if (!views.ContainsKey(p.Id)) views[p.Id] = CreateView(p); }
            foreach (var id in new List<int>(views.Keys))
                if (!alive.Contains(id)) { views[id].Root.RemoveFromHierarchy(); views.Remove(id); }
            foreach (var v in views.Values) { v.Signature = null; Layout(v); }
            refreshTimer = 0;
        }

        View CreateView(HudPanel p)
        {
            var v = new View { Panel = p };
            v.Root = new VisualElement();
            var r = v.Root.style;
            r.position = Position.Absolute; r.backgroundColor = PanelBg;
            r.borderTopWidth = 1; r.borderBottomWidth = 1; r.borderLeftWidth = 1; r.borderRightWidth = 1;
            r.borderTopColor = Border; r.borderBottomColor = Border; r.borderLeftColor = Border; r.borderRightColor = Border;
            r.borderTopLeftRadius = 4; r.borderTopRightRadius = 4; r.borderBottomLeftRadius = 4; r.borderBottomRightRadius = 4;
            r.overflow = Overflow.Hidden;

            v.Header = new VisualElement();
            v.Header.style.flexDirection = FlexDirection.Row; v.Header.style.backgroundColor = HeaderBg; v.Header.style.height = 22;
            v.Header.style.alignItems = Align.Center; v.Header.style.paddingLeft = 6; v.Header.style.paddingRight = 2;
            v.Title = new Label(p.Title) { pickingMode = PickingMode.Ignore };
            v.Title.style.flexGrow = 1; v.Title.style.color = Accent; v.Title.style.unityFontStyleAndWeight = FontStyle.Bold;
            v.Warn = new Label("!") { tooltip = "This box has HTML/CSS problems — open the editor (E)" };
            v.Warn.style.color = new Color(1f, 0.6f, 0.3f); v.Warn.style.unityFontStyleAndWeight = FontStyle.Bold; v.Warn.style.marginRight = 4;
            v.Header.Add(v.Title); v.Header.Add(v.Warn);
            v.Header.Add(MakeButton("E", () => EditRequest = p));
            v.Header.Add(MakeButton("_", () => { p.Collapsed = !p.Collapsed; Layout(v); session.State.Hud.Touch(); }));
            v.Header.Add(MakeButton("x", () => session.State.Hud.Remove(p)));
            v.Root.Add(v.Header);

            v.Body = new ScrollView(ScrollViewMode.Vertical);
            v.Body.style.flexGrow = 1; v.Body.style.paddingLeft = 6; v.Body.style.paddingRight = 6; v.Body.style.paddingTop = 4; v.Body.style.paddingBottom = 4;
            v.Root.Add(v.Body);

            var grip = new VisualElement();
            grip.style.position = Position.Absolute; grip.style.right = 0; grip.style.bottom = 0; grip.style.width = 12; grip.style.height = 12;
            grip.style.backgroundColor = new Color(0.3f, 0.45f, 0.5f, 0.6f);
            grip.tooltip = "Drag to resize";
            v.Root.Add(grip);

            Draggable(v.Header, (dx, dy) => { p.X += dx; p.Y += dy; Clamp(p); Layout(v); });
            Draggable(grip, (dx, dy) => { p.W = Mathf.Max(120, p.W + dx); p.H = Mathf.Max(60, p.H + dy); Layout(v); });
            root.Add(v.Root);
            return v;
        }

        void Draggable(VisualElement handle, System.Action<float, float> onDelta)
        {
            bool dragging = false; Vector2 last = default;
            handle.RegisterCallback<PointerDownEvent>(e => { if (e.target != handle && !(e.target is Label)) return; dragging = true; last = e.position; handle.CapturePointer(e.pointerId); e.StopPropagation(); });
            handle.RegisterCallback<PointerMoveEvent>(e => { if (!dragging) return; var d = (Vector2)e.position - last; last = e.position; onDelta(d.x, d.y); });
            handle.RegisterCallback<PointerUpEvent>(e => { if (!dragging) return; dragging = false; handle.ReleasePointer(e.pointerId); session.State.Hud.Touch(); });
        }

        void Clamp(HudPanel p)
        {
            var size = root.layout.size;
            if (float.IsNaN(size.x) || size.x <= 0) return;
            p.X = Mathf.Clamp(p.X, 0, Mathf.Max(0, size.x - 60)); p.Y = Mathf.Clamp(p.Y, 0, Mathf.Max(0, size.y - 22));
        }

        static void Layout(View v)
        {
            var p = v.Panel; var s = v.Root.style;
            s.left = p.X; s.top = p.Y; s.width = p.W; s.height = p.Collapsed ? 22 : p.H;
            v.Body.style.display = p.Collapsed ? DisplayStyle.None : DisplayStyle.Flex;
            v.Title.text = p.Title;
        }

        /// <summary>Problems for the editor: parse errors, render errors and unknown {{keys}}.</summary>
        public List<string> ProblemsOf(HudPanel p)
        {
            var list = new List<string>();
            if (!views.TryGetValue(p.Id, out var v)) return list;
            list.AddRange(v.ParseProblems);
            foreach (var r in v.RenderProblems) if (!list.Contains(r)) list.Add(r);
            foreach (var m in v.Missing) list.Add("{{" + m + "}} is not a known value (built-in or published with hud.set)");
            return list;
        }

        // ---------------------------------------------------------------- data
        public string Lookup(string key)
        {
            var st = session.State;
            var pos = session.Player.position;
            switch (key)
            {
                case "player.x": return Mathf.FloorToInt(pos.x).ToString(CultureInfo.InvariantCulture);
                case "player.y": return Mathf.FloorToInt(pos.y).ToString(CultureInfo.InvariantCulture);
                case "player.biome": return st.World.Generator.BiomeAt(GameSession.CellAt(pos)).ToString();
                case "planet.name": return StarSystem.Get(st.HomeSeed, st.PlanetId)?.Name ?? st.PlanetId;
                case "ship.phase": case "ship.altitude": case "ship.velocity": case "ship.heat": case "ship.fuel":
                {
                    LaunchPadState lp = null;
                    foreach (var b in st.Grid.Buildings) if (b.State is LaunchPadState l) { lp = l; break; }
                    if (lp == null) return key == "ship.phase" ? "no launch pad" : "0";
                    var f = lp.Flight;
                    switch (key)
                    {
                        case "ship.phase": return lp.Phase;
                        case "ship.altitude": return (f?.Altitude ?? 0).ToString("0.0", CultureInfo.InvariantCulture);
                        case "ship.velocity": return (f?.Velocity ?? 0).ToString("0.00", CultureInfo.InvariantCulture);
                        case "ship.heat": return Mathf.RoundToInt(f?.Heat ?? 0).ToString(CultureInfo.InvariantCulture);
                        default: return Mathf.RoundToInt(f?.Fuel ?? lp.FuelUnits).ToString(CultureInfo.InvariantCulture);
                    }
                }
                case "pollution.here": return Mathf.RoundToInt(st.Ecology.At(GameSession.CellAt(pos))).ToString(CultureInfo.InvariantCulture);
                case "pollution.total": return Mathf.RoundToInt(st.Ecology.Total).ToString(CultureInfo.InvariantCulture);
                case "player.health": return Mathf.RoundToInt(st.Vitals.Health).ToString(CultureInfo.InvariantCulture);
                case "player.hunger": return Mathf.RoundToInt(st.Vitals.Hunger).ToString(CultureInfo.InvariantCulture);
                case "player.thirst": return Mathf.RoundToInt(st.Vitals.Thirst).ToString(CultureInfo.InvariantCulture);
                case "player.oxygen": return Mathf.RoundToInt(st.Vitals.Oxygen).ToString(CultureInfo.InvariantCulture);
                case "player.temp": return st.Vitals.BodyTemp.ToString("0.0", CultureInfo.InvariantCulture);
                case "player.warning": return st.Vitals.Warning;
                case "player.ambient": return session.Surroundings.AmbientC.ToString("0", CultureInfo.InvariantCulture);
                case "game.day": return st.Clock.Day.ToString(CultureInfo.InvariantCulture);
                case "game.clock": return st.Clock.ClockText;
                case "game.season": return st.Clock.Season.ToString();
                case "game.daylight": return Mathf.RoundToInt(st.Clock.Daylight * 100).ToString(CultureInfo.InvariantCulture);
                case "game.weather": return WorldClock.WeatherName(session.Surroundings.Weather, st.World.Generator.BiomeAt(GameSession.CellAt(pos)));
                case "research.done": return st.Research.Done.Count.ToString(CultureInfo.InvariantCulture);
                case "ship.progress": return $"{st.Research.MainLineDone}/{st.Research.MainLineTotal}";
                case "game.seed": return st.Seed.ToString(CultureInfo.InvariantCulture);
                case "game.fps": return Mathf.RoundToInt(fps).ToString(CultureInfo.InvariantCulture);
                case "game.time": int t = Mathf.FloorToInt(Time.time - startTime); return $"{t / 60:00}:{t % 60:00}";
                case "machines.count": return st.Grid.Count.ToString(CultureInfo.InvariantCulture);
                case "programs.running": return programs.RunningCount.ToString(CultureInfo.InvariantCulture);
                case "inventory.free":
                    int free = 0; for (int i = 0; i < st.Inventory.SlotCount; i++) if (st.Inventory[i].IsEmpty) free++;
                    return free.ToString(CultureInfo.InvariantCulture);
                case "power.capacityKW": case "power.demandKW": case "power.usage": case "power.storedKJ":
                    var (cap, used, kj) = session.Factory.Totals();
                    float v = key == "power.capacityKW" ? cap : key == "power.demandKW" ? used : key == "power.storedKJ" ? kj : (cap > 0 ? 100f * used / cap : 0);
                    return Mathf.RoundToInt(v).ToString(CultureInfo.InvariantCulture);
            }
            if (key.StartsWith("inventory.")) { string id = key.Substring(10); return ItemDatabase.Exists(id) ? st.Inventory.Count(id).ToString(CultureInfo.InvariantCulture) : null; }
            return st.Hud.Values.TryGetValue(key, out var val) ? val : null;
        }

        public static readonly string[] BuiltInKeys =
        {
            "player.x", "player.y", "player.biome", "player.health", "player.hunger", "player.thirst", "player.oxygen", "player.temp", "player.warning", "player.ambient", "game.day", "game.clock", "game.season", "game.weather", "game.daylight", "research.done", "ship.progress", "game.seed", "game.fps", "game.time", "machines.count", "programs.running",
            "inventory.<item id>", "inventory.free", "power.capacityKW", "power.demandKW", "power.usage", "power.storedKJ", "pollution.here", "pollution.total", "planet.name", "ship.phase", "ship.altitude", "ship.velocity", "ship.heat", "ship.fuel",
        };

        // ---------------------------------------------------------------- update
        void Update()
        {
            fps = Mathf.Lerp(fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 1e-4f), 0.05f);
            var mouse = Mouse.current;
            if (mouse != null && root.panel != null)
            {
                var sp = mouse.position.ReadValue();
                var pp = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(sp.x, Screen.height - sp.y));
                var picked = root.panel.Pick(pp);
                PointerOverHud = picked != null && picked != root;
            }
            session.PointerOverHud = PointerOverHud;

            var hud = session.State.Hud;
            while (hud.Notifications.Count > 0) Toast(hud.Notifications.Dequeue());

            if (appliedSettings != Settings.Version) ApplySettings();
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer > 0) return;
            refreshTimer = 0.25f;
            foreach (var v in views.Values) Refresh(v);
        }

        void Refresh(View v)
        {
            var p = v.Panel;
            if (v.ParsedHtml != p.Html || v.ParsedCss != p.Css)
            {
                v.ParseProblems.Clear();
                v.Dom = HtmlParser.Parse(p.Html, v.ParseProblems);
                v.Rules = CssParser.Parse(p.Css, v.ParseProblems);
                v.ParsedHtml = p.Html; v.ParsedCss = p.Css; v.Signature = null;
            }
            if (p.Collapsed) return;
            // cheap change detection: only rebuild the elements when some interpolated value changed
            var sig = new StringBuilder();
            var missing = new HashSet<string>();
            Signature(v.Dom, sig, missing);
            string s = sig.ToString();
            if (s == v.Signature) return;
            v.Signature = s;
            v.Missing.Clear(); v.RenderProblems.Clear();
            var ctx = new HudRenderer.Context { Rules = v.Rules, Lookup = Lookup, Missing = v.Missing, Problems = v.RenderProblems };
            v.Body.Clear();
            v.Body.Add(HudRenderer.Render(v.Dom, ctx));
            v.Warn.style.display = v.ParseProblems.Count + v.RenderProblems.Count + v.Missing.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void Signature(HtmlNode n, StringBuilder sb, HashSet<string> missing)
        {
            if (n.IsText) { sb.Append(HudTemplate.Render(n.Text, Lookup, missing)).Append('\u0001'); return; }
            foreach (var a in n.Attrs) if (a.Value.Contains("{{")) sb.Append(HudTemplate.Render(a.Value, Lookup, missing)).Append('\u0001');
            foreach (var c in n.Children) Signature(c, sb, missing);
        }

        void Toast(string text)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.style.backgroundColor = new Color(0.2f, 0.12f, 0.05f, 0.92f); l.style.color = new Color(1f, 0.85f, 0.5f);
            l.style.paddingLeft = 10; l.style.paddingRight = 10; l.style.paddingTop = 4; l.style.paddingBottom = 4; l.style.marginBottom = 4;
            l.style.borderTopLeftRadius = 4; l.style.borderTopRightRadius = 4; l.style.borderBottomLeftRadius = 4; l.style.borderBottomRightRadius = 4;
            l.style.fontSize = 15; l.style.whiteSpace = WhiteSpace.Normal;
            toastBox.Add(l);
            while (toastBox.childCount > 4) toastBox.RemoveAt(0);
            l.schedule.Execute(() => l.RemoveFromHierarchy()).ExecuteLater(4000);
        }
    }
}
