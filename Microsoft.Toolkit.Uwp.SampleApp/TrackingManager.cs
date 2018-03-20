// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;

namespace Microsoft.Toolkit.Uwp.SampleApp
{
    public static class TrackingManager
    {
        public static void Init()
        {
            try
            {
                AppCenter.AppCenter.Start(string.Empty, typeof(Analytics));
            }
            catch
            {
            }
        }

        public static void TrackException(Exception ex)
        {
            try
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "message", ex.Message },
                    { "stackTrace", ex.StackTrace },
                    { "innerExceptionMessage", ex.InnerException?.Message },
                    { "innerExceptionStackTrace", ex.InnerException?.StackTrace },
                    { "source", ex.Source }
                };
                Analytics.TrackEvent("exception", properties);
            }
            catch
            {
                // Ignore error
            }
        }

        public static void TrackEvent(string category, string action, string label = "", long value = 0)
        {
            try
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "action", action },
                    { "label", label },
                    { "value", value.ToString() }
                };
                Analytics.TrackEvent(category, properties);
            }
            catch
            {
                // Ignore error
            }
        }

        public static void TrackPage(string pageName)
        {
            try
            {
                Dictionary<string, string> properties = new Dictionary<string, string>
                {
                    { "pageName", pageName }
                };
                Analytics.TrackEvent("pageView", properties);
            }
            catch
            {
                // Ignore error
            }
        }
    }
}
