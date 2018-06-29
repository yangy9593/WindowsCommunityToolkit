// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Windows.UI;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// A <see cref="ColorFilter"/> that uses the PorterDuff algorithm
    /// </summary>
    public abstract class PorterDuffColorFilter : ColorFilter
    {
        /// <summary>
        /// Gets or sets the color that this <see cref="ColorFilter"/> will use to blend the colors of it's target.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets the PorterDuff <see cref="PorterDuff.Mode"/> of this <see cref="ColorFilter"/>.
        /// </summary>
        public PorterDuff.Mode Mode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PorterDuffColorFilter"/> class.
        /// </summary>
        /// <param name="color">The color that this <see cref="ColorFilter"/> will use to blend the colors of it's target.</param>
        /// <param name="mode">The Porter Duff <see cref="PorterDuff.Mode"/> of this <see cref="ColorFilter"/>.</param>
        protected internal PorterDuffColorFilter(Color color, PorterDuff.Mode mode)
        {
            Color = color;
            Mode = mode;
        }

        internal override ICanvasBrush Apply(CanvasDevice device, ICanvasBrush brush)
        {
            var originalColor = Colors.Transparent;
            if (brush is CanvasSolidColorBrush compositionColorBrush)
            {
                originalColor = compositionColorBrush.Color;
                compositionColorBrush.Dispose();
            }

            if (Color == Colors.Transparent)
            {
                return new CanvasSolidColorBrush(device, originalColor);
            }

            return new CanvasSolidColorBrush(device, Blend(originalColor, Color));
        }

        private static Color Blend(Color d, Color s)
        {
            byte a = (byte)(((d.A * s.A) + ((255 - s.A) * d.A)) / 255);
            byte r = (byte)(((d.A * s.R) + ((255 - s.A) * d.R)) / 255);
            byte g = (byte)(((d.A * s.G) + ((255 - s.A) * d.G)) / 255);
            byte b = (byte)(((d.A * s.B) + ((255 - s.A) * d.B)) / 255);
            return Color.FromArgb(a, r, g, b);
        }
    }
}