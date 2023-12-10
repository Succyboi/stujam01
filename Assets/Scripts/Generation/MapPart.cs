using System;
using UnityEngine;

namespace Stupid.stujam01 {
    public class MapPart : MonoBehaviour {
        [Serializable]
        public class GenerationSettings {
            [Header("Constant")]
            public float Weight = 1f;
        }

        public Vector3 spawnPosition => spawnTransform.position;
        public GenerationSettings Settings => settings;

        [SerializeField] private GenerationSettings settings;

        [Header("Spawning")]
        [SerializeField] private LayerMask spawnPositionMask;
        [SerializeField] private float spawnCheckHeightOffset;
        [SerializeField] private float spawnCheckHeightRadius;

        [Header("References")]
        [SerializeField] private Transform spawnTransform;

        private MapGenerator mapGenerator;

        public void Initialize(MapGenerator mapGenerator, Random random) {
            this.mapGenerator = mapGenerator;

            transform.Rotate(Vector3.up, 90f * random.Range(0, 3));

            if(!Physics.SphereCast(spawnTransform.position + UnityEngine.Vector3.up * spawnCheckHeightOffset, spawnCheckHeightRadius, Vector3.down, out RaycastHit hit, Mathf.Infinity, spawnPositionMask, QueryTriggerInteraction.Ignore)) { return; }

            spawnTransform.position = hit.point;
        }
    }
}