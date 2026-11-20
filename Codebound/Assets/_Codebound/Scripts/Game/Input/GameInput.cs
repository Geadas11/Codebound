using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Codebound.Game
{
    /// <summary>
    /// Single place that reads raw input. Works with the Input System package and with the legacy
    /// Input Manager, whichever the project has enabled. Key choices live in <see cref="Bindings"/>
    /// so remapping (Etapa 8 accessibility) only changes data, not gameplay code.
    /// </summary>
    public static class GameInput
    {
        public enum Action { Up, Down, Left, Right, Interact, ToggleBuild, Rotate, Cancel, ToggleInventory, CycleLanguage }

        /// <summary>Current binding per action, as a key name understood by both input backends (lowercase letters, "tab", "escape", "f1").</summary>
        public static readonly System.Collections.Generic.Dictionary<Action, string> Bindings =
            new System.Collections.Generic.Dictionary<Action, string>
            {
                { Action.Up, "w" }, { Action.Down, "s" }, { Action.Left, "a" }, { Action.Right, "d" },
                { Action.Interact, "e" }, { Action.ToggleBuild, "b" }, { Action.Rotate, "r" },
                { Action.Cancel, "escape" }, { Action.ToggleInventory, "tab" }, { Action.CycleLanguage, "f1" },
            };

        public static Vector2 Move
        {
            get
            {
                float x = (Held(Action.Right) || HeldKey("right") ? 1 : 0) - (Held(Action.Left) || HeldKey("left") ? 1 : 0);
                float y = (Held(Action.Up) || HeldKey("up") ? 1 : 0) - (Held(Action.Down) || HeldKey("down") ? 1 : 0);
                return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            }
        }

        public static bool Held(Action a) => HeldKey(Bindings[a]);
        public static bool Pressed(Action a) => PressedKey(Bindings[a]);

        /// <summary>Number row key 1..9 pressed this frame, or 0.</summary>
        public static int NumberPressed()
        {
            for (int i = 1; i <= 9; i++) if (PressedKey(i.ToString())) return i;
            return 0;
        }

#if ENABLE_INPUT_SYSTEM
        public static Vector2 MousePosition => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        public static bool PrimaryHeld => Mouse.current != null && Mouse.current.leftButton.isPressed;
        public static bool PrimaryPressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        public static bool SecondaryPressed => Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        public static float Scroll => Mouse.current != null ? Mouse.current.scroll.ReadValue().y / 120f : 0f;

        private static KeyControl Control(string name)
        {
            var kb = Keyboard.current;
            if (kb == null) return null;
            switch (name)
            {
                case "escape": return kb.escapeKey;
                case "tab": return kb.tabKey;
                case "f1": return kb.f1Key;
                case "up": return kb.upArrowKey;
                case "down": return kb.downArrowKey;
                case "left": return kb.leftArrowKey;
                case "right": return kb.rightArrowKey;
                case "space": return kb.spaceKey;
            }
            if (name.Length == 1 && name[0] >= '1' && name[0] <= '9') return kb[Key.Digit1 + (name[0] - '1')];
            // Letters map to physical key positions (WASD stays WASD on any layout).
            return System.Enum.TryParse(name, true, out Key key) ? kb[key] : null;
        }

        private static bool HeldKey(string name) { var k = Control(name); return k != null && k.isPressed; }
        private static bool PressedKey(string name) { var k = Control(name); return k != null && k.wasPressedThisFrame; }
#else
        public static Vector2 MousePosition => Input.mousePosition;
        public static bool PrimaryHeld => Input.GetMouseButton(0);
        public static bool PrimaryPressed => Input.GetMouseButtonDown(0);
        public static bool SecondaryPressed => Input.GetMouseButtonDown(1);
        public static float Scroll => Input.mouseScrollDelta.y;

        private static bool HeldKey(string name) { try { return Input.GetKey(name); } catch (System.ArgumentException) { return false; } }
        private static bool PressedKey(string name) { try { return Input.GetKeyDown(name); } catch (System.ArgumentException) { return false; } }
#endif
    }
}
