// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// A color filter with a predefined transfer mode that applies the specified color on top of the
    /// original color. As there are many other transfer modes, please take a look at the definition
    /// of PorterDuff.Mode.SRC_ATOP to find one that suits your needs.
    /// This site has a great explanation of Porter/Duff compositing algebra as well as a visual
    /// representation of many of the transfer modes:
    /// http://ssp.impulsetrain.com/porterduff.html
    /// </summary>
    public class SimpleColorFilter : PorterDuffColorFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleColorFilter"/> class.
        /// This <see cref="ColorFilter"/> always uses the <see cref="PorterDuff.Mode.SrcAtop"/>.
        /// </summary>
        /// <param name="color">The color that this <see cref="ColorFilter"/> will use to blend the colors of it's target.</param>
        public SimpleColorFilter(Color color)
            : base(color, PorterDuff.Mode.SrcAtop)
        {
        }
    }
}