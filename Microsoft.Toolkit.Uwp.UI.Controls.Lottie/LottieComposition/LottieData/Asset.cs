namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    abstract class Asset
    {
        internal Asset(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public abstract AssetType Type { get; }

        public enum AssetType
        {
            LayerCollection,
            Image,
        }
    }
}
