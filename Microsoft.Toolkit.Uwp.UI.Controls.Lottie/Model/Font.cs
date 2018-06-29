// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model
{
    /// <summary>
    /// Represents a font that can be used to draw strings on the canvas
    /// </summary>
    public class Font
    {
        internal Font(string family, string name, string style, float ascent)
        {
            Family = family;
            Name = name;
            Style = style;
            _ascent = ascent;
        }

        /// <summary>
        /// Gets the family of the font
        /// </summary>
        public string Family { get; }

        /// <summary>
        /// Gets the name of the font
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the style of the font
        /// </summary>
        public string Style { get; }

        internal float Ascent => _ascent;

        private readonly float _ascent;
    }
}
