using System;

namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    struct Vector3 : IEquatable<Vector3>
    {
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public static Vector3 operator *(Vector3 left, double right) =>
            new Vector3(left.X * right, left.Y * right, left.Z * right);

        public static Vector3 operator +(Vector3 left, Vector3 right) =>
            new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
        public static bool operator !=(Vector3 left, Vector3 right) => !left.Equals(right);

        public override bool Equals(object obj) => obj is Vector3 && Equals((Vector3)obj);
        public bool Equals(Vector3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override int GetHashCode() => (X * Y * Z).GetHashCode();
        public override string ToString() => $"{{{X},{Y},{Z}}}";
    }
}

