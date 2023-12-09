using System;
using UnityEngine;

namespace Stupid {
    [Serializable]
    public class Cooldown {
        public Cooldown(float duration) {
            this.Duration = duration;
        }

        public bool IsFinished(float currentTime) {
            return Left(currentTime) <= 0f;
        }
        public float Elapsed(float currentTime) {
            return Mathf.Min(currentTime - timeStamp, Duration);
        }
        public float Left(float currentTime) {
            return Mathf.Max(Duration - Elapsed(currentTime), 0f);
        }
        public float Progress(float currentTime) {
            return Elapsed(currentTime) / Duration;
        }
        public float Duration = 0f;

        protected float timeStamp;

        public void Finish(float currentTime) {
            timeStamp = currentTime - Duration;
        }
        public void Start(float currentTime) => Reset(currentTime);
        public void Reset(float currentTime) {
            timeStamp = currentTime;
        }
    }
}