using UnityEngine;

namespace Stupid.stujam01 {
    public class HumanPlayerController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private NetworkedPlayer player;

        private InputManager input => InputManager.Instance;

        private Vector2 rotation;

        private void Update() {
            if (!player.NetworkObject.Authorized) { return; }

            rotation = new Vector2(rotation.x + input.MouseDelta.x, Mathf.Clamp(rotation.y - input.MouseDelta.y, -90f, 90f));
            player.SetRotation(rotation);

            player.SetMovement(input.Movement);
            player.SetCrouch(input.CrouchPressed);

            if (input.JumpPressed) {
                player.Jump();
            }
        }
    }
}