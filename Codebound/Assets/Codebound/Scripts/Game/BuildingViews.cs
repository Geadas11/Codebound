using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Keeps one sprite GameObject per placed building, driven by BuildGrid events.</summary>
    public sealed class BuildingViews : MonoBehaviour
    {
        GameSession session;
        readonly Dictionary<int, GameObject> views = new Dictionary<int, GameObject>();
        Transform root;

        public void Init(GameSession s)
        {
            session = s;
            root = new GameObject("Buildings").transform;
            s.Grid.Placed += OnPlaced;
            s.Grid.Removed += OnRemoved;
            foreach (var b in s.Grid.Buildings) OnPlaced(b);
        }

        /// <summary>World-space centre of a footprint and its z-rotation; shared with the build preview.</summary>
        public static void Pose(BuildingDef def, Cell origin, Rotation rot, Transform t)
        {
            var (w, h) = def.Size(rot);
            t.position = new Vector3(origin.X + w / 2f, origin.Y + h / 2f, 0);
            t.rotation = Quaternion.Euler(0, 0, -90f * (int)rot); // clockwise turns
        }

        void OnPlaced(PlacedBuilding b)
        {
            var go = new GameObject($"{b.Def.Id}#{b.InstanceId}");
            go.transform.SetParent(root, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArtFactory.Building(b.Def);
            sr.sortingOrder = 2;
            Pose(b.Def, b.Origin, b.Rotation, go.transform);
            views[b.InstanceId] = go;
        }

        void LateUpdate()
        {
            // damaged buildings glow red, proportional to the damage taken
            foreach (var b in session.Grid.Buildings)
                if (views.TryGetValue(b.InstanceId, out var go))
                {
                    float k = Mathf.Clamp01(b.Health / b.Def.MaxHealth);
                    go.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(1f, 0.45f, 0.45f), Color.white, k);
                }
        }

        void OnRemoved(PlacedBuilding b)
        {
            if (views.TryGetValue(b.InstanceId, out var go)) { Destroy(go); views.Remove(b.InstanceId); }
        }

        void OnDestroy()
        {
            if (session == null) return;
            session.Grid.Placed -= OnPlaced;
            session.Grid.Removed -= OnRemoved;
        }
    }
}
