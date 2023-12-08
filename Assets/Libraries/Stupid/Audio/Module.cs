using System;

namespace Stupid.Audio {
    // Class for rendering audio in realtime.
    [Serializable]
    public abstract class Module {
        public RenderBuffer RenderBuffer {
            get {
                return renderBuffer;
            }
            set {
                if(renderBuffer != value) {
                    renderBuffer = value;

                    HandleRenderBufferChanged();
                    return;
                }

                renderBuffer = value;
            }
        }

        protected uint triggerTick { get; private set; }
        protected int sampleRate => RenderBuffer.SampleRate;
        protected uint tick => RenderBuffer.Tick;
        protected RenderBuffer renderBuffer;

        #region Operators

        public static Module operator ++(Module from) {
            if (from.RenderBuffer == null) {
                throw new MissingRenderException($"Missing {typeof(RenderBuffer)}. Cannot evaluate {typeof(Module)}.");
            }

            from.EvaluateBuffer();
            return from;
        }

        public static Module operator +(Module from, Module to) {
            if(from.RenderBuffer == null) {
                throw new MissingRenderException($"Missing {typeof(RenderBuffer)}. Cannot evaluate {typeof(Module)}.");
            }

            to.RenderBuffer = from.RenderBuffer;
            to.EvaluateInputBuffer();

            return to;
        }

        public static Module operator +(RenderBuffer renderBuffer, Module by) {
            by.RenderBuffer = renderBuffer;
            by.EvaluateInputBuffer();

            return by;
        }

        public static implicit operator RenderBuffer(Module from) => from.RenderBuffer;

        #endregion

        #region Functions

        public void Trigger(uint triggerTick) {
            this.triggerTick = triggerTick;

            Trigger();
        }
        
        public float Evaluate(Module parent) {
            RenderBuffer = parent.RenderBuffer;

            return Evaluate();
        }

        public float EvaluateInput(Module parent, float input) {
            RenderBuffer = parent.RenderBuffer;

            return EvaluateInput(input);
        }

        protected virtual void Trigger() {
            throw new NotImplementedException();
        }

        protected virtual float Evaluate() {
            throw new NotImplementedException();
        }

        protected virtual void EvaluateBuffer() {
            for (int d = 0; d < RenderBuffer.Length; d++) {
                RenderBuffer[d] = Evaluate();
                RenderBuffer.Tick++;
            }

            RenderBuffer.Tick -= (uint)RenderBuffer.Length;
        }

        protected virtual float EvaluateInput(float input) {
            throw new NotImplementedException();
        }

        protected virtual void EvaluateInputBuffer() {
            for (int d = 0; d < RenderBuffer.Length; d++) {
                RenderBuffer[d] = EvaluateInput(RenderBuffer[d]);
                RenderBuffer.Tick++;
            }

            RenderBuffer.Tick -= (uint)RenderBuffer.Length;
        }

        protected virtual void HandleRenderBufferChanged() { }

        #endregion
    }
}