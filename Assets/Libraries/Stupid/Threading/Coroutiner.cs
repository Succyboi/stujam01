using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Stupid {
    /*
    public static class Coroutiner {
        #region Classes

        public class CoDelay {
            public virtual bool Expired => true;

            public Action Action { get; private set; } = null;

            public CoDelay(Action action) {
                this.Action = action;
            }
        }

        public class CoDelayTime : CoDelay {
            public override bool Expired => Time >= expireTime;

            private float expireTime;

            public CoDelayTime(Action action, float delay) : base(action) {
                expireTime = Time + delay;
            }
        }

        public class CoDelayFrames : CoDelay {
            public override bool Expired => Frame >= expireFrame;

            private int expireFrame;

            public CoDelayFrames(Action action, int delay) : base(action) {
                expireFrame = Frame + delay;
            }
        }

        public class CoDelayUntil : CoDelay {
            public override bool Expired => expireCondition?.Invoke() ?? false;

            private Func<bool> expireCondition;

            public CoDelayUntil(Action action, Func<bool> condition) : base(action) {
                expireCondition = condition;
            }
        }

        #endregion

        #region Variables

        public static float Time => timeSource?.Invoke() ?? 0f;
        public static int Frame => frameSource?.Invoke() ?? 0;

        private static List<CoDelay> delays = new List<CoDelay>();
        private static Func<float> timeSource = null;
        private static Func<int> frameSource = null;
        private static Action updateDelegate = null;
        private static bool bound = false;

        #endregion

        #region Functions

        public static void Bind(Func<float> timeSource, Func<int> frameSource, Action updateDelegate) {
            if (bound) {
                UnBind();
            }
            
            Coroutiner.timeSource = timeSource;
            Coroutiner.frameSource = frameSource;
            Coroutiner.updateDelegate = updateDelegate;

            updateDelegate += Update;

            bound = true;
        }

        public static void UnBind() {
            if (!bound) { return; }

            updateDelegate -= Update;

            timeSource = null;
            frameSource = null;
            updateDelegate = null;

            bound = false;
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

        public static void Update() {
            for (int d = delays.Count - 1; d >= 0; d--) {
                if (!delays[d].Expired) { continue; }

                CoDelay delay = delays[d];

                delays.RemoveAt(d);
                delay.Action?.Invoke();
            }
        }

        #endregion
    }
    */
}