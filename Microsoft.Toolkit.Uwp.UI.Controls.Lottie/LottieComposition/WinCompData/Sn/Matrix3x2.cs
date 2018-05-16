namespace WinCompData.Sn
{
#if !WINDOWS_UWP
    public
#endif
    struct Matrix3x2
    {
        public float M11;
        public float M12;
        public float M21;
        public float M22;
        public float M31;
        public float M32;

        public static Matrix3x2 Identity => new Matrix3x2 { M11 = 1, M22 = 1};
        public Matrix3x2(float m11, float m12, float m21, float m22, float m31, float m32)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            M31 = m31;
            M32 = m32;
        }

        public bool IsIdentity => M11 == 1 && M12 == 0 && M21 == 0 && M22 == 1 && M31 == 0 && M32 == 0;
    }
}
