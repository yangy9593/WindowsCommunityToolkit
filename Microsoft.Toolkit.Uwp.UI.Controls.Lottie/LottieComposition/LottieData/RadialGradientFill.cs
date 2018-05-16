namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class RadialGradientFill : ShapeLayerContent
    {
        public RadialGradientFill(
            string name,
            string matchName,
            Animatable<double> opacityPercent,
            IAnimatableVector3 startPoint,
            IAnimatableVector3 endPoint,
            Animatable<double> highlightLength,
            Animatable<double> highlightAngle)
            : base(name, matchName)
        {
            OpacityPercent = opacityPercent;
            StartPoint = startPoint;
            EndPoint = endPoint;
            HighlightLength = highlightLength;
            HighlightAngle = highlightAngle;
        }


        public IAnimatableVector3 StartPoint { get; }

        public IAnimatableVector3 EndPoint { get; }

        public Animatable<double> HighlightLength { get; }

        public Animatable<double> HighlightAngle { get; }

        public Animatable<double> OpacityPercent { get; }

        public override ShapeContentType ContentType => ShapeContentType.RadialGradientFill;

        public override LottieObjectType ObjectType => LottieObjectType.RadialGradientFill;
    }
}
