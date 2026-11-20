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

        // Play Mode without domain reload keeps statics and event subscriptions: reset them every session.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Session = null; pendingLoad = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene s, LoadSceneMode m) => Spawn();

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
            GameState state;
            if (pendingLoad != null) { state = pendingLoad; pendingLoad = null; }
            else
            {
                state = new GameState(ResolveSeed());
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
            gameObject.AddComponent<OptionsMenu>().Init(Session, this, build);
            gameObject.AddComponent<CreatureViews>().Init(Session);
            gameObject.AddComponent<AudioDirector>().Init(Session);
            var cr = state.Creatures;
            cr.WaveArrived += n => state.Hud.Notify($"{n} hostile creature{(n > 1 ? "s" : "")} approaching — your industry is noisy!");
            cr.BuildingDestroyed += b => state.Hud.Notify($"{b.Def.DisplayName} was destroyed");
            cr.ComputerCompromised += b => state.Hud.Notify($"SECURITY: PC-{b.InstanceId} was compromised by a rogue drone");
            state.Research.Completed += t =>
            {
                var r = state.Research;
                state.Hud.Notify(t.MainLine ? $"Ship progress {r.MainLineDone}/{r.MainLineTotal}: {t.Name} complete" : $"Research complete: {t.Name}");
                if (t.Id == "ship_launch")   // honest: the space phase (Etapa 7) does not exist yet
                    state.Hud.Notify("The ship is rebuilt! Launching and the space phase arrive in a future update — keep building.");
            };
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
                Session.Factory.Tick(TickSeconds);
                Session.State.Creatures.Tick(TickSeconds, Session.Player.position.x, Session.Player.position.y);
                tickAccumulator -= TickSeconds;
            }
            Session.State.PlayerX = Session.Player.position.x;
            Session.State.PlayerY = Session.Player.position.y;
            TickSurvival();

            var kb = Keyboard.current;
            if (kb == null || Session.KeyboardCaptured) return;
            if (Controls.Pressed("save")) Save();
            if (Controls.Pressed("load")) Load();
            if (Controls.Pressed("eat") && !OptionsMenu.IsOpen) Eat();
        }

        void TickSurvival()
        {
            var vitals = Session.State.Vitals;
            Session.State.Clock.Seconds += Time.deltaTime;
            var env = Vitals.At(Session.World.Generator, GameSession.CellAt(Session.Player.position), Session.State.Clock);
            Session.Surroundings = env;
            vitals.Tick(Time.deltaTime, env);
            if (!vitals.Dead) return;
            // not punishing: the emergency beacon brings you back to the crash site, inventory intact
            var spawn = Session.World.Generator.FindSpawn();
            Session.Player.position = new Vector3(spawn.X + 0.5f, spawn.Y + 0.5f, 0);
            vitals.Respawn();
            Say("You collapsed. The emergency beacon brought you back to the crash site.");
            Session.State.Hud.Notify("You collapsed — back at the crash site");
        }

        void Eat()
        {
            var vitals = Session.State.Vitals;
            if (Session.Inventory.Count(ItemDatabase.AlienFruit) == 0) { Say("Nothing to eat (alien fruit grows on trees)"); return; }
            if (vitals.Hunger >= Vitals.Max - 1) { Say("You are not hungry"); return; }
            Session.Inventory.TryRemove(ItemDatabase.AlienFruit, 1);
            vitals.Eat(ItemDatabase.AlienFruit);
            AudioDirector.Instance?.Play("eat", null, null);
            Say($"Ate an alien fruit (food {vitals.Hunger:0})");
        }

        void Say(string m) { LastMessage = m; MessageTime = Time.time; Debug.Log("[Codebound] " + m); }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                string tmp = SavePath + ".tmp";
                using (var w = new StreamWriter(tmp, false, new System.Text.UTF8Encoding(false))) SaveSystem.Write(Session.State, w);
                // atomic-ish replace: never leave a half-written save behind
                if (File.Exists(SavePath)) File.Replace(tmp, SavePath, SavePath + ".bak");
                else File.Move(tmp, SavePath);
                Say("Saved (F9 to load)");
            }
            catch (Exception e) { Say("Save failed: " + e.Message); Debug.LogException(e); }
        }

        public void Load()
        {
            if (!File.Exists(SavePath)) { Say("No save yet (F5 to save)"); return; }
            try
            {
                using (var r = new StreamReader(SavePath)) pendingLoad = SaveSystem.Read(r);
            }
            catch (Exception e) { Say("Load failed: " + e.Message); Debug.LogException(e); return; }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // rebuilds everything from pendingLoad
        }
    }
}
