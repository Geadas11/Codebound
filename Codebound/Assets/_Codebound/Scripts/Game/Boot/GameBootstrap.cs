using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Entry point. Builds the simulation (<see cref="GameSession"/>) and all presentation components.
    /// If a scene has no bootstrap, one is created automatically when entering Play Mode, so the game
    /// runs from any (even empty) scene. Menu "Codebound/Create Test Scene" makes a dedicated scene.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public const long DefaultSeed = 2026;

        [SerializeField] private long _seed = DefaultSeed;

        public GameSession Session { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null) return;
            new GameObject("Codebound").AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            CbLog.Sink = UnityLog;
            DefaultStrings.RegisterAll();
            Application.targetFrameRate = 120;

            Session = new GameSession(_seed);
            CbLog.Info("Boot", $"World seed {_seed}: {Session.World.Width}x{Session.World.Height}, {Session.World.Nodes.Count} resource nodes");

            var cam = EnsureCamera();
            var worldView = Create<WorldView>("WorldView");
            worldView.Init(Session);
            Create<StructureView>("Structures").Init(Session);
            var player = Create<PlayerController>("Player");
            player.Init(Session);
            cam.gameObject.AddComponent<CameraController>().Init(player.transform);
            var build = Create<BuildModeController>("BuildMode");
            build.Init(Session, cam);
            var interact = Create<InteractionController>("Interaction");
            var hud = Create<DevHud>("DevHud");
            interact.Init(Session, cam, build, hud);
            hud.Init(Session, build, interact);
        }

        private void Update()
        {
            Session.Clock.Advance(Time.deltaTime);
        }

        private T Create<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        private static Camera EnsureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f);
            cam.transform.position = new Vector3(0, 0, -10);
            return cam;
        }

        private static void UnityLog(LogLevel level, string module, string msg)
        {
            string line = $"[Codebound/{module}] {msg}";
            switch (level)
            {
                case LogLevel.Error: Debug.LogError(line); break;
                case LogLevel.Warning: Debug.LogWarning(line); break;
                default: Debug.Log(line); break;
            }
        }
    }
}
