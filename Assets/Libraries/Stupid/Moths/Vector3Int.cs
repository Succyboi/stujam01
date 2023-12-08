﻿using System;
using System.Globalization;

namespace Stupid {
    // BORROWED FROM UNITY. MODIFIED TO NOT HAVE ITS DEPENDENCIES. LIKELY VIOLATES THEIR REFERENCE ONLY LICENSE:
    // https://unity3d.com/legal/licenses/Unity_Reference_Only_License
    // Representation of 3D vectors and points.
    [Serializable]
    public partial struct Vector3Int : IEquatable<Vector3Int>, IFormattable {
        public int x {  get { return m_X; }  set { m_X = value; } }
        public int y {  get { return m_Y; }  set { m_Y = value; } }
        public int z {  get { return m_Z; }  set { m_Z = value; } }

        private int m_X;
        private int m_Y;
        private int m_Z;

        
        public Vector3Int(int x, int y) {
            m_X = x;
            m_Y = y;
            m_Z = 0;
        }

        
        public Vector3Int(int x, int y, int z) {
            m_X = x;
            m_Y = y;
            m_Z = z;
        }

        public Vector3Int(Vector2Int vector) {
            m_X = vector.x;
            m_Y = vector.y;
            m_Z = 0;
        }

        // Set x, y and z components of an existing Vector.

        public void Set(int x, int y, int z) {
            m_X = x;
            m_Y = y;
            m_Z = z;
        }

        // Access the /x/, /y/ or /z/ component using [0], [1] or [2] respectively.
        public int this[int index] {
            
            get {
                switch (index) {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default:
                        throw new IndexOutOfRangeException(string.Format("Invalid Vector3Int index addressed: {0}!", index));
                }
            }

            
            set {
                switch (index) {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default:
                        throw new IndexOutOfRangeException(string.Format("Invalid Vector3Int index addressed: {0}!", index));
                }
            }
        }

        // Returns the length of this vector (RO).
        public float magnitude {  get { return Moths.Sqrt((float)(x * x + y * y + z * z)); } }

        // Returns the squared length of this vector (RO).
        public int sqrMagnitude {  get { return x * x + y * y + z * z; } }

        // Returns the distance between /a/ and /b/.
        
        public static float Distance(Vector3Int a, Vector3Int b) { return (a - b).magnitude; }

        // Returns a vector that is made from the smallest components of two vectors.
        
        public static Vector3Int Min(Vector3Int lhs, Vector3Int rhs) { return new Vector3Int(Moths.Min(lhs.x, rhs.x), Moths.Min(lhs.y, rhs.y), Moths.Min(lhs.z, rhs.z)); }

        // Returns a vector that is made from the largest components of two vectors.
        
        public static Vector3Int Max(Vector3Int lhs, Vector3Int rhs) { return new Vector3Int(Moths.Max(lhs.x, rhs.x), Moths.Max(lhs.y, rhs.y), Moths.Max(lhs.z, rhs.z)); }

        // Multiplies two vectors component-wise.
        
        public static Vector3Int Scale(Vector3Int a, Vector3Int b) { return new Vector3Int(a.x * b.x, a.y * b.y, a.z * b.z); }

        // Multiplies every component of this vector by the same component of /scale/.
        
        public void Scale(Vector3Int scale) { x *= scale.x; y *= scale.y; z *= scale.z; }

        
        public void Clamp(Vector3Int min, Vector3Int max) {
            x = Math.Max(min.x, x);
            x = Math.Min(max.x, x);
            y = Math.Max(min.y, y);
            y = Math.Min(max.y, y);
            z = Math.Max(min.z, z);
            z = Math.Min(max.z, z);
        }

        // Converts a Vector3Int to a [[Vector3]].
        
        public static implicit operator Vector3(Vector3Int v) {
            return new Vector3(v.x, v.y, v.z);
        }

        // Converts a Vector3Int to a [[Vector2Int]].
        
