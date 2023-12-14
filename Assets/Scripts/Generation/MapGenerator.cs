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
            public MapPart[] CollapsedParts;
            public MapPart[] MandatoryParts;
            public Vector2Int[] MandatoryPartPositions;

            public Map(Vector2Int size, int mandatory) {
                this.Size = size;
                Parts = new MapPart[size.x, size.y];
                CollapsedParts = new MapPart[size.x * size.y];
                MandatoryParts = new MapPart[mandatory];
                MandatoryPartPositions = new Vector2Int[mandatory];
            }
        }

        [SerializeField] private GenerationSettings settings;
        [SerializeField] private MapPart[] mapParts;
        [SerializeField] private MapPart[] mandatoryMapParts;

        private Map map;
        private Random random;

        public void Generate(uint seed, bool registerNetworkObjects = true) {
            Map oldMap = map;
            bool fresh = map == null;

            random = new Random(seed);
            map = new Map(new Vector2Int(random.Range(settings.MinMapSize, settings.MaxMapSize), 
                random.Range(settings.MinMapSize, settings.MaxMapSize)),
                mandatoryMapParts.Length);

            if (fresh) {
                for (int p = 0; p < mandatoryMapParts.Length; p++) {
                    int x = random.Range(0, map.Size.x - 1);
                    int y = random.Range(0, map.Size.y - 1);

                    map.Parts[x, y] = Instantiate(mandatoryMapParts[p], transform);
                    map.Parts[x, y].transform.localPosition = new Vector3(x / 2f * settings.MapPartSize.x, 0f, y / 2f * settings.MapPartSize.y);
                    map.Parts[x, y].Initialize(this, random, registerNetworkObjects);

                    map.CollapsedParts[x + y * map.Size.x] = map.Parts[x, y];
                    map.MandatoryParts[p] = map.Parts[x, y];
                    map.MandatoryPartPositions[p] = new Vector2Int(x, y);
                }
            }
            else {
                for (int p = 0; p < oldMap.MandatoryParts.Length; p++) {
                    int x = random.Range(0, map.Size.x - 1);
                    int y = random.Range(0, map.Size.y - 1);

                    map.Parts[x, y] = oldMap.MandatoryParts[p];
                    map.Parts[x, y].transform.localPosition = new Vector3(x / 2f * settings.MapPartSize.x, 0f, y / 2f * settings.MapPartSize.y);
                    map.Parts[x, y].Initialize(this, random, registerNetworkObjects);

                    map.CollapsedParts[x + y * map.Size.x] = map.Parts[x, y];
                    map.MandatoryParts[p] = map.Parts[x, y];
                    map.MandatoryPartPositions[p] = new Vector2Int(x, y);
                }
            }

            for (int x = 0; x < map.Size.x; x++) {
                for (int y = 0; y < map.Size.y; y++) {
                    if (map.Parts[x, y] != null) { continue; }

                    map.Parts[x, y] = Instantiate(GetRandomPart(), transform);
                    map.Parts[x, y].transform.localPosition = new Vector3(x / 2f * settings.MapPartSize.x, 0f, y / 2f * settings.MapPartSize.y);
                    map.Parts[x, y].Initialize(this, random, registerNetworkObjects);

                    map.CollapsedParts[x + y * map.Size.x] = map.Parts[x, y];
                }
            }
        }

        public void Clear() {
            if(map == null) { return; }

            for (int x = 0; x < map.Size.x; x++) {
                for (int y = 0; y < map.Size.y; y++) {
                    if(map.MandatoryPartPositions.Any(p => p.x == x && p.y == y)) { continue; }

                    map.Parts[x, y].Destroy();
                }
            }
        }

        public MapPart[] GetMapParts() {
            return map.CollapsedParts;
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