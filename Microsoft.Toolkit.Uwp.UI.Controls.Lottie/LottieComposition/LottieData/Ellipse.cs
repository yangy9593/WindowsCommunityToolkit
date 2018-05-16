namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class Ellipse : ShapeLayerContent
    {
        public Ellipse(
            string name,
            string matchName,
            bool direction,
            IAnimatableVector3 position,
            IAnimatableVector3 diameter)
            : base(name, matchName)
        {
            Direction = direction;
            Position = position;
            Diameter = diameter;
        }

        public bool Direction { get; }
        public IAnimatableVector3 Position { get; }

        public IAnimatableVector3 Diameter { get; }

        public override ShapeContentType ContentType => ShapeContentType.Ellipse;

        public override LottieObjectType ObjectType => LottieObjectType.Ellipse;
    }
}