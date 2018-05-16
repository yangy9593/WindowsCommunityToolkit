namespace Lottie
{
    /// <summary>
    /// Defines a segment of a composition that can be played by the <see cref="CompositionPlayer"/>.
    /// </summary>
    public sealed class CompositionSegment
    {
        public double FromProgress { get; }
        public double ToProgress { get; }
        public bool IsLoopingEnabled { get; }
        public bool ReverseAnimation { get; }

        public string Name { get; }
        public CompositionSegment(string name, double fromProgress, double toProgress, bool isLoopingEnabled, bool reverseAnimation)
        {
            Name = name;
            FromProgress = fromProgress;
            ToProgress = toProgress;
            IsLoopingEnabled = isLoopingEnabled;
            ReverseAnimation = reverseAnimation;
        }

        /// <summary>
        /// Defines a segment that plays from <paramref name="fromProgress"/> to <paramref name="toProgress"/>
        /// without looping or repeating.
        /// </summary>
        public CompositionSegment(string name, double fromProgress, double toProgress)
            : this(name, fromProgress, toProgress, isLoopingEnabled: false, reverseAnimation: false)
        {
        }
    }
}
