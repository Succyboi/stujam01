using System;
using System.Runtime.CompilerServices;

namespace Stupid {
	// Contains code borrowed from Freya Holmér (https://github.com/FreyaHolmer/Mathfs)
	// Mathematics
	public static partial class Moths {
		#region Float

		//TODO: "Unborrow" borrowed moths to here

		// The infamous ''3.14159265358979...''.
		public const float PI = (float)Math.PI;

		// 2 * PI.
		public const float TWO_PI = PI * 2f;
		public const float TAU = TWO_PI;

		// PI / 2f
		public const float HALF_PI = PI / 2f;

		// A representation of positive infinity (RO).
		public const float Infinity = float.PositiveInfinity;

		// A representation of negative infinity.
		public const float NegativeInfinity = float.NegativeInfinity;

		// Degrees-to-radians conversion constant.
		public const float Deg2Rad = PI * 2f / 360F;

		// Radians-to-degrees conversion constant.
		public const float Rad2Deg = 1f / Deg2Rad;

		// The smallest positive floating point value.
		public static readonly float Epsilon = float.Epsilon;

		// Small float value
		public static readonly float Small = 0.00001f;

		// Euler's number.
		public const float E = 2.71828182846f;

		// The golden ratio.
		public const float GOLDEN_RATIO = 1.61803398875f;

		// The square root of two.
		public const float SQRT2 = 1.41421356237f;

		// The reciprocal of the square root of two.
		public const float RSQRT2 = 1f / SQRT2;

		#endregion

		#region Int

		// Interpolates between /a/ and /b/ by /t/. /t/ is clamped between 0 and 1.
		public static int Lerp(int a, int b, float t) {
			return RoundToInt(Lerp((float)a, (float)b, t));
		}

		// Interpolates between /a/ and /b/ by /t/ without clamping the interpolant.
		public static int LerpUnclamped(int a, int b, float t) {
			return RoundToInt(LerpUnclamped((float)a, (float)b, t));
		}

		#endregion

		#region Misc

		// Returns true if v is between or equal to min & max
		public static bool Within(this float v, float min, float max) => v >= min && v <= max;

		// Returns true if v is between or equal to min & max
		public static bool Within(this int v, int min, int max) => v >= min && v <= max;

		// Returns true if v is between, but not equal to min & max.
		public static bool Between(this float v, float min, float max) => v > min && v < max;

		// Returns true if v is between, but not equal to min & max.
		public static bool Between(this int v, int min, int max) => v > min && v < max;

		// Returns the direction of the input angle, as a normalized vector
		public static Vector2 AngToDir(float aRad) => new Vector2(MathF.Cos(aRad), MathF.Sin(aRad));

		// Returns the angle of the input vector, in radians.
		public static float DirToAng(Vector2 vec) => MathF.Atan2(vec.y, vec.x);

		// Returns Levenshtein distance between two strings.
		public static int LevenshteinDistance(string a, string b) {
			int aL = a.Length;
			int bL = b.Length;

			int[,] matrix = new int[aL + 1, bL + 1];

			if (aL == 0)
				return bL;

			if (bL == 0)
				return aL;

			for (int i = 0; i <= aL; matrix[i, 0] = i++) { }
			for (int j = 0; j <= bL; matrix[0, j] = j++) { }

			for (int i = 1; i <= aL; i++) {
				for (int j = 1; j <= bL; j++) {
					int cost = (b[j - 1] == a[i - 1]) ? 0 : 1;

					matrix[i, j] = Min(Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
						matrix[i - 1, j - 1] + cost);
				}
			}

			return matrix[aL, bL];
		}

		#endregion
	}
}
