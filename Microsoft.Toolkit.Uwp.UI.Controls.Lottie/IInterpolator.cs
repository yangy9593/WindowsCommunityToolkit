// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Provides a method for interpolating a value
    /// </summary>
    public interface IInterpolator
    {
        /// <summary>
        /// Returns a value based on your interpolation mechanism
        /// </summary>
        /// <param name="f">A float value from 0 to 1 representing the value to be interpolated</param>
        /// <returns>Returns a float based on the interpolation mechanism. Should be between 0 and 1.</returns>
        float GetInterpolation(float f);
    }
}