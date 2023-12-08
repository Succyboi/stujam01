using System;

namespace Stupid.Audio {
    // A module that produces renders of a determined length
    // For example percussive sounds
    [Serializable]
    public class Clip : Module {
        public virtual float DurationInSeconds => DurationInSamples / sampleRate;
        public virtual int DurationInSamples => Moths.RoundToInt(DurationInSeconds * sampleRate);

        public virtual RenderBuffer Render(int sampleRate) {
            Trigger(0);
            RenderBuffer = new RenderBuffer(sampleRate, DurationInSamples);
            EvaluateBuffer();

            return RenderBuffer;
        }
    }
}
