using System;

namespace WinCompData.Wui
{
#if !WINDOWS_UWP
    public
#endif
    struct Color : IEquatable<Color>
    {
        Color(byte a, byte r, byte g, byte b) { A = a; R = r; G = g; B = b; }

        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Color(a, r, g, b);
        }

        public byte A { get; }
        public byte B { get; }
        public byte G { get; }
        public byte R { get; }

        public override string ToString() => $"#{ToHex(A)}{ToHex(R)}{ToHex(G)}{ToHex(B)}";

        static string ToHex(byte value) => value.ToString("X2");

        public bool Equals(Color other) => A == other.A && R == other.R && G == other.G && B == other.B;
        public override bool Equals(object obj) => obj is Color && Equals((Color)obj);
        public override int GetHashCode() => A * R * G * B;

        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color  right) => !left.Equals(right);
    }
}
