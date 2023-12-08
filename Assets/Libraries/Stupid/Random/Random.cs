using System;

namespace Stupid {
    // Well512 by Chris Lomont From:
    // https://github.com/jarikomppa/soloud/blob/1157475881da0d7f76102578255b937c7d4e8f57/include/soloud_misc.h
    // https://github.com/jarikomppa/soloud/blob/1157475881da0d7f76102578255b937c7d4e8f57/src/core/soloud_misc.cpp
    public class Random : Singleton<Random> {
        private const float FLOAT_FRACTION = 2.3283064365386963e-10f;

        public static uint GetSeed() {
            if (!Exists) {
                Instance = new Random((uint)Environment.TickCount);
            }

            return Instance.NextUInt();
        }

        public uint Seed { get; private set; }
        public float Float => NextFloat();
        public uint UInt => NextUInt();
        public bool Bool => Chance(0.5f);
        public float Sign => Moths.Sign(Float);
        public float Direction1D => Sign;
        public Vector2 OnUnitCircle => Moths.AngToDir(Float * Moths.TAU);
        public Vector2 InUnitCircle => OnUnitCircle * Float;
        public Vector2 InUnitSquare => new Vector2(Float, Float);
        public Vector3 OnUnitSphere => GetOnUnitSphere();
        public Vector3 InUnitSphere => OnUnitSphere * Float;
        public Vector3 InUnitCube => new Vector3(Float, Float, Float);
        public float Angle => Float * Moths.TAU;

        private uint[] mState = new uint[16];
        private uint mIndex;

        public Random() : this(null) { }
        public Random(uint? seed = null) {
            SetSeed(seed);
        }

        public void SetSeed(uint? seed = null) {
            Seed = seed ?? GetSeed();

            mIndex = 0;
            for (uint i = 0; i < 16; i++) {
                mState[i] = Seed + i * Seed + i;
            }
        }

        // Returns true on chance.
        public bool Chance(float chance) => NextFloat() < chance;

        // Returns a random uint.
        private uint NextUInt() {
            uint a, b, c, d;
            a = mState[mIndex];
            c = mState[(mIndex + 13) & 15];
            b = a ^ c ^ (a << 16) ^ (c << 15);
            c = mState[(mIndex + 9) & 15];
            c ^= (c >> 11);
            a = mState[mIndex] = b ^ c;
            d = a ^ ((a << 5) & 0xDA442D20u);
            mIndex = (mIndex + 15) & 15;
            a = mState[mIndex];
            mState[mIndex] = a ^ b ^ d ^ (a << 2) ^ (b << 18) ^ (c << 28);
            return mState[mIndex];
        }
        
        // Returns a float in the 0-1 range.
        private float NextFloat() {
            return NextUInt() * FLOAT_FRACTION;
        }

        // Returns a float in the specified range.
        public float Range(float minInclusive, float maxInclusive) => Moths.Lerp(minInclusive, maxInclusive, NextFloat());
        
        // Returns an int in the specified range.
        public int Range(int minInclusive, int maxInclusive) => Moths.RoundToInt(Range((float)minInclusive, (float)maxInclusive));

        public Vector3 GetOnUnitSphere() {
            float u = Float;
            float v = Float;
            float theta = 2 * Moths.PI * u;
            float phi = Moths.Acos(2 * v - 1);
            float x = Moths.Sin(phi) * Moths.Cos(theta);
            float y = Moths.Sin(phi) * Moths.Sin(theta);
            float z = Moths.Cos(phi);
            return new Vector3(x, y, z);
        }
    }
}