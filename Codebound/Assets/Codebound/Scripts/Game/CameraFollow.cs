using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    public sealed class CameraFollow : MonoBehaviour
    {
        public float MinSize = 4, MaxSize = 24, Smooth = 10f;
        Transform target;
        Camera cam;

        public void Init(Transform t) { target = t; cam = GetComponent<Camera>(); }

        void LateUpdate()
        {
            if (target == null) return;
            var goal = new Vector3(target.position.x, target.position.y, -10);
            transform.position = Vector3.Lerp(transform.position, goal, 1 - Mathf.Exp(-Smooth * Time.deltaTime));

            var mouse = Mouse.current;
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (scroll != 0) cam.orthographicSize = Mathf.Clamp(cam.orthographicSize * (scroll > 0 ? 0.9f : 1.1f), MinSize, MaxSize);
            }
        }
    }
}
