using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Adaptive audio from synthesized clips: machine loops whose sound follows the machine's state, biome/weather
    /// ambience, four music layers composed in Music.cs (day / night / industry / danger) mixed by what is happening, one-shot effects,
    /// programmable alarms and on-screen sound captions with direction (accessibility).
    /// </summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }
        const int MachineVoices = 8;
        const float HearRange = 16f;

        sealed class Caption { public string Text; public float Until; public Vector2 From; public bool HasPos; }

        GameSession session;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioSource musCalm, musNight, musIndustry, musDanger, ambA, ambB, weather, sfx;
        readonly AudioSource[] voices = new AudioSource[MachineVoices];
        readonly int[] voiceOwner = new int[MachineVoices];
        readonly Dictionary<int, float> nextBeep = new Dictionary<int, float>();
        readonly List<Caption> captions = new List<Caption>();
        string ambName;
        bool ambOnA = true;
        float lastHealth = 100, hurtCooldown, mood;
        int lastShots;
        GUIStyle capStyle;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Instance = null;

        AudioClip Clip(string name)
        {
            if (clips.TryGetValue(name, out var c)) return c;
            var data = Synth.Render(name);
            c = AudioClip.Create(name, data.Length, 1, Synth.Rate, false);
            c.SetData(data, 0);
            clips[name] = c;
            return c;
        }

        AudioSource Source(string name, bool loop)
        {
            var s = new GameObject("audio_" + name).AddComponent<AudioSource>();
            s.transform.SetParent(transform, false);
            s.loop = loop; s.playOnAwake = false; s.spatialBlend = 0f; s.volume = 0;
            return s;
        }

        public void Init(GameSession s)
        {
            session = s; Instance = this;
            musCalm = Source("mus_calm", true); musNight = Source("mus_night", true);
            musIndustry = Source("mus_industry", true); musDanger = Source("mus_danger", true);
            ComposeMusic();
            ambA = Source("ambA", true); ambB = Source("ambB", true); weather = Source("weather", true);
            sfx = Source("sfx", false);
            for (int i = 0; i < MachineVoices; i++) { voices[i] = Source("machine" + i, true); voiceOwner[i] = 0; }

            var st = s.State;
            st.Grid.Placed += b => Play("place", Pos(b), null);
            st.Lightning.Struck += (x, y, rod) => Play("thunder", new Vector2((float)x, (float)y), rod ? Loc.T("cap.rod", "[thunder] the lightning rod took a strike") : Loc.T("cap.lightning", "[thunder] lightning strike"));
            st.Grid.Removed += b => Play("remove", Pos(b), null);
            st.Creatures.Died += c => Play("dissolve", new Vector2((float)c.X, (float)c.Y), null);
            st.Creatures.WaveArrived += n => Play("alarm", null, Loc.T(n > 1 ? "cap.waveN" : "cap.wave1", n > 1 ? "[alarm] {0} creatures approaching" : "[alarm] a creature is approaching", n));
            st.Creatures.BuildingDestroyed += b => Play("error", Pos(b), Loc.T("cap.destroyed", "[crash] {0} destroyed", Loc.Building(b.Def)));
            st.Creatures.IntrusionStarted += (b, c) => Play("alarm", Pos(b), Loc.T("cap.intrusion", "[intrusion alarm] drone breaking into PC-{0}", b.InstanceId));
            st.Creatures.ComputerCompromised += b => Play("alarm", Pos(b), Loc.T("cap.compromised", "[security alarm] PC-{0} compromised", b.InstanceId));
            st.Research.Completed += t => Play("chime", null, Loc.T("cap.research", "[chime] research complete: {0}", Loc.Tech(t)));
        }

        static Vector2 Pos(PlacedBuilding b) => new Vector2((float)b.CenterX, (float)b.CenterY);
        float Master => Settings.MasterVolume;

        /// <summary>Plays a one-shot; with a world position it is panned and attenuated. Caption shown if enabled.</summary>
        public void Play(string name, Vector2? at, string caption)
        {
            float vol = Settings.EffectsVolume * Master, pan = 0;
            if (at.HasValue)
            {
                var d = at.Value - (Vector2)session.Player.position;
                if (d.magnitude > HearRange * 1.5f) vol = 0;
                vol *= Mathf.Clamp01(1f - d.magnitude / (HearRange * 1.5f));
                pan = Mathf.Clamp(d.x / 10f, -0.8f, 0.8f);
                vol *= Mathf.Pow(0.45f, session.Grid.StructuresBetween(at.Value.x, at.Value.y, session.Player.position.x, session.Player.position.y));   // walls muffle
            }
            if (vol > 0.001f) { sfx.panStereo = pan; sfx.PlayOneShot(Clip(name), vol); }
            if (caption != null && Settings.SoundCaptions)
            {
                captions.Add(new Caption { Text = caption, Until = Time.unscaledTime + 4f, From = at ?? Vector2.zero, HasPos = at.HasValue });
                if (captions.Count > 4) captions.RemoveAt(0);
            }
        }

        // ---------------------------------------------------------------- machines
        string LoopFor(PlacedBuilding b)
        {
            switch (b.Def.Kind)
            {
                case BuildingKind.Drill: return "drill";
                case BuildingKind.Generator: return "generator";
                case BuildingKind.Furnace: return "furnace";
                case BuildingKind.Assembler: return "assembler";
                case BuildingKind.Lab: return "lab";
                case BuildingKind.Computer: return "computer";
                case BuildingKind.Belt: case BuildingKind.Conveyor: return "conveyor";
                case BuildingKind.Greenhouse: return "greenhouse";
                case BuildingKind.Turret: return "turret";
                default: return null;
            }
        }

        /// <summary>0 = silent; machines sound different when working (full, normal pitch) and idle (quiet, lower).</summary>
        (float vol, float pitch) StateSound(PlacedBuilding b)
        {
            var f = session.Factory;
            if (b.State == null) return (0, 1);
            bool working = f.HasAction(b);
            switch (b.State)
            {
                case GeneratorState g: return g.LoadKW > 0 ? (0.5f + 0.5f * g.LoadKW / Factory.GeneratorKW, 0.9f + 0.2f * g.LoadKW / Factory.GeneratorKW) : (0, 1);
                case ComputerState c: return c.Running ? (f.HasPower(b) ? (0.35f, 1f) : (0, 1)) : (0, 1);
                case ConveyorState cv: return cv.Item != null && cv.Running ? (0.35f, 1f) : (0, 1);
                case GreenhouseState gh: return gh.Status != "empty" ? (0.4f, 1f) : (0, 1);
                case TurretState _: return f.HasPower(b) ? (0.2f, 1f) : (0, 1);
            }
            if (working) return b.State.PowerSatisfaction < 1 && b.Def.Kind != BuildingKind.Furnace ? (0.6f, 0.7f) : (1f, 1f); // brown-out: slower, deeper
            return (0, 1);
        }

        static bool Blocked(PlacedBuilding b)
        {
            string s = b.State?.Status;
            return s == "output full" || s == "no fuel" || s == "no power" || s == "blocked" || s == "target full";
        }

        void UpdateMachines()
        {
            var p = (Vector2)session.Player.position;
            var near = new List<(PlacedBuilding b, float d, float vol, float pitch, string clip)>();
            foreach (var b in session.Grid.Buildings)
            {
                float d = Vector2.Distance(Pos(b), p);
                if (d > HearRange) continue;
                string clip = LoopFor(b);
                if (clip == null) continue;
                var (vol, pitch) = StateSound(b);
                if (Blocked(b))
                {   // a stuck machine beeps every few seconds so you can hear what needs attention
                    if (!nextBeep.TryGetValue(b.InstanceId, out float t) || Time.time >= t)
                    {
                        nextBeep[b.InstanceId] = Time.time + 4f;
                        Play("beep", Pos(b), Loc.T("cap.beep", "[warning beep] {0}: {1}", Loc.Building(b.Def), Loc.T("st." + b.State.Status.Replace(' ', '_'), b.State.Status)));
                    }
                }
                if (vol > 0) near.Add((b, d, vol, pitch, clip));
            }
            near.Sort((x, y) => x.d.CompareTo(y.d));
            var keep = new HashSet<int>();
            for (int i = 0; i < near.Count && i < MachineVoices; i++) keep.Add(near[i].b.InstanceId);
            // free voices whose machine is no longer among the audible ones
            for (int v = 0; v < MachineVoices; v++)
                if (voiceOwner[v] != 0 && !keep.Contains(voiceOwner[v])) { voices[v].Stop(); voiceOwner[v] = 0; }
            for (int i = 0; i < near.Count && i < MachineVoices; i++)
            {
                var n = near[i];
                int v = System.Array.IndexOf(voiceOwner, n.b.InstanceId);
                if (v < 0)
                {
                    v = System.Array.IndexOf(voiceOwner, 0);
                    if (v < 0) break;
                    voiceOwner[v] = n.b.InstanceId;
                    voices[v].clip = Clip(n.clip);
                    voices[v].time = Random.value * voices[v].clip.length * 0.9f;   // desync identical machines
                    voices[v].Play();
                }
                float atten = Mathf.Clamp01(1f - n.d / HearRange);
                voices[v].volume = n.vol * atten * atten * 0.6f * Settings.MachineVolume * Master;
                voices[v].pitch = n.pitch;
                voices[v].panStereo = Mathf.Clamp(((float)n.b.CenterX - p.x) / 10f, -0.8f, 0.8f);
                // occlusion: each wall between the machine and the player halves the level and dulls the sound
                int walls = session.Grid.StructuresBetween(n.b.CenterX, n.b.CenterY, p.x, p.y, n.b);
                voices[v].volume *= Mathf.Pow(0.45f, walls);
                var lp = voices[v].GetComponent<AudioLowPassFilter>() ?? voices[v].gameObject.AddComponent<AudioLowPassFilter>();
                lp.cutoffFrequency = Mathf.Lerp(lp.cutoffFrequency, walls > 0 ? 900f : 22000f, 0.2f);
            }
        }

        // ---------------------------------------------------------------- ambience & music
        void UpdateAmbience(float dt)
        {
            var env = session.Surroundings;
            var biome = session.World.Generator.BiomeAt(GameSession.CellAt(session.Player.position));
            string want = "amb_" + biome.ToString().ToLowerInvariant();
            if (want != ambName)
            {   // cross-fade to the new biome
                ambName = want; ambOnA = !ambOnA;
                var to = ambOnA ? ambA : ambB;
                to.clip = Clip(want); to.volume = 0; to.Play();
            }
            var on = ambOnA ? ambA : ambB; var off = ambOnA ? ambB : ambA;
            float target = 0.35f * Settings.AmbientVolume * Master * (session.State.Clock.IsNight ? 0.7f : 1f);
            on.volume = Mathf.MoveTowards(on.volume, target, dt * 0.3f);
            off.volume = Mathf.MoveTowards(off.volume, 0, dt * 0.3f);
            if (off.volume <= 0 && off.isPlaying) off.Stop();

            string w = env.Weather == Weather.Storm ? "storm" : env.Rain ? "rain" : null;
            if (w != null && (weather.clip == null || weather.clip.name != w)) { weather.clip = Clip(w); weather.Play(); }
            weather.volume = Mathf.MoveTowards(weather.volume, w != null ? 0.45f * Settings.AmbientVolume * Master : 0, dt * 0.3f);
            if (w == null && weather.volume <= 0) weather.Stop();
        }

        void UpdateMusic(float dt)
        {
            var p = (Vector2)session.Player.position;
            bool danger = session.State.Vitals.Warning != "";
            foreach (var c in session.State.Creatures.All) if (c.Alive && Vector2.Distance(new Vector2((float)c.X, (float)c.Y), p) < 18) { danger = true; break; }
            int working = 0;
            foreach (var v in voiceOwner) if (v != 0) working++;
            float industry = Mathf.Clamp01(working / 4f);
            mood = Mathf.MoveTowards(mood, danger ? 1 : 0, dt * 0.4f);
            if (!musicStarted)
            {
                if (musicJob == null || !musicJob.IsCompleted) return;   // still composing in the background
                StartMusic();
            }
            float music = 0.3f * Settings.MusicVolume * Master;
            float day = Mathf.Clamp01(session.State.Clock.Daylight * 1.5f - 0.25f);   // the bells take over at dusk
            musCalm.volume = music * (1f - 0.7f * mood) * day;
            musNight.volume = music * (1f - 0.7f * mood) * (1f - day) * 1.1f;
            musIndustry.volume = music * industry * (1f - mood) * 0.8f;
            musDanger.volume = music * mood;
        }

        System.Threading.Tasks.Task<float[][]> musicJob;
        bool musicStarted;

        /// <summary>The four music layers are composed off the main thread (a few hundred ms), then start together.</summary>
        void ComposeMusic() => musicJob = System.Threading.Tasks.Task.Run(() => System.Array.ConvertAll(Music.Layers, Synth.Render));

        void StartMusic()
        {
            musicStarted = true;
            var data = musicJob.Result;
            var sources = new[] { musCalm, musNight, musIndustry, musDanger };
            for (int i = 0; i < Music.Layers.Length; i++)
            {
                var c = AudioClip.Create(Music.Layers[i], data[i].Length, 1, Synth.Rate, false);
                c.SetData(data[i], 0);
                clips[Music.Layers[i]] = c;
                sources[i].clip = c;
            }
            foreach (var m in sources) m.Play();   // same length and start: the layers stay in sync
        }

        void Update()
        {
            if (session == null) return;
            float dt = Time.unscaledDeltaTime;
            AudioListener.pause = OptionsMenu.IsOpen;   // the pause menu silences the world
            if (OptionsMenu.IsOpen) return;
            UpdateMachines();
            UpdateAmbience(dt);
            UpdateMusic(dt);

            // turret shots
            int shots = 0; PlacedBuilding shooter = null;
            foreach (var b in session.Grid.Buildings) if (b.State is TurretState ts) { shots += ts.Shots; if (ts.LastShotTime >= session.State.Clock.Seconds - 0.2) shooter = b; }
            if (shots > lastShots && shooter != null) Play("shot", Pos(shooter), Loc.T("cap.turret", "[zap] turret firing"));
            lastShots = shots;

            // the player getting hurt
            float hp = session.State.Vitals.Health;
            hurtCooldown -= dt;
            if (hp < lastHealth - 0.5f && hurtCooldown <= 0) { Play("hurt", null, Loc.T("cap.hurt", "[pain] you are hurt")); hurtCooldown = 1.2f; }
            lastHealth = hp;

            // programmable alarms
            var hud = session.State.Hud;
            while (hud.Alarms.Count > 0) Play("alarm", null, Loc.T("cap.programAlarm", "[program alarm] {0}", hud.Alarms.Dequeue()));
        }

        void OnGUI()
        {
            if (!Settings.SoundCaptions) return;
            captions.RemoveAll(c => Time.unscaledTime > c.Until);
            if (captions.Count == 0) return;
            UiGui.Begin();
            if (capStyle == null)
            {
                var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.85f)); bg.Apply();
                capStyle = new GUIStyle(GUI.skin.box) { richText = true, alignment = TextAnchor.MiddleCenter, wordWrap = true,
                    normal = { background = bg, textColor = Color.white } };
            }
            capStyle.fontSize = UiGui.Font(14);
            float y = UiGui.H * 0.62f;
            foreach (var c in captions)
            {
                string arrow = "";
                if (c.HasPos)
                {
                    var d = c.From - (Vector2)session.Player.position;
                    if (d.magnitude > 2f) arrow = Mathf.Abs(d.x) > Mathf.Abs(d.y) ? (d.x < 0 ? "<< left: " : "right >> ") : (d.y > 0 ? "^ above: " : "v below: ");
                }
                GUI.Box(new Rect(UiGui.W / 2 - 220, y, 440, 26), arrow + c.Text, capStyle);
                y += 30;
            }
        }

        void OnDestroy() { if (Instance == this) Instance = null; AudioListener.pause = false; }
    }
}
