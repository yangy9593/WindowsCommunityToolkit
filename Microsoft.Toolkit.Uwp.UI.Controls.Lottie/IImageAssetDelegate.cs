// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graphics.Canvas;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Delegate to handle the loading of bitmaps that are not packaged in the assets of your app.
    /// </summary>
    public interface IImageAssetDelegate
    {
        /// <summary>
        /// Returns a <see cref="CanvasBitmap"/> based on the information of a <see cref="LottieImageAsset"/>
        /// </summary>
        /// <param name="asset">The <see cref="LottieImageAsset"/> with all the information about the image asset.</param>
        /// <returns>Returns a Win2D <see cref="CanvasBitmap"/> that corresponds to the provided <see cref="LottieImageAsset"/>.</returns>
        CanvasBitmap FetchBitmap(LottieImageAsset asset);
    }
}