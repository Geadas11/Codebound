using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Codebound.Game
{
    /// <summary>
    /// The main menu's 3D stage: a corner of a Codebound factory (Resources/MenuStage/menu_factory.fbx, made by
    /// Tools/art/menu_stage.py in Blender) lit, animated and filmed here. Belts carry items, arms pick and place, fans and
    /// rings turn, the crane travels, furnaces flicker, LEDs blink, the control terminal shows its program.
    /// The camera flies from the overview to the terminal (GoTerminal) and back (GoOverview); while it is there,
    /// ScreenRect() is where the terminal's glass is on screen, and the menu screens are drawn into it.
    /// The stage lives in the title scene only (destroyed with it); it draws with its own Forward+ renderer
    /// (MenuStageSetup) on layer 31, so the 2D game world and its lights never see it.
    /// </summary>
    public sealed class MenuStage : MonoBehaviour
    {
        public const int Layer = 31;
        const int RendererIndex = 1;

        public static MenuStage Instance { get; private set; }
        public static bool Active => Instance != null && Instance.ready;

        Camera cam, worldCam;
        int worldMask;
        GameObject root;
        bool ready;
        Vector3 overPos, overLook, termPos, termLook, screenCenter, screenNormal;
        Vector3[] screen = new Vector3[4];   // TL, TR, BR, BL
        float overFov = 58, termFov = 30;
        float travel, target, travelTime = 1.6f;
        Action arrived;
        readonly List<(Transform t, float speed)> spinners = new List<(Transform, float)>();
        readonly List<(Renderer r, float period, float phase)> blinkers = new List<(Renderer, float, float)>();
        readonly List<(Light l, float baseI, float seed)> fires = new List<(Light, float, float)>();
        Transform trolley; Vector3 trolleyA, trolleyB;
        MenuFactory factory; string cursorStatus; bool cursorOn;
        TextMesh[] codeLines; TextMesh cursor;
        AmbientMode oldAmbient; Color oldSky, oldEquator, oldGround; bool oldFog; Color oldFogColor; float oldFogDensity; FogMode oldFogMode;



        // ---------------------------------------------------------------- lifecycle
        public static MenuStage Open()
        {
            if (Instance != null) return Instance;
            var prefab = Resources.Load<GameObject>("MenuStage/menu_factory");
            if (prefab == null) return null;
            var go = new GameObject("Codebound Menu Stage");
            var s = go.AddComponent<MenuStage>();
            try { s.Build(prefab); }
            catch (Exception e) { Debug.LogException(e); Destroy(go); return null; }
            return s;
        }

        public static void Close() { if (Instance != null) Destroy(Instance.gameObject); }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (!ready) return;
            if (worldCam != null) worldCam.cullingMask = worldMask;
            RenderSettings.ambientMode = oldAmbient; RenderSettings.ambientSkyColor = oldSky; RenderSettings.ambientEquatorColor = oldEquator; RenderSettings.ambientGroundColor = oldGround;
            RenderSettings.fog = oldFog; RenderSettings.fogColor = oldFogColor; RenderSettings.fogDensity = oldFogDensity; RenderSettings.fogMode = oldFogMode;
        }

        static Transform Find(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
            return null;
        }

        void Build(GameObject prefab)
        {
            Instance = this;
            transform.position = new Vector3(0, -2000, 0);   // far below the 2D world
            root = Instantiate(prefab, transform);
            root.transform.localPosition = Vector3.zero;
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = Layer;

            Materials();
            Lights();
            Markers();
            Animated();
            Terminal();
            CameraAndPost();
            ready = true;
        }

        // ---------------------------------------------------------------- materials (URP Lit, tuned per name)
        static Texture2D grime;

        static Texture2D Grime()
        {
            if (grime != null) return grime;
            const int S = 256;
            grime = new Texture2D(S, S, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, hideFlags = HideFlags.HideAndDontSave };
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float n = 0, amp = 0.5f, f = 4f / S;
                    for (int o = 0; o < 5; o++) { n += amp * Mathf.PerlinNoise((x % S) * f + o * 17, (y % S) * f + o * 31); amp *= 0.5f; f *= 2; }
                    float streak = Mathf.PerlinNoise(x * 0.5f, y * 0.02f) * 0.08f;   // vertical streaks
                    float v = Mathf.Clamp01(0.72f + (n - 0.48f) * 0.6f - streak);
                    px[y * S + x] = new Color(v, v, v, 1);
                }
            grime.SetPixels(px); grime.Apply(true);
            return grime;
        }

        static readonly Dictionary<string, (Color color, float metal, float smooth, Color emit)> Look = new Dictionary<string, (Color, float, float, Color)>
        {
            ["Steel"] = (new Color(0.5f, 0.52f, 0.55f), 0.9f, 0.62f, Color.black),
            ["DarkSteel"] = (new Color(0.09f, 0.095f, 0.11f), 0.8f, 0.45f, Color.black),
            ["Graphite"] = (new Color(0.16f, 0.17f, 0.19f), 0.6f, 0.5f, Color.black),
            ["SafetyOrange"] = (new Color(0.95f, 0.36f, 0.05f), 0.25f, 0.45f, Color.black),
            ["PaintGrey"] = (new Color(0.22f, 0.23f, 0.25f), 0.35f, 0.42f, Color.black),
            ["ArmYellow"] = (new Color(0.95f, 0.66f, 0.08f), 0.3f, 0.55f, Color.black),
            ["Copper"] = (new Color(0.85f, 0.48f, 0.3f), 0.95f, 0.65f, Color.black),
            ["CopperCoil"] = (new Color(0.9f, 0.5f, 0.28f), 0.95f, 0.7f, Color.black),
            ["Rubber"] = (new Color(0.03f, 0.03f, 0.035f), 0f, 0.25f, Color.black),
            ["Grating"] = (new Color(0.2f, 0.21f, 0.22f), 0.8f, 0.4f, Color.black),
            ["Crate"] = (new Color(0.55f, 0.37f, 0.18f), 0f, 0.25f, Color.black),
            ["IronOre"] = (new Color(0.3f, 0.35f, 0.4f), 0.7f, 0.35f, Color.black),
            ["IronPlate"] = (new Color(0.66f, 0.68f, 0.72f), 0.9f, 0.7f, Color.black),
            ["Concrete"] = (new Color(0.11f, 0.11f, 0.115f), 0f, 0.35f, Color.black),
            ["LanePaint"] = (new Color(0.75f, 0.5f, 0.08f), 0f, 0.3f, Color.black),
            ["WallPanel"] = (new Color(0.07f, 0.075f, 0.085f), 0.5f, 0.35f, Color.black),
            ["Hazard"] = (new Color(0.92f, 0.62f, 0.05f), 0.1f, 0.35f, Color.black),
            ["FurnaceGlow"] = (Color.black, 0, 0.5f, new Color(1f, 0.36f, 0.06f) * 9f),
            ["MachineInner"] = (Color.black, 0, 0.5f, new Color(1f, 0.5f, 0.15f) * 3f),
            ["MachineWindow"] = (Color.black, 0, 0.8f, new Color(1f, 0.55f, 0.2f) * 2.2f),
            ["AmberLed"] = (Color.black, 0, 0.5f, new Color(1f, 0.55f, 0.1f) * 5f),
            ["GreenLed"] = (Color.black, 0, 0.5f, new Color(0.35f, 1f, 0.45f) * 4f),
            ["CyanLed"] = (Color.black, 0, 0.5f, new Color(0.3f, 0.8f, 1f) * 4f),
            ["RedLed"] = (Color.black, 0, 0.5f, new Color(1f, 0.15f, 0.06f) * 4f),
            ["LampTube"] = (Color.black, 0, 0.5f, new Color(1f, 0.8f, 0.55f) * 8f),
            ["WindowSky"] = (Color.black, 0, 0.5f, new Color(0.45f, 0.62f, 0.9f) * 2.2f),
            ["ScreenBg"] = (new Color(0.01f, 0.02f, 0.025f), 0, 0.92f, new Color(0.03f, 0.08f, 0.09f)),
        };

        void Materials()
        {
            var done = new Dictionary<Material, Material>();
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var src = mats[i];
                    if (src == null) continue;
                    if (!done.TryGetValue(src, out var m))
                    {
                        m = new Material(src) { name = src.name };
                        string key = src.name.StartsWith("FireBrick") ? "FireBrick" : src.name;
                        if (Look.TryGetValue(key, out var lk))
                        {
                            m.SetColor("_BaseColor", lk.color); m.SetFloat("_Metallic", lk.metal); m.SetFloat("_Smoothness", lk.smooth);
                            if (lk.emit.maxColorComponent > 0) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", lk.emit); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive; }
                        }
                        else if (key == "FireBrick") { m.SetColor("_BaseColor", new Color(0.3f, 0.12f, 0.07f)); m.SetFloat("_Metallic", 0); m.SetFloat("_Smoothness", 0.2f); }
                        if (lk.emit.maxColorComponent <= 0 && key != "ScreenBg")   // worn surfaces: grime in the base map
                        {
                            m.SetTexture("_BaseMap", Grime());
                            m.SetTextureScale("_BaseMap", new Vector2(0.35f, 0.35f));
                        }
                        done[src] = m;
                    }
                    mats[i] = m;
                }
                r.sharedMaterials = mats;
                r.shadowCastingMode = ShadowCastingMode.On;
            }
        }

        // ---------------------------------------------------------------- lights
        void Lights()
        {
            foreach (var l in root.GetComponentsInChildren<Light>(true))
            {
                l.cullingMask = 1 << Layer;
                string n = l.name;
                l.shadows = LightShadows.None;
                if (n.StartsWith("LampLight")) { l.type = LightType.Spot; l.intensity = 42; l.range = 19; l.spotAngle = 82; l.innerSpotAngle = 34; l.color = new Color(1f, 0.66f, 0.36f); }
                else if (n.StartsWith("WindowShaft")) { l.type = LightType.Spot; l.intensity = 60; l.range = 90; l.spotAngle = 32; l.innerSpotAngle = 10; l.color = new Color(0.62f, 0.74f, 1f); }
                else if (n.EndsWith("Fire")) { l.type = LightType.Point; l.intensity = 9; l.range = 9; l.color = new Color(1f, 0.42f, 0.1f); fires.Add((l, 9, n.GetHashCode() % 100)); }
                else if (n.StartsWith("TermKeyLight")) { l.intensity = 16; l.range = 9; l.spotAngle = 60; l.color = new Color(1f, 0.75f, 0.5f); l.shadows = LightShadows.Soft; }
                else if (n.StartsWith("EmblemLight")) { l.intensity = 90; l.range = 18; l.spotAngle = 58; l.color = new Color(1f, 0.72f, 0.45f); }
                else if (n.StartsWith("CraneSweep")) { l.intensity = 30; l.range = 22; l.spotAngle = 34; l.color = new Color(1f, 0.5f, 0.12f); }
                else if (n.StartsWith("HeroKey")) { l.type = LightType.Spot; l.intensity = 30; l.range = 18; l.spotAngle = 50; l.shadows = LightShadows.Soft; }
                else if (n.StartsWith("BackGlow")) { l.type = LightType.Point; l.intensity = 14; l.range = 28; l.color = new Color(1f, 0.48f, 0.18f); }
                else if (n.StartsWith("HeroBack")) { l.type = LightType.Point; l.intensity = 6; l.range = 12; l.color = new Color(0.55f, 0.68f, 1f); }
                else if (n.StartsWith("Fill")) { l.type = LightType.Point; l.intensity = 3; l.range = 25; l.color = new Color(0.45f, 0.6f, 1f); }
                else if (n.EndsWith("BeaconL")) { l.intensity = 2; l.range = 3; }
                else { l.intensity = Mathf.Min(l.intensity, 10); l.range = Mathf.Max(l.range, 8); }
            }
            int shadows = 0;   // the lamps nearest the camera cast soft shadows
            foreach (var l in root.GetComponentsInChildren<Light>(true))
                if (l.name.StartsWith("LampLight") && l.transform.localPosition.z > -20 && shadows++ < 4) l.shadows = LightShadows.Soft;
            // a cool key from above (moonlight through the roof lights) gives every shape a readable top
            var sun = new GameObject("Menu Key").AddComponent<Light>();
            sun.transform.SetParent(transform, false);
            sun.type = LightType.Directional; sun.color = new Color(0.85f, 0.78f, 0.72f); sun.intensity = 0.22f;
            sun.transform.rotation = Quaternion.Euler(58, 150, 0); sun.cullingMask = 1 << Layer; sun.shadows = LightShadows.Soft;
            sun.gameObject.layer = Layer;
            // the Codebound emblem on the right wall gets its own warm spot (Blender (19.5, 6, 7.5) → stage (-19.5, 7.5, -6))
            var emblemAt = transform.TransformPoint(new Vector3(-19.4f, 7.5f, -6f));
            var spot = new GameObject("Emblem Spot") { layer = Layer }.AddComponent<Light>();
            spot.transform.SetParent(transform, false);
            spot.transform.position = transform.TransformPoint(new Vector3(-13.5f, 11.5f, -3.5f));
            spot.transform.rotation = Quaternion.LookRotation(emblemAt - spot.transform.position);
            spot.type = LightType.Spot; spot.spotAngle = 42; spot.innerSpotAngle = 20; spot.range = 16; spot.intensity = 70;
            spot.color = new Color(1f, 0.7f, 0.42f); spot.cullingMask = 1 << Layer; spot.shadows = LightShadows.Soft;

            oldAmbient = RenderSettings.ambientMode; oldSky = RenderSettings.ambientSkyColor; oldEquator = RenderSettings.ambientEquatorColor; oldGround = RenderSettings.ambientGroundColor;
            oldFog = RenderSettings.fog; oldFogColor = RenderSettings.fogColor; oldFogDensity = RenderSettings.fogDensity; oldFogMode = RenderSettings.fogMode;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.2f, 0.18f, 0.17f);
            RenderSettings.ambientEquatorColor = new Color(0.17f, 0.12f, 0.08f);
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.05f, 0.04f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.17f, 0.12f, 0.085f); RenderSettings.fogDensity = 0.02f;
            Shafts();
        }

        /// <summary>Soft additive cones under the spot lights: the haze a volumetric renderer would show.</summary>
        void Shafts()
        {
            var shader = Resources.Load<Shader>("MenuStage/LightShaft");
            if (shader == null) return;
            foreach (var l in root.GetComponentsInChildren<Light>(true))
            {
                if (l.type != LightType.Spot) continue;
                bool window = l.name.StartsWith("WindowShaft"), lamp = l.name.StartsWith("LampLight");
                if (!window && !lamp && !l.name.StartsWith("EmblemLight")) continue;
                var dir = l.transform.forward;
                float len = l.range * (window ? 0.75f : 0.62f);
                if (dir.y < -0.05f) len = Mathf.Min(len, (l.transform.position.y - transform.position.y) / -dir.y);
                float r1 = len * Mathf.Tan(l.spotAngle * 0.5f * Mathf.Deg2Rad) * (window ? 0.9f : 0.7f);
                var go = new GameObject("Shaft " + l.name) { layer = Layer };
                go.transform.SetParent(transform, false);
                go.transform.position = l.transform.position;
                go.transform.rotation = Quaternion.LookRotation(dir);
                go.AddComponent<MeshFilter>().sharedMesh = Cone(window ? 0.6f : 0.35f, r1, len);
                var m = new Material(shader);
                m.SetColor("_Color", l.color);
                m.SetFloat("_Intensity", window ? 0.07f : 0.11f);
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = m; mr.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        static Mesh Cone(float r0, float r1, float len)
        {
            const int N = 28;
            var v = new Vector3[N * 2]; var c = new Color[N * 2]; var nrm = new Vector3[N * 2]; var tri = new int[N * 6];
            for (int i = 0; i < N; i++)
            {
                float a = i / (float)N * Mathf.PI * 2; var ring = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
                v[i] = ring * r0; v[i + N] = ring * r1 + Vector3.forward * len;
                nrm[i] = nrm[i + N] = ring;
                c[i] = new Color(1, 1, 1, 1); c[i + N] = new Color(1, 1, 1, 0.08f);
                int j = (i + 1) % N, t = i * 6;
                tri[t] = i; tri[t + 1] = j; tri[t + 2] = i + N; tri[t + 3] = j; tri[t + 4] = j + N; tri[t + 5] = i + N;
            }
            var m = new Mesh { vertices = v, colors = c, normals = nrm, triangles = tri };
            m.RecalculateBounds();
            return m;
        }

        // ---------------------------------------------------------------- markers: camera poses and the terminal glass
        void Markers()
        {
            Vector3 P(string n) { var t = Find(root.transform, "Mark_" + n); if (t == null) throw new Exception("menu stage: no marker " + n); return t.position; }
            overPos = P("Cam_Overview"); overLook = P("Cam_Overview_Look");
            overPos -= (overLook - overPos).normalized * 1.2f;   // a step back: the whole station and the line behind it
            overPos += Vector3.up * 0.6f;   // a little higher and 7° towards the line: the arms and belts read between the panel and the terminal
            overLook = overPos + Quaternion.Euler(0, -7, 0) * (overLook - overPos);
            screen[0] = P("Screen_TL"); screen[1] = P("Screen_TR"); screen[2] = P("Screen_BR"); screen[3] = P("Screen_BL");
            screenCenter = (screen[0] + screen[1] + screen[2] + screen[3]) / 4;
            screenNormal = Vector3.Cross(screen[1] - screen[0], screen[3] - screen[0]).normalized;
            if (Vector3.Dot(screenNormal, overPos - screenCenter) < 0) screenNormal = -screenNormal;   // facing the viewer
            termLook = screenCenter;
        }

        /// <summary>Camera distance and field of view that fit the glass to the window (a little margin), at any aspect.</summary>
        void FitTerminal()
        {
            float w = (screen[1] - screen[0]).magnitude, h = (screen[3] - screen[0]).magnitude;
            float aspect = Mathf.Max(0.5f, cam.aspect);
            const float margin = 1.17f;   // a little of the bezel stays in view
            float halfV = Mathf.Max(h * margin / 2, w * margin / 2 / aspect);
            termFov = 30;
            float dist = halfV / Mathf.Tan(termFov * 0.5f * Mathf.Deg2Rad);
            termPos = screenCenter + screenNormal * dist;
        }

        // ---------------------------------------------------------------- moving parts
        void Animated()
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.EndsWith("Fan")) spinners.Add((t, 140));
                else if (t.name.EndsWith("RingPivot")) spinners.Add((t, 50));
                else if (t.name == "CraneSweepPivot") spinners.Add((t, 120));
                var r = t.GetComponent<Renderer>();
                if (r != null && (t.name.EndsWith("Beacon") || t.name.StartsWith("Blink_")))
                    blinkers.Add((r, t.name.StartsWith("Blink_") ? 0.6f + (t.name.GetHashCode() & 7) * 0.17f : 1f, (t.name.GetHashCode() & 255) / 255f));
            }
            trolley = Find(root.transform, "Trolley");
            var ca = Find(root.transform, "Mark_Crane_A"); var cb = Find(root.transform, "Mark_Crane_B");
            if (trolley != null && ca != null && cb != null)
            {   // the trolley travels the span between the two marks, around where it was modelled
                var mid = (ca.position + cb.position) / 2;
                trolleyA = trolley.position + (ca.position - mid); trolleyB = trolley.position + (cb.position - mid);
            }
            else trolley = null;

            factory = new MenuFactory(this, root.transform);   // belts, arms, machine cycles, vibration, particles
        }

        // ---------------------------------------------------------------- the terminal's own screen (idle: the program running)
        static readonly string[] Code =
        {
            "// line 2: smelt what the drills send", "while (true) {", "  const ore = belt.take();", "  if (ore) furnace.insert(ore);",
            "  if (furnace.done()) arm.place(\"out\");", "  hud.set(\"rate\", furnace.rate());", "  sleep(1);", "}",
        };

        void Terminal()
        {
            var right = (screen[1] - screen[0]).normalized; var down = (screen[3] - screen[0]).normalized;
            float w = (screen[1] - screen[0]).magnitude, h = (screen[3] - screen[0]).magnitude;
            var rot = Quaternion.LookRotation(-screenNormal, -down);
            var font = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Cascadia Mono", "Courier New" }, 64);
            codeLines = new TextMesh[Code.Length];
            for (int i = 0; i < Code.Length; i++)
            {
                var go = new GameObject("TerminalCode"); go.layer = Layer;
                go.transform.SetParent(transform, false);
                go.transform.position = screen[0] + right * (w * 0.06f) + down * (h * (0.1f + i * 0.085f)) + screenNormal * 0.004f;
                go.transform.rotation = rot;
                var tm = go.AddComponent<TextMesh>();
                tm.font = font; tm.fontSize = 64; tm.characterSize = h * 0.0075f; tm.anchor = TextAnchor.UpperLeft;
                tm.text = Code[i];
                tm.color = i == 0 ? new Color(0.45f, 0.75f, 0.8f) : new Color(1f, 0.68f, 0.28f);
                go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
                codeLines[i] = tm;
            }
            var cgo = new GameObject("TerminalCursor"); cgo.layer = Layer; cgo.transform.SetParent(transform, false);
            cgo.transform.position = screen[0] + right * (w * 0.06f) + down * (h * (0.1f + Code.Length * 0.085f)) + screenNormal * 0.004f;
            cgo.transform.rotation = rot;
            cursor = cgo.AddComponent<TextMesh>(); cursor.font = font; cursor.fontSize = 64; cursor.characterSize = h * 0.0075f; cursor.anchor = TextAnchor.UpperLeft;
            cursor.text = "_"; cursor.color = new Color(1f, 0.7f, 0.3f);
            cgo.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        // ---------------------------------------------------------------- camera and post-processing
        void CameraAndPost()
        {
            worldCam = Camera.main;
            if (worldCam != null) { worldMask = worldCam.cullingMask; worldCam.cullingMask &= ~(1 << Layer); }
            var cgo = new GameObject("Menu Camera") { layer = Layer };
            cgo.transform.SetParent(transform, false);
            cam = cgo.AddComponent<Camera>();
            cam.cullingMask = 1 << Layer;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.02f, 0.02f, 0.025f);
            cam.nearClipPlane = 0.05f; cam.farClipPlane = 200; cam.fieldOfView = overFov;
            cam.depth = (worldCam != null ? worldCam.depth : 0) + 10;
            cam.allowHDR = true;
            var data = cam.GetUniversalAdditionalCameraData();
            data.SetRenderer(RendererIndex);
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;   // SMAA has no resources in this renderer: it turns the screen cyan
            data.volumeLayerMask = 1 << Layer;
            data.renderShadows = true;

            var vgo = new GameObject("Menu Post") { layer = Layer };
            vgo.transform.SetParent(transform, false);
            var vol = vgo.AddComponent<Volume>();
            vol.isGlobal = true; vol.priority = 10;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom = profile.Add<Bloom>(true); bloom.threshold.Override(1.0f); bloom.intensity.Override(0.9f); bloom.scatter.Override(0.72f); bloom.tint.Override(new Color(1f, 0.85f, 0.7f));
            var tone = profile.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.ACES);
            var ca = profile.Add<ColorAdjustments>(true); ca.postExposure.Override(1.05f); ca.contrast.Override(16f); ca.saturation.Override(10f);
            var vig = profile.Add<Vignette>(true); vig.intensity.Override(0.32f); vig.smoothness.Override(0.5f);
            var smh = profile.Add<ShadowsMidtonesHighlights>(true); smh.shadows.Override(new Vector4(0.95f, 0.98f, 1.08f, 0)); smh.highlights.Override(new Vector4(1.06f, 1.0f, 0.92f, 0));
            vol.sharedProfile = profile;
            Place(0);
        }

        // ---------------------------------------------------------------- per frame
        static float Ease(float t) => t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;   // cubic in-out

        void Place(float k)
        {
            FitTerminal();
            float e = Ease(k);
            // an arc: rise a little and swing out, so the camera never cuts through the desk or the cabinets
            var mid = Vector3.Lerp(overPos, termPos, 0.5f) + Vector3.up * 0.6f + (overPos - termPos).normalized * 0.4f;
            var a = Vector3.Lerp(overPos, mid, e); var b = Vector3.Lerp(mid, termPos, e);
            var pos = Vector3.Lerp(a, b, e);
            var look = Vector3.Lerp(overLook, termLook, Mathf.SmoothStep(0, 1, Mathf.Clamp01(e * 1.15f)));
            var breathe = ready && Ui.Animate && k < 0.01f ? new Vector3(Mathf.Sin(Time.unscaledTime * 0.21f) * 0.12f, Mathf.Sin(Time.unscaledTime * 0.17f) * 0.05f, 0) : Vector3.zero;
            cam.transform.position = pos + breathe;
            cam.transform.rotation = Quaternion.LookRotation(look - cam.transform.position, Vector3.up);
            cam.fieldOfView = Mathf.Lerp(overFov, termFov, e);
        }

        public bool AtTerminal => travel >= 1f && target >= 1f;
        public bool AtOverview => travel <= 0f && target <= 0f;
        public bool Moving => !Mathf.Approximately(travel, target);
        /// <summary>0 at the overview, 1 at the terminal (eased).</summary>
        public float Progress => Ease(travel);

        public void GoTerminal(Action onArrive) { target = 1; arrived = onArrive; if (!Ui.Animate) travel = 0.999f; }
        public void GoOverview(Action onArrive = null) { target = 0; arrived = onArrive; if (!Ui.Animate) travel = 0.001f; }

        void Update()
        {
            if (!ready) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f), t = Time.unscaledTime;
            if (!Mathf.Approximately(travel, target))
            {
                travel = Mathf.MoveTowards(travel, target, dt / travelTime);
                if (Mathf.Approximately(travel, target)) { var a = arrived; arrived = null; a?.Invoke(); }
            }
            Place(travel);
            bool motion = Ui.Animate;
            foreach (var (tr, speed) in spinners) if (motion) tr.Rotate(Vector3.up, speed * dt, Space.World);
            foreach (var (r, period, phase) in blinkers) if (r != null) r.enabled = !motion || Mathf.Repeat(t / period + phase, 1f) < 0.55f;
            foreach (var (l, baseI, seed) in fires) l.intensity = baseI * (motion ? 0.7f + 0.3f * Mathf.PerlinNoise(t * 6f, seed) : 1f);
            if (trolley != null && motion) trolley.position = Vector3.Lerp(trolleyA, trolleyB, 0.5f - 0.5f * Mathf.Cos(t * 0.35f));
            factory?.Tick(dt, t, motion);
            if (cursor != null)
            {   // the line's live counters and a blinking cursor; the text is rebuilt only when it changes
                string status = factory?.Status ?? ""; bool on = !motion || Mathf.Repeat(t, 1.1f) < 0.6f;
                if (!ReferenceEquals(status, cursorStatus) || on != cursorOn) { cursorStatus = status; cursorOn = on; cursor.text = status + (on ? " _" : ""); }
                cursor.gameObject.SetActive(true);
            }
            if (codeLines != null) foreach (var c in codeLines) c.gameObject.SetActive(travel < 0.85f);   // the menu screen takes over the glass
            if (cursor != null && travel >= 0.85f) cursor.gameObject.SetActive(false);
        }

        // ---------------------------------------------------------------- the terminal glass on screen
        /// <summary>The terminal glass in IMGUI coordinates (scaled by UiGui), for drawing menu screens "on" it.</summary>
        public Rect ScreenRect()
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            foreach (var p in screen)
            {
                var s = cam.WorldToScreenPoint(p);
                float gx = s.x / UiGui.S, gy = (Screen.height - s.y) / UiGui.S;
                x0 = Mathf.Min(x0, gx); y0 = Mathf.Min(y0, gy); x1 = Mathf.Max(x1, gx); y1 = Mathf.Max(y1, gy);
            }
            return Rect.MinMaxRect(x0, y0, x1, y1);
        }
    }
}
