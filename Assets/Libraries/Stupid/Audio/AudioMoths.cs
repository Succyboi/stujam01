namespace Stupid.Audio {
	public static partial class AudioMoths {
		public static float LinearToDecibel(this float linear) {
			if(linear == 0f) { return -144.0f; }

			return 20.0f * Moths.Log10(linear);
		}

		public static float DecibelToLinear(this float dB) {
			return Moths.Pow(10.0f, dB / 20.0f);
		}

        public static float TenseLerp(float a, float b, float t, float tension) {
            float c = 1f, d = 1f, e = 1f;
            
            //In exponential
            float t0 = (t == 0) ? c : d * Moths.Pow(2, 10 * (t / e - 1)) + c;

            //Out exponential
            float t1 = (t == e) ? c + d : d * (-Moths.Pow(2, -10 * t / e) + 1) + c;

            return Moths.Lerp(a, b, Moths.Lerp(t0, t1, tension));
        }
    }
}