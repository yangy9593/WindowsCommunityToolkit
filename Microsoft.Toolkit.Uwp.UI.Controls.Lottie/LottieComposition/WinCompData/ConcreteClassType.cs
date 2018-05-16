using System;
using System.Collections.Generic;
using System.Text;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    enum ConcreteClassType
    {
        AnimationController,
        ColorKeyFrameAnimation,
        CompositionColorBrush,
        CompositionContainerShape,
        CompositionEllipseGeometry,
        CompositionPathGeometry,
        CompositionRectangleGeometry,
        CompositionRoundedRectangleGeometry,
        CompositionSpriteShape,
        CompositionViewBox,
        ContainerVisual,
        CubicBezierEasingFunction,
        ExpressionAnimation,
        InsetClip,
        LinearEasingFunction,
        PathKeyFrameAnimation,
        ScalarKeyFrameAnimation,
        ShapeVisual,
        StepEasingFunction,
        Vector2KeyFrameAnimation,
        Vector3KeyFrameAnimation,
    }
}
