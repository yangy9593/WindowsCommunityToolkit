namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class Shape : ShapeLayerContent
    {
        public Shape(
            string name,
            string matchName,
            bool direction,
            Animatable<PathGeometry> geometry)
            : base(name, matchName)
        {
            Direction = direction;
            PathData = geometry;
        }

        public bool Direction { get; }

        public Animatable<PathGeometry> PathData { get; }

        public override ShapeContentType ContentType => ShapeContentType.Path;

        public override LottieObjectType ObjectType => LottieObjectType.Shape;
    }
}

