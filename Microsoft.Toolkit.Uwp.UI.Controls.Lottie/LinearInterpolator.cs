// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    internal class LinearInterpolator : IInterpolator
    {
        public float GetInterpolation(float f)
        {
            if (f < 0 || float.IsNaN(f))
            {
                f = 0;
            }

            if (f > 1)
            {
                f = 1;
            }

            return f;
        }
    }
}