using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>Top-down movement with grid collision (no physics engine needed for a tile world).</summary>
    public sealed class PlayerController : MonoBehaviour
    {
        public float Speed = 5f;
        const float HalfW = 0.3f, HalfH = 0.25f, FootOffset = -0.2f; // collision box around the feet

        GameSession session;
        SpriteRenderer sr;
        float walkTime;

        public void Init(GameSession s)
        {
            session = s;
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArtFactory.Player();
            sr.sortingOrder = 10;
        }

        public Vector2 Feet => (Vector2)transform.position + new Vector2(0, FootOffset);

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || session.KeyboardCaptured || OptionsMenu.IsOpen) return;
            Vector2 dir = Vector2.zero;
            if (Controls.Held("up") || kb.upArrowKey.isPressed) dir.y += 1;
            if (Controls.Held("down") || kb.downArrowKey.isPressed) dir.y -= 1;
            if (Controls.Held("left") || kb.leftArrowKey.isPressed) dir.x -= 1;
            if (Controls.Held("right") || kb.rightArrowKey.isPressed) dir.x += 1;
            if (dir.sqrMagnitude > 1) dir.Normalize();

            Vector2 step = dir * Speed * Mathf.Min(Time.deltaTime, 0.05f);
            Vector3 p = transform.position;
            // axis-separated so the player slides along walls
            if (step.x != 0 && !Blocked(p.x + step.x, p.y)) p.x += step.x;
            if (step.y != 0 && !Blocked(p.x, p.y + step.y)) p.y += step.y;
            transform.position = p;

            if (dir.x != 0) sr.flipX = dir.x < 0;
            walkTime = dir == Vector2.zero ? 0 : walkTime + Time.deltaTime;
            sr.sprite = PixelArtFactory.Player(dir == Vector2.zero ? -1 : (int)(walkTime * 9f) % PixelArtFactory.PlayerFrames);
        }

        bool Blocked(float x, float y)
        {
            float fy = y + FootOffset;
            return Solid(x - HalfW, fy - HalfH) || Solid(x + HalfW, fy - HalfH) || Solid(x - HalfW, fy + HalfH) || Solid(x + HalfW, fy + HalfH);
        }

        bool Solid(float x, float y) => session.IsSolid(new Cell(Mathf.FloorToInt(x), Mathf.FloorToInt(y)));

        /// <summary>Cells overlapped by the player's collision box (used to forbid building on top of the player).</summary>
        public bool Overlaps(Cell c)
        {
            var f = Feet;
            return f.x + HalfW > c.X && f.x - HalfW < c.X + 1 && f.y + HalfH > c.Y && f.y - HalfH < c.Y + 1;
        }
    }
}
