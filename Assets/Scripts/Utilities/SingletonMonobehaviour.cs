using UnityEngine;

namespace Stupid.Utilities {
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : Component {
        public virtual bool ForceSingleInstance => true;

        public static T Instance {
            get {
                if (instance != null) return instance;
                instance = FindObjectOfType<T>();
                if (instance != null) return instance;

                var obj = new GameObject { name = typeof(T).Name };
                instance = obj.AddComponent<T>();
                return instance;
            }
        }
        public static bool Exists => instance != null;

        private static T instance;

        protected virtual void Awake() {
            if (instance == null || instance == this) {
                instance = this as T;
            }
            else if ((instance as SingletonMonoBehaviour<T>).ForceSingleInstance) {
                Debug.LogWarning($"An instance of {typeof(T).Name} already exists, deleting this instance.", this.gameObject);
                Destroy(this);
            }
            else {
                Debug.LogWarning($"An instance of {typeof(T).Name} already exists, there may now be multple instances.", this.gameObject);
                instance = this as T;
            }
        }

        protected virtual void OnDestroy() {
            if (instance != this) { return; }

            instance = null;
        }
    }
}