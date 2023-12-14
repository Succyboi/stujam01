using System.Linq;
using UnityEngine;

namespace Stupid.stujam01 {
    public class BotPlayerController : MonoBehaviour {
        [SerializeField] private float lookDuration;
        [SerializeField] private float jumpChance;
        [SerializeField] private float crouchFrequency;
        [SerializeField] private float crouchChance;
        [SerializeField] private float throwChance;

        [Header("Physics checks")]
        [SerializeField] private LayerMask environmentMask;

        [Header("References")]
        [SerializeField] private NetworkedPlayer player;

        private Random random = new Random();
        private Vector2 rotation;

        private void Update() {
            if (!player.NetworkObject.Authorized) { return; }

            Transform target = null;

            if (!player.HoldingBall) {
                Dodgeball targetBall = Dodgeball.Instances
                    .Select(p => p.Value)
                    .Where(p => p.Idle)
                    .Where(p => !Physics.Linecast(player.Head.position, p.transform.position, environmentMask))
                    .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
                    .FirstOrDefault();

                target = targetBall?.transform;
            }
            else {
                NetworkedPlayer targetPlayer = NetworkedPlayer.Instances
                    .Select(p => p.Value)
                    .Where(p => p != player)
                    .Where(p => p.Alive)
                    .Where(p => !Physics.Linecast(player.Head.position, p.Head.position, environmentMask, QueryTriggerInteraction.Ignore))
                    .OrderBy(p => Vector3.Distance(transform.position, p.Center))
                    .FirstOrDefault();

                target = targetPlayer?.Head;
            }

            if(target == null) { return; }
            LookTowards(target.transform.position);
            MoveTowards(target.transform.position);

            if(random.Float < crouchFrequency * Time.deltaTime) {
                player.SetCrouch(random.Float < crouchChance);
            }

            if (random.Float < jumpChance * Time.deltaTime) {
                player.Jump();
            }

            if (random.Float < throwChance * Time.deltaTime) {
                player.Throw();
            }
        }

        private void LookTowards(Vector3 position) {
            Quaternion lookRotation = Quaternion.LookRotation((position - (Vector3)player.transform.position).normalized, Vector3.up);
            rotation = Vector2.MoveTowards(rotation, new Vector2(lookRotation.eulerAngles.y, lookRotation.eulerAngles.x), Time.deltaTime / lookDuration);

            player.SetRotation(rotation);
        }

        private void MoveTowards(Vector3 position) {
            Vector3 direction = (position - (Vector3)player.transform.position).normalized;
            direction = player.transform.TransformDirection(direction);

            player.SetMovement(new Vector2(direction.x, direction.z));
        }
    }
}