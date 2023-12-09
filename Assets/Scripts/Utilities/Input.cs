using UnityEngine.InputSystem;

namespace Stupid.stujam01 {
    public static class Input {
        public static bool KeyPressed(Key key) {
            return Keyboard.current[key].isPressed;
        }
        public static bool KeyDown(Key key) {
            return Keyboard.current[key].wasPressedThisFrame;
        }
        public static bool KeyUp(Key key) {
            return Keyboard.current[key].wasReleasedThisFrame;
        }

        public static void SetMousePosition(Vector2 position) {
            Mouse.current.WarpCursorPosition(position);
        }

        public static Vector2 GetMousePosition() {
            return Mouse.current.position.ReadValue();
        }

        public static Vector2 GetMouseDelta() {
            return Mouse.current.delta.ReadValue();
        }

        public static Vector2 GetScrollDelta() {
            return Mouse.current.scroll.ReadValue();
        }

        public static bool MBPressed(int index) {
            switch (index) {
                case 1:
                    return Mouse.current.rightButton.isPressed;
                case 2:
                    return Mouse.current.middleButton.isPressed;
                default:
                    return Mouse.current.leftButton.isPressed;
            }
        }

        public static bool MBDown(int index) {
            switch (index) {
                case 1:
                    return Mouse.current.rightButton.wasPressedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasPressedThisFrame;
                default:
                    return Mouse.current.leftButton.wasPressedThisFrame;
            }
        }

        public static bool MBUp(int index) {
            switch (index) {
                case 1:
                    return Mouse.current.rightButton.wasReleasedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasReleasedThisFrame;
                default:
                    return Mouse.current.leftButton.wasReleasedThisFrame;
            }
        }
    }
}