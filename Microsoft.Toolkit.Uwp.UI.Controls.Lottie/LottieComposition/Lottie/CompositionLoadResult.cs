
namespace Lottie
{
    internal sealed class CompositionLoadResult
    {
        internal bool LoadSucceeded { get; set; }

        internal VisualPlayer VisualPlayer { get; set; }

        /// <summary>
        /// Optional diagnostics information.
        /// </summary>
        internal object Diagnostics { get; set; }
    }
}
