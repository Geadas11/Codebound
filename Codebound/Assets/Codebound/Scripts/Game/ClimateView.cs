using Codebound.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Codebound.Game
{
    /// <summary>
    /// Day/night lighting (drives the scene's Global Light 2D), weather tint and rain/dust particles.
    /// Accessibility: <see cref="ReduceEffects"/> turns particles off and keeps tints subtle.
    /// </summary>
    public sealed class ClimateView : MonoBehaviour
    {
        public static bool ReduceEffects { get => Settings.ReduceEffects; set => Settings.ReduceEffects = value; }

        GameSession session;
        Camera cam;
        Light2D sun;
        SpriteRenderer overlay;
        ParticleSystem particles;
        Color overlayColor = Color.clear;

        public void Init(GameSession s, Camera c)
        {
            session = s; cam = c;
            foreach (var l in FindObjectsByType<Light2D>())
                if (l.lightType == Light2D.LightType.Global) { sun = l; break; }
            if (sun == null)
            {
                sun = new GameObject("Global Light 2D").AddComponent<Light2D>();
                sun.lightType = Light2D.LightType.Global;
            }

            overlay = new GameObject("WeatherOverlay").AddComponent<SpriteRenderer>();
            overlay.sprite = PixelArtFactory.White();
            overlay.sortingOrder = 40;
            overlay.color = Color.clear;
            overlay.transform.SetParent(cam.transform, false);

            var go = new GameObject("WeatherParticles");
            go.transform.SetParent(cam.transform, false);
            particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = true; main.playOnAwake = false; main.maxParticles = 1500;
            main.startLifetime = 1.2f; main.startSize = 0.06f; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch; r.lengthScale = 6f; r.sortingOrder = 41;
            var player = session.Player.GetComponent<SpriteRenderer>();
            if (player != null) r.sharedMaterial = player.sharedMaterial;   // the project's sprite material (URP 2D)
            var emission = particles.emission; emission.rateOverTime = 0;
            particles.Play();
        }

        void LateUpdate()
        {
            var clock = session.State.Clock;
            var env = session.Surroundings;
            var biome = session.World.Generator.BiomeAt(GameSession.CellAt(session.Player.position));
            var w = env.Weather;
            bool dust = w == Weather.Storm && (biome == Biome.Desert || biome == Biome.Volcanic);
            bool fallout = w == Weather.Storm && biome == Biome.Radioactive;
            bool rain = env.Rain;

            // light: night blue, warm dawn/dusk, white day; clouds and storms dim it
            float day = clock.Daylight;
            float dim = w == Weather.Storm ? 0.7f : w == Weather.Rain ? 0.85f : w == Weather.Cloudy ? 0.93f : 1f;
            sun.intensity = Mathf.Lerp(0.22f, 1f, day) * dim;
            Color night = new Color(0.55f, 0.65f, 1f), dusk = new Color(1f, 0.72f, 0.5f);
            float twilight = 1f - Mathf.Abs(day - 0.5f) * 2f; // 1 at mid-transition
            sun.color = Color.Lerp(Color.Lerp(night, Color.white, day), dusk, Mathf.Clamp01(twilight) * 0.6f);

            // weather tint over the world
            Color target = dust ? (biome == Biome.Desert ? new Color(0.85f, 0.68f, 0.38f, 0.38f) : new Color(0.3f, 0.27f, 0.27f, 0.42f))
                : fallout ? new Color(0.45f, 0.95f, 0.3f, 0.2f)
                : rain ? new Color(0.2f, 0.25f, 0.35f, w == Weather.Storm ? 0.22f : 0.12f)
                : Color.clear;
            if (ReduceEffects) target.a *= 0.5f;
            overlayColor = Color.Lerp(overlayColor, target, 1f - Mathf.Exp(-1.5f * Time.deltaTime));
            overlay.color = overlayColor;
            float h = cam.orthographicSize * 2f + 2f, wdt = h * cam.aspect + 2f;
            overlay.transform.localPosition = new Vector3(-wdt / 2f, -h / 2f, 10f);   // sprite pivot is bottom-left, 4 px = 0.25 units
            overlay.transform.localScale = new Vector3(wdt * 4f, h * 4f, 1f);

            // particles: rain falls, dust blows sideways
            var shape = particles.shape;
            shape.position = new Vector3(0, h / 2f, 10f);
            shape.scale = new Vector3(wdt * 1.4f, 1f, 1f);
            var main = particles.main;
            var vel = particles.velocityOverLifetime;
            vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            if (dust) { vel.x = 14f; vel.y = -4f; main.startColor = biome == Biome.Desert ? new Color(0.9f, 0.75f, 0.45f, 0.7f) : new Color(0.4f, 0.38f, 0.38f, 0.7f); }
            else { vel.x = 1.5f; vel.y = -h * 1.1f; main.startColor = fallout ? new Color(0.6f, 1f, 0.4f, 0.6f) : new Color(0.7f, 0.8f, 1f, 0.55f); }
            main.startLifetime = dust ? 0.9f : 1.0f;
            var emission = particles.emission;
            float rate = rain || dust || fallout ? (w == Weather.Storm ? 700f : 350f) * (wdt / 30f) : 0f;
            emission.rateOverTime = ReduceEffects ? 0f : rate;
        }
    }
}
