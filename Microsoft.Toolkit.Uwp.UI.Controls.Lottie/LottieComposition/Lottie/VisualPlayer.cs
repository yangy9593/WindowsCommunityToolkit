using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace Lottie
{
    /// <summary>
    /// A Visual tree configured with an animation that can be played, looped, paused, and stopped.
    /// </summary>
    sealed class VisualPlayer : IDisposable
    {
        readonly Compositor _compositor;
        readonly CompositionPropertySet _progressPropertySet;
        readonly string _progressPropertyName;
        PlayAsyncState _currentPlay;
        bool _isDisposed;

        public VisualPlayer(
            Visual rootVisual,
            Vector2 size,
            CompositionPropertySet progressPropertySet,
            string progressPropertyName,
            TimeSpan animationDuration)
        {
            Root = rootVisual;
            _compositor = rootVisual.Compositor;
            Size = size;
            AnimationDuration = animationDuration;
            _progressPropertySet = progressPropertySet;
            _progressPropertyName = progressPropertyName;
        }

        /// <summary>
        /// The duration of the animation.
        /// </summary>
        public TimeSpan AnimationDuration { get; }

        /// <summary>
        /// True if any <see cref="Task"/>s returned by <see cref="PlayAsync(double, double, bool, bool)"/> 
        /// have not yet completed, i.e. if the animation is currently playing or paused.
        /// </summary>
        public bool IsPlaying => _currentPlay != null;

        /// <summary>
        /// The root of the Visual tree.
        /// </summary>
        public Visual Root { get; }

        /// <summary>
        /// The size of the bounding box of the <see cref="VisualPlayer"/>.
        /// </summary>
        public Vector2 Size { get; }

        /// <summary>
        /// Convenience method for setting the progress of the animation. This 
        /// simply inserts a scalar value into the <see cref="Root"/>'s
        /// property set with the name of <see cref="PositionPropertyName"/>.
        /// </summary>
        /// <remarks>
        /// The value must be 0 to 1. The animation must not be currently
        /// playing.
        /// </remarks>
        public void SetProgress(double progress)
        {
            AssertNotDisposed();

            if (progress < 0 || progress > 1)
            {
                throw new ArgumentException($"{nameof(progress)} must be >= 0 and <= 1.");
            }

            _progressPropertySet.InsertScalar(_progressPropertyName, (float)progress);
        }

        /// <summary>
        /// Pauses the currently playing animation.
        /// </summary>
        public void Pause()
        {
            AssertNotDisposed();

            if (_currentPlay != null && !_currentPlay.IsPaused)
            {
                _currentPlay.IsPaused = true;
                _currentPlay.Controller.Pause();
            }
        }

        /// <summary>
        /// Starts playing the animation. Completes when the animation completes
        /// or when <see cref="Stop"/> is called.
        /// </summary>
        public Task PlayAsync(double fromProgress, double toProgress, bool loop, bool reversed)
        {
            AssertNotDisposed();

            // Cause any other tasks waiting in this method to return. The most
            // recent request to play always wins.
            Stop();

            Debug.Assert(_currentPlay == null);

            // Start the animation.
            return StartAnimating(
                fromProgress: ClampFloat0to1(fromProgress),
                toProgress: ClampFloat0to1(toProgress),
                looped: loop,
                reversed: reversed);
        }

        /// <summary>
        /// Resumes the currently paused animation.
        /// </summary>
        public void Resume()
        {
            AssertNotDisposed();

            if (_currentPlay != null && _currentPlay.IsPaused)
            {
                _currentPlay.IsPaused = false;
                _currentPlay.Controller.Resume();
            }
        }

        /// <summary>
        /// Stops the currently playing or paused animation. This will complete the task
        /// that started the playing.
        /// </summary>
        public void Stop()
        {
            AssertNotDisposed();

            var playBeingStopped = _currentPlay;

            // Stop the current play.
            if (playBeingStopped != null)
            {
                _currentPlay = null;

                // Stop the animation.
                _progressPropertySet.StopAnimation(_progressPropertyName);

                // Use TrySet because it may have been completed by the Batch.Completed callback already.
                // Batch.Completed can not be relied on for looping animations as it fires immediately.
                playBeingStopped.PlayCompletedSource.TrySetResult(true);
            }
        }

        // Starts animating.
        Task StartAnimating(float fromProgress, float toProgress, bool looped, bool reversed)
        {
            Debug.Assert(_currentPlay == null);

            if (fromProgress == toProgress)
            {
                // Nothing to play.
                return Task.CompletedTask;
            }

            var duration = AnimationDuration * (fromProgress < toProgress ? (toProgress - fromProgress) : ((1 - fromProgress) + toProgress));

            if (duration.TotalMilliseconds < 20)
            {
                // Nothing to play.
                SetProgress(fromProgress);
                return Task.CompletedTask;
            }
            var playAnimation = _compositor.CreateScalarKeyFrameAnimation();
            playAnimation.Duration = duration;
            var linearEasing = _compositor.CreateLinearEasingFunction();


            if (reversed)
            {
                // Play backwards from toProgress to fromProgress
                playAnimation.InsertKeyFrame(0, toProgress);
                if (fromProgress > toProgress)
                {
                    // Play to the beginning.
                    var timeToBeginning = toProgress / (toProgress + (1 - fromProgress));
                    playAnimation.InsertKeyFrame(timeToBeginning, 0, linearEasing);
                    // Jump to the end.
                    playAnimation.InsertKeyFrame(timeToBeginning + float.Epsilon, 1, linearEasing);
                }
                playAnimation.InsertKeyFrame(1, fromProgress, linearEasing);
            }
            else
            {
                // Play forwards from fromProgress to toProgress
                playAnimation.InsertKeyFrame(0, fromProgress);
                if (fromProgress > toProgress)
                {
                    // Play to the end
                    var timeToEnd = (1 - fromProgress) / ((1 - fromProgress) + toProgress);
                    playAnimation.InsertKeyFrame(timeToEnd, 1, linearEasing);
                    // Jump to the beginning
                    playAnimation.InsertKeyFrame(timeToEnd + float.Epsilon, 0, linearEasing);
                }
                playAnimation.InsertKeyFrame(1, toProgress, linearEasing);
            }

            if (looped)
            {
                playAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            }
            else
            {
                playAnimation.IterationBehavior = AnimationIterationBehavior.Count;
                playAnimation.IterationCount = 1;
            }

            // Create a batch so that we can know when the animation finishes. This only
            // works for non-looping animations (the batch will complete immediately
            // for looping animations).
            CompositionScopedBatch batch = looped
                ? null
                : _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            _progressPropertySet.StartAnimation(_progressPropertyName, playAnimation);

            // Get a controller for the animation and save it in the PlayState.
            var playState = _currentPlay =
                new PlayAsyncState(_progressPropertySet.TryGetAnimationController(_progressPropertyName));

            if (batch != null)
            {
                batch.Completed += (sender, args) =>
                {
                    // Signal the task that created the animation that it has finished.
                    // Use TrySet because it may have been completed by the Stop() method already.
                    playState.PlayCompletedSource.TrySetResult(true);
                };
                batch.End();
            }

            return playState.PlayCompletedSource.Task;
        }

        void _SetPosition(double progress)
        {
            var floatProgress = ClampFloat0to1(progress);

            if (_currentPlay == null)
            {
                _progressPropertySet.Properties.InsertScalar(_progressPropertyName, floatProgress);

                return;
            }

            // Set the position.
            // TODO - can we do this if we're playing and not paused?
            _currentPlay.Controller.Progress = floatProgress;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Stop();
                Root.Dispose();
                _isDisposed = true;
            }
        }

        // State for a single PlayAsync.
        sealed class PlayAsyncState
        {
            internal PlayAsyncState(AnimationController controller)
            {
                Controller = controller;
                PlayCompletedSource = new TaskCompletionSource<bool>();
            }

            readonly internal AnimationController Controller;

            // Used to signal that the play has finished, either through
            // the animation batch completing, or through Stop() being called.
            readonly internal TaskCompletionSource<bool> PlayCompletedSource;

            internal bool IsPaused;
        }

        void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(VisualPlayer));
            }
        }

        static float ClampFloat0to1(double value) => (float)Math.Min(1, Math.Max(value, 0));
    }

}

