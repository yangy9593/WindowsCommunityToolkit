// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Layer;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Utils;
using Moq;
using Windows.Foundation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Lottie
{
    [TestClass]
    public class LottieValueAnimatorUnitTest
    {
        private Mock<TestLottieValueAnimator> _mockAnimator;
        private LottieComposition _composition;
        private TestLottieValueAnimator _animator;
        private volatile bool _isDone;

        [TestInitialize]
        public void Init()
        {
            _composition = CreateComposition(0, 1000);
            _mockAnimator = CreateAnimator();
            _animator = _mockAnimator.Object;
            _animator.Composition = _composition;

            _isDone = false;
        }

        private static Mock<TestLottieValueAnimator> CreateAnimator()
        {
            return new Mock<TestLottieValueAnimator>
            {
                CallBase = true,
            };
        }

        private LottieComposition CreateComposition(int startFrame, int endFrame)
        {
            var composition = new LottieComposition();
            composition.Init(default(Rect), startFrame, endFrame, 1000, new List<Layer>(), new Dictionary<long, Layer>(0), new Dictionary<string, List<Layer>>(0), new Dictionary<string, LottieImageAsset>(0), new Dictionary<int, FontCharacter>(0), new Dictionary<string, Font>(0));
            return composition;
        }

        internal class TestLottieValueAnimator : LottieValueAnimator
        {
            public int OnValueChangedCount { get; set; } = 0;

            public void OnValueChanged2()
            {
                OnValueChangedCount++;
            }

            protected internal override void RemoveFrameCallback()
            {
                InternalRunning = false;
            }

            protected override void OnValueChanged()
            {
                base.OnValueChanged();
                OnValueChanged2();
            }

            protected override void PostFrameCallback()
            {
                InternalRunning = true;
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            _animator.Cancel();
            _animator = null;
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestInitialState()
        {
            AssertClose(0f, _animator.Frame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestResumingMaintainsValue()
        {
            _animator.Frame = 500;
            _animator.ResumeAnimation();
            AssertClose(500f, _animator.Frame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestFrameConvertsToAnimatedFraction()
        {
            _animator.Frame = 500;
            _animator.ResumeAnimation();
            AssertClose(0.5f, _animator.AnimatedFraction);
            AssertClose(0.5f, _animator.AnimatedValueAbsolute);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestPlayingResetsValue()
        {
            _animator.Frame = 500;
            _animator.PlayAnimation();
            AssertClose(0f, _animator.Frame);
            AssertClose(0f, _animator.AnimatedFraction);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestReversingMaintainsValue()
        {
            _animator.Frame = 250;
            _animator.ReverseAnimationSpeed();
            AssertClose(250, _animator.Frame);
            AssertClose(0.75f, _animator.AnimatedFraction);
            AssertClose(0.25f, _animator.AnimatedValueAbsolute);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestReversingWithMinValueMaintainsValue()
        {
            _animator.MinFrame = 100;
            _animator.Frame = 1000;
            _animator.ReverseAnimationSpeed();
            AssertClose(1000f, _animator.Frame);
            AssertClose(0f, _animator.AnimatedFraction);
            AssertClose(1f, _animator.AnimatedValueAbsolute);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestReversingWithMaxValueMaintainsValue()
        {
            _animator.MaxFrame = 900;
            _animator.ReverseAnimationSpeed();
            AssertClose(0f, _animator.Frame);
            AssertClose(1f, _animator.AnimatedFraction);
            AssertClose(0f, _animator.AnimatedValueAbsolute);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestResumeReversingWithMinValueMaintainsValue()
        {
            _animator.MaxFrame = 900;
            _animator.ReverseAnimationSpeed();
            _animator.ResumeAnimation();
            AssertClose(900f, _animator.Frame);
            AssertClose(0f, _animator.AnimatedFraction);
            AssertClose(0.9f, _animator.AnimatedValueAbsolute);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestPlayReversingWithMinValueMaintainsValue()
        {
            _animator.MaxFrame = 900;
            _animator.ReverseAnimationSpeed();
            _animator.PlayAnimation();
            AssertClose(900f, _animator.Frame);
            AssertClose(0f, _animator.AnimatedFraction);
            AssertClose(0.9f, _animator.AnimatedValueAbsolute);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestMinAndMaxBothSet()
        {
            _animator.MinFrame = 200;
            _animator.MaxFrame = 800;
            _animator.Frame = 400;
            AssertClose(0.33333f, _animator.AnimatedFraction);
            AssertClose(0.4f, _animator.AnimatedValueAbsolute);
            _animator.ReverseAnimationSpeed();
            AssertClose(400f, _animator.Frame);
            AssertClose(0.66666f, _animator.AnimatedFraction);
            AssertClose(0.4f, _animator.AnimatedValueAbsolute);
            _animator.ResumeAnimation();
            AssertClose(400f, _animator.Frame);
            AssertClose(0.66666f, _animator.AnimatedFraction);
            AssertClose(0.4f, _animator.AnimatedValueAbsolute);
            _animator.PlayAnimation();
            AssertClose(800f, _animator.Frame);
            AssertClose(0f, _animator.AnimatedFraction);
            AssertClose(0.8f, _animator.AnimatedValueAbsolute);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestDefaultAnimator()
        {
            int state = 0;

            _mockAnimator.Setup(l => l.OnAnimationStart(false)).Callback(() =>
            {
                if (state == 0)
                {
                    state = 1;
                }
            }).Verifiable();
            _mockAnimator.Setup(l => l.OnAnimationEnd(false)).Callback(() =>
            {
                if (state == 1)
                {
                    state = 2;
                }

                _mockAnimator.Verify();
                Assert.AreEqual(2, state);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Never);

                _isDone = true;
            }).Verifiable();

            TestAnimator(null);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestReverseAnimator()
        {
            _animator.ReverseAnimationSpeed();

            int state = 0;

            _mockAnimator.Setup(l => l.OnAnimationStart(true)).Callback(() =>
            {
                if (state == 0)
                {
                    state = 1;
                }
            }).Verifiable();
            _mockAnimator.Setup(l => l.OnAnimationEnd(true)).Callback(() =>
            {
                if (state == 1)
                {
                    state = 2;
                }

                _mockAnimator.Verify();
                Assert.AreEqual(2, state);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Never);

                _isDone = true;
            }).Verifiable();

            TestAnimator(null);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestLoopingAnimatorOnce()
        {
            _animator.RepeatCount = 1;
            TestAnimator(() =>
            {
                _mockAnimator.Verify(l => l.OnAnimationStart(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationEnd(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
            });
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestLoopingAnimatorZeroTimes()
        {
            _animator.RepeatCount = 0;
            TestAnimator(() =>
            {
                _mockAnimator.Verify(l => l.OnAnimationStart(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Never);
                _mockAnimator.Verify(l => l.OnAnimationEnd(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
            });
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestLoopingAnimatorTwice()
        {
            _animator.RepeatCount = 2;
            TestAnimator(() =>
            {
                _mockAnimator.Verify(l => l.OnAnimationStart(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Exactly(2));
                _mockAnimator.Verify(l => l.OnAnimationEnd(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
            });
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void TestLoopingAnimatorOnceReverse()
        {
            _animator.Frame = 1000;
            _animator.RepeatCount = 1;
            _animator.ReverseAnimationSpeed();
            TestAnimator(() =>
            {
                _mockAnimator.Verify(l => l.OnAnimationStart(true), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationEnd(true), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
            });
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void SetMinFrameSmallerThanComposition()
        {
            _animator.MinFrame = -9000;
            AssertClose(_animator.MinFrame, _composition.StartFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void SetMaxFrameLargerThanComposition()
        {
            _animator.MaxFrame = 9000;
            AssertClose(_animator.MaxFrame, _composition.EndFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void SetMinFrameBeforeComposition()
        {
            LottieValueAnimator animator = CreateAnimator().Object;
            animator.MinFrame = 100;
            animator.Composition = _composition;
            AssertClose(100.0f, animator.MinFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void SetMaxFrameBeforeComposition()
        {
            LottieValueAnimator animator = CreateAnimator().Object;
            animator.MaxFrame = 100;
            animator.Composition = _composition;
            AssertClose(100.0f, animator.MaxFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void SetMinAndMaxFrameBeforeComposition()
        {
            LottieValueAnimator animator = CreateAnimator().Object;
            animator.SetMinAndMaxFrames(100, 900);
            animator.Composition = _composition;
            AssertClose(100.0f, animator.MinFrame);
            AssertClose(900.0f, animator.MaxFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void SetMinFrameAfterComposition()
        {
            LottieValueAnimator animator = CreateAnimator().Object;
            animator.Composition = _composition;
            animator.MinFrame = 100;
            AssertClose(100.0f, animator.MinFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void SetMaxFrameAfterComposition()
        {
            LottieValueAnimator animator = CreateAnimator().Object;
            animator.Composition = _composition;
            animator.MaxFrame = 100;
            AssertClose(100.0f, animator.MaxFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void SetMinAndMaxFrameAfterComposition()
        {
            LottieValueAnimator animator = CreateAnimator().Object;
            animator.Composition = _composition;
            animator.SetMinAndMaxFrames(100, 900);
            AssertClose(100.0f, animator.MinFrame);
            AssertClose(900.0f, animator.MaxFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void MaxFrameOfNewShorterComposition()
        {
            LottieValueAnimator animator = CreateAnimator().Object;
            animator.Composition = _composition;
            LottieComposition composition2 = CreateComposition(0, 500);
            animator.Composition = composition2;
            AssertClose(500.0f, animator.MaxFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void MaxFrameOfNewLongerComposition()
        {
            LottieValueAnimator animator = CreateAnimator().Object;
            animator.Composition = _composition;
            LottieComposition composition2 = CreateComposition(0, 1500);
            animator.Composition = composition2;
            AssertClose(1500.0f, animator.MaxFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void ClearComposition()
        {
            _animator.ClearComposition();
            AssertClose(0.0f, _animator.MaxFrame);
            AssertClose(0.0f, _animator.MinFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void ResetComposition()
        {
            _animator.ClearComposition();
            _animator.Composition = _composition;
            AssertClose(0.0f, _animator.MinFrame);
            AssertClose(1000.0f, _animator.MaxFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void ResetAndSetMinBeforeComposition()
        {
            _animator.ClearComposition();
            _animator.MinFrame = 100;
            _animator.Composition = _composition;
            AssertClose(100.0f, _animator.MinFrame);
            AssertClose(1000.0f, _animator.MaxFrame);
        }

        [TestCategory("Lottie")]
        [TestMethod]
        public void ResetAndSetMinAterComposition()
        {
            _animator.ClearComposition();
            _animator.Composition = _composition;
            _animator.MinFrame = 100;
            AssertClose(100.0f, _animator.MinFrame);
            AssertClose(1000.0f, _animator.MaxFrame);
        }

        /// <summary>
        /// Animations don't render on the out frame so if an animation is 1000 frames, the actual end will be 999.99. This causes
        /// actual fractions to be something like .74999 when you might expect 75.
        /// </summary>
        /// <param name="expected">The first value to compare.</param>
        /// <param name="actual">The second value to compare.</param>
        private static void AssertClose(float expected, float actual)
        {
            Assert.IsTrue(Math.Abs(expected - actual) <= expected * 0.01f);
        }

        private void TestAnimator(Action verifyListener)
        {
            _animator.AnimationEnd += (s, e) =>
            {
                verifyListener?.Invoke();
                _isDone = true;
            };

            _animator.PlayAnimation();
            while (!_isDone)
            {
                _animator.DoFrame();
            }
        }
    }
}
