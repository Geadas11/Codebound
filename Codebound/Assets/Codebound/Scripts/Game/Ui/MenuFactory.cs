using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// The working production line of the main-menu stage (visual only; the game's simulation is untouched).
    ///
    ///  Front belt ─ ore + crates ─▶ A-03 (takes the crates: press cycle, lamp, vibration, steam)
    ///       └─ arm A picks the ore ─▶ Incline belt ─▶ High belt ─▶ out
    ///  Mid belt ─ plates + coils ─▶ out
    ///       ├─ arm B picks coils ─▶ Incline belt
    ///       └─ arm C picks plates ─▶ furnace B (flare, sparks, lamp)
    ///
    /// Items are pooled (no instantiation after start) and always visible on their way: they come out of a portal,
    /// ride the belts (accumulating when the belt ahead is blocked), cross to the next belt, are gripped and carried by
    /// an arm, or disappear into a machine or portal. A belt brakes and waits while an arm picks from it. Arms use
    /// analytic two-bone IK with eased phases; machines run their own cycles at their own pace; vibration is local,
    /// tiny and comes from events (a press, a drop, a running belt). Everything uses unscaled time (the title runs
    /// with timeScale 0) and stops with "reduce motion".
    /// </summary>
    public sealed class MenuFactory
    {
        // ------------------------------------------------------------------ items
        sealed class Kind { public string name; public Transform proto; public float halfH, halfW; public readonly Stack<Item> pool = new Stack<Item>(); }
        sealed class Item { public Transform t; public Kind kind; public Belt belt; public float d, jitter; public bool held; }

        sealed class Belt
        {
            public string name; public Vector3 a, b, dir, side; public float len;
            public float baseSpeed, speed, phase; public int holds;
            public Belt next; public float nextAt; public Vector3 nextEntry; public float bridge;   // the short hop onto the next belt
            public bool spawn, sink; public Kind[] kinds; public float gap, nextGap; public int spawnCount;
            public readonly List<Item> items = new List<Item>();   // front (largest d) first
            public readonly List<Transform> rollers = new List<Transform>();
            public Material surface; public float travel;
            public Shaker frame;
        }

        sealed class Shaker
        {
            public Transform t; public Vector3 basePos; public float hum, impulse, freq = 31f, seed;
            public void Kick(float amount) => impulse = Mathf.Max(impulse, amount);
        }

        // ------------------------------------------------------------------ machines
        sealed class Machine
        {
            public string name; public Shaker body; public Transform[] rods; public Vector3[] rodBase; public Renderer lamp; public Material lampMat;
            public Renderer beacon; public Light fire; public float fireBase; public Material glow; public Color glowBase;
            public float work, workLen = 3.2f, idleUntil, flare; public bool autonomous; public float minIdle, maxIdle;
            public Vector3 mouth, steamAt; public int produced; public float faultUntil;
        }

        // ------------------------------------------------------------------ arms
        enum Phase { Wait, Brake, Reach, Descend, Grip, Lift, Carry, Lower, Release, Rise, Return }

        sealed class Arm
        {
            public string name; public Transform turret, shoulder, elbow, wrist; public Transform[] fingers; public Renderer led; public Material ledMat;
            public Vector3 basePos; public Belt src; public float pickD; public Kind want;
            public Belt dst; public float placeD; public Machine feed;
            public Phase phase; public float time, dur, idle;
            public Vector3 from, to, home; public Item item; public float grip = 1;   // 1 open, 0 closed on the item
            public Vector3 wrist0;   // wrist target now
        }

        const float L1 = 1.7f, L2 = 1.4f, ShoulderUp = 0.5f, GripReach = 0.36f;

        readonly MenuStage stage;
        readonly Transform root, holder;
        readonly Dictionary<string, Kind> kinds = new Dictionary<string, Kind>();
        readonly List<Belt> belts = new List<Belt>();
        readonly List<Arm> arms = new List<Arm>();
        readonly List<Machine> machines = new List<Machine>();
        readonly List<Shaker> shakers = new List<Shaker>();
        readonly List<(Transform t, Quaternion rest, float amp, float speed, float seed)> sways = new List<(Transform, Quaternion, float, float, float)>();
        readonly System.Random rnd = new System.Random(7);
        ParticleSystem sparks, steam, embers, dust;
        public string Status { get; private set; } = "";
        int oreLifted, coilsMoved, platesSmelted, cratesBuilt;

        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        public MenuFactory(MenuStage stage, Transform root)
        {
            this.stage = stage; this.root = root;
            holder = new GameObject("Factory Items").transform;
            holder.SetParent(stage.transform, false);
            Kinds();
            Belts();
            Machines();
            Arms();
            Sways();
            Particles();
            Prime();
        }

        static Transform Find(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
            return null;
        }

        // ------------------------------------------------------------------ setup
        void Kinds()
        {
            foreach (var (n, h, w) in new[] { ("crate", 0.25f, 0.25f), ("ore", 0.18f, 0.24f), ("plate", 0.04f, 0.3f), ("coil", 0.22f, 0.19f) })
            {
                var p = Find(root, "Proto_Item_" + n);
                if (p == null) continue;
                p.gameObject.SetActive(false);
                kinds[n] = new Kind { name = n, proto = p, halfH = h, halfW = w };
            }
        }

        Item Take(Kind k)
        {
            Item it = k.pool.Count > 0 ? k.pool.Pop() : new Item { kind = k, t = UnityEngine.Object.Instantiate(k.proto.gameObject, holder).transform };
            it.t.gameObject.SetActive(true); it.held = false; it.belt = null; it.jitter = R(-8, 8);
            it.t.SetParent(holder, true);
            return it;
        }

        void Give(Item it) { it.t.gameObject.SetActive(false); it.belt = null; it.t.SetParent(holder, true); it.kind.pool.Push(it); }

        Shaker Shake(Transform t, float hum, float freq)
        {
            if (t == null) return null;
            var s = new Shaker { t = t, basePos = t.localPosition, hum = hum, freq = freq, seed = R(0, 100) };
            shakers.Add(s);
            return s;
        }

        void Belts()
        {
            var surfaceMat = BeltMaterial();
            Belt Make(string name, float speed, bool spawn, bool sink, string[] kn, float gap)
            {
                Transform A = null, B = Find(root, "Mark_Belt_" + name + "_B");
                foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith("Mark_Belt_" + name + "_A")) A = t;
                if (A == null || B == null) return null;
                var b = new Belt { name = name, a = A.position, b = B.position, baseSpeed = speed, speed = speed, spawn = spawn, sink = sink, gap = gap, phase = R(0, 6) };
                b.len = (b.b - b.a).magnitude; b.dir = (b.b - b.a) / b.len;
                b.side = Vector3.Cross(Vector3.up, b.dir).normalized;
                var list = new List<Kind>(); foreach (var k in kn) if (kinds.TryGetValue(k, out var kk)) list.Add(kk);
                b.kinds = list.ToArray();
                b.nextGap = gap;
                foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith("Roller_" + name + "_")) b.rollers.Add(t);
                var surf = Find(root, "BeltSurface_" + name);
                if (surf != null && surf.TryGetComponent<Renderer>(out var sr)) { b.surface = new Material(surfaceMat); b.surface.SetTextureScale("_BaseMap", new Vector2(b.len / 0.45f, 1)); sr.sharedMaterial = b.surface; }
                b.frame = Shake(Find(root, "Grp_" + name), 0.0004f, 38f);
                belts.Add(b);
                return b;
            }
            var front = Make("Front", 0.62f, true, true, new[] { "ore", "crate" }, 1.25f);
            var mid = Make("Mid", 0.55f, true, true, new[] { "plate", "coil", "plate" }, 1.1f);
            var high = Make("High", 0.7f, true, true, new[] { "crate", "plate" }, 1.6f);
            var incline = Make("Incline", 0.6f, false, false, new string[0], 0.9f);
            if (incline != null && high != null)
            {   // the incline hands over to the high belt where they meet
                incline.next = high;
                incline.nextAt = Mathf.Clamp(Vector3.Dot(incline.b - high.a, high.dir), 0, high.len);
                incline.nextEntry = high.a + high.dir * incline.nextAt;
                incline.bridge = Mathf.Max(0.05f, (incline.nextEntry - incline.b).magnitude);
            }
        }

        static Material BeltMaterial()
        {
            var tex = new Texture2D(64, 8, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < 8; y++) for (int x = 0; x < 64; x++)
                    tex.SetPixel(x, y, x < 10 ? new Color(0.13f, 0.13f, 0.14f) : new Color(0.035f, 0.035f, 0.04f));
            tex.Apply(true);
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetTexture("_BaseMap", tex); m.SetColor("_BaseColor", Color.white); m.SetFloat("_Metallic", 0); m.SetFloat("_Smoothness", 0.3f);
            return m;
        }

        void Machines()
        {
            Machine M(string name, bool autonomous, float minIdle, float maxIdle)
            {
                var m = new Machine { name = name, autonomous = autonomous, minIdle = minIdle, maxIdle = maxIdle, idleUntil = Time.unscaledTime + R(1, maxIdle) };
                m.body = Shake(Find(root, "Grp_" + name), 0.0006f, 26f);
                var rods = new List<Transform>();
                for (int k = 0; k < 2; k++) { var r = Find(root, "PistonRod_" + name + "_" + k); if (r != null) rods.Add(r); }
                m.rods = rods.ToArray(); m.rodBase = Array.ConvertAll(m.rods, r => r.localPosition);
                var lamp = Find(root, "Status_" + name);
                if (lamp != null && lamp.TryGetComponent<Renderer>(out var lr)) { m.lamp = lr; m.lampMat = new Material(lr.sharedMaterial); lr.sharedMaterial = m.lampMat; m.lampMat.EnableKeyword("_EMISSION"); m.lampMat.SetColor("_BaseColor", new Color(0.04f, 0.04f, 0.04f)); }   // dark glass: the light is the colour
                var beacon = Find(root, name + "Beacon");
                if (beacon != null) m.beacon = beacon.GetComponent<Renderer>();
                foreach (var l in root.GetComponentsInChildren<Light>(true)) if (l.name == name + "Fire") { m.fire = l; m.fireBase = l.intensity; }
                var mouth = Find(root, "Mark_Mouth_" + name);
                if (mouth != null) m.mouth = mouth.position;
                if (m.body != null) m.steamAt = m.body.t.position + Vector3.up * 4.6f;
                machines.Add(m);
                return m;
            }
            M("AsmHero", false, 0, 0);
            M("AsmB", true, 4, 9);
            M("AsmFar", true, 6, 12);
            M("FurnA", true, 5, 11);
            M("FurnB", false, 0, 0);
            M("FurnC", true, 7, 13);
            // the furnaces share one glow material: each flare lifts it a little
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                foreach (var mat in r.sharedMaterials)
                    if (mat != null && mat.name.StartsWith("FurnaceGlow")) foreach (var m in machines) if (m.name.StartsWith("Furn") && m.glow == null) { m.glow = mat; m.glowBase = mat.GetColor("_EmissionColor"); }
        }

        Machine Get(string name) => machines.Find(m => m.name == name);

        void Arms()
        {
            var ap = new Dictionary<string, Transform>();
            foreach (var n in new[] { "Turret", "TurretCap", "Joint", "Upper", "Fore", "Wrist", "Finger", "Led" })
            { var p = Find(root, "Proto_Arm" + n); if (p != null) { ap[n] = p; p.gameObject.SetActive(false); } }
            foreach (var n in new[] { "Cable" }) { var p = Find(root, "Proto_Arm" + n); if (p != null) p.gameObject.SetActive(false); }
            Belt B(string n) => belts.Find(b => b.name == n);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("Mark_Arm_")) continue;
                var arm = Build(t, ap);
                switch (arm.name)
                {
                    case "ArmA": Route(arm, B("Front"), kinds.GetValueOrDefault("ore"), B("Incline"), null); break;
                    case "ArmB": Route(arm, B("Mid"), kinds.GetValueOrDefault("coil"), B("Incline"), null); break;
                    case "ArmC": Route(arm, B("Mid"), kinds.GetValueOrDefault("plate"), null, Get("FurnB")); break;
                }
                if (arm.src != null) arms.Add(arm);
            }
        }

        static float Project(Belt b, Vector3 p) => Mathf.Clamp(Vector3.Dot(p - b.a, b.dir), 0.6f, b.len - 0.6f);

        void Route(Arm a, Belt src, Kind want, Belt dst, Machine feed)
        {
            if (src == null || want == null) return;
            a.src = src; a.want = want; a.dst = dst; a.feed = feed;
            a.pickD = Project(src, a.basePos);
            if (dst != null) a.placeD = Project(dst, a.basePos);
            var pick = src.a + src.dir * a.pickD;
            a.home = Vector3.Lerp(a.basePos, pick, 0.55f) + Vector3.up * 1.55f;
            a.wrist0 = a.home;
            a.phase = Phase.Wait; a.idle = R(0, 1);
        }

        Transform Clone(Dictionary<string, Transform> ap, string n, Transform parent, Vector3 localPos)
        {
            var p = ap[n];
            var c = UnityEngine.Object.Instantiate(p.gameObject).transform;
            c.gameObject.SetActive(true);
            c.SetParent(parent, false);
            c.localPosition = localPos;
            c.localRotation = Quaternion.Inverse(parent.rotation) * p.rotation;   // keep the part's upright import orientation
            return c;
        }

        Arm Build(Transform mark, Dictionary<string, Transform> ap)
        {
            var parts = mark.name.Split('_');
            var arm = new Arm { name = parts[2], basePos = mark.position };
            arm.turret = new GameObject(arm.name + "Turret").transform; arm.turret.SetParent(stage.transform, false); arm.turret.position = mark.position;
            Clone(ap, "Turret", arm.turret, Vector3.zero);
            Clone(ap, "TurretCap", arm.turret, Vector3.zero);
            arm.shoulder = new GameObject("Shoulder").transform; arm.shoulder.SetParent(arm.turret, false); arm.shoulder.localPosition = new Vector3(0, ShoulderUp, 0);
            Clone(ap, "Joint", arm.shoulder, Vector3.zero);
            Clone(ap, "Upper", arm.shoulder, Vector3.zero);
            arm.elbow = new GameObject("Elbow").transform; arm.elbow.SetParent(arm.shoulder, false); arm.elbow.localPosition = new Vector3(0, L1, 0);
            Clone(ap, "Joint", arm.elbow, Vector3.zero).localScale *= 0.85f;
            Clone(ap, "Fore", arm.elbow, Vector3.zero);
            arm.wrist = new GameObject("Wrist").transform; arm.wrist.SetParent(arm.elbow, false); arm.wrist.localPosition = new Vector3(0, L2, 0);
            Clone(ap, "Wrist", arm.wrist, Vector3.zero);
            arm.fingers = new[] { Clone(ap, "Finger", arm.wrist, new Vector3(-0.3f, 0.12f, 0)), Clone(ap, "Finger", arm.wrist, new Vector3(0.3f, 0.12f, 0)) };
            var led = Clone(ap, "Led", arm.wrist, new Vector3(0, 0.12f, 0.17f));
            if (led.TryGetComponent<Renderer>(out var lr)) { arm.led = lr; arm.ledMat = new Material(lr.sharedMaterial); lr.sharedMaterial = arm.ledMat; arm.ledMat.EnableKeyword("_EMISSION"); arm.ledMat.SetColor("_BaseColor", new Color(0.04f, 0.04f, 0.04f)); }
            foreach (var t in arm.turret.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = MenuStage.Layer;
            return arm;
        }

        void Sways()
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("Sway_")) continue;
                bool cable = t.name.EndsWith("Cable");
                sways.Add((t, t.localRotation, cable ? 0.55f : 0.06f, cable ? R(0.5f, 0.8f) : R(5f, 7f), R(0, 10)));
            }
        }

        /// <summary>The line starts already running: belts carry items from the first frame.</summary>
        void Prime()
        {
            foreach (var b in belts)
            {
                if (!b.spawn || b.kinds.Length == 0) continue;
                for (float d = b.len - 0.5f; d > -0.5f; d -= b.gap * R(0.9f, 1.6f))
                {
                    var it = Take(b.kinds[b.spawnCount++ % b.kinds.Length]);
                    it.belt = b; it.d = d; b.items.Add(it);
                }
            }
        }

        // ------------------------------------------------------------------ particles (unscaled time: the title runs paused)
        void Particles()
        {
            var shader = Resources.Load<Shader>("MenuStage/SoftParticle");
            if (shader == null) return;
            ParticleSystem Make(string name, Color c, bool additive)
            {
                var go = new GameObject(name) { layer = MenuStage.Layer };
                go.transform.SetParent(stage.transform, false);
                var ps = go.AddComponent<ParticleSystem>();
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = ps.main; main.useUnscaledTime = true; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World; main.startColor = c;
                var em = ps.emission; em.rateOverTime = 0;
                var mat = new Material(shader); mat.SetFloat("_DstBlend", additive ? 1 : 10);
                var pr = go.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = mat; pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                return ps;
            }
            sparks = Make("Sparks", new Color(1f, 0.6f, 0.2f) * 3f, true);
            { var m = sparks.main; m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.6f); m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f); m.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f); m.gravityModifier = 0.9f; m.maxParticles = 80;
              var sh = sparks.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 35; sh.radius = 0.1f; }
            steam = Make("Steam", new Color(0.8f, 0.78f, 0.75f, 0.22f), false);
            { var m = steam.main; m.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.6f); m.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.2f); m.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f); m.maxParticles = 60;
              var sz = steam.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0.6f, 1, 2.4f));
              var col = steam.colorOverLifetime; col.enabled = true; var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.15f), new GradientAlphaKey(0, 1) }); col.color = g;
              var sh = steam.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 12; sh.radius = 0.12f; sh.rotation = new Vector3(-90, 0, 0); }
            embers = Make("Embers", new Color(1f, 0.5f, 0.15f) * 2.2f, true);
            { var m = embers.main; m.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f); m.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f); m.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.05f); m.gravityModifier = -0.05f; m.maxParticles = 40;
              var no = embers.noise; no.enabled = true; no.strength = 0.35f; no.frequency = 0.6f; }
            dust = Make("Dust", new Color(1f, 0.85f, 0.65f, 0.35f), false);
            { var m = dust.main; m.startLifetime = 14f; m.startSpeed = 0.02f; m.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.04f); m.maxParticles = 160; m.prewarm = true; m.loop = true;
              var em = dust.emission; em.rateOverTime = 11;
              var sh = dust.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(16, 6, 14);
              var no = dust.noise; no.enabled = true; no.strength = 0.08f; no.frequency = 0.15f;
              var hero = Get("AsmHero"); dust.transform.position = hero?.body != null ? hero.body.t.position + new Vector3(4, 3, 3) : stage.transform.position + new Vector3(0, 3, -6); }
            foreach (var ps in new[] { sparks, steam, embers, dust }) ps.Play();
        }

        void Burst(ParticleSystem ps, Vector3 at, Vector3 dir, int n)
        {
            if (ps == null || !Ui.Animate) return;
            ps.transform.position = at;
            if (dir != Vector3.zero) ps.transform.rotation = Quaternion.LookRotation(dir);
            ps.Emit(n);
        }

        // ------------------------------------------------------------------ per frame
        public void Tick(float dt, float t, bool motion)
        {
            if (!motion) { foreach (var s in shakers) s.t.localPosition = s.basePos; return; }
            foreach (var b in belts) MoveBelt(b, dt, t);
            foreach (var a in arms) RunArm(a, dt);
            foreach (var m in machines) RunMachine(m, dt, t);
            foreach (var s in shakers)
            {
                s.impulse = Mathf.MoveTowards(s.impulse, 0, dt * s.impulse * 5f + dt * 0.0005f);
                float amp = s.hum + s.impulse, w = s.freq * t;
                s.t.localPosition = s.basePos + new Vector3(Mathf.Sin(w + s.seed) * 0.35f, Mathf.Sin(w * 1.31f + s.seed * 2), Mathf.Sin(w * 0.87f + s.seed * 3) * 0.35f) * amp;
            }
            foreach (var (tr, rest, amp, speed, seed) in sways)
                tr.localRotation = rest * Quaternion.Euler(Mathf.Sin(t * speed + seed) * amp, 0, Mathf.Sin(t * speed * 0.73f + seed * 2) * amp * 0.6f);
            if ((rndEmber -= dt) < 0) { rndEmber = R(0.25f, 0.7f); var f = machines[3 + rnd.Next(3)]; if (f.mouth != Vector3.zero) Burst(embers, f.mouth + Vector3.up * 0.3f + Vector3.back * 0.1f, Vector3.up, 1); }
            int sum = oreLifted + coilsMoved + platesSmelted + cratesBuilt;
            if (sum != shown) { shown = sum; Status = Codebound.Core.Loc.T("menu.factoryStatus", "> ore {0}  coils {1}  smelted {2}  built {3}", oreLifted, coilsMoved, platesSmelted, cratesBuilt); }
        }

        float rndEmber; int shown = -1;

        void MoveBelt(Belt b, float dt, float t)
        {
            float target = b.holds > 0 ? 0 : b.baseSpeed * (1 + 0.05f * Mathf.Sin(t * 0.37f + b.phase));
            b.speed = Mathf.MoveTowards(b.speed, target, dt * (target < b.speed ? 1.6f : 0.6f));   // brakes firmly, starts gently
            b.travel += b.speed * dt;
            if (b.surface != null) b.surface.SetTextureOffset("_BaseMap", new Vector2(-b.travel / 0.45f, 0));
            float spin = b.speed * dt / 0.075f * Mathf.Rad2Deg;
            foreach (var r in b.rollers) r.Rotate(b.side, spin, Space.World);
            if (b.frame != null) b.frame.hum = 0.00025f + 0.0006f * (b.speed / Mathf.Max(0.01f, b.baseSpeed));

            // items: the front one moves freely; the others keep their gap (they accumulate when the front stops)
            float limit = float.MaxValue;
            for (int i = 0; i < b.items.Count; i++)
            {
                var it = b.items[i];
                float d = Mathf.Min(it.d + b.speed * dt, limit);
                float end = b.next != null ? b.len + b.bridge : b.sink ? b.len + 1.1f : b.len - 0.25f;
                if (b.next != null && d >= end)
                {
                    if (Free(b.next, b.nextAt, it.kind.halfW + 0.35f)) { b.items.RemoveAt(i); i--; Insert(b.next, it, b.nextAt); continue; }
                    d = end;   // wait for room on the next belt
                }
                if (d >= end && b.sink)
                {
                    b.items.RemoveAt(i); i--;
                    var m = b.name == "Front" ? Get("AsmHero") : null;
                    if (m != null && it.kind.name == "crate") Feed(m);
                    Give(it); continue;
                }
                it.d = Mathf.Min(d, end);
                Place(b, it);
                limit = it.d - Mathf.Max(0.55f, (it.kind.halfW + 0.3f) * 1.4f);
            }
            if (b.spawn && b.kinds.Length > 0)
            {
                float rear = b.items.Count > 0 ? b.items[b.items.Count - 1].d : float.MaxValue;
                if (rear > -0.9f + b.nextGap)
                {
                    var it = Take(b.kinds[b.spawnCount++ % b.kinds.Length]);
                    it.d = -0.9f; it.belt = b; b.items.Add(it); Place(b, it);
                    b.nextGap = b.gap * R(0.85f, 1.9f);   // natural spacing
                }
            }
        }

        void Place(Belt b, Item it)
        {
            Vector3 p;
            if (it.d > b.len && b.next != null) p = Vector3.Lerp(b.b, b.nextEntry, (it.d - b.len) / b.bridge);   // the hop onto the next belt
            else p = b.a + b.dir * it.d;
            it.t.position = p;
            it.t.rotation = Quaternion.LookRotation(b.dir, Vector3.Cross(b.dir, b.side)) * Quaternion.Euler(0, it.jitter, 0) * it.kind.proto.rotation;
        }

        static bool Free(Belt b, float d, float clearance)
        {
            foreach (var o in b.items) if (Mathf.Abs(o.d - d) < clearance + o.kind.halfW) return false;
            return true;
        }

        static void Insert(Belt b, Item it, float d)
        {
            it.belt = b; it.d = d; it.held = false;
            int i = 0; while (i < b.items.Count && b.items[i].d > d) i++;
            b.items.Insert(i, it);
        }

        // ------------------------------------------------------------------ arms
        /// <summary>Two-bone IK: turret yaw, shoulder and elbow pitch reach `w` (wrist), the wrist keeps the gripper vertical.</summary>
        void Solve(Arm a, Vector3 w)
        {
            var s = a.basePos + Vector3.up * ShoulderUp;
            var flat = new Vector3(w.x - s.x, 0, w.z - s.z);
            float h = flat.magnitude, v = w.y - s.y;
            float yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
            float D = Mathf.Clamp(Mathf.Sqrt(h * h + v * v), 0.35f, L1 + L2 - 0.02f);
            float phi = Mathf.Atan2(h, v);
            float alpha = Mathf.Acos(Mathf.Clamp((L1 * L1 + D * D - L2 * L2) / (2 * L1 * D), -1, 1));
            float beta = Mathf.Acos(Mathf.Clamp((L1 * L1 + L2 * L2 - D * D) / (2 * L1 * L2), -1, 1));
            float sh = (phi - alpha) * Mathf.Rad2Deg, el = (Mathf.PI - beta) * Mathf.Rad2Deg;
            a.turret.rotation = Quaternion.Euler(0, yaw, 0);
            a.shoulder.localRotation = Quaternion.Euler(sh, 0, 0);
            a.elbow.localRotation = Quaternion.Euler(el, 0, 0);
            a.wrist.localRotation = Quaternion.Euler(180 - sh - el, 0, 0);
        }

        /// <summary>Moves the wrist target from → to: around the turret (cylindrical) for long swings, straight for short ones.</summary>
        Vector3 Path(Arm a, Vector3 from, Vector3 to, float k)
        {
            var c = a.basePos;
            Vector3 f = from - c, g = to - c;
            float af = Mathf.Atan2(f.x, f.z), ag = Mathf.Atan2(g.x, g.z);
            float rf = new Vector2(f.x, f.z).magnitude, rg = new Vector2(g.x, g.z).magnitude;
            float ang = af + Mathf.DeltaAngle(af * Mathf.Rad2Deg, ag * Mathf.Rad2Deg) * Mathf.Deg2Rad * k;
            float r = Mathf.Lerp(rf, rg, k), y = Mathf.Lerp(from.y, to.y, k) + Mathf.Sin(k * Mathf.PI) * (Mathf.Abs(ag - af) > 0.6f ? 0.25f : 0);
            return new Vector3(c.x + Mathf.Sin(ang) * r, y, c.z + Mathf.Cos(ang) * r);
        }

        static float Ease(float k) => k * k * (3 - 2 * k);
        static float EaseInOut(float k) => k < 0.5f ? 4 * k * k * k : 1 - Mathf.Pow(-2 * k + 2, 3) / 2;

        void Go(Arm a, Phase p, Vector3 to, float dur) { a.phase = p; a.from = a.wrist0; a.to = to; a.dur = dur; a.time = 0; }

        Vector3 GraspPoint(Item it) => it.t.position + Vector3.up * (it.kind.halfH + 0.06f + GripReach * 0.62f);

        void RunArm(Arm a, float dt)
        {
            a.time += dt;
            float k = a.dur > 0 ? Mathf.Clamp01(a.time / a.dur) : 1;
            switch (a.phase)
            {
                case Phase.Wait:
                {
                    a.idle -= dt;
                    var it = Candidate(a);
                    if (it != null && a.idle <= 0)
                    {
                        float stopDist = a.src.speed * a.src.speed / (2 * 1.6f);
                        if (it.d >= a.pickD - stopDist - 0.05f) { a.item = it; a.src.holds++; a.phase = Phase.Brake; a.time = 0; }
                    }
                    var rest = a.home + Vector3.up * Mathf.Sin(Time.unscaledTime * 1.3f + a.basePos.x) * 0.02f;
                    if (it != null)
                    {   // the arm has seen the part coming: it turns and leans towards it as it approaches
                        float near = Mathf.Clamp01(1 - (a.pickD - it.d) / 3f);
                        rest = Vector3.Lerp(rest, GraspPoint(it) + Vector3.up * 0.8f, Ease(near) * 0.6f);
                    }
                    a.wrist0 = Vector3.Lerp(a.wrist0, rest, 1 - Mathf.Exp(-dt * 4));
                    break;
                }
                case Phase.Brake:   // the belt stops under the gripper; meanwhile the arm leans towards the part
                    a.wrist0 = Vector3.Lerp(a.wrist0, GraspPoint(a.item) + Vector3.up * 0.55f, 1 - Mathf.Exp(-dt * 2.5f));
                    if (a.src.speed <= 0.01f) Go(a, Phase.Reach, GraspPoint(a.item) + Vector3.up * 0.45f, 0.45f);
                    break;
                case Phase.Reach:
                    a.wrist0 = Path(a, a.from, a.to, EaseInOut(k));
                    if (k >= 1) Go(a, Phase.Descend, GraspPoint(a.item), 0.38f);
                    break;
                case Phase.Descend:
                    a.wrist0 = Vector3.Lerp(a.from, a.to, EaseInOut(k));
                    if (k >= 1) { a.phase = Phase.Grip; a.time = 0; a.dur = 0.22f; }
                    break;
                case Phase.Grip:
                    a.grip = 1 - Ease(k);
                    if (k >= 1)
                    {   // the part leaves the belt with the gripper; the belt starts again
                        a.src.items.Remove(a.item); a.src.holds = Mathf.Max(0, a.src.holds - 1);
                        a.item.held = true; a.item.belt = null; a.item.t.SetParent(a.wrist, true);
                        a.src.frame?.Kick(0.0012f);
                        Go(a, Phase.Lift, a.wrist0 + Vector3.up * 0.5f, 0.4f);
                    }
                    break;
                case Phase.Lift:
                    a.wrist0 = Vector3.Lerp(a.from, a.to, EaseInOut(k));
                    if (k >= 1)
                    {
                        var place = PlacePoint(a) + Vector3.up * 0.5f;
                        float swing = Vector3.Angle(Vector3.ProjectOnPlane(a.from - a.basePos, Vector3.up), Vector3.ProjectOnPlane(place - a.basePos, Vector3.up));
                        Go(a, Phase.Carry, place, Mathf.Lerp(0.8f, 1.5f, swing / 180f));
                    }
                    break;
                case Phase.Carry:
                    a.wrist0 = Path(a, a.from, a.to, EaseInOut(k));
                    AlignHeld(a, k);
                    if (k >= 1)
                    {
                        if (a.dst != null && !Free(a.dst, a.placeD, a.item.kind.halfW + 0.4f)) { a.time = a.dur; break; }   // hold until the target belt has room
                        Go(a, Phase.Lower, PlacePoint(a), 0.42f);
                    }
                    break;
                case Phase.Lower:
                    a.wrist0 = Vector3.Lerp(a.from, a.to, EaseInOut(k));
                    AlignHeld(a, 1);
                    if (k >= 1) { a.phase = Phase.Release; a.time = 0; a.dur = 0.2f; }
                    break;
                case Phase.Release:
                    a.grip = Ease(k);
                    if (k >= 1)
                    {
                        var it = a.item; a.item = null;
                        it.t.SetParent(holder, true);
                        if (a.dst != null)
                        {
                            Insert(a.dst, it, a.placeD); Place(a.dst, it);
                            a.dst.frame?.Kick(it.kind.name == "crate" || it.kind.name == "coil" ? 0.004f : 0.0025f);   // a heavy part lands
                            if (a.name == "ArmA") oreLifted++; else coilsMoved++;
                        }
                        else if (a.feed != null) { Swallow(it, a.feed); }
                        Go(a, Phase.Rise, a.wrist0 + Vector3.up * 0.45f, 0.32f);
                    }
                    break;
                case Phase.Rise:
                    a.wrist0 = Vector3.Lerp(a.from, a.to, EaseInOut(k));
                    if (k >= 1) Go(a, Phase.Return, a.home, 0.9f);
                    break;
                case Phase.Return:
                    a.wrist0 = Path(a, a.from, a.to, EaseInOut(k));
                    if (k >= 1) { a.phase = Phase.Wait; a.idle = R(0.2f, 1.4f); }
                    break;
            }
            Solve(a, a.wrist0);
            // fingers open wide, close on the part's width
            float half = a.item != null ? a.item.kind.halfW + 0.03f : 0.3f;
            float x = Mathf.Lerp(half, 0.3f, a.grip);
            a.fingers[0].localPosition = new Vector3(-x, 0.12f, 0); a.fingers[1].localPosition = new Vector3(x, 0.12f, 0);
            if (a.ledMat != null)
            {
                var c = a.phase == Phase.Wait ? new Color(0.3f, 1f, 0.4f) * 2.5f : a.item != null ? new Color(1f, 0.55f, 0.1f) * 4f : new Color(1f, 0.85f, 0.5f) * 3f;
                a.ledMat.SetColor("_EmissionColor", c);
            }
        }

        Item Candidate(Arm a)
        {
            foreach (var it in a.src.items)
                if (it.kind == a.want && !it.held && it.d <= a.pickD + 0.05f && it.d > a.pickD - 3f) return it;
            return null;
        }

        Vector3 PlacePoint(Arm a)
        {
            float lift = (a.item?.kind.halfH ?? 0.2f) + 0.06f + GripReach * 0.62f;
            if (a.dst != null) return a.dst.a + a.dst.dir * a.placeD + Vector3.up * lift;
            if (a.feed != null) return a.feed.mouth + Vector3.up * (lift + 0.1f) + (a.basePos - a.feed.mouth).normalized * 0.25f;
            return a.home;
        }

        void AlignHeld(Arm a, float k)
        {
            if (a.item == null || a.dst == null) return;   // a part going into a furnace keeps its angle
            var want = Quaternion.LookRotation(a.dst.dir, Vector3.Cross(a.dst.dir, a.dst.side)) * a.item.kind.proto.rotation;
            a.item.t.rotation = Quaternion.Slerp(a.item.t.rotation, want, Mathf.Clamp01(k));
        }

        // ------------------------------------------------------------------ machines
        void Feed(Machine m)
        {
            if (m.work > 0) m.produced++; else m.work = m.workLen;
            if (m.name == "AsmHero") cratesBuilt++;
        }

        readonly List<(Item it, Vector3 from, Vector3 to, float k, Machine m)> swallowed = new List<(Item, Vector3, Vector3, float, Machine)>();

        void Swallow(Item it, Machine m)
        {
            swallowed.Add((it, it.t.position, m.mouth + Vector3.forward * 0, 0, m));
        }

        void RunMachine(Machine m, float dt, float t)
        {
            // parts pushed into a furnace slide in, then the furnace flares
            for (int i = swallowed.Count - 1; i >= 0; i--)
            {
                var s = swallowed[i];
                if (s.m != m) continue;
                s.k += dt / 0.45f;
                s.it.t.position = Vector3.Lerp(s.from, s.to + (s.to - s.from).normalized * 0.4f, Ease(Mathf.Clamp01(s.k)));
                if (s.k >= 1)
                {
                    Give(s.it); swallowed.RemoveAt(i);
                    m.work = m.workLen; m.flare = 1; platesSmelted++;
                    Burst(sparks, m.mouth, (Vector3.up + (s.from - s.to).normalized).normalized, 14);
                    m.body?.Kick(0.0015f);
                }
                else swallowed[i] = s;
            }
            if (m.autonomous && m.work <= 0 && t > m.idleUntil) { m.work = m.workLen; m.flare = m.name.StartsWith("Furn") ? 0.7f : 0; }
            bool working = m.work > 0;
            if (working)
            {
                m.work -= dt;
                float k = 1 - m.work / m.workLen;
                if (m.rods.Length > 0)
                {   // two press strokes per cycle, the rods slightly out of phase
                    for (int r = 0; r < m.rods.Length; r++)
                    {
                        float c = Mathf.Repeat(k * 2 + r * 0.12f, 1);
                        float stroke = c < 0.35f ? Ease(c / 0.35f) : c < 0.55f ? 1 : 1 - Ease((c - 0.55f) / 0.45f);
                        m.rods[r].localPosition = m.rodBase[r] + m.rods[r].parent.InverseTransformVector(Vector3.down * 0.28f) * stroke;
                        if (Mathf.Abs(c - 0.35f) < dt * 2 / m.workLen) m.body?.Kick(0.0035f);   // the press bottoms out
                    }
                }
                if (m.work <= 0)
                {
                    m.idleUntil = t + R(m.minIdle, m.maxIdle);
                    if (m.name == "AsmHero" || m.name == "AsmB") Burst(steam, m.steamAt, Vector3.up, 6);   // relief valve at the end of a cycle
                    if (m.autonomous && rnd.NextDouble() < 0.12) m.faultUntil = t + 2.4f;              // now and then: a short warning
                    if (m.produced > 0) { m.produced--; m.work = m.workLen; }
                }
            }
            else for (int r = 0; r < m.rods.Length; r++) m.rods[r].localPosition = Vector3.Lerp(m.rods[r].localPosition, m.rodBase[r], 1 - Mathf.Exp(-dt * 6));
            if (m.body != null) m.body.hum = working ? 0.0009f : 0.00035f;

            // status lamp: green idle, amber working, red blinking on a warning
            if (m.lampMat != null)
            {
                Color c = t < m.faultUntil ? (Mathf.Repeat(t, 0.5f) < 0.25f ? new Color(1f, 0.08f, 0.03f) * 3 : Color.black)
                        : working ? new Color(1f, 0.45f, 0.05f) * (2.2f + 0.6f * Mathf.Sin(t * 9)) : new Color(0.2f, 1f, 0.35f) * 1.5f;
                m.lampMat.SetColor("_EmissionColor", c);
            }
            if (m.beacon != null) m.beacon.enabled = working && Mathf.Repeat(t * 1.6f, 1f) < 0.5f;
            // furnace light: a slow breathing flame, a flare when fed
            if (m.fire != null)
            {
                m.flare = Mathf.MoveTowards(m.flare, 0, dt * 0.7f);
                m.fire.intensity = m.fireBase * (0.75f + 0.22f * Mathf.PerlinNoise(t * 2.3f, m.name.Length) + 0.9f * m.flare);
            }
            if (m.glow != null && m.name == "FurnB") m.glow.SetColor("_EmissionColor", m.glowBase * (1 + 0.6f * m.flare));
        }
    }
}
