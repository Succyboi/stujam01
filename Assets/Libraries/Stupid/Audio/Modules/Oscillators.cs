using System;
using Random = Stupid.Random;

namespace Stupid.Audio {
    public abstract class Oscillator : Module {
        public float FrequencyHz { get; set; } = Pitch.BaseNoteHz;

        protected float phase = 0f;

        protected override void Trigger() {
            phase = 0f;
        }
    }

    [Serializable]
    public class SineOscillator : Oscillator {
        protected override float Evaluate() {
            float delta = 2f * FrequencyHz / sampleRate;

            float returnValue = Moths.Sin(phase * Moths.PI);

            phase += delta;
            phase %= 2f;

            return returnValue;
        }
    }

    [Serializable]
    public class TriangleOscillator : Oscillator {
        protected override float Evaluate() {
            phase += 2 * FrequencyHz / sampleRate;
            phase %= 2;

            return Moths.Abs((phase % 4) - 2) - 1;
        }
    }

    [Serializable]
    public class SawOscillator : Oscillator {
        protected override float Evaluate() {
            phase += 2 * FrequencyHz / sampleRate;
            phase %= 2;

            return phase - 1;
        }
    }

    [Serializable]
    public class SquareOscillator : Oscillator {
        protected override float Evaluate() {
            phase += 2 * FrequencyHz / sampleRate;
            phase %= 2;

            return phase - 1 >= 0 ? 1f : -1f;
        }
    }

    [Serializable]
    public class NoiseOscillator : Module {
        private Random random = new Random();

        protected override void Trigger() {
            random.SetSeed();
        }

        protected override float Evaluate() {
            return random.Float * 2 - 1f;
        }
    }
}
