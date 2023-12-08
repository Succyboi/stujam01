using System;

namespace Stupid.Audio {
    [Serializable]
    public class Crusher : Module {
        public int Crush { get; set; } = 1;

        private float n;

        protected override float EvaluateInput(float input) {
            if (tick % Crush == 0) { n = input; }

            return n;
        }
    }

    [Serializable]
    public class WaveShaper : Module {
        private const float MIN_POWER = 1f;

        public float Power {
            get {
                return power;
            }

            set {
                power = Moths.Max(value, MIN_POWER);
            }
        }

        private float power = MIN_POWER;

        protected override float EvaluateInput(float input) {
            return input * (Moths.Abs(input) + Power) / (input * input + (Power - 1) * Moths.Abs(input) + 1);
        }
    }

    [Serializable]
    public class WaveFolder : Module {
        private const float MIN_POWER = 0f;
        private const float MAX_POWER = 1f;

        public float Power {
            get {
                return power;
            }

            set {
                power = Moths.Clamp(value, MIN_POWER, MAX_POWER);
            }
        }

        public float Threshold => Moths.Max(1f - Power, Moths.Epsilon) * 2f;

        private float power = MIN_POWER;

        protected override float EvaluateInput(float Input) {
            return Moths.Sign(Input) * Moths.PingPong(Moths.Abs(Input), Threshold) / Threshold;
        }
    }
}