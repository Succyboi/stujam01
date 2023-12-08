using System;

namespace Stupid {
    public static class Sterp {
        public enum Mode {
            Linear,

            InQuad,
            OutQuad,
            InOutQuad,

            InCubic,
            OutCubic,
            InOutCubic,

            InQuart,
            OutQuart,
            InOutQuart,

            InQuint,
            OutQuint,
            InOutQuint,

            InSine,
            OutSine,
            InOutSine,

            InExpo,
            OutExpo,
            InOutExpo,

            InCirc,
            OutCirc,
            InOutCirc,

            InElastic,
            OutElastic,
            InOutElastic,

            InBack,
            OutBack,
            InOutBack,

            InBounce,
            OutBounce,
            InOutBounce,
        }

        public static float Ease(float a, float b, float t, Mode mode) => Moths.Lerp(a, b, Ease(t, mode));

        public static float Ease(float t, Mode mode) => Moths.DeNaN(EaseUnsafe(t, mode));
        private static float EaseUnsafe(float t, Mode mode) {
            switch (mode) {
                default:
                    return t;

                case Mode.InQuad:
                    return InQuad(t);
                case Mode.OutQuad:
                    return OutQuad(t);
                case Mode.InOutQuad:
                    return InOutQuad(t);

                case Mode.InCubic:
                    return InCubic(t);
                case Mode.OutCubic:
                    return OutCubic(t);
                case Mode.InOutCubic:
                    return InOutCubic(t);

                case Mode.InQuart:
                    return InQuart(t);
                case Mode.OutQuart:
                    return OutQuart(t);
                case Mode.InOutQuart:
                    return InOutQuart(t);

                case Mode.InQuint:
                    return InQuint(t);
                case Mode.OutQuint:
                    return OutQuint(t);
                case Mode.InOutQuint:
                    return InOutQuint(t);

                case Mode.InSine:
                    return InSine(t);
                case Mode.OutSine:
                    return OutSine(t);
                case Mode.InOutSine:
                    return InOutSine(t);

                case Mode.InExpo:
                    return InExpo(t);
                case Mode.OutExpo:
                    return OutExpo(t);
                case Mode.InOutExpo:
                    return InOutExpo(t);

                case Mode.InCirc:
                    return InCirc(t);
                case Mode.OutCirc:
                    return OutCirc(t);
                case Mode.InOutCirc:
                    return InOutCirc(t);

                case Mode.InElastic:
                    return InElastic(t);
                case Mode.OutElastic:
                    return OutElastic(t);
                case Mode.InOutElastic:
                    return InOutElastic(t);

                case Mode.InBack:
                    return InBack(t);
                case Mode.OutBack:
                    return OutBack(t);
                case Mode.InOutBack:
                    return InOutBack(t);

                case Mode.InBounce:
                    return InBounce(t);
                case Mode.OutBounce:
                    return OutBounce(t);
                case Mode.InOutBounce:
                    return InOutBounce(t);
            }
        }

        private static float InQuad(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return c * (t /= d) * t + b;
        }

        private static float OutQuad(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return -c * (t /= d) * (t - 2) + b;
        }

        private static float InOutQuad(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            if ((t /= d / 2) < 1) return c / 2 * t * t + b;
            return -c / 2 * ((--t) * (t - 2) - 1) + b;
        }

        private static float InCubic(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return c * (t /= d) * t * t + b;
        }

        private static float OutCubic(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return c * ((t = t / d - 1) * t * t + 1) + b;
        }

        private static float InOutCubic(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t + 2) + b;
        }

        private static float InQuart(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return c * (t /= d) * t * t * t + b;
        }

        private static float OutQuart(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return -c * ((t = t / d - 1) * t * t * t - 1) + b;
        }

        private static float InOutQuart(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;
            return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
        }

        private static float InQuint(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return c * (t /= d) * t * t * t * t + b;
        }

        private static float OutQuint(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
        }

        private static float InOutQuint(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
        }

        private static float InSine(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return -c * Moths.Cos(t / d * (Moths.PI / 2)) + c + b;
        }

        private static float OutSine(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return c * Moths.Sin(t / d * (Moths.PI / 2)) + b;
        }

        private static float InOutSine(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return -c / 2 * (Moths.Cos(Moths.PI * t / d) - 1) + b;
        }

