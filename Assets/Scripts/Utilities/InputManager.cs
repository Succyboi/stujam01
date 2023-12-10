using Stupid.Utilities;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stupid.stujam01 {
    public class InputManager : SingletonMonoBehaviour<InputManager> {
        [Serializable]
        public class Settings {
            [Header("Mouse")]
            public float MouseSensitivity = 1f;

            [Header("Keybinds")]
            public Key LeftKey = Key.A;
            public Key RightKey = Key.D;
            public Key ForwardKey = Key.W;
            public Key BackKey = Key.S;
            [Space]
            public Key JumpKey = Key.Space;
            public Key CrouchKey = Key.LeftShift;
            [Space]
            public Key MenuKey = Key.Escape;
        }

        [SerializeField] private Settings settings = new Settings();

        public Vector2 MovementRaw => (Input.KeyPressed(settings.LeftKey) ? Vector2.left : Vector2.zero)
            + (Input.KeyPressed(settings.RightKey) ? Vector2.right : Vector2.zero)
            + (Input.KeyPressed(settings.ForwardKey) ? Vector2.up : Vector2.zero)
            + (Input.KeyPressed(settings.BackKey) ? Vector2.down : Vector2.zero);
        public Vector2 Movement => MovementRaw.magnitude > 1f ? MovementRaw.normalized : MovementRaw;

        public Vector2 MouseDelta => Input.GetMouseDelta() * settings.MouseSensitivity;

        public bool JumpDown => Input.KeyDown(settings.JumpKey);
        public bool JumpPressed => Input.KeyPressed(settings.JumpKey);
        public bool JumpUp => Input.KeyUp(settings.JumpKey);

        public bool CrouchDown => Input.KeyDown(settings.CrouchKey);
        public bool CrouchPressed => Input.KeyPressed(settings.CrouchKey);
        public bool CrouchUp => Input.KeyUp(settings.CrouchKey);

        public bool MenuDown => Input.KeyDown(settings.MenuKey);
        public bool MenuPressed => Input.KeyPressed(settings.MenuKey);
        public bool MenuUp => Input.KeyUp(settings.MenuKey);

        public bool ThrowDown => Input.MBDown(0);
        public bool ThrowPressed => Input.MBPressed(0);
        public bool ThrowUp => Input.MBUp(0);

        public void LockCursor() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor() {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}