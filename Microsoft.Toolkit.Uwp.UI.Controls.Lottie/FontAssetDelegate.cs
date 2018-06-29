// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Delegate to handle the loading of fonts that are not packaged in the assets of your app or don't
    /// have the same file name.
    /// </summary>
    /// <seealso cref="LottieDrawable.FontAssetDelegate"></seealso>
    public abstract class FontAssetDelegate
    {
        /// <summary>
        /// Override this if you want to return a Typeface from a font family.
        /// </summary>
        /// <returns>The <see cref="Typeface"/> to be used for the specified fontFamily</returns>
        public abstract Typeface FetchFont(string fontFamily);

        /// <summary>
        /// Override this if you want to specify the asset path for a given font family.
        /// </summary>
        /// <returns>The path of the font to be loaded for the specified fontFamily</returns>
        public abstract string GetFontPath(string fontFamily);
    }
}
