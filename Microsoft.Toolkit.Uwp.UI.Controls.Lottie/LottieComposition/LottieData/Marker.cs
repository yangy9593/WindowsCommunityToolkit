namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class Marker : LottieObject
    {
        public Marker(
            double progress, 
            string name,
            double durationSeconds) : base(name)
        {
            Progress = progress;
            DurationSeconds = durationSeconds;
        }

        /// <summary>
        /// The time value of the marker. This value must be multipled by the composition
        /// duration to get the actualy time.
        /// </summary>
        public double Progress { get; }

        public double DurationSeconds { get; }

        public override LottieObjectType ObjectType => LottieObjectType.Marker;
    }
}
