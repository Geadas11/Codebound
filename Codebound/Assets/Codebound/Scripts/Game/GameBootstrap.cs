using System;
using System.IO;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Codebound.Game
{
    /// <summary>Builds the whole game in code when the scene loads (no hand-edited scene YAML). Owns the sim tick and saving.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public const float TickSeconds = 0.05f; // 20 simulation ticks per second

        public static GameSession Session { get; private set; }
        static GameState pendingLoad;

        float tickAccumulator;
        public string LastMessage { get; private set; }
        public float MessageTime { get; private set; }

        public static string SavePath => Path.Combine(Application.persistentDataPath, "saves", "quicksave.cbsave");
        public static string AutoSavePath => Path.Combine(Application.persistentDataPath, "saves", "autosave.cbsave");
        public const float AutoSaveSeconds = 300f;
        float autoSaveTimer = AutoSaveSeconds;
        static int? newGameSeed;

        /// <summary>Starts a fresh world with the given seed (title screen).</summary>
        public static void NewGame(int seed) { newGameSeed = seed; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

        // Play Mode without domain reload keeps statics and event subscriptions: reset them every session.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Session = null; pendingLoad = null; newGameSeed = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene s, LoadSceneMode m) => Spawn();

        /// <summary>Replaces the running world (multiplayer guests receiving the host's world).</summary>
        public static void LoadState(GameState s) { pendingLoad = s; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Spawn();
        }

        static void Spawn()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null) return;
            new GameObject("Codebound").AddComponent<GameBootstrap>();
        }

        /// <summary>Seed from "-seed N" on the command line, otherwise random (shown in the HUD so it can be shared).</summary>
        static int ResolveSeed()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-seed" && int.TryParse(args[i + 1], out int s)) return s;
            return UnityEngine.Random.Range(1, int.MaxValue);
        }

        void Awake()
        {
            Settings.ApplyLanguage();
            GameState state;
            if (pendingLoad != null) { state = pendingLoad; pendingLoad = null; }
            else
            {
                state = new GameState(newGameSeed ?? ResolveSeed());
                newGameSeed = null;
                state.GiveStartingKit();
                state.Hud.Add(Codebound.Core.Hud.HudLayout.Starter());
                var sp = state.World.Generator.FindSpawn();
                state.PlayerX = sp.X + 0.5; state.PlayerY = sp.Y + 0.5;
            }
            Session = new GameSession(state);
            Application.targetFrameRate = 120;

            var cam = Camera.main;
            if (cam == null) cam = new GameObject("Main Camera", typeof(Camera)) { tag = "MainCamera" }.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);

            var player = new GameObject("Player");
            player.transform.position = new Vector3((float)state.PlayerX, (float)state.PlayerY, 0);
            player.AddComponent<PlayerController>().Init(Session);
            Session.Player = player.transform;

            cam.gameObject.AddComponent<CameraFollow>().Init(player.transform);
            cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);

            gameObject.AddComponent<ChunkRenderer>().Init(Session, cam);
            gameObject.AddComponent<BuildingViews>().Init(Session);
            gameObject.AddComponent<ConveyorItemsView>().Init(Session);
            Session.Surroundings = Vitals.At(state.World.Generator, GameSession.CellAt(player.transform.position), state.Clock);
            gameObject.AddComponent<ClimateView>().Init(Session, cam);
            var build = gameObject.AddComponent<BuildController>();
            build.Init(Session, cam);
            var mining = gameObject.AddComponent<MiningController>();
            mining.Init(Session, cam, build);
            var programs = gameObject.AddComponent<ProgramManager>();
            programs.Init(Session);
            gameObject.AddComponent<MachinePanel>().Init(Session, build, programs);
            // UI input (HUD buttons, dragging) goes through an EventSystem driven by the Input System package
            if (UnityEngine.EventSystems.EventSystem.current == null)
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            var hud = gameObject.AddComponent<HudLayer>();
            hud.Init(Session, programs);
            gameObject.AddComponent<HudEditor>().Init(Session, hud);
            gameObject.AddComponent<ResearchPanel>().Init(Session);
            gameObject.AddComponent<ExplorationController>().Init(Session, build);
            gameObject.AddComponent<NetSession>().Init(Session, programs, cam);
            gameObject.AddComponent<SpaceView>().Init(Session);
            gameObject.AddComponent<ColorFilter>().Init(cam);
            gameObject.AddComponent<CaveController>().Init(Session, build, this);
            state.Skills.LeveledUp += (sk, lvl) => state.Hud.Notify(Loc.T("notify.skill", "Skill up: {0} level {1}", Loc.T("skill." + sk, sk), lvl));
            gameObject.AddComponent<TitleMenu>().Init(Session, this);
            gameObject.AddComponent<OptionsMenu>().Init(Session, this, build);
            gameObject.AddComponent<CreatureViews>().Init(Session);
            gameObject.AddComponent<AudioDirector>().Init(Session);
            var cr = state.Creatures;
            cr.WaveArrived += n => state.Hud.Notify(Loc.T("notify.wave", "{0} hostile creature(s) approaching — your industry is noisy!", n));
            cr.BuildingDestroyed += b => state.Hud.Notify(Loc.T("notify.destroyed", "{0} was destroyed", Loc.Building(b.Def)));
            cr.IntrusionStarted += (b, c) => state.Hud.Notify(Loc.T("notify.intrusion", "INTRUSION: a drone is breaking into PC-{0} ({1} s)", b.InstanceId, Mathf.RoundToInt(cr.HackTime((ComputerState)b.State))));
            cr.ComputerCompromised += b => state.Hud.Notify(Loc.T("notify.compromised", "SECURITY: PC-{0} was compromised by a rogue drone", b.InstanceId));
            state.Research.Completed += t =>
            {
                var r = state.Research;
                state.Hud.Notify(t.MainLine ? Loc.T("notify.ship", "Ship progress {0}/{1}: {2} complete", r.MainLineDone, r.MainLineTotal, Loc.Tech(t)) : Loc.T("notify.research", "Research complete: {0}", Loc.Tech(t)));
                if (t.Id == "ship_launch")
                    state.Hud.Notify(Loc.T("notify.rebuilt", "The ship is rebuilt! Build a Launch Pad near a computer, fuel it with coal or crude oil, and fly the ascent with a program."));
            };
            state.Factory.FlightEnded += (pad, result) =>
            {
                if (result == "orbit")
                {
                    var here = StarSystem.Get(state.HomeSeed, state.PlanetId);
                    state.Hud.Notify(Loc.T("notify.orbit", "ORBIT REACHED above {0}! Programs can now list worlds with pad.planets() and fly with pad.travel(id).", here?.Name ?? state.PlanetId));
                    AudioDirector.Instance?.Play("chime", null, Loc.T("cap.orbit", "[chime] orbit reached"));
                }
                else state.Hud.Notify(Loc.T("notify.aborted", "Launch {0}. The capsule parachuted back to the pad.", result));
            };
            var ending = gameObject.AddComponent<EndingView>();
            ending.Init(Session);
            state.Story.EndingReached += () =>
            {
                ending.Show();
                AudioDirector.Instance?.Play("chime", null, Loc.T("cap.ending", "[fanfare] the long-range antenna transmits"));
            };
            ProgramManager.TravelRequested += OnTravel;
            gameObject.AddComponent<DebugHud>().Init(Session, build, mining, this);
            if (SelfTest.Requested) gameObject.AddComponent<SelfTest>();
        }

        void Update()
        {
            // fixed-step simulation, decoupled from frame rate (clamped so a hitch can't spiral)
            tickAccumulator = Mathf.Min(tickAccumulator + Time.deltaTime, 0.5f);
            while (tickAccumulator >= TickSeconds)
            {
                Session.Factory.Daylight = Session.State.Clock.Daylight;
                Session.Factory.SolarFactor = Session.State.Clock.SolarFactor(Session.Surroundings.Weather);
                if (!Net.IsGuest)   // guests mirror the host's simulation instead of running their own
                {
                    Session.Factory.Tick(TickSeconds);
                    Session.State.Creatures.Tick(TickSeconds, Session.Player.position.x, Session.Player.position.y);
                    Session.State.Ecology.Tick(TickSeconds);
                }
                tickAccumulator -= TickSeconds;
            }
            Session.State.PlayerX = Session.Player.position.x;
            Session.State.PlayerY = Session.Player.position.y;
            TickSurvival();
            // autosave: a crash or power cut costs at most 5 minutes (guests do not own the world; the title screen has not started a game yet)
            if (!Net.IsGuest && !TitleMenu.IsOpen && (autoSaveTimer -= Time.unscaledDeltaTime) <= 0) { autoSaveTimer = AutoSaveSeconds; Save(AutoSavePath, quiet: true); }

            var kb = Keyboard.current;
            if (kb == null || Session.KeyboardCaptured) return;
            if ((Controls.Pressed("save") || Controls.Pressed("load")) && Net.IsGuest) Say("In a multiplayer game the host saves the world");
            else if (Controls.Pressed("save")) Save();
            else if (Controls.Pressed("load")) Load();
            if (Controls.Pressed("eat") && !OptionsMenu.IsOpen) Eat();
        }

        void TickSurvival()
        {
            var vitals = Session.State.Vitals;
            Session.State.Clock.Seconds += Time.deltaTime;
            var env = Vitals.At(Session.World.Generator, GameSession.CellAt(Session.Player.position), Session.State.Clock);
            if (Session.State.Ecology.IsSmog(GameSession.CellAt(Session.Player.position))) env.Breathable = false;   // smog: suit air only
            env = Equipment.Apply(Session.Inventory, Shelter.Apply(Session.State, GameSession.CellAt(Session.Player.position), env));
            if (!Net.IsGuest) vitals.Health = Math.Max(0, vitals.Health - Session.State.Lightning.Tick(Time.deltaTime, Session.Player.position.x, Session.Player.position.y));
            Session.Surroundings = env;
            vitals.Tick(Time.deltaTime, env);
            if (!vitals.Dead) return;
            // not punishing: the emergency beacon brings you back to the crash site, inventory intact
            var spawn = Session.World.Generator.FindSpawn();
            Session.Player.position = new Vector3(spawn.X + 0.5f, spawn.Y + 0.5f, 0);
            vitals.Respawn();
            Say("You collapsed. The emergency beacon brought you back to the crash site.");
            Session.State.Hud.Notify(Loc.T("notify.collapsed", "You collapsed — back at the crash site"));
        }

        void Eat()
        {
            var vitals = Session.State.Vitals;
            // medicine first when hurt, otherwise the most filling food
            string food = vitals.Health < 60 && Session.Inventory.Count(ItemDatabase.Glowcap) > 0 ? ItemDatabase.Glowcap
                        : Session.Inventory.Count(ItemDatabase.Ration) > 0 ? ItemDatabase.Ration
                        : Session.Inventory.Count(ItemDatabase.AlienFruit) > 0 ? ItemDatabase.AlienFruit
                        : Session.Inventory.Count(ItemDatabase.Glowcap) > 0 ? ItemDatabase.Glowcap : null;
            if (food == null) { Say("Nothing to eat (alien fruit grows on trees)"); return; }
            if (vitals.Hunger >= Vitals.Max - 1 && food != ItemDatabase.Glowcap) { Say("You are not hungry"); return; }
            if (Net.IsGuest) Net.SendAct(new System.Collections.Generic.Dictionary<string, object> { { "a", "consume" }, { "item", food } });
            Session.Inventory.TryRemove(food, 1);
            vitals.Eat(food);
            AudioDirector.Instance?.Play("eat", null, null);
            Say($"Ate {Loc.Item(food)} (food {vitals.Hunger:0}, health {vitals.Health:0})");
        }

        void OnDestroy() => ProgramManager.TravelRequested -= OnTravel;

        static string PlanetPath(int homeSeed, string id) => Path.Combine(Application.persistentDataPath, "saves", $"planet_{homeSeed}_{id}.cbsave");

        /// <summary>Etapa 7: the ship in orbit flies to another world. The world left behind is saved to its own file
        /// (so you can come back to your base there); the destination is loaded from its file or generated.</summary>
        void OnTravel(PlacedBuilding pad, Planet to) =>
            SwitchWorld(to.Id, saved => Voyage.Arrive(Session.State, pad, to, saved),   // removes the pad from the world left behind
                        Loc.T("notify.landed", "Landed on {0}. Your base on the previous world was saved and waits for you.", to.Name));

        /// <summary>Leaves the current world (saved to its own file) for `targetId` (loaded from its file, or made by `arrive`).</summary>
        public void SwitchWorld(string targetId, Func<GameState, GameState> arrive, string message)
        {
            var from = Session.State;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                GameState saved = null;
                string dest = PlanetPath(from.HomeSeed, targetId);
                if (File.Exists(dest)) using (var r = new StreamReader(dest)) saved = SaveSystem.Read(r);
                var arrived = arrive(saved);
                using (var w = new StreamWriter(PlanetPath(from.HomeSeed, from.PlanetId), false, new System.Text.UTF8Encoding(false))) SaveSystem.Write(from, w);
                if (message != null) arrived.Hud.Notify(message);
                LoadState(arrived);
            }
            catch (Exception e) { Say("Travel failed: " + e.Message); Debug.LogException(e); }
        }

        void Say(string m) { LastMessage = m; MessageTime = Time.time; Debug.Log("[Codebound] " + m); }

        public void Save() => Save(SavePath, quiet: false);

        void Save(string path, bool quiet)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string tmp = path + ".tmp";
                using (var w = new StreamWriter(tmp, false, new System.Text.UTF8Encoding(false))) SaveSystem.Write(Session.State, w);
                // atomic-ish replace: never leave a half-written save behind
                if (File.Exists(path)) File.Replace(tmp, path, path + ".bak");
                else File.Move(tmp, path);
                if (!quiet) Say("Saved (F9 to load)");
            }
            catch (Exception e) { Say("Save failed: " + e.Message); Debug.LogException(e); }
        }

        public void Load() => LoadFrom(SavePath);

        public void LoadFrom(string path)
        {
            if (!File.Exists(path)) { Say("No save yet (F5 to save)"); return; }
            try
            {
                using (var r = new StreamReader(path)) pendingLoad = SaveSystem.Read(r);
            }
            catch (Exception e) { Say("Load failed: " + e.Message); Debug.LogException(e); return; }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // rebuilds everything from pendingLoad
        }
    }
}
