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
    /// shortcut: IMGUI until the Etapa 4 UI framework; debugger with breakpoints/variable inspection still pending.
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
        readonly System.Collections.Generic.Dictionary<int, string> guestPasswords = new System.Collections.Generic.Dictionary<int, string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetEditing() => Editing = null;

        void SendProgram(string op)
        {
            var cs = (ComputerState)open.State;
            Net.Program(open.Origin, cs.Language, cs.Code, op, guestPasswords.TryGetValue(open.InstanceId, out var pw) ? pw : null);
        }

        void Close() { Editing = null; open = null; session.PanelOpen = false; session.KeyboardCaptured = false; GUIUtility.keyboardControl = 0; }

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

        // ------------------------------------------------------------ item transfer
        void DrawMachine()
        {
            var st = open.State;
            float w = 420, h = 360;
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, (UiGui.H - h) / 2, w, h), box);
            GUILayout.Label($"{open.Def.DisplayName} #{open.InstanceId}", title);
            string power = open.Def.Kind == BuildingKind.Drill ? $"   power {st.PowerSatisfaction * 100:0}%" : "";
            GUILayout.Label($"Status: <b>{st.Status}</b>{power}", small);
            if (open.Def.Programmable) GUILayout.Label("<i>Works only when a nearby computer program commands it.</i>", small);
            if (st is GeneratorState g) GUILayout.Label($"Fuel energy left: {g.BurnLeftKJ:0} kJ   output {g.LoadKW:0}/{Factory.GeneratorKW:0} kW", small);
            if (st is FurnaceState f) GUILayout.Label($"Fuel energy left: {f.BurnLeftKJ:0} kJ", small);

            foreach (var (role, inv) in st.Inventories)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(role, small, GUILayout.Width(60));
                bool any = false;
                for (int i = 0; i < inv.SlotCount; i++)
                {
                    var s = inv[i];
                    if (s.IsEmpty) continue;
                    any = true;
                    bool take = GUILayout.Button($"{Loc.Item(s.ItemId)} x{s.Count}  take", btn);
                    if (take && Net.IsGuest)
                        Net.SendAct(new System.Collections.Generic.Dictionary<string, object> { { "a", "take" }, { "x", (double)open.Origin.X }, { "y", (double)open.Origin.Y }, { "item", s.ItemId }, { "count", (double)s.Count } });
                    else if (take)
                    {
                        int left = session.Inventory.Add(s.ItemId, s.Count);
                        inv.TryRemove(s.ItemId, s.Count - left);
                    }
                }
                if (!any) GUILayout.Label("<i>empty</i>", small);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);
            GUILayout.Label("Your inventory — click to insert:", small);
            sideScroll = GUILayout.BeginScrollView(sideScroll);
            var seen = new HashSet<string>();
            for (int i = 0; i < session.Inventory.SlotCount; i++)
            {
                var s = session.Inventory[i];
                if (s.IsEmpty || !seen.Add(s.ItemId)) continue;
                int have = session.Inventory.Count(s.ItemId);
                bool put = GUILayout.Button($"{Loc.Item(s.ItemId)} x{have}  put", btn);
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
            GUILayout.Label("<size=11>E / Esc to close</size>", small);
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
            "<b>generator</b>, <b>chest</b>\n.status()  .contents()\n\n" +
            "<b>hud</b>\n.set(key, value)  .notify(text)  .alarm(text)\n\n" +
            "<b>Debug</b>: click line numbers for breakpoints, then Debug. F8 continue, F10 step.\n\n" +
            "<b>SQL</b> (runs every cycle): read-only tables machines(id,type,name,status,power,busy,distance), contents(machine,role,item,count), energy(capacity_kw,demand_kw,satisfaction,available), events(name,machine,item,time), game(time,cycle). Act: INSERT INTO commands(machine,action,arg) — mine, smelt, start, stop, set_filter. Publish: INSERT OR REPLACE INTO hud(key,value). Your tables are saved. -- @every N / -- @once\n\n" +
            "<b>Python</b>: same API in snake_case (set_filter), print() = log(), @computer.on(\"tick\") works as a decorator. Allowed imports: math, random, json, collections, itertools, functools, re, statistics...\n\n" +
            "<i>Each call uses CPU time: a Basic Computer runs 20 calls/s. Machines must be within 8 cells.</i>";

        // computers unlocked with their password during this play session (never saved)
        readonly System.Collections.Generic.HashSet<int> unlocked = new System.Collections.Generic.HashSet<int>();
        string passwordInput = "", newPassword = "", lockMessage;

        bool DrawLock(ComputerState cs)
        {
            if (!cs.HasPassword || unlocked.Contains(open.InstanceId)) return false;
            float w = 380, h = 170;
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, (UiGui.H - h) / 2, w, h), box);
            GUILayout.Label($"PC-{open.InstanceId} is locked", title);
            GUILayout.Label("Enter the computer's password:", small);
            GUI.SetNextControlName("pwd");
            passwordInput = GUILayout.PasswordField(passwordInput, '*', 64);
            var e = Event.current;
            bool submit = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock", btn) || submit)
            {
                if (Net.IsGuest) { guestPasswords[open.InstanceId] = passwordInput; unlocked.Add(open.InstanceId); lockMessage = null; }   // the host checks it
                else if (cs.CheckPassword(passwordInput)) { unlocked.Add(open.InstanceId); lockMessage = null; }
                else lockMessage = "Wrong password";
                passwordInput = "";
            }
            if (GUILayout.Button("Close (Esc)", btn)) Close();
            GUILayout.EndHorizontal();
            if (lockMessage != null) GUILayout.Label(lockMessage, err);
            GUILayout.EndArea();
            return true;
        }

        void DrawEditor(ComputerState cs)
        {
            if (open == null || DrawLock(cs)) return;
            var rect = new Rect(UiGui.W * 0.04f, UiGui.H * 0.04f, UiGui.W * 0.92f, UiGui.H * 0.86f);
            GUILayout.BeginArea(rect, box);
            bool running = programs.IsRunning(open);
            var stt = programs.GetStats(open);
            var dbg = programs.GetDebug(open);
            bool paused = dbg.PausedLine != 0;
            var ev = Event.current;
            if (paused && ev.type == EventType.KeyDown && (ev.keyCode == KeyCode.F8 || ev.keyCode == KeyCode.F10))
            { programs.Resume(open, ev.keyCode == KeyCode.F10); ev.Use(); }

            // toolbar
            GUILayout.BeginHorizontal();
            string langName = cs.Language == "typescript" ? "<color=#3178c6>TypeScript</color>"
                : cs.Language == "python" ? "<color=#ffd43b>Python</color>" : cs.Language == "sql" ? "<color=#e38c00>SQL</color>" : "<color=#f7df1e>JavaScript</color>";
            string state = !running ? "<color=#8a8f99>STOPPED</color>" : paused ? $"<color=#ffd43b>PAUSED at line {dbg.PausedLine}</color>" : dbg.Debugging ? "<color=#5ef0a0>DEBUGGING</color>" : "<color=#5ef0a0>RUNNING</color>";
            GUILayout.Label($"PC-{open.InstanceId}  |  {langName}  |  {state}", title);
            GUILayout.FlexibleSpace();
            if (!running)
            {
                if (GUILayout.Button("Language >", btn))
                {
                    int li = System.Array.IndexOf(ComputerState.Languages, cs.Language);
                    cs.Language = ComputerState.Languages[(li + 1) % ComputerState.Languages.Length];
                }
                if (GUILayout.Button("Run", btn, GUILayout.Width(70))) { if (Net.IsGuest) SendProgram("run"); else programs.StartProgram(open); }
                if (!Net.IsGuest && GUILayout.Button("Debug", btn, GUILayout.Width(70))) programs.StartProgram(open, debug: true);
                if (Net.IsGuest && GUILayout.Button("Save", btn, GUILayout.Width(70))) SendProgram("");
            }
            else
            {
                if (paused && GUILayout.Button("Continue (F8)", btn)) programs.Resume(open, false);
                if (paused && GUILayout.Button("Step (F10)", btn)) programs.Resume(open, true);
                if (GUILayout.Button("Stop", btn, GUILayout.Width(70))) { if (Net.IsGuest) SendProgram("stop"); else programs.Stop(open); }
            }
            foreach (var ex in cs.Language == "python" ? PyExamples : cs.Language == "sql" ? SqlExamples : cs.Language == "php" ? PhpExamples : cs.Language == "csharp" ? CsExamples : cs.Language == "c" ? CExamples : cs.Language == "cpp" ? CppExamples : Examples)
                if (GUILayout.Button(ex.name, btn) && !running) cs.Code = ex.code;
            if (GUILayout.Button("Clear log", btn)) cs.Log.Clear();
            if (GUILayout.Button("Close (Esc)", btn)) { Close(); GUILayout.EndHorizontal(); GUILayout.EndArea(); return; }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            // code + line numbers
            GUILayout.BeginVertical(GUILayout.Width(rect.width * 0.64f));
            codeScroll = GUILayout.BeginScrollView(codeScroll, GUILayout.Height(rect.height * 0.62f));
            GUILayout.BeginHorizontal();
            int lines = 1; foreach (char c in cs.Code) if (c == '\n') lines++;
            var nums = new StringBuilder();
            for (int i = 1; i <= lines; i++)
                nums.Append(i == dbg.PausedLine ? ">" : cs.Breakpoints.Contains(i) ? "•" : " ").Append(i).Append('\n');
            GUILayout.Label(nums.ToString(), monoNum, GUILayout.Width(44));
            var gutter = GUILayoutUtility.GetLastRect();
            if (ev.type == EventType.MouseDown && gutter.Contains(ev.mousePosition))
            {
                int clicked = Mathf.FloorToInt((ev.mousePosition.y - gutter.y - monoNum.padding.top) / monoNum.lineHeight) + 1;
                if (clicked >= 1 && clicked <= lines) programs.ToggleBreakpoint(open, clicked);
                ev.Use();
            }
            HandleTab(cs);
            GUI.SetNextControlName("code");
            GUI.enabled = !running; // edit while stopped; the running program keeps the code it started with
            string edited = GUILayout.TextArea(cs.Code, mono, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUI.enabled = true;
            var area = GUILayoutUtility.GetLastRect();
            HighlightLine(area, dbg.PausedLine, new Color(1f, 0.85f, 0.2f, 0.22f));
            if (!running) HighlightLine(area, cs.LastErrorLine, new Color(1f, 0.3f, 0.3f, 0.22f));
            if (!running && edited != cs.Code) cs.Code = edited;
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            // error + hint
            if (!string.IsNullOrEmpty(cs.LastError))
            {
                GUILayout.Label("Error: " + cs.LastError, err);
                string hint = Hint(cs.LastError);
                if (hint != null) GUILayout.Label("Hint: " + hint, small);
            }

            // console
            GUILayout.Label("<b>Console</b>", small);
            logScroll = GUILayout.BeginScrollView(logScroll, box);
            int from = Mathf.Max(0, cs.Log.Count - 60);
            var sb = new StringBuilder();
            for (int i = from; i < cs.Log.Count; i++) sb.Append(cs.Log[i]).Append('\n');
            GUILayout.Label(sb.ToString(), new GUIStyle(small) { font = monoFont, richText = false });
            string lastWarn = null;
            for (int i = cs.Log.Count - 1; i >= from; i--) if (cs.Log[i].StartsWith("[warn]")) { lastWarn = cs.Log[i]; break; }
            GUILayout.EndScrollView();
            if (Event.current.type == EventType.Repaint) logScroll.y = float.MaxValue; // follow the tail
            if (lastWarn != null && Hint(lastWarn) is string wh) GUILayout.Label("Hint: " + wh, small);
            GUILayout.EndVertical();

            // side: hardware, monitor, machines, API
            GUILayout.BeginVertical(box);
            sideScroll = GUILayout.BeginScrollView(sideScroll);
            int bytes = Encoding.UTF8.GetByteCount(cs.Code);
            GUILayout.Label("<b>Security</b>", small);
            bool firewall = session.State.Research.IsDone("cybersecurity");
            GUILayout.Label($"Firewall: {(firewall ? "<color=" + Settings.Good + ">ON</color>" : "off (research Cybersecurity)")}   Password: {(cs.HasPassword ? "set" : "none")}   drone break-in: {session.State.Creatures.HackTime(cs):0}s", small);
            GUILayout.BeginHorizontal();
            newPassword = GUILayout.PasswordField(newPassword, '*', 64, GUILayout.Width(150));
            if (GUILayout.Button(cs.HasPassword ? "Change" : "Set password", btn)) { cs.SetPassword(newPassword); newPassword = ""; unlocked.Add(open.InstanceId); }
            if (cs.HasPassword && GUILayout.Button("Remove", btn)) cs.SetPassword(null);
            GUILayout.EndHorizontal();
            GUILayout.Label("<i>Game password: don't reuse a real one.</i>", small);
            GUILayout.Label("<b>Hardware</b>", small);
            GUILayout.Label($"CPU {cs.CallsPerSecond} calls/s   RAM {cs.RamMb} MB   Storage {bytes / 1024f:0.0}/{cs.MaxCodeBytes / 1024} KB", small);
            bool powered = session.Factory.HasPower(open);
            GUILayout.Label(powered ? "Power: <color=#5ef0a0>OK</color>" : "Power: <color=#ff7060>NONE</color> — build a Coal Generator (with coal) or Power Poles within reach", small);
            if (running)
            {
                GUILayout.Label("<b>Monitor</b>", small);
                GUILayout.Label($"CPU {stt.Cpu}%   heap {stt.HeapMb:0.0}/{stt.RamMb} MB   API {stt.CallsPerSec}/{cs.CallsPerSecond} calls/s", small);
                if (stt.Waiting != null) GUILayout.Label("waiting: " + stt.Waiting, small);
            }
            if (programs.RuntimeError != null) GUILayout.Label(programs.RuntimeError, err);

            if (paused)
            {
                GUILayout.Label($"<b>Variables</b> (line {dbg.PausedLine})", small);
                if (dbg.Vars == null || dbg.Vars.Count == 0) GUILayout.Label("<i>none in scope</i>", small);
                else foreach (var kv in dbg.Vars)
                {
                    string v = Json.Write(kv.Value);
                    if (v.Length > 160) v = v.Substring(0, 160) + "...";
                    GUILayout.Label($"<color=#9cdcfe>{kv.Key}</color> = {Escape(v)}", small);
                }
                GUILayout.Space(6);
            }
            else if (!running) GUILayout.Label("<i>Click a line number to toggle a breakpoint, then Debug.</i>", small);
            GUILayout.Label("<b>Machines in range</b>", small);
            var near = session.Factory.MachinesNear(open);
            if (near.Count == 0) GUILayout.Label("<i>none within 8 cells</i>", small);
            foreach (var m in near) GUILayout.Label($"m{m.InstanceId}  {m.Def.Id}  [{m.State.Status}]", small);
            GUILayout.Space(6);
            GUILayout.Label(ApiRef, small);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

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
                cs.Code = te.text;
            }
            e.Use();
        }

        /// <summary>Guided-level hints: explain the problem, don't hand over the solution.</summary>
        public static string Hint(string msg)
        {
            if (msg == null) return null;
            if (msg.Contains("no_power")) return "The machine has no electricity. Is a Coal Generator with coal (or a chain of Power Poles) close enough?";
            if (msg.Contains("no_fuel")) return "Furnaces burn coal. Put coal in its fuel slot (E on the furnace) or route a coal drill into it.";
            if (msg.Contains("no_input")) return "The furnace input is empty. What should feed it — you, or a drill facing it?";
            if (msg.Contains("output_full")) return "The output is full. Empty it, or place a chest/furnace in front of the machine (the side it faces).";
            if (msg.Contains("depleted")) return "No ore left under this drill. Time to explore for a new deposit.";
            if (msg.Contains("busy")) return "That machine is already doing something. Each machine runs one action at a time.";
            if (msg.Contains("CPU watchdog")) return "Your loop never calls the world API or sleep(). Every loop that waits should call sleep() or a machine action.";
            if (msg.Contains("Out of memory")) return "The program keeps growing data (arrays/objects) without limit. What keeps adding items?";
            if (msg.Contains("is not defined")) return "A name is unknown. Check spelling and case: JavaScript is case-sensitive. Is the machine within 8 cells?";
            if (msg.Contains("is not a function")) return "You called something that isn't a function. Check the API list on the right for the exact names.";
            if (msg.Contains("SyntaxError")) return "The code isn't valid JavaScript at that line: look for a missing bracket, quote or parenthesis just before it.";
            if (msg.Contains("Cannot read properties of null")) return "Something is null — e.g. no drill/furnace in range, so `drill` is null. Check before using it.";
            if (msg.Contains("not found or failed to start")) return "Install Node.js (nodejs.org) for development builds; release builds will bundle it.";
            return null;
        }
    }
}
