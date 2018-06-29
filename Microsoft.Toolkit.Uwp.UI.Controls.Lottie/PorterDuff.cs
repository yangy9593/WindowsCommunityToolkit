// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graphics.Canvas;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Static PorterDuff class that holds the <see cref="Mode"/> enum.
    /// </summary>
    public static class PorterDuff
    {
        /// <summary>
        /// PorterDuff Mode enum
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Not supported
            /// </summary>
            Clear,

            /// <summary>
            /// Not supported
            /// </summary>
            DstIn,

            /// <summary>
            /// Not supported
            /// </summary>
            DstOut,

            /// <summary>
            /// Only method supported right now.
            /// </summary>
            SrcAtop
        }

        internal static CanvasComposite ToCanvasComposite(Mode mode)
        {
            switch (mode)
            {
                case Mode.SrcAtop:
                    return CanvasComposite.SourceAtop;
                case Mode.DstIn:
                    return CanvasComposite.DestinationIn;
                case Mode.DstOut:
                    return CanvasComposite.DestinationOut;

                // case Mode.Clear:
                default:
                    return CanvasComposite.Copy;
            }
        }
    }
}