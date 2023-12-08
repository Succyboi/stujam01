using System;
using System.Linq;
using UnityEngine;

namespace Stupid.stujam01 {
    public class MapGenerator : MonoBehaviour {
        [Serializable]
        public class GenerationSettings {
            [Header("Static")]
            public Vector2 MapPartSize;

            [Header("Ranged")]
            public int MinMapSize;
            public int MaxMapSize;
        }

        [Serializable]
        public class Map {
            public Vector2Int Size;
            public MapPart[,] Parts;

            public Map(Vector2Int size) {
                this.Size = size;
                Parts = new MapPart[size.x, size.y];
            }
        }

        [SerializeField] private GenerationSettings settings;
        [SerializeField] private MapPart[] mapParts;

        private Map map;
        private Random random;

        public void Generate(uint seed) {
            random = new Random(seed);
            map = new Map(new Vector2Int(random.Range(settings.MinMapSize, settings.MaxMapSize), 
                random.Range(settings.MinMapSize, settings.MaxMapSize)));

            for(int x = 0; x < map.Size.x; x++) {
                for (int y = 0; y < map.Size.y; y++) {
                    map.Parts[x, y] = Instantiate(GetRandomPart(), transform);
                    map.Parts[x, y].transform.localPosition = new Vector3(x / 2f * settings.MapPartSize.x, 0f, y / 2f * settings.MapPartSize.y);
                    map.Parts[x, y].Initialize(this, random);
                }
            }
        }

        public void Clear() {
            for (int x = 0; x < map.Size.x; x++) {
                for (int y = 0; y < map.Size.y; y++) {
                    Destroy(map.Parts[x, y]);
                }
            }
        }

        private MapPart GetRandomPart() {
            float totalWeight = mapParts.Sum(p => p.Settings.Weight);
            float selectedWeight = random.Range(0f, totalWeight);
            float weight = 0;

            foreach (MapPart mapPart in mapParts) {
                weight += mapPart.Settings.Weight;
                if (selectedWeight < weight) {
                    return mapPart;
                }
            }

            throw new Exception($"The total weight must be greater than 0");
        }
    }
}