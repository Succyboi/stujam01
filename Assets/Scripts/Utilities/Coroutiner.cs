using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Stupid.Utilities;
using Random = System.Random;

namespace Stupid.Utilities {
    [Serializable]
    public class CoTimer {
        public CoTimer(float duration) {
            this.Duration = duration;
        }

        public bool Finished => Left == 0f;
        public float Elapsed => Mathf.Min(time - timeStamp, Duration);
        public float Left => Mathf.Max(Duration - Elapsed, 0f);
        public float Progress => Elapsed / Duration;
        public float Duration = 0f;

        protected virtual float time => Time.time;
        protected float timeStamp;

        public void Finish() {
            timeStamp = time - Duration;
        }
        public void Start() => Reset();
        public void Reset() {
            timeStamp = time;
        }
    }

    public class Coroutiner : SingletonMonoBehaviour<Coroutiner> {
        public class CoDelay {
            public virtual bool Expired => true;

            public Action Action { get; private set; } = null;

            public CoDelay(Action action) {
                this.Action = action;
            }
        }

        public class CoDelayTime : CoDelay {
            public override bool Expired => Time.time >= expireTime;

            private float expireTime;

            public CoDelayTime(Action action, float delay) : base(action) {
                expireTime = Time.time + delay;
            }
        }

        public class CoDelayFrames : CoDelay {
            public override bool Expired => Time.frameCount >= expireFrame;

            private int expireFrame;

            public CoDelayFrames(Action action, int delay) : base(action) {
                expireFrame = Time.frameCount + delay;
            }
        }

        public class CoDelayUntil : CoDelay {
            public override bool Expired => expireCondition?.Invoke() ?? false;

            private Func<bool> expireCondition;

            public CoDelayUntil(Action action, Func<bool> condition) : base(action) {
                expireCondition = condition;
            }
        }

        public static WaitForEndOfFrame WaitForEndOfFrame { get; } = new WaitForEndOfFrame();
        public static WaitForFixedUpdate WaitForFixedUpdate { get; } = new WaitForFixedUpdate();

        private static List<CoDelay> delays = new List<CoDelay>();

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize() {
            if (Exists) { return; }

            GameObject instanceGameObject = new GameObject();
            instanceGameObject.AddComponent<Coroutiner>();
        }

        public static void ReInitialize() {
            if (!Exists) { return; }

            Destroy(Instance.gameObject);
            Initialize();
        }

        public static void DelayTime(Action action, float delay) {
            if (delay <= 0) {
                action?.Invoke();
                return;
            }

            delays.Add(new CoDelayTime(action, delay));
        }

        public static void DelayFrames(Action action, int delay) {
            if (delay <= 0) {
                action?.Invoke();
                return;
            }

            delays.Add(new CoDelayFrames(action, delay));
        }

        public static void DelayUntil(Action action, Func<bool> condition) {
            if (condition.Invoke()) {
                action?.Invoke();
                return;
            }

            delays.Add(new CoDelayUntil(action, condition));
        }

        public static void DoASAP(Action action) {
            delays.Add(new CoDelay(action));
        }

        public static void DoAsync(Action action, Action<bool> callback = null) {
            Task task = Task.Run(action);

            DelayUntil(() => callback?.Invoke(task.IsCompletedSuccessfully),
                () => task.IsCompleted);
        }

        public static Coroutine Start(IEnumerator coroutine) => Instance.StartCoroutine(coroutine);
        public static void Stop(IEnumerator coroutine) => Instance.StopCoroutine(coroutine);
        public static void Stop(Coroutine coroutine) {
            if (coroutine == null) { return; }
            Instance.StopCoroutine(coroutine);
        }

        protected override void Awake() {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            for (int d = delays.Count - 1; d >= 0; d--) {
                if (!delays[d].Expired) { continue; }

                CoDelay delay = delays[d];

                delays.RemoveAt(d);
                delay.Action?.Invoke();
            }
        }
    }
}