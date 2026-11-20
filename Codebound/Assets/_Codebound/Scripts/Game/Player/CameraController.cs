using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Smooth follow camera with integer pixel-art zoom levels (each world pixel = N screen pixels),
    /// and position snapping to the screen-pixel grid to avoid shimmering.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraController : MonoBehaviour
    {
        public const int MinZoom = 1;
        public const int MaxZoom = 8;

        private Camera _cam;
        private Transform _target;
        private Vector3 _smoothPos;

        /// <summary>Screen pixels per art pixel.</summary>
        public int Zoom { get; private set; } = 3;
        public float FollowSharpness { get; set; } = 10f;

        public void Init(Transform target)
        {
            _cam = GetComponent<Camera>();
            _target = target;
            _smoothPos = target.position;
            Zoom = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 270f), MinZoom, MaxZoom);
            Apply();
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            float scroll = GameInput.Scroll;
            if (scroll > 0.01f) Zoom = Mathf.Min(MaxZoom, Zoom + 1);
            else if (scroll < -0.01f) Zoom = Mathf.Max(MinZoom, Zoom - 1);

            float k = 1f - Mathf.Exp(-FollowSharpness * Time.deltaTime);
            _smoothPos = Vector3.Lerp(_smoothPos, _target.position, k);
            Apply();
        }

        private void Apply()
        {
            float unitsPerScreenPixel = 1f / (PixelArt.PPU * Zoom);
            _cam.orthographicSize = Screen.height * 0.5f * unitsPerScreenPixel;
            float sx = Mathf.Round(_smoothPos.x / unitsPerScreenPixel) * unitsPerScreenPixel;
            float sy = Mathf.Round(_smoothPos.y / unitsPerScreenPixel) * unitsPerScreenPixel;
            transform.position = new Vector3(sx, sy, -10f);
        }
    }
}
