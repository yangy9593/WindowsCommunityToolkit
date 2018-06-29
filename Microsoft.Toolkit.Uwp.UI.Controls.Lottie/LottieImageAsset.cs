// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Data class describing an image asset exported by bodymovin.
    /// </summary>
    public class LottieImageAsset
    {
        internal LottieImageAsset(int width, int height, string id, string fileName, string dirName)
        {
            Width = width;
            Height = height;
            Id = id;
            FileName = fileName;
            DirName = dirName;
        }

        /// <summary>
        /// Gets the width that the image asset should have
        /// </summary>
        public virtual int Width { get; }

        /// <summary>
        /// Gets the height that the image asset should have
        /// </summary>
        public virtual int Height { get; }

        /// <summary>
        /// Gets the id of the image asset
        /// </summary>
        public virtual string Id { get; }

        /// <summary>
        /// Gets the file name of the image asset
        /// </summary>
        public virtual string FileName { get; }

        /// <summary>
        /// Gets the directory name of the image asset
        /// </summary>
        public virtual string DirName { get; }
    }
}