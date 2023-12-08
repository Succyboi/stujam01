using System;
using UnityEngine;

namespace Stupid.stujam01 {
    public class MapPart : MonoBehaviour {
        [Serializable]
        public class GenerationSettings {
            [Header("Constant")]
            public float Weight = 1f;
        }

        public GenerationSettings Settings => settings;

        [SerializeField] private GenerationSettings settings;

        private MapGenerator mapGenerator;

        public void Initialize(MapGenerator mapGenerator, Random random) {
            this.mapGenerator = mapGenerator;

            transform.Rotate(Vector3.up, 90f * random.Range(0, 3));
        }
    }
}