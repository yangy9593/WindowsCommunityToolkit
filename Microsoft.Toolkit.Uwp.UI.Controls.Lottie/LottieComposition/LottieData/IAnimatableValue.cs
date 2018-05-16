namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    interface IAnimatableValue<T>
    {
        /// <summary>
        /// The initial value.
        /// </summary>
        T InitialValue { get; }

        /// <summary>
        /// True if the value is animated.
        /// </summary>
        bool IsAnimated { get; }
    }
}
