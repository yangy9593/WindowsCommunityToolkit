// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Content
{
    /// <summary>
    /// Content that may want to absorb and take ownership of the content around it.
    /// For example, merge paths will absorb the shapes above it and repeaters will absorb the content
    /// above it.
    /// </summary>
    internal interface IGreedyContent
    {
        /// <summary>
        /// An iterator of contents that can be used to take ownership of contents. If ownership is taken,
        /// the content should be removed from the iterator.
        ///
        /// The contents should be iterated by calling hasPrevious() and previous() so that the list of
        /// contents is traversed from bottom to top which is the correct order for handling AE logic.
        /// </summary>
        void AbsorbContent(List<IContent> contents);
    }
}
