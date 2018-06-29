// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Utils
{
    /// <summary>
    /// Class to calculate the average in a stream of numbers on a continuous basis.
    /// </summary>
    public class MeanCalculator
    {
        private float _sum;
        private int _n;

        /// <summary>
        /// Adds a value to the mean
        /// </summary>
        /// <param name="number">The number to be added to the mean</param>
        public virtual void Add(float number)
        {
            _sum += number;
            _n++;
            if (_n == int.MaxValue)
            {
                _sum /= 2f;
                _n /= 2;
            }
        }

        /// <summary>
        /// Gets the current mean of all values added using the <see cref="Add(float)"/> method.
        /// </summary>
        public virtual float Mean
        {
            get
            {
                if (_n == 0)
                {
                    return 0;
                }

                return _sum / _n;
            }
        }
    }
}
