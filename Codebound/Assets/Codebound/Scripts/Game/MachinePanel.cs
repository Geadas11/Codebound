using System.Collections.Generic;
using System.Text;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>
    /// E on a machine: item transfer panel; on a computer: the in-game code editor (run/stop, console,
    /// CPU/RAM monitor, errors with line numbers, contextual hints, examples, API reference).
    /// </summary>
    public sealed class MachinePanel : MonoBehaviour
    {
        public const float Reach = 5f;

        GameSession session;
        BuildController build;
        ProgramManager programs;
        PlacedBuilding open;
        Vector2 codeScroll, logScroll, sideScroll;
        GUIStyle box, label, small, mono, monoNum, err, title, btn;
        Font monoFont;
        int styleVersion = -1;

        public void Init(GameSession s, BuildController b, ProgramManager pm)
        {
            session = s; build = b; programs = pm;
            s.Grid.Removed += r => { if (r == open) Close(); };
        }

        /// <summary>The building whose panel is open (multiplayer guests keep their unsent code while snapshots arrive).</summary>
        public static PlacedBuilding Editing { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetEditing() => Editing = null;

        void Close() { if (Net.IsGuest && open?.State is ComputerState) Net.View(null); Editing = null; open = null; session.PanelOpen = false; session.KeyboardCaptured = false; GUIUtility.keyboardControl = 0; }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (open != null)
            {
                if (kb.escapeKey.wasPressedThisFrame || (!session.KeyboardCaptured && Controls.Pressed("interact"))) Close();
                else if (Vector2.Distance(new Vector2((float)open.CenterX, (float)open.CenterY), session.Player.position) > Reach + 3) Close();
                return;
            }
            if (!Controls.Pressed("interact") || build.Active || OptionsMenu.IsOpen) return;
            var target = session.Grid.At(build.MouseCell());
            if (target?.State == null) return;
            if (Vector2.Distance(new Vector2((float)target.CenterX, (float)target.CenterY), session.Player.position) > Reach) return;
            open = target; Editing = target;
            if (Net.IsGuest && target.State is ComputerState) Net.View(target.Origin);   // the host starts sending this computer's console and debugger
            session.PanelOpen = true;
            session.KeyboardCaptured = target.State is ComputerState; // typing code must not move the player
        }

        void Styles()
        {
            if (box != null && styleVersion == Settings.Version) return;
            styleVersion = Settings.Version;
            var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.05f, 0.06f, 0.09f, 0.96f)); bg.Apply();
            box = new GUIStyle(GUI.skin.box) { normal = { background = bg }, padding = new RectOffset(10, 10, 8, 8) };
            label = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true, normal = { textColor = new Color(0.86f, 0.95f, 0.9f) } };
            small = new GUIStyle(label) { fontSize = 12, wordWrap = true };
            title = new GUIStyle(label) { fontSize = 16, fontStyle = FontStyle.Bold };
            err = new GUIStyle(small) { normal = { textColor = new Color(1f, 0.45f, 0.4f) } };
            btn = new GUIStyle(GUI.skin.button) { fontSize = 13 };
            monoFont = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Menlo", "DejaVu Sans Mono", "Liberation Mono", "Courier New" }, 14);
            mono = new GUIStyle(GUI.skin.textArea) { font = monoFont, fontSize = UiGui.Font(14), wordWrap = false, richText = false };
            mono.normal.textColor = mono.focused.textColor = new Color(0.9f, 0.95f, 0.9f);
            monoNum = new GUIStyle(label) { font = monoFont, fontSize = UiGui.Font(14), alignment = TextAnchor.UpperRight, padding = mono.padding, normal = { textColor = new Color(0.45f, 0.5f, 0.55f) } };
        }

        void OnGUI()
        {
            UiGui.Begin();
            if (open == null) return;
            Styles();
            if (open.State is ComputerState cs) DrawEditor(cs);
            else DrawMachine();
        }

        void SetSorter(string item)
        {
            if (Net.IsGuest) Net.SendAct(new Dictionary<string, object> { { "a", "sort" }, { "x", (double)open.Origin.X }, { "y", (double)open.Origin.Y }, { "item", item } });
            else ((ConveyorState)open.State).Filter = item;
        }

        // ------------------------------------------------------------ item transfer
        void DrawMachine()
        {
            var st = open.State;
            float w = 420, h = 360;
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, (UiGui.H - h) / 2, w, h), box);
            GUILayout.Label($"{Loc.Building(open.Def)} #{open.InstanceId}", title);
            string power = open.Def.Kind == BuildingKind.Drill ? "   " + L("mp.power", "power {0}%", (st.PowerSatisfaction * 100).ToString("0")) : "";
            GUILayout.Label(L("mp.status", "Status: <b>{0}</b>", St(st.Status)) + power, small);
            if (open.Def.Programmable) GUILayout.Label("<i>" + L("mp.programmable", "Works only when a nearby computer program commands it.") + "</i>", small);
            if (st is GeneratorState g) GUILayout.Label(L("mp.genFuel", "Fuel energy left: {0} kJ   output {1}/{2} kW", g.BurnLeftKJ.ToString("0"), g.LoadKW.ToString("0"), Factory.GeneratorKW.ToString("0")), small);
            if (st is FurnaceState f) GUILayout.Label(L("mp.fuel", "Fuel energy left: {0} kJ", f.BurnLeftKJ.ToString("0")), small);
            if (open.Def.Id == "sorter" && st is ConveyorState sc)   // set by hand: sorters need no program
            {
                GUILayout.Label(L("mp.sorter", "Straight on: <b>{0}</b> — other items turn right", sc.Filter == null ? L("mp.everything", "everything") : Loc.Item(sc.Filter)), small);
                GUILayout.BeginHorizontal();
                var kinds = new HashSet<string>();
                for (int i = 0; i < session.Inventory.SlotCount; i++) if (!session.Inventory[i].IsEmpty) kinds.Add(session.Inventory[i].ItemId);
                foreach (var k in kinds)
                    if (GUILayout.Button(Loc.Item(k), btn)) SetSorter(k);
                if (GUILayout.Button(L("mp.any", "any"), btn)) SetSorter(null);
                GUILayout.EndHorizontal();
            }

            foreach (var (role, inv) in st.Inventories)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(L("mp.role." + role, role), small, GUILayout.Width(60));
                bool any = false;
                for (int i = 0; i < inv.SlotCount; i++)
                {
                    var s = inv[i];
                    if (s.IsEmpty) continue;
                    any = true;
                    bool take = GUILayout.Button($"{Loc.Item(s.ItemId)} x{s.Count}  " + L("mp.take", "take"), btn);
                    if (take && Net.IsGuest)
                        Net.SendAct(new System.Collections.Generic.Dictionary<string, object> { { "a", "take" }, { "x", (double)open.Origin.X }, { "y", (double)open.Origin.Y }, { "item", s.ItemId }, { "count", (double)s.Count } });
                    else if (take)
                    {
                        int left = session.Inventory.Add(s.ItemId, s.Count);
                        inv.TryRemove(s.ItemId, s.Count - left);
                    }
                }
                if (!any) GUILayout.Label("<i>" + L("mp.empty", "empty") + "</i>", small);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);
            GUILayout.Label(L("mp.yourInventory", "Your inventory — click to insert:"), small);
            sideScroll = GUILayout.BeginScrollView(sideScroll);
            var seen = new HashSet<string>();
            for (int i = 0; i < session.Inventory.SlotCount; i++)
            {
                var s = session.Inventory[i];
                if (s.IsEmpty || !seen.Add(s.ItemId)) continue;
                int have = session.Inventory.Count(s.ItemId);
                bool put = GUILayout.Button($"{Loc.Item(s.ItemId)} x{have}  " + L("mp.put", "put"), btn);
                if (put && Net.IsGuest) Net.SendAct(new System.Collections.Generic.Dictionary<string, object> { { "a", "put" }, { "x", (double)open.Origin.X }, { "y", (double)open.Origin.Y }, { "item", s.ItemId }, { "count", (double)have } });
                else if (put)
                {
                    int moved = 0;
                    // insert one at a time so partial fits (e.g. a nearly full slot) still move what they can
                    while (moved < have && st.TryInsert(s.ItemId, 1)) moved++;
                    session.Inventory.TryRemove(s.ItemId, moved);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.Label("<size=11>" + L("mp.closeHint", "E / Esc to close") + "</size>", small);
            GUILayout.EndArea();
        }

        // ------------------------------------------------------------ code editor
        static readonly (string name, string code)[] SqlExamples =
        {
            ("Drill rule", "-- SQL programs run as a cycle every second (change with -- @every N, or -- @once).\n-- Read the world from tables, act by inserting rows into `commands`.\n-- @every 1\nINSERT INTO commands (machine, action, arg)\nSELECT id, 'mine', NULL\nFROM machines\nWHERE type = 'drill' AND busy = 0\n  AND (SELECT available FROM energy) = 1;\n"),
            ("Report", "-- @once\n-- What is in range, and what is inside it?\nSELECT m.id, m.type, m.status, c.item, c.count\nFROM machines m LEFT JOIN contents c ON c.machine = m.id\nORDER BY m.id;\n"),
            ("Production log", "-- Your own tables are saved with the computer.\nCREATE TABLE IF NOT EXISTS production (item TEXT, total INTEGER);\nINSERT OR IGNORE INTO production SELECT DISTINCT item, 0 FROM events WHERE name = 'item_mined';\nUPDATE production SET total = total + (SELECT COUNT(*) FROM events e WHERE e.name = 'item_mined' AND e.item = production.item);\nINSERT OR REPLACE INTO hud (key, value)\nSELECT 'mined.' || item, total FROM production;\nSELECT * FROM production;\n"),
            ("Power alarm", "-- Publish the grid state to the HUD: write {{grid}} in a HUD box.\nINSERT OR REPLACE INTO hud (key, value)\nSELECT 'grid', CASE WHEN available = 1 THEN demand_kw || ' / ' || capacity_kw || ' kW' ELSE 'NO POWER' END\nFROM energy;\n"),
        };

        static readonly (string name, string code)[] CExamples =
        {
            ("Drill loop", "/* Codebound - Basic Computer (C). Compiled to WebAssembly when you press Run. */\n#include <codebound.h>\n\nint main(void) {\n    char drill[32], item[64];\n    if (!cb_find(\"drill\", drill, sizeof drill)) { puts(\"no drill within 8 cells\"); return 0; }\n    for (;;) {\n        if (cb_op(drill, \"mine\", \"[]\", item, sizeof item) > 0) printf(\"mined %s\\n\", item);\n        else cb_sleep(1);\n    }\n}\n"),
            ("Furnace", "#include <codebound.h>\n\nint main(void) {\n    char furnace[32], plate[64];\n    if (!cb_find(\"furnace\", furnace, sizeof furnace)) return 0;\n    for (;;) {\n        if (cb_op(furnace, \"smelt\", \"[]\", plate, sizeof plate) > 0) printf(\"made %s\\n\", plate);\n        else cb_sleep(2); /* nothing to do: wait instead of spinning */\n    }\n}\n"),
            ("Power report", "#include <codebound.h>\n\nint main(void) {\n    char s[512];\n    for (;;) {\n        if (cb_call(\"energy\", \"stats\", \"[]\", s, sizeof s) > 0)\n            printf(\"grid %.0f / %.0f kW\\n\", cb_num(s, \"demandKW\"), cb_num(s, \"capacityKW\"));\n        else printf(\"no grid: %s\\n\", cb_error());\n        cb_sleep(5);\n    }\n}\n"),
            ("Events", "#include <codebound.h>\n\nint main(void) {\n    char e[512];\n    cb_subscribe(\"item_mined\");\n    cb_subscribe(\"power_lost\");\n    puts(\"listening...\");\n    while (cb_wait_event(e, sizeof e) > 0) puts(e);\n}\n"),
        };

        static readonly (string name, string code)[] CppExamples =
        {
            ("Drill loop", "// Codebound - Basic Computer (C++17, compiled to WebAssembly)\n#include <codebound.h>\n#include <cstdio>\n\nint main() {\n    cb::Machine drill(\"drill\");\n    if (!drill.found()) { std::puts(\"no drill within 8 cells\"); return 0; }\n    for (;;) {\n        std::string item = drill.op(\"mine\");\n        if (!item.empty()) std::printf(\"mined %s\\n\", item.c_str());\n        else cb::sleep(1);\n    }\n}\n"),
            ("Totals", "// Count what the drill mines with a std::map\n#include <codebound.h>\n#include <map>\n#include <string>\n#include <cstdio>\n\nint main() {\n    cb::Machine drill(\"drill\");\n    std::map<std::string, int> total;\n    for (int i = 1; drill.found(); i++) {\n        std::string item = drill.op(\"mine\");\n        if (item.empty()) { cb::sleep(1); continue; }\n        total[item]++;\n        if (i % 10 == 0) for (auto &[k, v] : total) std::printf(\"%s: %d\\n\", k.c_str(), v);\n    }\n}\n"),
        };

        static readonly (string name, string code)[] PhpExamples =
        {
            ("Drill loop", "<?php\n// Codebound - Basic Computer (PHP)\n// Machines within 8 cells are yours to command: $drill, $furnace, ... or $machines->find('drill').\necho \"Hello from {$computer->name}\\n\";\nwhile (true) {\n    if ($energy->available() && $drill) {\n        $item = $drill->mine();\n        if ($item) echo \"mined $item\\n\";\n    } else {\n        sleep(1);\n    }\n}\n"),
            ("Furnace", "<?php\n// Smelt whatever is in the furnace input (needs coal as fuel)\nwhile (true) {\n    $plate = $furnace->smelt();\n    if ($plate) echo \"made $plate\\n\";\n    else sleep(2); // nothing to do: wait instead of spinning\n}\n"),
            ("Events", "<?php\n// Event mode: react instead of polling\n$computer->on('item_mined', function ($e) { echo \"mined {$e['item']} by {$e['machine']}\\n\"; });\n$computer->on('power_lost', fn($e) => print(\"ALERT: power lost!\\n\"));\necho \"listening...\\n\";\n"),
            ("Inventory", "<?php\n// Machine data comes back as PHP arrays\nforeach ($machines->list() as $m) {\n    echo $m->name, ' ', $m->status()['status'], ' ', json_encode($m->contents()), \"\\n\";\n}\n"),
        };

        static readonly (string name, string code)[] CsExamples =
        {
            ("Drill loop", "// Codebound - Basic Computer (C#)\n// Machines within 8 cells are yours to command: Drill, Furnace, ... or Machines.Find(\"drill\").\nLog(\"Hello from\", Computer.Name);\nwhile (true)\n{\n    if (Energy.Available() && Drill != null)\n    {\n        var item = Drill.Mine();\n        if (item != null) Log(\"mined\", item);\n    }\n    else Sleep(1);\n}\n"),
            ("Furnace", "// Smelt whatever is in the furnace input (needs coal as fuel)\nwhile (true)\n{\n    var plate = Furnace.Smelt();\n    if (plate != null) Log(\"made\", plate);\n    else Sleep(2); // nothing to do: wait instead of spinning\n}\n"),
            ("Events", "// Event mode: react instead of polling\nComputer.On(\"item_mined\", e => Log(\"mined\", e[\"item\"], \"by\", e[\"machine\"]));\nComputer.On(\"power_lost\", e => Log(\"ALERT: power lost!\"));\nLog(\"listening...\");\n"),
            ("Inventory", "// Results are JsonNode: index them and cast, e.g. (string)s[\"status\"]\nforeach (var m in Machines.List())\n{\n    var s = m.Status();\n    Log(m.Name, (string)s[\"status\"], m.Contents());\n}\n"),
        };

        static readonly (string name, string code)[] PyExamples =
        {
            ("Drill loop", "# Codebound - Basic Computer (Python)\n# Machines within 8 cells are yours to command.\nlog(\"Hello from\", computer.name)\nwhile True:\n    if energy.available() and drill:\n        item = drill.mine()\n        if item:\n            log(\"mined\", item)\n    else:\n        sleep(1)\n"),
            ("Furnace", "# Smelt whatever is in the furnace input (needs coal as fuel)\nwhile True:\n    plate = furnace.smelt()\n    if plate:\n        log(\"made\", plate)\n    else:\n        sleep(2)  # nothing to do: wait instead of spinning\n"),
            ("Events", "# Event mode: react instead of polling\n@computer.on(\"item_mined\")\ndef mined(e):\n    log(\"mined\", e[\"item\"], \"by\", e[\"machine\"])\n\n@computer.on(\"power_lost\")\ndef alarm(e):\n    log(\"ALERT: power lost!\")\n\nlog(\"listening...\")\n"),
            ("Inventory", "# Read machine contents (returns a dict)\nfor m in machines.list():\n    log(m.name, m.status()[\"status\"], m.contents())\n"),
        };

        static readonly (string name, string code)[] Examples =
        {
            ("Drill loop", ComputerState.DefaultProgram),
            ("Furnace", "// Smelt whatever is in the furnace input (needs coal as fuel)\nwhile (true) {\n  const plate = furnace.smelt();\n  if (plate) log(\"made\", plate);\n  else sleep(2); // nothing to do: wait instead of spinning\n}\n"),
            ("Events", "// Event mode: react instead of polling\ncomputer.on(\"item_mined\", e => log(\"mined\", e.item, \"by\", e.machine));\ncomputer.on(\"power_lost\", () => log(\"ALERT: power lost!\"));\ncomputer.on(\"tick\", () => {\n  const s = energy.stats();\n  log(\"grid\", s.demandKW + \"/\" + s.capacityKW + \" kW\");\n});\nlog(\"listening...\");\n"),
            ("Inventory", "// Read machine contents (returns an object)\nfor (const m of machines.list()) {\n  log(m.name, m.status().status, m.contents());\n}\n"),
        };

        const string ApiRef =
            "<b>Globals</b>\nlog(...)  print(...)\nsleep(seconds)\n\n" +
            "<b>computer</b>\n.name  .cps\n.on(event, fn)\n.time()\n<i>events: tick, item_mined, item_smelted, power_lost, power_restored</i>\n\n" +
            "<b>energy</b>\n.available() → bool\n.stats() → {capacityKW, demandKW, satisfaction, storedKJ}\n\n" +
            "<b>machines</b>\n.list()  .get(id)  .find(type)\n\n" +
            "<b>drill</b> (nearest)\n.mine() → item | null\n.status()  .contents()\n\n" +
            "<b>furnace</b> (nearest)\n.smelt() → product | null\n\n" +
            "<b>conveyor</b> (programmable), <b>inserter</b>\n.start()  .stop()\n.setFilter(itemId | null)\n\n" +
            "<b>greenhouse</b>\n.plant(plot?)  .water(plot?)  .harvest(plot?)\n.plots() → {plots:[{plot,planted,growth,ripe,water}], tank, light}\n<i>seeds = alien fruit; harvest gives 3</i>\n\n" +
            "<b>turret</b>\n.targets() → [{id,type,health,distance}] (range 10)\n.fire(id) → true | false  (cooldown 0.6 s, needs power)\n<i>events: creature_detected, intrusion {computer, drone, seconds}</i>\n\n" +
            "<b>lab</b>\n.research(\"tech id\") → id | null (waits until done)\n.techs() → [{id,name,area,status,cost}]  — T shows the tree\n\n" +
            "<b>assembler</b>\n.craft(\"iron_gear\" | \"copper_wire\" | \"circuit\") → product | null\n\n" +
            "<b>pad</b> (launch pad)\n.launch()  .throttle(0..1)  .flight()\nin orbit: .planets()  .travel(id)  .land()\n.asteroids()  .mineAsteroid(id)\n.dock() → {relay, log, check, data}  .answer(value)\n.transmit()  <i>(home orbit, 4 key fragments)</i>\n\n" +
            "<b>generator</b>, <b>chest</b>\n.status()  .contents()\n\n" +
            "<b>hud</b>\n.set(key, value)  .notify(text)  .alarm(text)\n\n" +
            "<b>Debug</b>: click line numbers for breakpoints, then Debug. F8 continue, F10 step.\n\n" +
            "<b>SQL</b> (runs every cycle): read-only tables machines(id,type,name,status,power,busy,distance), contents(machine,role,item,count), energy(capacity_kw,demand_kw,satisfaction,available), events(name,machine,item,time), game(time,cycle). Act: INSERT INTO commands(machine,action,arg) — mine, smelt, start, stop, set_filter. Publish: INSERT OR REPLACE INTO hud(key,value). Your tables are saved. -- @every N / -- @once\n\n" +
            "<b>Python</b>: same API in snake_case (set_filter), print() = log(), @computer.on(\"tick\") works as a decorator. Allowed imports: math, random, json, collections, itertools, functools, re, statistics...\n\n" +
            "<i>Each call uses CPU time: a Basic Computer runs 20 calls/s. Machines must be within 8 cells.</i>";

        // computers unlocked with their password during this play session (never saved)
        readonly HashSet<int> unlocked = new HashSet<int>();
        string passwordInput = "", newPassword = "", lockMessage;

        static string L(string key, string english, params object[] args) => Loc.T(key, english, args);
        static string St(string status) => L("st." + status.Replace(' ', '_'), status);   // display only: programs see the English codes

        bool DrawLock(ComputerState cs)
        {
            if (!cs.HasPassword) return false;
            if (Net.IsGuest ? Net.GuestUnlocked.Contains(open.Origin) : unlocked.Contains(open.InstanceId)) return false;
            float w = 400, h = 180;
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, (UiGui.H - h) / 2, w, h), box);
            GUILayout.Label(L("ed.locked", "PC-{0} is locked", open.InstanceId), title);
            GUILayout.Label(L("ed.enterPassword", "Enter the computer's password:"), small);
            GUI.SetNextControlName("pwd");
            passwordInput = GUILayout.PasswordField(passwordInput, '*', 64);
            var e = Event.current;
            bool submit = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter);
            string msg = Net.IsGuest ? Net.UnlockMessage : lockMessage;   // shown above the buttons: same controls in every IMGUI event
            if (msg != null) GUILayout.Label(msg, err);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L("ed.unlock", "Unlock"), btn) || submit)
            {
                if (Net.IsGuest) Net.Unlock(open.Origin, passwordInput);   // the host checks it and then sends the code
                else if (cs.CheckPassword(passwordInput)) { unlocked.Add(open.InstanceId); lockMessage = null; }
                else lockMessage = L("ed.wrongPassword", "Wrong password");
                passwordInput = "";
            }
            if (GUILayout.Button(L("ed.close", "Close (Esc)"), btn)) Close();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            return true;
        }

        // ------------------------------------------------------------ code editor (Etapa 3: files, terminal, history, diagram, assistance)
        static readonly string[] SideTabKeys = { "monitor", "debug", "files", "terminal", "history", "diagram", "api" };
        static readonly string[] SideTabNames = { "Monitor", "Debug", "Files", "Terminal", "History", "Diagram", "API" };
        int sideTab, lastPausedLine;
        string activeFile;            // null = main file
        string newFileName = "", watchInput = "", terminalInput = "", deleteArmed;
        Texture2D lineTex;

        static string LangName(string lang)
        {
            switch (lang)
            {
                case "typescript": return "<color=#3178c6>TypeScript</color>";
                case "python": return "<color=#ffd43b>Python</color>";
                case "sql": return "<color=#e38c00>SQL</color>";
                case "php": return "<color=#8892bf>PHP</color>";
                case "csharp": return "<color=#9b4f96>C#</color>";
                case "c": return "<color=#a8b9cc>C</color>";
                case "cpp": return "<color=#659ad2>C++</color>";
                default: return "<color=#f7df1e>JavaScript</color>";
            }
        }

        string ActiveCode(ComputerState cs) => activeFile != null && cs.Files.TryGetValue(activeFile, out var f) ? f : cs.Code;
        void SetActiveCode(ComputerState cs, string v) { if (activeFile != null && cs.Files.ContainsKey(activeFile)) cs.Files[activeFile] = v; else cs.Code = v; }

        // ---- what the program is doing: from the local ProgramManager, or for a guest from the host's "pc" messages
        Dictionary<string, object> Remote => Net.IsGuest ? Net.RemotePc(open.Origin) : null;
        T R<T>(string k) => Remote is Dictionary<string, object> r && r.TryGetValue(k, out var v) && v is T t ? t : default;
        bool IsRunning() => Net.IsGuest ? R<bool>("run") : programs.IsRunning(open);
        ProgramManager.DebugInfo DebugState() => Net.IsGuest
            ? new ProgramManager.DebugInfo { Debugging = R<bool>("dbg"), PausedLine = (int)R<double>("line"), Vars = R<Dictionary<string, object>>("vars"), Stack = R<List<object>>("stack"), Watches = R<List<object>>("watches") }
            : programs.GetDebug(open);
        List<string> LogOf(ComputerState cs)
        {
            if (!Net.IsGuest) return cs.Log;
            var l = new List<string>();
            if (R<List<object>>("log") is List<object> lo) foreach (var x in lo) if (x is string s) l.Add(s);
            return l;
        }
        string ErrorOf(ComputerState cs) => Net.IsGuest ? R<string>("err") : cs.LastError;
        int ErrorLineOf(ComputerState cs) => Net.IsGuest ? (int)R<double>("errLine") : cs.LastErrorLine;
        string ErrorFileOf(ComputerState cs) => Net.IsGuest ? R<string>("errFile") : cs.LastErrorFile;

        // ---- actions: done here, or asked of the host
        void SendProgram(string op, object arg = null) => Net.Program(open.Origin, (ComputerState)open.State, op, arg);
        void Run(bool debug) { if (Net.IsGuest) SendProgram(debug ? "debug" : "run"); else programs.StartProgram(open, debug: debug); }
        void StopProgram() { if (Net.IsGuest) SendProgram("stop"); else programs.Stop(open); }
        void Resume(string mode) { if (Net.IsGuest) SendProgram("resume", mode); else programs.Resume(open, mode); }
        void ClearLog(ComputerState cs) { if (Net.IsGuest) SendProgram("clear"); else cs.Log.Clear(); }
        void Synced() { if (Net.IsGuest) SendProgram(""); }   // a guest's edits (files, watches) go to the host

        void DrawEditor(ComputerState cs)
        {
            if (open == null || DrawLock(cs)) return;
            if (activeFile != null && !cs.Files.ContainsKey(activeFile)) activeFile = null;
            var rect = new Rect(UiGui.W * 0.04f, UiGui.H * 0.04f, UiGui.W * 0.92f, UiGui.H * 0.86f);
            GUILayout.BeginArea(rect, box);
            bool running = IsRunning();
            var dbg = DebugState();
            bool paused = dbg.PausedLine != 0;
            string lastError = ErrorOf(cs);
            var log = LogOf(cs);
            var ev = Event.current;
            if (paused && ev.type == EventType.KeyDown)
            {   // F8 continue · F11 step into · F10 step over · Shift+F11 step out
                if (ev.keyCode == KeyCode.F8) { Resume("continue"); ev.Use(); }
                else if (ev.keyCode == KeyCode.F10) { Resume("over"); ev.Use(); }
                else if (ev.keyCode == KeyCode.F11) { Resume(ev.shift ? "out" : "step"); ev.Use(); }
            }

            // title row, then a row of buttons
            GUILayout.BeginHorizontal();
            string state = !running ? "<color=#8a8f99>" + L("ed.stopped", "STOPPED") + "</color>"
                         : paused ? "<color=#ffd43b>" + L("ed.pausedAt", "PAUSED at line {0}", dbg.PausedLine) + "</color>"
                         : dbg.Debugging ? "<color=#5ef0a0>" + L("ed.debugging", "DEBUGGING") + "</color>" : "<color=#5ef0a0>" + L("ed.running", "RUNNING") + "</color>";
            GUILayout.Label($"PC-{open.InstanceId}  |  {LangName(cs.Language)}  |  {state}", title);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(L("ed.close", "Close (Esc)"), btn)) { Close(); GUILayout.EndHorizontal(); GUILayout.EndArea(); return; }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (!running)
            {
                if (GUILayout.Button(L("ed.language", "Language >"), btn))
                {
                    int li = System.Array.IndexOf(ComputerState.Languages, cs.Language);
                    cs.Language = ComputerState.Languages[(li + 1) % ComputerState.Languages.Length];
                }
                if (GUILayout.Button(L("ed.run", "Run"), btn, GUILayout.MinWidth(60))) Run(false);
                if (cs.Language != "sql" && cs.Language != "c" && cs.Language != "cpp" && GUILayout.Button(L("ed.debug", "Debug"), btn, GUILayout.MinWidth(60))) Run(true);
                if (Net.IsGuest && GUILayout.Button(L("ed.save", "Save"), btn, GUILayout.MinWidth(60))) SendProgram("");
            }
            else
            {
                if (paused)
                {
                    if (GUILayout.Button(L("ed.continue", "Continue F8"), btn)) Resume("continue");
                    if (GUILayout.Button(L("ed.into", "Into F11"), btn)) Resume("step");
                    if (GUILayout.Button(L("ed.over", "Over F10"), btn)) Resume("over");
                    if (GUILayout.Button(L("ed.out", "Out Shift+F11"), btn)) Resume("out");
                }
                if (GUILayout.Button(L("ed.stop", "Stop"), btn, GUILayout.MinWidth(60))) StopProgram();
            }
            if (Settings.Assist < 2 && !running)   // examples are help: hidden in Advanced and while running (their names stay in English, like the code)
                foreach (var ex in cs.Language == "python" ? PyExamples : cs.Language == "sql" ? SqlExamples : cs.Language == "php" ? PhpExamples : cs.Language == "csharp" ? CsExamples : cs.Language == "c" ? CExamples : cs.Language == "cpp" ? CppExamples : Examples)
                    if (GUILayout.Button(ex.name, btn)) { activeFile = null; cs.Code = ex.code; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(L("ed.clearLog", "Clear log"), btn)) ClearLog(cs);
            GUILayout.EndHorizontal();

            // guided mode: what to do next
            if (Settings.Assist == 0)
                GUILayout.Label("<color=#9cdcfe>" + Assist.Tip(session.Factory.HasPower(open), session.Factory.MachinesNear(open).Count, cs.History.Count > 0, running, lastError) + "</color>", small);

            // file tabs
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(activeFile == null, "main", btn, GUILayout.Width(70))) activeFile = null;
            foreach (var f in cs.Files.Keys) if (GUILayout.Toggle(activeFile == f, f, btn)) activeFile = f;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            // code + line numbers
            GUILayout.BeginVertical(GUILayout.Width(rect.width * 0.62f));
            codeScroll = GUILayout.BeginScrollView(codeScroll, GUILayout.Height(rect.height * (Settings.Assist == 0 ? 0.55f : 0.58f)));
            GUILayout.BeginHorizontal();
            string code = ActiveCode(cs);
            bool main = activeFile == null;
            int lines = 1; foreach (char c in code) if (c == '\n') lines++;
            var nums = new StringBuilder();
            for (int i = 1; i <= lines; i++)
                nums.Append(main && i == dbg.PausedLine ? ">" : main && cs.Breakpoints.Contains(i) ? "•" : " ").Append(i).Append('\n');
            GUILayout.Label(nums.ToString(), monoNum, GUILayout.Width(44));
            var gutter = GUILayoutUtility.GetLastRect();
            if (main && ev.type == EventType.MouseDown && gutter.Contains(ev.mousePosition))
            {
                int clicked = Mathf.FloorToInt((ev.mousePosition.y - gutter.y - monoNum.padding.top) / monoNum.lineHeight) + 1;
                if (clicked >= 1 && clicked <= lines)
                {
                    if (Net.IsGuest) { if (!cs.Breakpoints.Remove(clicked)) cs.Breakpoints.Add(clicked); SendProgram("bp", (double)clicked); }
                    else programs.ToggleBreakpoint(open, clicked);
                }
                ev.Use();
            }
            HandleTab(cs);
            GUI.SetNextControlName("code");
            GUI.enabled = !running; // edit while stopped; the running program keeps the code it started with
            string edited = GUILayout.TextArea(code, mono, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUI.enabled = true;
            var area = GUILayoutUtility.GetLastRect();
            if (main) HighlightLine(area, dbg.PausedLine, new Color(1f, 0.85f, 0.2f, 0.22f));
            if (!running && ErrorFileOf(cs) == activeFile) HighlightLine(area, ErrorLineOf(cs), new Color(1f, 0.3f, 0.3f, 0.22f));
            if (!running && edited != code) SetActiveCode(cs, edited);
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            // error + hint (hints in Guided and Normal)
            if (!string.IsNullOrEmpty(lastError))
            {
                GUILayout.Label(L("ed.error", "Error: {0}", lastError), err);
                string hint = Settings.Assist < 2 ? Assist.Hint(cs.Language, lastError) : null;
                if (hint != null) GUILayout.Label(L("ed.hint", "Hint: {0}", hint), small);
            }

            // console
            GUILayout.Label("<b>" + L("ed.console", "Console") + "</b>", small);
            logScroll = GUILayout.BeginScrollView(logScroll, box);
            int from = Mathf.Max(0, log.Count - 60);
            var sb = new StringBuilder();
            for (int i = from; i < log.Count; i++) sb.Append(log[i]).Append('\n');
            GUILayout.Label(sb.ToString(), new GUIStyle(small) { font = monoFont, richText = false });
            string lastWarn = null;
            for (int i = log.Count - 1; i >= from; i--) if (log[i].StartsWith("[warn]")) { lastWarn = log[i]; break; }
            GUILayout.EndScrollView();
            if (Event.current.type == EventType.Repaint) logScroll.y = float.MaxValue; // follow the tail
            if (lastWarn != null && Settings.Assist < 2 && Assist.Hint(cs.Language, lastWarn) is string wh) GUILayout.Label(L("ed.hint", "Hint: {0}", wh), small);
            GUILayout.EndVertical();

            // side panel with tabs
            GUILayout.BeginVertical(box, GUILayout.Width(rect.width * 0.35f));
            if (dbg.PausedLine != lastPausedLine) { if (paused) sideTab = 1; lastPausedLine = dbg.PausedLine; }   // jump to the debugger once per pause
            var tabs = new string[SideTabKeys.Length];
            for (int i = 0; i < tabs.Length; i++) tabs[i] = L("ed.tab." + SideTabKeys[i], SideTabNames[i]);
            sideTab = GUILayout.SelectionGrid(sideTab, tabs, 4, new GUIStyle(btn) { fontSize = Mathf.Max(10, btn.fontSize - 3), padding = new RectOffset(2, 2, 3, 3) });
            sideScroll = GUILayout.BeginScrollView(sideScroll);
            switch (sideTab)
            {
                case 0: DrawMonitor(cs, running); break;
                case 1: DrawDebug(cs, dbg, paused, running); break;
                case 2: DrawFiles(cs, running); break;
                case 3: DrawTerminal(cs, running); break;
                case 4: DrawHistory(cs); break;
                case 5: DrawDiagram(cs, rect.width * 0.34f); break;
                default: GUILayout.Label(ApiRef, small); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawMonitor(ComputerState cs, bool running)
        {
            GUILayout.Label("<b>" + L("ed.security", "Security") + "</b>", small);
            bool firewall = session.State.Research.IsDone("cybersecurity");
            GUILayout.Label(L("ed.securityLine", "Firewall: {0}   Password: {1}   drone break-in: {2}s",
                firewall ? "<color=" + Settings.Good + ">" + L("ed.on", "ON") + "</color>" : L("ed.firewallOff", "off (research Cybersecurity)"),
                cs.HasPassword ? L("ed.pwdSet", "set") : L("ed.pwdNone", "none"), session.State.Creatures.HackTime(cs).ToString("0")), small);
            GUILayout.BeginHorizontal();
            newPassword = GUILayout.PasswordField(newPassword, '*', 64, GUILayout.Width(150));
            if (GUILayout.Button(cs.HasPassword ? L("ed.change", "Change") : L("ed.setPassword", "Set password"), btn))
            {
                if (Net.IsGuest) { SendProgram("setpwd", newPassword); Net.GuestUnlocked.Add(open.Origin); }
                else { cs.SetPassword(newPassword); unlocked.Add(open.InstanceId); }
                newPassword = "";
            }
            if (cs.HasPassword && GUILayout.Button(L("ed.remove", "Remove"), btn)) { if (Net.IsGuest) SendProgram("setpwd", ""); else cs.SetPassword(null); }
            GUILayout.EndHorizontal();
            GUILayout.Label("<i>" + L("ed.pwdWarning", "Game password: don't reuse a real one.") + "</i>", small);
            GUILayout.Label("<b>" + L("ed.hardware", "Hardware") + "</b>", small);
            GUILayout.Label(L("ed.hardwareLine", "CPU {0} calls/s   RAM {1} MB   Storage {2}/{3} KB", cs.CallsPerSecond, cs.RamMb, (cs.TotalCodeBytes / 1024f).ToString("0.0"), cs.MaxCodeBytes / 1024), small);
            bool powered = session.Factory.HasPower(open);
            GUILayout.Label(powered ? L("ed.powerOk", "Power: <color=#5ef0a0>OK</color>")
                                    : L("ed.powerNone", "Power: <color=#ff7060>NONE</color> — build a Coal Generator (with coal) or Power Poles within reach"), small);
            if (running)
            {
                int cpu, ram, cps; double heap; string waiting;
                if (Net.IsGuest) { cpu = (int)R<double>("cpu"); heap = R<double>("heap"); ram = (int)R<double>("ram"); cps = (int)R<double>("cps"); waiting = R<string>("waiting"); }
                else { var stt = programs.GetStats(open); cpu = stt.Cpu; heap = stt.HeapMb; ram = stt.RamMb; cps = stt.CallsPerSec; waiting = stt.Waiting; }
                GUILayout.Label("<b>" + L("ed.monitor", "Monitor") + "</b>", small);
                GUILayout.Label(L("ed.monitorLine", "CPU {0}%   heap {1}/{2} MB   API {3}/{4} calls/s", cpu, heap.ToString("0.0"), ram, cps, cs.CallsPerSecond), small);
                if (waiting != null) GUILayout.Label(L("ed.waiting", "waiting: {0}", waiting), small);
            }
            if (!Net.IsGuest && programs.RuntimeError != null) GUILayout.Label(programs.RuntimeError, err);
            GUILayout.Label("<b>" + L("ed.inRange", "Machines in range") + "</b>", small);
            var near = session.Factory.MachinesNear(open);
            if (near.Count == 0) GUILayout.Label("<i>" + L("ed.noneInRange", "none within 8 cells") + "</i>", small);
            foreach (var m in near) GUILayout.Label($"m{m.InstanceId}  {Loc.Building(m.Def)}  [{St(m.State.Status)}]", small);
        }

        void DrawDebug(ComputerState cs, ProgramManager.DebugInfo dbg, bool paused, bool running)
        {
            if (cs.Language == "c" || cs.Language == "cpp")
                GUILayout.Label("<i>" + L("ed.dbg.c", "C and C++ run as compiled WebAssembly: no step debugger, but runtime errors show the file and line.") + "</i>", small);
            else if (cs.Language == "sql") GUILayout.Label("<i>" + L("ed.dbg.sql", "SQL runs statement by statement each cycle: use SELECT to look at the data.") + "</i>", small);
            if (paused)
            {
                GUILayout.Label("<b>" + L("ed.pausedAtLine", "Paused at line {0}", dbg.PausedLine) + "</b>  —  " + L("ed.keys", "Continue F8 · Into F11 · Over F10 · Out Shift+F11"), small);
                GUILayout.Label("<b>" + L("ed.callStack", "Call stack") + "</b>", small);
                if (dbg.Stack != null) foreach (var f in dbg.Stack) GUILayout.Label("  " + Escape(f as string ?? ""), small);
                GUILayout.Label("<b>" + L("ed.variables", "Variables") + "</b>" + (cs.Language == "php" ? " <i>" + L("ed.phpGlobals", "(PHP: globals; locals of functions are not visible)") + "</i>" : ""), small);
                if (dbg.Vars == null || dbg.Vars.Count == 0) GUILayout.Label("<i>" + L("ed.noVars", "none in scope") + "</i>", small);
                else foreach (var kv in dbg.Vars) GUILayout.Label($"<color=#9cdcfe>{Escape(kv.Key)}</color> = {Escape(Short(Json.Write(kv.Value)))}", small);
            }
            else if (!running) GUILayout.Label("<i>" + L("ed.dbg.howto", "Click a line number of the main file to toggle a breakpoint, then press Debug.") + "</i>", small);
            else GUILayout.Label("<i>" + L("ed.dbg.running", "Running. It will pause at the next breakpoint.") + "</i>", small);

            GUILayout.Label("<b>" + L("ed.watches", "Watches") + "</b> <size=10>" + L("ed.watchesHelp", "(a name or a path, e.g. items[0].count — no code is run)") + "</size>", small);
            var results = new Dictionary<string, object>();
            if (dbg.Watches != null) foreach (var w in dbg.Watches) if (w is List<object> pair && pair.Count == 2 && pair[0] is string k) results[k] = pair[1];
            string remove = null;
            foreach (var w in cs.Watches)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<color=#9cdcfe>{Escape(w)}</color> = {(results.TryGetValue(w, out var v) ? Escape(Short(Json.Write(v))) : "<i>" + L("ed.whenPaused", "(when paused)") + "</i>")}", small);
                if (GUILayout.Button("x", btn, GUILayout.Width(22))) remove = w;
                GUILayout.EndHorizontal();
            }
            if (remove != null) { cs.Watches.Remove(remove); Synced(); }
            GUILayout.BeginHorizontal();
            watchInput = GUILayout.TextField(watchInput, 80);
            if (GUILayout.Button(L("ed.watch", "Watch"), btn, GUILayout.MinWidth(60)) && watchInput.Trim().Length > 0 && cs.Watches.Count < 12)
            { if (!cs.Watches.Contains(watchInput.Trim())) cs.Watches.Add(watchInput.Trim()); watchInput = ""; Synced(); }
            GUILayout.EndHorizontal();
        }

        void DrawFiles(ComputerState cs, bool running)
        {
            GUILayout.Label("<b>" + L("ed.files", "Files") + $"</b>  {cs.TotalCodeBytes / 1024f:0.0}/{cs.MaxCodeBytes / 1024} KB", small);
            GUILayout.Label($"main  ({System.Text.Encoding.UTF8.GetByteCount(cs.Code)} B)", small);
            string del = null;
            foreach (var kv in cs.Files)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{kv.Key}  ({System.Text.Encoding.UTF8.GetByteCount(kv.Value)} B)", small);
                if (!running && GUILayout.Button(deleteArmed == kv.Key ? L("ed.sure", "sure?") : L("ed.delete", "delete"), btn, GUILayout.MinWidth(60)))
                { if (deleteArmed == kv.Key) del = kv.Key; else deleteArmed = kv.Key; }
                GUILayout.EndHorizontal();
            }
            if (del != null) { cs.Files.Remove(del); deleteArmed = null; if (activeFile == del) activeFile = null; Synced(); }
            if (!running && cs.Files.Count < ComputerState.MaxFiles)
            {
                if (lockMessage != null) GUILayout.Label(lockMessage, err);   // above the button: IMGUI needs the same controls in every event
                GUILayout.BeginHorizontal();
                newFileName = GUILayout.TextField(newFileName, 28);
                if (GUILayout.Button(L("ed.newFile", "New file"), btn, GUILayout.MinWidth(80)))
                {
                    string n = newFileName.Trim();
                    if (ComputerState.ValidFileName(n) && !cs.Files.ContainsKey(n)) { cs.Files[n] = ""; activeFile = n; newFileName = ""; lockMessage = null; Synced(); }
                    else lockMessage = L("ed.fileNames", "File names: letters, digits and _ with an extension, e.g. utils.js");
                }
                GUILayout.EndHorizontal();
            }
            string how;
            switch (cs.Language)
            {
                case "python": how = "utils.py → import utils  /  from utils import f"; break;
                case "php": how = L("ed.files.php", "lib.php is loaded before main: its functions and classes are ready to use"); break;
                case "csharp": how = L("ed.files.cs", "Util.cs → more classes in the same program (top-level code only in main)"); break;
                case "c": case "cpp": how = L("ed.files.c", "util.h + util.c(pp) → #include \"util.h\" (all .c/.cpp files are compiled together)"); break;
                case "sql": how = L("ed.files.sql", "SQL programs use only the main file"); break;
                default: how = "utils.js → const u = require(\"utils\")  (TypeScript: import { f } from \"./utils\")"; break;
            }
            GUILayout.Label("<i>" + how + "</i>", small);
        }

        void DrawTerminal(ComputerState cs, bool running)
        {
            GUILayout.Label("<b>" + L("ed.terminal", "Terminal") + "</b>  <size=10>" + L("ed.terminalHelp", "type help · output appears in the Console") + "</size>", small);
            var e = Event.current;
            bool submit = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) && GUI.GetNameOfFocusedControl() == "term";
            GUILayout.BeginHorizontal();
            GUILayout.Label(">", mono, GUILayout.Width(14));
            GUI.SetNextControlName("term");
            terminalInput = GUILayout.TextField(terminalInput, 400, mono);
            if (GUILayout.Button(L("ed.enter", "Enter"), btn, GUILayout.MinWidth(60)) || submit)
            {
                if (submit) e.Use();
                string input = terminalInput; terminalInput = "";
                var (action, output, snippet) = Terminal.Execute(cs, input, running);
                if (Net.IsGuest)   // the console lives on the host: show the answer locally as a notice
                {
                    if (output != null) session.State.Hud.Notify("> " + input + "\n" + output);
                }
                else
                {
                    cs.AddLog("> " + input);
                    if (output != null) foreach (var l in output.Split('\n')) cs.AddLog(l);
                }
                switch (action)
                {
                    case Terminal.Action.Run: Run(false); break;
                    case Terminal.Action.Debug: Run(true); break;
                    case Terminal.Action.Stop: StopProgram(); break;
                    case Terminal.Action.Clear: ClearLog(cs); break;
                    case Terminal.Action.Snippet: if (Net.IsGuest) SendProgram("snippet", snippet); else programs.StartProgram(open, snippet: snippet); break;
                    default: Synced(); break;   // new/rm/lang/watch changed the computer
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawHistory(ComputerState cs)
        {
            GUILayout.Label("<b>" + L("ed.history", "Run history") + "</b>  " + L("ed.lastN", "(last {0})", ComputerState.MaxHistory), small);
            if (cs.History.Count == 0) GUILayout.Label("<i>" + L("ed.noRuns", "no runs yet") + "</i>", small);
            for (int i = cs.History.Count - 1; i >= 0; i--)
            {
                var r = cs.History[i];
                string col = r.Result == "done" ? Settings.Good : r.Result == "error" ? Settings.Bad : "#8a8f99";
                GUILayout.Label($"{r.When}  {r.Language}  <color={col}>{L("ed.result." + r.Result, r.Result)}</color>  {r.Seconds:0.0}s  " + L("ed.calls", "{0} calls", r.Calls)
                                + (r.Message != null ? "\n   " + Escape(Short(r.Message)) : ""), small);
            }
        }

        /// <summary>The computer, the machines it reaches (laid out as they stand in the world) and how much the running program uses each.</summary>
        void DrawDiagram(ComputerState cs, float width)
        {
            if (lineTex == null) { lineTex = new Texture2D(1, 1); lineTex.SetPixel(0, 0, Color.white); lineTex.Apply(); }
            var near = session.Factory.MachinesNear(open);
            var calls = new Dictionary<string, int>();
            if (Net.IsGuest) { if (R<Dictionary<string, object>>("calls") is Dictionary<string, object> rc) foreach (var kv in rc) if (kv.Value is double d) calls[kv.Key] = (int)d; }
            else if (programs.CallMap(open) is Dictionary<string, int> cm) calls = cm;
            int max = 1; foreach (var v in calls.Values) max = Mathf.Max(max, v);
            GUILayout.Label("<b>" + L("ed.diagram", "Diagram") + "</b>  <size=10>" + L("ed.diagramHelp", "machines within 8 cells; lines get brighter the more the program calls them") + "</size>", small);
            double far = 1.5;   // radial layout: direction as in the world; shortcut: labels of machines one cell apart can still touch
            foreach (var m in near) far = System.Math.Max(far, System.Math.Max(System.Math.Abs(m.CenterX - open.CenterX), System.Math.Abs(m.CenterY - open.CenterY)));
            float w = Mathf.Max(200, width - 30), h = Mathf.Min(w, Mathf.Clamp(UiGui.H * 0.45f, 150f, 230f));
            float rMin = 55f, rMax = Mathf.Min(w, h) / 2f - 18f;
            var area = GUILayoutUtility.GetRect(w, h);
            Vector2 center = area.center;
            Vector2 Pos(PlacedBuilding b) { var d = new Vector2((float)(b.CenterX - open.CenterX), -(float)(b.CenterY - open.CenterY)); return d.sqrMagnitude < 0.01f ? center : center + d.normalized * (rMin + (rMax - rMin) * Mathf.Min(1f, d.magnitude / (float)far)); }
            foreach (var m in near)
            {
                calls.TryGetValue("m" + m.InstanceId, out int n);
                float k = n == 0 ? 0f : 0.35f + 0.65f * n / max;
                DrawLine(center, Pos(m), n == 0 ? new Color(0.4f, 0.45f, 0.5f, 0.5f) : Color.Lerp(new Color(0.3f, 0.6f, 0.5f), new Color(0.37f, 0.94f, 0.63f), k), n == 0 ? 1f : 1f + 3f * k);
            }
            foreach (var m in near)
            {
                calls.TryGetValue("m" + m.InstanceId, out int n);
                var p = Pos(m);
                GUI.Label(new Rect(p.x - 45, p.y - 10, 90, 36), $"<b>m{m.InstanceId}</b> {m.Def.Id}" + (n > 0 ? "\n<color=#5ef0a0>" + L("ed.calls", "{0} calls", n) + "</color>" : ""), new GUIStyle(small) { alignment = TextAnchor.UpperCenter });
            }
            GUI.Label(new Rect(center.x - 40, center.y - 10, 80, 20), $"<b><color=#9cdcfe>PC-{open.InstanceId}</color></b>", new GUIStyle(small) { alignment = TextAnchor.UpperCenter });
            if (near.Count == 0) GUILayout.Label("<i>" + L("ed.noMachines", "no machines in range") + "</i>", small);
            foreach (var kv in calls) if (!kv.Key.StartsWith("m")) GUILayout.Label($"{kv.Key}: " + L("ed.calls", "{0} calls", kv.Value), small);
        }

        void DrawLine(Vector2 a, Vector2 b, Color c, float w)
        {
            var d = b - a;
            if (d.sqrMagnitude < 1) return;
            var m = GUI.matrix; var old = GUI.color;
            GUI.color = c;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, a);
            GUI.DrawTexture(new Rect(a.x, a.y - w / 2, d.magnitude, w), lineTex);
            GUI.matrix = m; GUI.color = old;
        }

        static string Short(string s) => s.Length > 160 ? s.Substring(0, 160) + "..." : s;

        void HighlightLine(Rect area, int line, Color c)
        {
            if (line <= 0 || Event.current.type != EventType.Repaint) return;
            var r = new Rect(area.x, area.y + mono.padding.top + (line - 1) * mono.lineHeight, area.width, mono.lineHeight);
            var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = old;
        }

        static string Escape(string s) => s.Replace("<", "<​");  // keep rich text from interpreting values

        void HandleTab(ComputerState cs)
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown || (e.keyCode != KeyCode.Tab && e.character != '\t') || GUI.GetNameOfFocusedControl() != "code") return;
            if (e.keyCode == KeyCode.Tab && GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) is TextEditor te)
            {
                te.ReplaceSelection("  ");
                SetActiveCode(cs, te.text);
            }
            e.Use();
        }
    }
}
