using System;

namespace Stupid.Audio{
    [Serializable]
    public class TiltEq : Module {
        public virtual float BottomFreq => 500f;
        public virtual float TopFreq => 1500f;
        public virtual float LowMix => Moths.Lerp(1f, 0f, Moths.InverseLerp(0.5f, 1f, Tilt));
        public virtual float HighMix => Moths.Lerp(0f, 1f, Moths.InverseLerp(0f, 0.5f, Tilt));

        public float Tilt = 0.5f;
        public float Resonance = 0.75f;

        private float pole0;
        private float pole1;

        protected override float EvaluateInput(float Input) {
            float freq = Moths.Lerp(BottomFreq, TopFreq, Tilt);
            float f = 2f * Moths.Sin(Moths.PI * freq / sampleRate);
            float fb = Resonance + Resonance / (1.0f - f);

            pole0 = pole0 + f * (Input - pole0 + fb * (pole0 - pole1));
            pole1 = pole1 + f * (pole0 - pole1);

            return LowMix * pole1 + HighMix * (Input - pole1);
        }
    }

    [Serializable]
    public class SVF : Module {
        // https://www.musicdsp.org/en/latest/Filters/92-state-variable-filter-double-sampled-stable.html
        // Andrew Simper, Laurent de Soras and Steffan Diedrichsen

        public enum Mode {
            Low,
            High,
            Band,
            Notch
        }

        public Mode FilterMode;
        public float Cutoff = Pitch.MAX_AUDIBLE_FREQUENCY;
        public float Resonance = 0f;
        public float Distortion = 0f;

        private float low = 0f;
        private float high = 0f;
        private float band = 0f;
        private float notch = 0f;

        protected override float EvaluateInput(float Input) {
            float output = 0f;
            float cutoffFreq = 2.0f * Moths.Sin(Moths.PI * Moths.Min(0.25f, Cutoff / (sampleRate * 2)));
            float damp = Moths.Min(2.0f * (1.0f - Moths.Pow(Resonance, 0.25f)), Moths.Min(2.0f, 2.0f / cutoffFreq - cutoffFreq * 0.5f));

            notch = Input - damp * band;
            low = low + cutoffFreq * band;
            high = notch - low;
            band = cutoffFreq * high + band - Distortion * band * band * band;

            output += GetFilterSignal() * 0.5f;

            notch = Input - damp * band;
            low = low + cutoffFreq * band;
            high = notch - low;
            band = cutoffFreq * high + band - Distortion * band * band * band;

            output += GetFilterSignal() * 0.5f;

            return output;
        }

        private float GetFilterSignal() {
            switch (FilterMode) {
                default:
                case Mode.Low:
                    return low;

                case Mode.High:
                    return high;

                case Mode.Band:
                    return band;

                case Mode.Notch:
                    return notch;
            }
        }
    }
}
