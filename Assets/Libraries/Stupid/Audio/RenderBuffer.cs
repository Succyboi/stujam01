using System;

namespace Stupid.Audio {
    // Class for passing audio between objects
    [Serializable]
    public class RenderBuffer {
        public int SampleRate { get; private set; }
        public int Length => BufferSize;
        public int BufferSize => buffer.Length;
        public uint Tick { get; set; }

        private float[] buffer;

        public RenderBuffer(int sampleRate) : this(sampleRate, null) { }
        public RenderBuffer(int sampleRate, int bufferSize) : this(sampleRate, new float[bufferSize]) { }
        public RenderBuffer(int sampleRate, float[] buffer) {
            this.SampleRate = sampleRate;
            this.buffer = buffer;
        }

        public static implicit operator float[](RenderBuffer from) => from.buffer;

        public float this[int i] {
            get {
                if(buffer == null) { return 0f; }

                return buffer[i]; 
            }
            set {
                if (buffer == null) { return; }

                buffer[i] = value; 
            }
        }

        public void Clear() {
            for(int s = 0; s < buffer.Length; s++) {
                buffer[s] = 0f;
            }

            Tick += (uint)buffer.Length;
        }
    }

    public class MissingRenderException : Exception {
        public MissingRenderException() { }
        public MissingRenderException(string message) : base(message) { }
        public MissingRenderException(string message, Exception inner) : base(message, inner) { }
    }
}
