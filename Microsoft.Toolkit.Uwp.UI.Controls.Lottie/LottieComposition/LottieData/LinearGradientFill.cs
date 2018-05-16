namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class LinearGradientFill : ShapeLayerContent
    {
        public LinearGradientFill(
            string name,
            string matchName,
            Animatable<double> opacity,
            IAnimatableVector3 startPoint,
            IAnimatableVector3 endPoint)
            : base(name, matchName)
        {
            Opacity = opacity;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public IAnimatableVector3 StartPoint { get; }

        public IAnimatableVector3 EndPoint { get; }

        public Animatable<double> Opacity { get; }

        public override ShapeContentType ContentType => ShapeContentType.LinearGradientFill;

        public override LottieObjectType ObjectType => LottieObjectType.LinearGradientFill;
    }
}
