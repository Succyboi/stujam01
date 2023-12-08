using System;

namespace Stupid.Audio {
    [Serializable]
    public class ADRMS : Module {
        public const float SILENT_TRESHOLD = 1.0E-10f;

        public float AttackMS = 1f;
        public float DecayMS = 1f;

        public float Level => Moths.Sqrt(state);
        public bool Silent => Level <= SILENT_TRESHOLD;

        private float attackCoef => Moths.Exp(-1000f / (AttackMS * sampleRate));
        private float decayCoeff => Moths.Exp(-1000f / (AttackMS * sampleRate));
        private float state;
        private float lastState;

        protected override float EvaluateInput(float input) {
            float power = Moths.Denormal(Moths.Abs(input * input));
            float delta = state - lastState;
            state = power + (delta > 0 ? attackCoef : decayCoeff) * (state - power);

            lastState = state;
            return input;
        }

        protected override void Trigger() {
            state = 0f;
        }
    }

    [Serializable]
    public class RMS : Module {
        public const float SILENT_TRESHOLD = 1.0E-10f;

        public float WindowSizeMS = 1f;

        public float Level => Moths.Sqrt(state);
        public bool Silent => Level <= SILENT_TRESHOLD;

        private float windowCoef => Moths.Exp(-1000f / (WindowSizeMS * sampleRate));
        private float state;

        protected override float EvaluateInput(float input) {
            float power = Moths.Denormal(Moths.Abs(input * input));
            state = power + windowCoef * (state - power);

            return input;
        }

        protected override void Trigger() {
            state = 0f;
        }
    }
}