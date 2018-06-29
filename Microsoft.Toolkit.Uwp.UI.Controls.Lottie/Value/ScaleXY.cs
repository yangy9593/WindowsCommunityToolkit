// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    /// <summary>
    /// A class that provides the ability to change an animation layer on the X and Y axis, using diferent values for X and Y.
    /// </summary>
    public class ScaleXy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleXy"/> class.
        /// </summary>
        /// <param name="sx">The scale on the X axis</param>
        /// <param name="sy">The scale on the Y axis</param>
        public ScaleXy(float sx, float sy)
        {
            ScaleX = sx;
            ScaleY = sy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleXy"/> class, using 100% of scale (1f).
        /// </summary>
        public ScaleXy()
            : this(1f, 1f)
        {
        }

        internal virtual float ScaleX { get; }

        internal virtual float ScaleY { get; }

        /// <summary>
        /// For debuging purposes.
        /// </summary>
        /// <returns>A formated text to help understand the current KeyPath.</returns>
        public override string ToString()
        {
            return ScaleX + "x" + ScaleY;
        }
    }
}