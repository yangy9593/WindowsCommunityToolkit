using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace Lottie
{
    /// <summary>
    /// A Visual tree configured with an animation.
    /// </summary>
    sealed class AnimatedComposition : IDisposable
    {
        readonly Compositor _compositor;
        readonly CompositionObject _animatedObject;
        readonly string _animatedPropertyName;
        PlayAsyncState _currentPlay;
        bool _isDisposed;

        internal AnimatedComposition(
            Visual rootVisual,
            CompositionObject animatedObject,
            string animatedPropertyName,
            TimeSpan animationDuration)
        {
            Root = rootVisual;
            _compositor = rootVisual.Compositor;
            AnimationDuration = animationDuration;
            _animatedObject = animatedObject;
            _animatedPropertyName = animatedPropertyName;
        }

        /// <summary>
        /// The duration of the animation.
        /// </summary>
        public TimeSpan AnimationDuration { get; }

        /// <summary>
        /// True if any <see cref="Task"/>s returned by <see cref="PlayAsync(bool)"/> 
        /// have not yet completed, i.e. if the animation is currently playing or paused.
        /// </summary>
        public bool IsPlaying => _currentPlay != null;

        /// <summary>
        /// The root of the Visual tree.
        /// </summary>
        public Visual Root { get; }

        /// <summary>
        /// Convenience method for setting the position of the animation. This 
        /// simply inserts a scalar value into the <see cref="Root"/>'s
        /// property set with the name of <see cref="PositionPropertyName"/>.
        /// </summary>
        /// <remarks>
        /// The value must be 0 to 1. The animation must not be currently
        /// playing.
        /// </remarks>
        public void SetPosition(double position)
        {
            AssertNotDisposed();

            if (position < 0 || position > 1)
            {
                throw new ArgumentException($"{nameof(position)} must be >= 0 and <= 1.");
            }

            _animatedObject.Properties.InsertScalar(_animatedPropertyName, (float)position);
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
        public Task PlayAsync(bool loop)
        {
            AssertNotDisposed();

            // Cause any other tasks waiting in this method to return. The most
            // recent request to play always wins.
            Stop();

            Debug.Assert(_currentPlay == null);

            // Start the animation
            StartAnimating(looped: loop);

            // Return a Task that will be completed when the play is stopped
            // or the animation batch completes.
            return _currentPlay.PlayCompletedSource.Task;
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
                _animatedObject.StopAnimation(_animatedPropertyName);

                // Use TrySet because it may have been completed by the Batch.Completed callback already.
                // Batch.Completed can not be relied on for looping animations as it fires immediately.
                playBeingStopped.PlayCompletedSource.TrySetResult(true);
            }
        }

        // Starts animating.
        void StartAnimating(bool looped)
        {
            Debug.Assert(_currentPlay == null);
            var playAnimation = _compositor.CreateScalarKeyFrameAnimation();

            playAnimation.Duration = AnimationDuration;
            playAnimation.InsertKeyFrame(0, 0);
            playAnimation.InsertKeyFrame(1, 1, _compositor.CreateLinearEasingFunction());

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

            _animatedObject.StartAnimation(_animatedPropertyName, playAnimation);

            // Get a controller for the animation and save it in the PlayState.
            var playState = _currentPlay = 
                new PlayAsyncState(_animatedObject.TryGetAnimationController(_animatedPropertyName));

            //// Start from where it stopped last time.
            //_target.Properties.TryGetScalar(_progressPropertyName, out var currentPosition);
            //playState._animationController.Progress = currentPosition;

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
        }

        void _SetPosition(double progress)
        {
            if (_currentPlay == null)
            {
                _animatedObject.Properties.InsertScalar(_animatedPropertyName, (float)progress);

                return;
            }

            // Set the position.
            // TODO - can we do this if we're playing and not paused?
            _currentPlay.Controller.Progress = (float)progress;
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
                throw new ObjectDisposedException(nameof(AnimatedComposition));
            }
        }

    }

}

