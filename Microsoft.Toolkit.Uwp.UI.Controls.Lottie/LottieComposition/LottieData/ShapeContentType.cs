namespace LottieData
{
    /// <summary>
    /// Types of <see cref="ShapeLayerContent"/> objects.
    /// </summary>
#if !WINDOWS_UWP
    public
#endif
    enum ShapeContentType
    {
        Ellipse,
        Group,
        LinearGradientFill,
        LinearGradientStroke,
        MergePaths,
        Path,
        Polystar,
        RadialGradientFill,
        RadialGradientStroke,
        Rectangle,
        Repeater,
        RoundedCorner,
        SolidColorFill,
        SolidColorStroke,
        Transform,
        TrimPath,
    }
}
