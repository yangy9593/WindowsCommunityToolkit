
namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    abstract class LottieObject
    {
        internal LottieObject(string name) { Name = name; }

        public string Name { get; }

        public abstract LottieObjectType ObjectType { get; }
    }
}
