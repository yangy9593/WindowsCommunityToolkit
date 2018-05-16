using System;

namespace WinCompData.Sn
{
#if !WINDOWS_UWP
    public
#endif
    struct Vector3 : IEquatable<Vector3>
    {
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public override string ToString() => $"{{{X},{Y},{Z}}}";

        public bool Equals(Vector3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object obj) => obj is Vector3 && Equals((Vector3)obj);
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
        public static bool operator !=(Vector3 left, Vector3 right) => !left.Equals(right);


        public static Vector3 One { get; } = new Vector3(1, 1, 1);

        public static Vector3 operator *(Vector3 left, float right) => new Vector3(left.X * right, left.Y * right, left.Z * right);
        public static Vector3 operator -(Vector3 left, float right) => new Vector3(left.X - right, left.Y - right, left.Z - right);
        public static Vector3 operator -(Vector3 left, Vector3 right) => new Vector3(left.X - right.X, left.Y - right.Y, left.Y - right.Y);


    }
}


