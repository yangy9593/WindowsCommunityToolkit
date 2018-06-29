// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    internal class SimpleImplLottieValueCallback<T> : LottieValueCallback<T>
    {
        private readonly SimpleLottieValueCallback<T> _callback;

        public SimpleImplLottieValueCallback(SimpleLottieValueCallback<T> callback)
        {
            _callback = callback;
        }

        public override T GetValue(LottieFrameInfo<T> frameInfo)
        {
            if (_callback != null)
            {
                return _callback.Invoke(frameInfo);
            }

            return default(T);
        }
    }
}