        private static float InExpo(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return (t == 0) ? b : c * Moths.Pow(2, 10 * (t / d - 1)) + b;
        }

        private static float OutExpo(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return (t == d) ? b + c : c * (-Moths.Pow(2, -10 * t / d) + 1) + b;
        }

        private static float InOutExpo(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            if (t == 0) return b;
            if (t == d) return b + c;
            if ((t /= d / 2) < 1) return c / 2 * Moths.Pow(2, 10 * (t - 1)) + b;
            return c / 2 * (-Moths.Pow(2, -10 * --t) + 2) + b;
        }

        private static float InCirc(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return -c * (Moths.Sqrt(1 - (t /= d) * t) - 1) + b;
        }

        private static float OutCirc(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            return c * Moths.Sqrt(1 - (t = t / d - 1) * t) + b;
        }

        private static float InOutCirc(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            if ((t /= d / 2) < 1) return -c / 2 * (Moths.Sqrt(1 - t * t) - 1) + b;
            return c / 2 * (Moths.Sqrt(1 - (t -= 2) * t) + 1) + b;
        }

        private static float InElastic(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            float s = 1.70158f; float p = 0; float a = c;
            if (t == 0) return b; if ((t /= d) == 1) return b + c; if (p == 0) p = d * .3f;
            if (a < Moths.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Moths.PI) * Moths.Asin(c / a);
            return -(a * Moths.Pow(2, 10 * (t -= 1)) * Moths.Sin((t * d - s) * (2 * Moths.PI) / p)) + b;
        }

        private static float OutElastic(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            float s = 1.70158f; float p = 0; float a = c;
            if (t == 0) return b; if ((t /= d) == 1) return b + c; if (p == 0) p = d * .3f;
            if (a < Moths.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Moths.PI) * Moths.Asin(c / a);
            return a * Moths.Pow(2, -10 * t) * Moths.Sin((t * d - s) * (2 * Moths.PI) / p) + c + b;
        }

        private static float InOutElastic(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            float s = 1.70158f; float p = 0; float a = c;
            if (t == 0) return b; if ((t /= d / 2) == 2) return b + c; if (p == 0) p = d * (.3f * 1.5f);
            if (a < Moths.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Moths.PI) * Moths.Asin(c / a);
            if (t < 1) return -.5f * (a * Moths.Pow(2, 10 * (t -= 1)) * Moths.Sin((t * d - s) * (2 * Moths.PI) / p)) + b;
            return a * Moths.Pow(2, -10 * (t -= 1)) * Moths.Sin((t * d - s) * (2 * Moths.PI) / p) * .5f + c + b;
        }

        private static float InBack(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            float s = 1.70158f;
            return c * (t /= d) * t * ((s + 1) * t - s) + b;
        }

        private static float OutBack(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            float s = 1.70158f;
            return c * ((t = t / d - 1) * t * ((s + 1) * t + s) + 1) + b;
        }

        private static float InOutBack(float x) {
            float t = x; float b = 0; float c = 1; float d = 1;
            float s = 1.70158f;
            if ((t /= d / 2) < 1) return c / 2 * (t * t * (((s *= (1.525f)) + 1) * t - s)) + b;
            return c / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + b;
        }

        private static float OutBounce(float x, float t, float b, float c, float d) {
            if ((t /= d) < (1 / 2.75f)) {
                return c * (7.5625f * t * t) + b;
            }
            else if (t < (2 / 2.75f)) {
                return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + b;
            }
            else if (t < (2.5 / 2.75f)) {
                return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + b;
            }
            else {
                return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + b;
            }
        }

        private static float OutBounce(float x) {
            return OutBounce(x, x, 0, 1, 1);
        }

        private static float InBounce(float x, float t, float b, float c, float d) {
            return c - OutBounce(x, d - t, 0, c, d) + b;
        }

        private static float InBounce(float x) {
            return OutBounce(x, x, 0, 1, 1);
        }

        private static float InOutBounce(float x, float t, float b, float c, float d) {
            if (t < d / 2) return OutBounce(x, t * 2, 0, c, d) * .5f + b;
            return OutBounce(x, t * 2 - d, 0, c, d) * .5f + c * .5f + b;
        }

        private static float InOutBounce(float x) {
            return InOutBounce(x, x, 0, 1, 1);
        }
    }
}