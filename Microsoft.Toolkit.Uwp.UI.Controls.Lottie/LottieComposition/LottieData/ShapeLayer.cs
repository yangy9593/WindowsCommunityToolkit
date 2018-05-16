using System.Collections.Generic;
using System.Linq;

namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class ShapeLayer : Layer
    {
        readonly IEnumerable<ShapeLayerContent> _shapes;

        public ShapeLayer(
            string name,
            IEnumerable<ShapeLayerContent> shapes,
            int index,
            int? parent,
            bool isHidden,
            Transform transform,
            double timeStretch,
            double startFrame,
            double inFrame,
            double outFrame,
            BlendMode blendMode,
            bool is3d,
            bool autoOrient)
         : base(
             name,
             index,
             parent,
             isHidden,
             transform,
             timeStretch,
             startFrame,
             inFrame,
             outFrame,
             blendMode,
             is3d,
             autoOrient)
        {
            _shapes = shapes;
        }


        public IEnumerable<ShapeLayerContent> Contents => _shapes.AsEnumerable();

        public override LayerType Type => LayerType.Shape;

        public override LottieObjectType ObjectType => LottieObjectType.ShapeLayer;

    }
}
