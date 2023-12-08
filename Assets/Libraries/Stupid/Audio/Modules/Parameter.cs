namespace Stupid.Audio {
    // Parameter class for safe and smooth parameter changes on the audio thread from other threads.
    public abstract class Parameter<T> : Module {
        protected const float DEFAULT_SMOOTHING_MS = 1f;

        public T Value {
            get { return GetValue(); }
            set { SetValue(value); }
        }
        public T Min { get; private set; }
        public T Max { get; private set; }
        public float SmoothingMS { get; private set; }

        protected float coef => Moths.Exp(-1000f / (SmoothingMS * sampleRate));
        protected float targetValue;
        protected float internalValue;

        public Parameter(T min, T max, T value, float smoothingMS = DEFAULT_SMOOTHING_MS) {
            this.Min = min;
            this.Max = max;
            this.SmoothingMS = smoothingMS;

            SetValue(value, true);
        }

        protected abstract void SetValue(T value, bool instant = false);

        protected abstract T GetValue();

        protected override float Evaluate() {
            float target = Moths.Denormal(targetValue * targetValue);
            internalValue = target + coef * (internalValue - target);

            return 0f;
        }
    }

    public class FloatParameter : Parameter<float> {
        public FloatParameter(float min, float max, float value, float smoothingMS = 1) : base(min, max, value, smoothingMS) {}

        protected override void SetValue(float value, bool instant = false) {
            targetValue = Moths.InverseLerp(Min, Max, value);

            if (instant) {
                internalValue = targetValue * targetValue;
            }
        }

        protected override float GetValue() {
            return Moths.Lerp(Min, Max, Moths.Sqrt(internalValue));
        }
    }

    public class IntParameter : Parameter<int> {
        public IntParameter(int min, int max, int value, float smoothingMS = 1) : base(min, max, value, smoothingMS) { }

        protected override void SetValue(int value, bool instant = false) {
            targetValue = Moths.InverseLerp(Min, Max, value);

            if (instant) {
                internalValue = targetValue * targetValue;
            }
        }

        protected override int GetValue() {
            return Moths.Lerp(Min, Max, Moths.Sqrt(internalValue));
        }
    }

    public class BoolParameter : Parameter<bool> {
        public BoolParameter(bool value) : base(false, true, value, DEFAULT_SMOOTHING_MS) { }

        protected override void SetValue(bool value, bool instant = false) {
            targetValue = value ? 1f : 0f;
        }

        protected override bool GetValue() {
            return targetValue > 0.5f;
        }
    }
}
