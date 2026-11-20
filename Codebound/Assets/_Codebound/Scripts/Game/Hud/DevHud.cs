using System.Collections.Generic;
using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Energy;
using Codebound.Items;
using Codebound.Machines;
using Codebound.Player;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// TEMPORARY development HUD drawn with IMGUI: inventory, build palette, storage transfer window,
    /// interaction prompt and toasts. It is functional (all actions go through the real systems) but the
    /// final HUD is the player-programmable HTML/CSS HUD planned for Etapa 4.
    /// </summary>
    public sealed class DevHud : MonoBehaviour
    {
        private const float SlotSize = 52f;
        private const float ToastSeconds = 2.5f;

        private readonly List<(string text, bool error, float until)> _toasts = new List<(string, bool, float)>();
        private readonly List<Rect> _panelRects = new List<Rect>();
        private GameSession _session;
        private BuildModeController _build;
        private InteractionController _interaction;
        private PlacedStructure _openStorage;
        private PlacedStructure _inspected;

        /// <summary>Computer waiting for the player to click a machine to link, or null.</summary>
        public Computer LinkSource { get; set; }
        private bool _showInventory = true;
        private GUIStyle _label, _small, _slotCount, _toast, _box;

        /// <summary>UI scale multiplier (accessibility: interface scale). 1 = default.</summary>
        public float UiScale { get; set; } = 1f;

        public void Init(GameSession session, BuildModeController build, InteractionController interaction)
        {
            _session = session;
            _build = build;
            _interaction = interaction;
            _build.PointerOverHud = PointerOverHud;
            _session.Events.Subscribe<ToastEvent>(OnToast);
        }

        private void OnDestroy()
        {
            _session?.Events.Unsubscribe<ToastEvent>(OnToast);
        }

        public void OpenStorage(PlacedStructure s) { _openStorage = s; _inspected = null; }

        public void Inspect(PlacedStructure s) { _inspected = s; _openStorage = null; }

        /// <summary>True when the mouse is over any HUD panel drawn last frame.</summary>
        public bool PointerOverHud()
        {
            var m = GameInput.MousePosition;
            var gui = new Vector2(m.x, Screen.height - m.y) / Scale;
            foreach (var r in _panelRects) if (r.Contains(gui)) return true;
            return false;
        }

        private float Scale => Mathf.Max(0.5f, UiScale * Mathf.Max(1f, Screen.height / 900f));

        private void OnToast(ToastEvent e)
        {
            _toasts.Add((e.Text, e.IsError, Time.unscaledTime + ToastSeconds));
            if (_toasts.Count > 5) _toasts.RemoveAt(0);
        }

        private void Update()
        {
            if (_session == null) return;
            if (GameInput.Pressed(GameInput.Action.ToggleInventory)) _showInventory = !_showInventory;
            if (GameInput.Pressed(GameInput.Action.Cancel)) { _openStorage = null; _inspected = null; LinkSource = null; }
            if (GameInput.Pressed(GameInput.Action.CycleLanguage))
            {
                Loc.SetLanguage(Loc.Language == "pt-PT" ? "en" : "pt-PT");
                OnToast(new ToastEvent(Loc.Format("toast.language", Loc.Language)));
            }
            // Close storage if the structure is gone or the player walked away.
            if (_openStorage != null)
            {
                bool exists = _session.Grid.GetById(_openStorage.InstanceId) != null;
                float d = Vec2.Distance(_session.Motor.Position, _openStorage.Center);
                if (!exists || d > InteractionSystem.DefaultRange + 2.5f) _openStorage = null;
            }
            if (_inspected != null && _session.Grid.GetById(_inspected.InstanceId) == null) { _inspected = null; LinkSource = null; }
            _toasts.RemoveAll(t => t.until < Time.unscaledTime);
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true };
            _label.normal.textColor = new Color(0.92f, 0.94f, 0.96f);
            _small = new GUIStyle(_label) { fontSize = 12, wordWrap = true };
            _slotCount = new GUIStyle(_label) { fontSize = 13, alignment = TextAnchor.LowerRight, fontStyle = FontStyle.Bold };
            _toast = new GUIStyle(_label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            _box = new GUIStyle(GUI.skin.box);
        }

        private void OnGUI()
        {
            if (_session == null) return;
            EnsureStyles();
            _panelRects.Clear();
            float s = Scale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1));
            float sw = Screen.width / s, sh = Screen.height / s;

            DrawTopBar(sw);
            DrawBuildPalette(sh);
            if (_showInventory) DrawInventory(sw, sh);
            if (_openStorage != null) DrawStorage(sw, sh);
            if (_inspected != null) DrawInspector(sw, sh);
            if (LinkSource != null) DrawLinkBanner(sw);
            DrawPrompt(sw, sh);
            DrawToasts(sw, sh);
        }

        private void DrawTopBar(float sw)
        {
            var r = new Rect(8, 8, sw - 16, 44);
            GUI.Box(r, GUIContent.none, _box);
            var p = _session.Motor.Position;
            GUI.Label(new Rect(16, 10, sw - 32, 20),
                $"<b>CODEBOUND</b>  ·  seed {_session.World.Seed}  ·  ({p.X:0.0}, {p.Y:0.0})  ·  tick {_session.Clock.CurrentTick}" +
                (_build.Active ? $"  ·  <color=#8f8>{Loc.Get("hud.build_mode_on")}</color>" : ""), _label);
            GUI.Label(new Rect(16, 30, sw - 32, 20), Loc.Get("hud.controls"), _small);
            GUI.Label(new Rect(sw - 520, 54, 512, 20), "<color=#fc6>" + Loc.Get("hud.dev_notice") + "</color>", _small);
        }

        private void DrawBuildPalette(float sh)
        {
            var defs = StructureCatalog.Buildable;
            float w = 250, h = 30 + defs.Count * 46;
            var r = new Rect(8, sh - h - 8, w, h);
            _panelRects.Add(r);
            GUI.Box(r, GUIContent.none, _box);
            GUI.Label(new Rect(r.x + 8, r.y + 4, w, 22), $"<b>{Loc.Get("hud.build")}</b> (B)", _label);
            for (int i = 0; i < defs.Count; i++)
            {
                var d = defs[i];
                bool affordable = _session.PlayerInventory.HasAll(d.Cost);
                bool selected = _build.Active && _build.SelectedIndex == i;
                var row = new Rect(r.x + 6, r.y + 28 + i * 46, w - 12, 42);
                var prev = GUI.backgroundColor;
                GUI.backgroundColor = selected ? new Color(0.4f, 1f, 0.5f) : Color.white;
                if (GUI.Button(row, GUIContent.none)) _build.Select(i);
                GUI.backgroundColor = prev;
                var size = d.Width == d.Height ? "" : $" {d.Width}x{d.Height}";
                GUI.Label(new Rect(row.x + 6, row.y + 2, row.width, 20), $"{(i + 1) % 10}. {Loc.Get(d.NameKey)}{size}", _label);
                GUI.Label(new Rect(row.x + 6, row.y + 21, row.width, 20),
                    $"<color={(affordable ? "#cfc" : "#f99")}>{CostText(d.Cost)}</color>", _small);
            }
        }

        private void DrawInventory(float sw, float sh)
        {
            DrawInventoryPanel(_session.PlayerInventory, Loc.Get("hud.inventory") + " (Tab)", sw - 6 * SlotSize - 24, sh - 4 * SlotSize - 46, 6, null);
        }

        private void DrawStorage(float sw, float sh)
        {
            var inv = _openStorage.Inventory;
            float x = sw / 2 - 2 * SlotSize - 8, y = sh / 2 - 2 * SlotSize - 20;
            DrawInventoryPanel(inv, Loc.Get("hud.storage") + " — " + Loc.Get(_openStorage.Definition.NameKey), x, y, 4, _session.PlayerInventory);
            var close = new Rect(x + 4 * SlotSize + 16 - 70, y + 4, 62, 20);
            if (GUI.Button(close, Loc.Get("hud.close"))) _openStorage = null;
            var m = _session.Machines.Get(_openStorage);
            if (m != null)
            {
                var info = new Rect(x, y - 64, 4 * SlotSize + 16, 60);
                _panelRects.Add(info);
                GUI.Box(info, GUIContent.none, _box);
                GUI.Label(new Rect(info.x + 8, info.y + 4, info.width - 16, 54), MachineSummary(m), _small);
            }
        }

        /// <summary>Draws a slot grid. Clicking a slot moves its stack to <paramref name="transferTarget"/> (or to an open storage).</summary>
        private void DrawInventoryPanel(ItemInventory inv, string title, float x, float y, int cols, ItemInventory transferTarget)
        {
            int rows = Mathf.CeilToInt(inv.SlotCount / (float)cols);
            var r = new Rect(x, y, cols * SlotSize + 16, rows * SlotSize + 38);
            _panelRects.Add(r);
            GUI.Box(r, GUIContent.none, _box);
            GUI.Label(new Rect(x + 8, y + 4, r.width, 22), $"<b>{title}</b>", _label);
            var target = transferTarget ?? _openStorage?.Inventory;
            for (int i = 0; i < inv.SlotCount; i++)
            {
                var slot = inv.GetSlot(i);
                var sr = new Rect(x + 8 + (i % cols) * SlotSize, y + 30 + (i / cols) * SlotSize, SlotSize - 4, SlotSize - 4);
                if (GUI.Button(sr, GUIContent.none) && target != null && !slot.IsEmpty) inv.TransferSlotTo(i, target);
                if (slot.IsEmpty) continue;
                var icon = PixelArt.Icon(slot.ItemId);
                DrawSprite(new Rect(sr.x + 6, sr.y + 4, sr.width - 12, sr.height - 12), icon);
                GUI.Label(new Rect(sr.x, sr.y, sr.width - 3, sr.height - 1), slot.Count.ToString(), _slotCount);
                if (sr.Contains(Event.current.mousePosition))
                    GUI.Label(new Rect(sr.x, sr.y + sr.height, 220, 20), Loc.Get("name." + slot.ItemId), _small);
            }
        }

        private void DrawInspector(float sw, float sh)
        {
            var s = _inspected;
            var node = _session.Machines.GetPowerNode(s);
            float w = 380, h = 250;
            var r = new Rect(sw / 2 - w / 2, sh / 2 - h / 2, w, h);
            _panelRects.Add(r);
            GUI.Box(r, GUIContent.none, _box);
            GUI.Label(new Rect(r.x + 8, r.y + 4, w - 90, 22), "<b>" + Loc.Get(s.Definition.NameKey) + "</b>", _label);
            if (GUI.Button(new Rect(r.xMax - 70, r.y + 4, 62, 20), Loc.Get("hud.close"))) { _inspected = null; LinkSource = null; return; }
            float y = r.y + 30;

            if (node is Battery b)
            {
                GUI.Label(new Rect(r.x + 8, y, w - 16, 60), Loc.Format("hud.battery", (b.ChargeFraction * 100).ToString("0"), (b.StoredJoules / 1000).ToString("0.0"), (b.CapacityJoules / 1000).ToString("0")) + "\n" + NetworkLine(b), _small);
                return;
            }
            if (!(node is Machine m)) return;
            GUI.Label(new Rect(r.x + 8, y, w - 16, 60), MachineSummary(m), _small);
            y += 62;
            if (m is Computer c)
            {
                var hw = c.Hardware;
                GUI.Label(new Rect(r.x + 8, y, w - 16, 40), Loc.Format("hud.computer.hw", hw.CpuFuelPerTick / 1000, hw.RamBytes / (1024 * 1024), hw.StorageBytes / 1024, hw.NetworkPorts, hw.NetworkRange), _small);
                y += 36;
                GUI.Label(new Rect(r.x + 8, y, w - 16, 20), Loc.Format("hud.computer.links", c.Links.Count, hw.NetworkPorts), _small);
                y += 20;
                for (int i = 0; i < c.Links.Count && i < 4; i++)
                {
                    var linked = c.Links[i];
                    GUI.Label(new Rect(r.x + 16, y, w - 110, 20), "• " + Loc.Get(linked.Structure.Definition.NameKey) + " — " + Loc.Get("state." + linked.State), _small);
                    if (GUI.Button(new Rect(r.xMax - 92, y, 84, 18), Loc.Get("hud.unlink"))) { c.Unlink(linked); break; }
                    y += 20;
                }
                if (GUI.Button(new Rect(r.x + 8, r.yMax - 52, 170, 22), Loc.Get("hud.link_machine"))) LinkSource = c;
                // Honest status: there is no code editor or runtime yet.
                GUI.Label(new Rect(r.x + 8, r.yMax - 26, w - 16, 22), "<color=#fc6>" + Loc.Get("hud.computer.no_runtime") + "</color>", _small);
            }
        }

        private void DrawLinkBanner(float sw)
        {
            var r = new Rect(sw / 2 - 220, 100, 440, 26);
            GUI.Box(r, GUIContent.none, _box);
            GUI.Label(r, Loc.Get("hud.link_mode"), _toast);
        }

        private string MachineSummary(Machine m)
        {
            var lines = new List<string> { Loc.Get("hud.state") + ": <b>" + Loc.Get("state." + m.State) + "</b>" };
            lines.Add(NetworkLine(m));
            if (m is Computer c)
                lines.Add(Loc.Get("hud.program") + ": " + Loc.Get("program." + c.ProgramStatus) + (c.Language.HasValue ? " (" + c.Language + ")" : ""));
            else
                lines.Add(m.ControlledBy != null ? Loc.Get("hud.controlled") : Loc.Get("hud.not_controlled"));
            if (m is Drill d) lines.Add(Loc.Format("hud.drill", (d.Progress * 100).ToString("0"), d.TotalMined, d.OreRemaining(_session.World)));
            if (m is Furnace f) lines.Add(Loc.Format("hud.furnace", (f.Progress * 100).ToString("0"), f.Current != null ? Loc.Get("name." + f.Current.Output) : "—", f.TotalProduced));
            if (m is CoalGenerator g) lines.Add(Loc.Format("hud.generator", g.LastOutputWatts.ToString("0"), CoalGenerator.MaxWatts, g.FuelCount));
            return string.Join("\n", lines);
        }

        private string NetworkLine(PowerNode n)
        {
            if (!n.Connected) return Loc.Get("hud.power.disconnected");
            var net = _session.Machines.Power.GetNetwork(n.NetworkId);
            if (net == null) return Loc.Get("hud.power.disconnected");
            return Loc.Format("hud.power.network", n.NetworkId, net.ProductionWatts.ToString("0"), net.DemandWatts.ToString("0"),
                (net.Satisfaction * 100).ToString("0"), (net.StoredJoules / 1000).ToString("0.0"));
        }

        private void DrawPrompt(float sw, float sh)
        {
            var t = _interaction.Current;
            if (t.Kind == InteractionKind.None) return;
            string text = t.Kind == InteractionKind.Harvest
                ? Loc.Format("hud.target.harvest", Loc.Get("name." + t.Node.ItemId), t.Node.Remaining)
                : Loc.Format("hud.target.storage", Loc.Get(t.Structure.Definition.NameKey));
            var r = new Rect(sw / 2 - 180, sh - 90, 360, 28);
            GUI.Box(r, GUIContent.none, _box);
            GUI.Label(r, "[E] " + text, _toast);
        }

        private void DrawToasts(float sw, float sh)
        {
            for (int i = 0; i < _toasts.Count; i++)
            {
                var (text, error, _) = _toasts[i];
                var r = new Rect(sw / 2 - 200, 70 + i * 30, 400, 26);
                GUI.Box(r, GUIContent.none, _box);
                GUI.Label(r, error ? $"<color=#f88>{text}</color>" : text, _toast);
            }
        }

        private static void DrawSprite(Rect r, Sprite s)
        {
            if (s == null) return;
            var tex = s.texture;
            var tr = s.textureRect;
            GUI.DrawTextureWithTexCoords(r, tex, new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height));
        }

        private static string CostText(IReadOnlyList<ItemStack> cost)
        {
            var parts = new List<string>();
            foreach (var c in cost) parts.Add($"{c.Count} {Loc.Get("name." + c.ItemId)}");
            return Loc.Get("hud.cost") + ": " + string.Join(", ", parts);
        }
    }
}
