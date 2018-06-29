// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Apply a color filter to the current brush
    /// </summary>
    public abstract class ColorFilter
    {
        /// <summary>
        /// Apply the color filter
        /// </summary>
        /// <param name="device">The device that can be used to create resources for this color filter</param>
        /// <param name="brush">The original brush</param>
        /// <returns>The new brush to be used</returns>
        internal abstract ICanvasBrush Apply(CanvasDevice device, ICanvasBrush brush);
    }
}