// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model
{
    /// <summary>
    /// Represents one character of the font used to draw to the canvas, when using text glyphs
    /// </summary>
    public class FontCharacter
    {
        internal static int HashFor(char character, string fontFamily, string style)
        {
            var result = 0;
            result = (31 * result) + character;
            result = (31 * result) + fontFamily.GetHashCode();
            result = (31 * result) + style.GetHashCode();
            return result;
        }

        private readonly char _character;
        private readonly string _fontFamily;

        internal FontCharacter(List<ShapeGroup> shapes, char character, int size, double width, string style, string fontFamily)
        {
            Shapes = shapes;
            _character = character;
            _size = size;
            Width = width;
            _style = style;
            _fontFamily = fontFamily;
        }

        internal List<ShapeGroup> Shapes { get; }

        private int _size;

        /// <summary>
        /// Gets the width of this font character
        /// </summary>
        public double Width { get; }

        private readonly string _style;

        /// <summary>
        /// Gets a hashcode used internally to represent this FontCharacter, for caching.
        /// </summary>
        /// <returns>Returns the hash for this FontCharacter, based on the character, it's family and it's style.</returns>
        public override int GetHashCode()
        {
            return HashFor(_character, _fontFamily, _style);
        }
    }
}
