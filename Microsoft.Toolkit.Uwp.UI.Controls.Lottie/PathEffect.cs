// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Content;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    internal abstract class PathEffect
    {
        public abstract void Apply(CanvasStrokeStyle canvasStrokeStyle, Paint paint);
    }
}