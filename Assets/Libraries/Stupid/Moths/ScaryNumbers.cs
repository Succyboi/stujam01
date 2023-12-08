using System.Runtime.InteropServices;

namespace Stupid {
    //Features NaN filtering, denormal prevention
    //Parts scavenged from: https://stackoverflow.com/questions/639010/how-can-i-compare-a-float-to-nan-if-comparisons-to-nan-always-return-false
    public static partial class Moths {
        private const int NAN_SIGNALING = 0x7F800000;
        private const int NAN_QUIET = 0x007FFFFF;
        private const float DENORMAL_VALUE = 1.0E-25f;

        [StructLayout(LayoutKind.Explicit)]
        struct FloatUnion {
            [FieldOffset(0)]
            public float value;

            [FieldOffset(0)]
            public int binary;
        }

        private static FloatUnion union = new FloatUnion();

        public static bool IsNaN(float Input) {
            union.value = Input;
            return ((union.binary & NAN_SIGNALING) == NAN_SIGNALING) && ((union.binary & NAN_QUIET) != 0);
        }

        public static float DeNaN(float Input) => IsNaN(Input) ? 0f : Input;

        public static float Denormal(float Input) {
            return Input + DENORMAL_VALUE;
        }
    }
}