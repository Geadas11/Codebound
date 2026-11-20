using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Feeds input into the engine-independent <see cref="Codebound.Player.PlayerMotor"/> and renders the result.</summary>
    public sealed class PlayerController : MonoBehaviour
    {
        private GameSession _session;
        private SpriteRenderer _sprite;
        private float _animTime;

        /// <summary>When false (e.g. a text field has focus) movement input is ignored.</summary>
        public bool InputEnabled { get; set; } = true;

        public void Init(GameSession session)
        {
            _session = session;
            var body = new GameObject("Sprite");
            body.transform.SetParent(transform, false);
            // Sprite pivot is bottom-left; centre the 1x1 sprite on the motor position, feet slightly below centre.
            body.transform.localPosition = new Vector3(-0.5f, -0.35f, 0);
            _sprite = body.AddComponent<SpriteRenderer>();
            SyncTransform();
        }

        private void Update()
        {
            if (_session == null) return;
            var input = InputEnabled ? GameInput.Move : Vector2.zero;
            _session.Motor.Move(new Vec2(input.x, input.y), Mathf.Min(Time.deltaTime, 0.1f), _session.Walkability);
            SyncTransform();
        }

        private void SyncTransform()
        {
            var p = _session.Motor.Position;
            transform.position = new Vector3(p.X, p.Y, 0);
            bool moving = _session.Motor.LastVelocity.Length > 0.1f;
            _animTime = moving ? _animTime + Time.deltaTime : 0f;
            int frame = moving ? (int)(_animTime * 8f) % 2 : 0;
            _sprite.sprite = PixelArt.Player(_session.Motor.Facing, frame);
            _sprite.sortingOrder = SortingOrder.ForY(p.Y - 0.35f) + 1;
        }
    }
}
