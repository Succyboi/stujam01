using System;

namespace Stupid.Audio {
    [Serializable]
    public class ASDEnvelope : Module {
        public float Duration => Attack + Sustain + Decay;
        public float DurationInSamples => Moths.CeilToInt(Duration * sampleRate);
        public bool Expired => (tick - triggerTick) > DurationInSamples;

        public float Attack = 0f;
        public float Sustain = 0f;
        public float Decay = 0f;
        public float Tension = 0.5f;

        protected override float Evaluate() {
            float t = (tick - triggerTick) / (float)sampleRate;

            return EvaluateAt(t);
        }

        protected override float EvaluateInput(float input) {
            return input * Evaluate();
        }

        public float EvaluateAt(float t) {
            float linear = t < Attack
                ? Moths.Clamp01(t / Moths.Max(Attack, Moths.Epsilon))
                : t < Attack + Sustain ? 1f : (1 - Moths.Clamp01((t - Attack - Sustain) / Moths.Max(Decay, Moths.Epsilon)));
            return AudioMoths.TenseLerp(0f, 1f, linear, Tension);
        }
    }
}