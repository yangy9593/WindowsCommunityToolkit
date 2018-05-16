namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CompositionPath
    {
        public CompositionPath(Wg.IGeometrySource2D source)
        {
            Source = source;
        }

        public Wg.IGeometrySource2D Source { get; }
    }
}
