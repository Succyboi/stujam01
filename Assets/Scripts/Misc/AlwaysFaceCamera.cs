using UnityEngine;

namespace Stupid.stujam01 {
    public class AlwaysFaceCamera : MonoBehaviour {
        [SerializeField] private Vector3 additionalRotation;

        private void OnEnable() {
            Camera.onPreRender += OnPreRender;
        }

        private void OnDisable() {
            Camera.onPreRender -= OnPreRender;
        }

        private void OnPreRender(Camera camera) {
            transform.rotation = Quaternion.LookRotation(-(camera.transform.position - transform.position).normalized) * Quaternion.Euler(additionalRotation);
        }
    }
}