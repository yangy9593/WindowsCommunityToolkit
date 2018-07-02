// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Layer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;

namespace UnitTests.Lottie
{
    [TestClass]
    public class LottieDrawableTest
    {
        [TestCategory("Lottie")]
        [TestMethod]
        public async Task TestMinFrame()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var composition = CreateComposition(31, 391);
                var drawable = new LottieDrawable();

                drawable.SetComposition(composition);
                drawable.MinProgress = 0.42f;
                Assert.AreEqual(182.2f, drawable.MinFrame);

                drawable.ClearComposition();
            });
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public async Task TestMinWithStartFrameFrame()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var composition = CreateComposition(100, 200);
                var drawable = new LottieDrawable();
                drawable.SetComposition(composition);
                drawable.MinProgress = 0.5f;
                Assert.AreEqual(150f, drawable.MinFrame);
            });
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public async Task TestMaxFrame()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var composition = CreateComposition(31, 391);
                var drawable = new LottieDrawable();
                drawable.SetComposition(composition);
                drawable.MaxProgress = 0.25f;
                Assert.AreEqual(121f, drawable.MaxFrame);
            });
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public async Task TestMinMaxFrame()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var composition = CreateComposition(31, 391);
                var drawable = new LottieDrawable();
                drawable.SetComposition(composition);
                drawable.SetMinAndMaxProgress(0.25f, 0.42f);
                Assert.AreEqual(121f, drawable.MinFrame);
                Assert.AreEqual(182f, drawable.MaxFrame);
            });
        }

        private LottieComposition CreateComposition(int startFrame, int endFrame)
        {
            LottieComposition composition = new LottieComposition();
            composition.Init(default(Rect), startFrame, endFrame, 1000, new List<Layer>(), new Dictionary<long, Layer>(0), new Dictionary<string, List<Layer>>(0), new Dictionary<string, LottieImageAsset>(0), new Dictionary<int, FontCharacter>(0), new Dictionary<string, Font>(0));
            return composition;
        }
    }
}