        public static explicit operator Vector2Int(Vector3Int v) {
            return new Vector2Int(v.x, v.y);
        }

        
        public static Vector3Int FloorToInt(Vector3 v) {
            return new Vector3Int(
                Moths.FloorToInt(v.x),
                Moths.FloorToInt(v.y),
                Moths.FloorToInt(v.z)
            );
        }

        
        public static Vector3Int CeilToInt(Vector3 v) {
            return new Vector3Int(
                Moths.CeilToInt(v.x),
                Moths.CeilToInt(v.y),
                Moths.CeilToInt(v.z)
            );
        }

        
        public static Vector3Int RoundToInt(Vector3 v) {
            return new Vector3Int(
                Moths.RoundToInt(v.x),
                Moths.RoundToInt(v.y),
                Moths.RoundToInt(v.z)
            );
        }

        
        public static Vector3Int operator +(Vector3Int a, Vector3Int b) {
            return new Vector3Int(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        
        public static Vector3Int operator -(Vector3Int a, Vector3Int b) {
            return new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        
        public static Vector3Int operator *(Vector3Int a, Vector3Int b) {
            return new Vector3Int(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        
        public static Vector3Int operator -(Vector3Int a) {
            return new Vector3Int(-a.x, -a.y, -a.z);
        }

        
        public static Vector3Int operator *(Vector3Int a, int b) {
            return new Vector3Int(a.x * b, a.y * b, a.z * b);
        }

        
        public static Vector3Int operator *(int a, Vector3Int b) {
            return new Vector3Int(a * b.x, a * b.y, a * b.z);
        }

        
        public static Vector3Int operator /(Vector3Int a, int b) {
            return new Vector3Int(a.x / b, a.y / b, a.z / b);
        }

        
        public static bool operator ==(Vector3Int lhs, Vector3Int rhs) {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }

        
        public static bool operator !=(Vector3Int lhs, Vector3Int rhs) {
            return !(lhs == rhs);
        }

        
        public override bool Equals(object other) {
            if (!(other is Vector3Int)) return false;

            return Equals((Vector3Int)other);
        }

        
        public bool Equals(Vector3Int other) {
            return this == other;
        }

        
        public override int GetHashCode() {
            var yHash = y.GetHashCode();
            var zHash = z.GetHashCode();
            return x.GetHashCode() ^ (yHash << 4) ^ (yHash >> 28) ^ (zHash >> 4) ^ (zHash << 28);
        }

        
        public override string ToString() {
            return ToString(null, null);
        }

        
        public string ToString(string format) {
            return ToString(format, null);
        }

        
        public string ToString(string format, IFormatProvider formatProvider) {
            if (formatProvider == null)
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            return string.Format("({0}, {1}, {2})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), z.ToString(format, formatProvider));
        }

        public static Vector3Int zero {  get { return s_Zero; } }
        public static Vector3Int one {  get { return s_One; } }
        public static Vector3Int up {  get { return s_Up; } }
        public static Vector3Int down {  get { return s_Down; } }
        public static Vector3Int left {  get { return s_Left; } }
        public static Vector3Int right {  get { return s_Right; } }
        public static Vector3Int forward {  get { return s_Forward; } }
        public static Vector3Int back {  get { return s_Back; } }

        private static readonly Vector3Int s_Zero = new Vector3Int(0, 0, 0);
        private static readonly Vector3Int s_One = new Vector3Int(1, 1, 1);
        private static readonly Vector3Int s_Up = new Vector3Int(0, 1, 0);
        private static readonly Vector3Int s_Down = new Vector3Int(0, -1, 0);
        private static readonly Vector3Int s_Left = new Vector3Int(-1, 0, 0);
        private static readonly Vector3Int s_Right = new Vector3Int(1, 0, 0);
        private static readonly Vector3Int s_Forward = new Vector3Int(0, 0, 1);
        private static readonly Vector3Int s_Back = new Vector3Int(0, 0, -1);
    }
}