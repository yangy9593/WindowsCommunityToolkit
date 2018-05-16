using System;

namespace Lottie
{
    /// <summary>
    /// Options for controlling how the <see cref="LottieCompositionSource"/> processes a Lottie file.
    /// </summary>
    [Flags]
    public enum LottieCompositionOptions
    {
        None = 0,

        IncludeDiagnostics = 1,

        All = IncludeDiagnostics,
    }
}
