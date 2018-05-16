using System.Collections.Generic;

namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class ShapeGroup : ShapeLayerContent
    {
        public ShapeGroup(
            string name,
            string matchName,
            IEnumerable<ShapeLayerContent> items) 
            : base(name, matchName)
        {
            Items = items;
        }

        public IEnumerable<ShapeLayerContent> Items { get; }

        public override ShapeContentType ContentType => ShapeContentType.Group;

        public override LottieObjectType ObjectType => LottieObjectType.ShapeGroup;
    }
}
