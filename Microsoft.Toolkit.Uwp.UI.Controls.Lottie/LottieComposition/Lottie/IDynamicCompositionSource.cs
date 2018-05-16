namespace Lottie
{
    public delegate void DynamicCompositionSourceEventHandler(object sender);

    /// <summary>
    /// An <see cref="ICompositionSource"/> that has the ability to change the
    /// composition that it provides.
    /// </summary>
    public interface IDynamicCompositionSource : ICompositionSource
    {

        /// <summary>
        /// Event that fires to indicate that the receiver should call <see cref="IPlayableComposition.TryCreateInstance"/>
        /// to replace any existing instance acquired by the receiver.
        /// </summary>
        event DynamicCompositionSourceEventHandler CompositionInvalidated;
    }
}
