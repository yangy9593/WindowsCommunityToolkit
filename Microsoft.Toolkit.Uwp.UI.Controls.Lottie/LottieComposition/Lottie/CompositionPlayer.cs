#if DEBUG
// If uncommented, outputs measure and arrange info.
//#define DebugMeasureAndArrange
#endif // DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Lottie
{
    /// <summary>
    /// A XAML element that displays and controls an animated composition.
    /// </summary>
    [ContentProperty(Name = nameof(Source))]
    public sealed class CompositionPlayer : FrameworkElement
    {
        // The name of the property in the _progressPropertySet that 
        // controls the progress of the animation.
        const string c_progressPropertyName = "Progress";

        // The Visual to which the current composition will be attached.
        readonly SpriteVisual _rootVisual;

        // The PropertySet that is animated as the progress of the player advances.
        // The the current composition's progress is synched to this.
        readonly CompositionPropertySet _progressPropertySet;

        // Commands (Pause/Play/Resume/Stop) that were requested before
        // the composition was fully loaded. These will be played back
        // when the load completes.
        readonly List<Command> _queuedCommands = new List<Command>();

        // Set true when a play/resume/stop/pause request is made
        // for a new Source. This is used to control the AutoPlay
        // behavior, which, when set, will initiate a play if there
        // have been no explicit requests yet.
        bool _requestSeen;

        // The root visual of the current composition.
        Visual _compositionRoot;

        // The size of the current composition. Only valid if _compositionRoot is not null.
        Vector2 _compositionSize;

        // If a PlayAsync is active, its state.
        PlayAsyncState _currentPlay;

        // Used to detect reentrance in RunAnimationAsync.
        int _runAnimationAsyncVersion;

        #region Dependency properties
        public static DependencyProperty AutoPlayProperty { get; } =
            RegisterDP(nameof(AutoPlay), true,
                (owner, oldValue, newValue) => owner.HandleAutoPlayPropertyChanged(oldValue, newValue));

        public static DependencyProperty BackgroundColorProperty { get; } =
            RegisterDP(nameof(BackgroundColor), Colors.Transparent,
                (owner, oldValue, newValue) => owner.HandleBackgroundColorPropertyChanged(oldValue, newValue));

        public static DependencyProperty DiagnosticsProperty { get; } =
            RegisterDP(nameof(Diagnostics), (object)null);

        public static DependencyProperty DurationProperty { get; } =
            RegisterDP(nameof(Duration), TimeSpan.Zero);

        public static DependencyProperty FromProgressProperty { get; } =
            RegisterDP(nameof(FromProgress), 0.0);

        public static DependencyProperty IsCompositionLoadedProperty { get; } =
            RegisterDP(nameof(IsCompositionLoaded), false);

        public static DependencyProperty IsLoopingEnabledProperty { get; } =
            RegisterDP(nameof(IsLoopingEnabled), true);

        public static DependencyProperty IsPlayingProperty { get; } =
            RegisterDP(nameof(IsPlaying), false);

        public static DependencyProperty PlaybackRateProperty { get; } =
            RegisterDP(nameof(PlaybackRate), 1.0,
                (owner, oldValue, newValue) => owner.HandlePlaybackRateChanged(oldValue, newValue));

        public static DependencyProperty ReverseAnimationProperty { get; } =
            RegisterDP(nameof(ReverseAnimation), false);

        public static DependencyProperty SourceProperty { get; } =
            RegisterDP(nameof(Source), (ICompositionSource)null,
                (owner, oldValue, newValue) => owner.HandleSourcePropertyChanged(oldValue, newValue));

        public static DependencyProperty StretchProperty { get; } =
            RegisterDP(nameof(Stretch), Stretch.Uniform,
            (owner, oldValue, newValue) => owner.HandleStretchPropertyChanged(oldValue, newValue));

        public static DependencyProperty ToProgressProperty { get; } =
            RegisterDP(nameof(ToProgress), 1.0);

        #endregion Dependency properties

        /// <summary>
        /// A <see cref="CompositionObject"/> that will be animated along with 
        /// the progress of the <see cref="CompositionPlayer"/>.
        /// </summary>
        /// <remarks>
        /// This is exposed to support advanced scenarios where other <see cref="CompositionObject"/>s
        /// are animated at the same rate as the <see cref="CompositionPlayer"/>.
        /// To bind a property to the progress of this player, use an <see cref="ExpressionAnimation"/>
        /// with an expression that references a scalar property named "Progress" on this object.
        /// </remarks>
        public CompositionObject ProgressObject => _progressPropertySet;

        public CompositionPlayer()
        {
            // Create a visual to parent, clip, and center the content,
            // and to host the Progress property
            var compositor = Window.Current.Compositor;
            _rootVisual = compositor.CreateSpriteVisual();
            _progressPropertySet = _rootVisual.Properties;
            ElementCompositionPreview.SetElementChildVisual(this, _rootVisual);

            // Set an initial value for the Progress property.
            _progressPropertySet.InsertScalar(c_progressPropertyName, 0);

            // Ensure the content can't render outside the bounds of the element.
            _rootVisual.Clip = compositor.CreateInsetClip();

            // Ensure the resources get cleaned up when the element is unloaded.
            Unloaded += (sender, e) => UnloadComposition();
        }

        #region Properties
        public bool AutoPlay
        {
            get => (bool)GetValue(AutoPlayProperty);
            set => SetValue(AutoPlayProperty, value);
        }

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }
        /// <summary>
        /// Contains optional diagnostics information about the composition.
        /// </summary>
        public object Diagnostics => GetValue(DiagnosticsProperty);

        public TimeSpan Duration => (TimeSpan)GetValue(DurationProperty);

        /// <summary>
        /// The point at which to start the animation, as a value from 0 to 1.
        /// </summary>
        public double FromProgress
        {
            get => (double)GetValue(FromProgressProperty);
            set => SetValue(FromProgressProperty, value);
        }

        public bool IsCompositionLoaded => (bool)GetValue(IsCompositionLoadedProperty);

        /// <summary>
        /// If true, the animation will loop continuously between <see cref="FromProgress"/>
        /// and <see cref="ToProgress"/>. If false, the animation will play once each
        /// time the <see cref="PlayAsync"/> method is called, or when the <see cref="Source"/>
        /// property is set and the <see cref="AutoPlay"/> property is true.
        /// </summary>
        public bool IsLoopingEnabled
        {
            get => (bool)GetValue(IsLoopingEnabledProperty);
            set => SetValue(IsLoopingEnabledProperty, value);
        }

        public bool IsPlaying => (bool)GetValue(IsPlayingProperty);

        /// <summary>
        /// If true, the animation will play backwards, from <see cref="ToProgress"/> to <see cref="FromProgress"/>.
        /// </summary>
        public bool ReverseAnimation
        {
            get => (bool)GetValue(ReverseAnimationProperty);
            set => SetValue(ReverseAnimationProperty, value);
        }

        public double PlaybackRate
        {
            get => (double)GetValue(PlaybackRateProperty);
            set => SetValue(PlaybackRateProperty, value);
        }

        public ICompositionSource Source
        {
            get => (ICompositionSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        /// <summary>
        /// The point at which to finish the animation, as a value from 0 to 1.
        /// </summary>
        public double ToProgress
        {
            get => (double)GetValue(ToProgressProperty);
            set => SetValue(ToProgressProperty, value);
        }

        #endregion Properties

        /// <summary>
        /// Plays a segment of the composition.
        /// </summary>
        public IAsyncAction PlayAsync(CompositionSegment segment) => _PlayAsync(segment).AsAsyncAction();

        /// <summary>
        /// Plays the composition.
        /// </summary>
        public IAsyncAction PlayAsync() =>
            PlayAsync(new CompositionSegment(null, FromProgress, ToProgress, IsLoopingEnabled, ReverseAnimation));

        /// <summary>
        /// Plays the composition.
        /// </summary>
        public void Play() =>
            _PlayAsync(new CompositionSegment(null, FromProgress, ToProgress, IsLoopingEnabled, ReverseAnimation));

        public void SetProgress(double progress)
        {
            if (_compositionRoot == null)
            {
                _queuedCommands.Add(new SetProgressCommand(progress));
            }
            else
            {
                _requestSeen = true;
                Stop();
                var floatProgress = ClampFloat0to1(progress);
                _progressPropertySet.InsertScalar(c_progressPropertyName, floatProgress);
            }
        }

        public void Stop()
        {
            if (_compositionRoot == null)
            {
                _queuedCommands.Add(Command.Stop);
            }
            else
            {
                _requestSeen = true;

                var playBeingStopped = _currentPlay;

                // Stop the current play.
                if (playBeingStopped != null)
                {
                    _currentPlay = null;

                    // WARNING: Reentrance may occur from StopAnimation or TrySetResult.

                    // Stop the animation.
                    _progressPropertySet.StopAnimation(c_progressPropertyName);

                    // Use TrySet because it may have been completed by the Batch.Completed callback already.
                    // Batch.Completed can not be relied on for looping animations as it fires as soon
                    // as the animation starts.
                    playBeingStopped.PlayCompletedSource.TrySetResult(true);
                }
            }
        }

        public void Pause()
        {
            if (_compositionRoot == null)
            {
                _queuedCommands.Add(Command.Pause);
            }
            else
            {
                _requestSeen = true;
                if (_currentPlay != null && !_currentPlay.IsPaused)
                {
                    _currentPlay.IsPaused = true;
                    _currentPlay.Controller.Pause();
                }
            }
        }

        public void Resume()
        {
            if (_compositionRoot == null)
            {
                _queuedCommands.Add(Command.Resume);
            }
            else
            {
                _requestSeen = true;
                if (_currentPlay != null && _currentPlay.IsPaused)
                {
                    _currentPlay.IsPaused = false;
                    _currentPlay.Controller.Resume();
                }
            }
        }

        // Requests that the animation starts playing and returns a Task
        // that completes when the animation completes.
        Task _PlayAsync(CompositionSegment segment)
        {
            if (_compositionRoot == null)
            {
                // Enqueue a command.
                var playCommand = new PlayAsyncCommand(segment.FromProgress, segment.ToProgress, segment.IsLoopingEnabled, segment.ReverseAnimation);
                _queuedCommands.Add(playCommand);
                return playCommand.Task;
            }
            else
            {
                _requestSeen = true;
                Debug.WriteLine($"Playing segment {segment.Name}");
                return RunAnimationAsync(segment.FromProgress, segment.ToProgress, segment.IsLoopingEnabled, segment.ReverseAnimation);
            }
        }

        // Converts infinity to double.MaxValue so as to avoid needing special handling for infinite values.
        static double AbstractInfinity(double value) => double.IsInfinity(value) ? double.MaxValue : value;

        protected override Size MeasureOverride(Size availableSize)
        {
            DebugMeasureAndArrange($"Measure availableSize: {availableSize} Stretch: {Stretch}");

            Size measuredSize;

            // No Width or Height are specified.
            if (_compositionRoot == null || _compositionSize.ToSize().IsEmpty)
            {
                // No composition is loaded or it has a 0 size, so it will take up no space.
                // It's not valid to return Size.Empty (it will cause a div/0 in the caller),
                // so return the smallest possible size.
                measuredSize = new Size(double.Epsilon, double.Epsilon);
            }
            else
            {
                // Measure the size based on the stretch mode.
                switch (Stretch)
                {
                    case Stretch.None:
                        {
                            if (_compositionSize.X < AbstractInfinity(availableSize.Width) && _compositionSize.Y < AbstractInfinity(availableSize.Height))
                            {
                                // The native size of the composition will fit inside the available size.
                                measuredSize = _compositionSize.ToSize();
                            }
                            else if (double.IsInfinity(availableSize.Width))
                            {
                                // The native size won't fit, and width is infinite
                                if (double.IsInfinity(availableSize.Height))
                                {
                                    // The width and height are infinite.
                                    measuredSize = availableSize;
                                }
                                else
                                {
                                    // Just the width is infinite. The native height fits.
                                    measuredSize = new Size(_compositionSize.X, availableSize.Height);
                                }
                            }
                            else if (double.IsInfinity(availableSize.Height))
                            {
                                // Just the height is infinite. The native width fits.
                                measuredSize = new Size(availableSize.Width, _compositionSize.Y);
                            }
                            else
                            {
                                // The native size is too big and no available dimension is infinite.
                                measuredSize = availableSize;
                            }
                        }
                        break;
                    case Stretch.Fill:
                        if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height))
                        {
                            // One of the dimensions is infinite so we can't fill both dimensions. Fall back
                            // to Uniform so at least the non-infinite dimension will be filled.
                            goto case Stretch.Uniform;
                        }
                        else
                        {
                            // We will fill all available space.
                            measuredSize = availableSize;
                        }
                        break;
                    case Stretch.UniformToFill:
                        if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height))
                        {
                            // One of the dimensions is infinite, we can't scale in such a way as to leave
                            // no space around the edge, so fall back to Uniform.
                            goto case Stretch.Uniform;
                        }
                        else
                        {
                            // Scale so there is no space around the edge.
                            var widthScale = availableSize.Width / _compositionSize.X;
                            var heightScale = availableSize.Height / _compositionSize.Y;
                            if (widthScale < heightScale)
                            {
                                heightScale = widthScale;
                            }
                            else
                            {
                                widthScale = heightScale;
                            }
                            var measuredX = Math.Min(_compositionSize.X * widthScale, availableSize.Width);
                            var measuredY = Math.Min(_compositionSize.Y * heightScale, availableSize.Height);

                            measuredSize = new Size(measuredX, measuredY);
                        }
                        break;
                    case Stretch.Uniform:
                        {
                            // Scale so that one dimension fits exactly and no dimension exceeds the boundary.
                            var widthScale = AbstractInfinity(availableSize.Width) / _compositionSize.X;
                            var heightScale = AbstractInfinity(availableSize.Height) / _compositionSize.Y;
                            measuredSize = (heightScale > widthScale)
                                ? new Size(availableSize.Width, _compositionSize.Y * widthScale)
                                : new Size(_compositionSize.X * heightScale, availableSize.Height);
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            DebugMeasureAndArrange($"Measure returning: {measuredSize}");
            return measuredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            DebugMeasureAndArrange($"Arrange finalSize: {finalSize} Stretch: {Stretch}");

            if (_compositionRoot == null)
            {
                // No content. 
                _rootVisual.Size = new Vector2(0);
                return finalSize;
            }

            var widthScale = 1.0;
            var heightScale = 1.0;

            // Now that we know how much size we have to fill, set the scaling and offset appropriately.
            switch (Stretch)
            {
                case Stretch.None:
                    // Do not scale, do not center.
                    break;
                case Stretch.Fill:
                    widthScale = finalSize.Width / _compositionSize.X;
                    heightScale = finalSize.Height / _compositionSize.Y;
                    break;
                case Stretch.Uniform:
                    {
                        widthScale = finalSize.Width / _compositionSize.X;
                        heightScale = finalSize.Height / _compositionSize.Y;
                        if (widthScale < heightScale)
                        {
                            heightScale = widthScale;
                        }
                        else
                        {
                            widthScale = heightScale;
                        }
                    }
                    break;
                case Stretch.UniformToFill:
                    {

                        widthScale = finalSize.Width / _compositionSize.X;
                        heightScale = finalSize.Height / _compositionSize.Y;
                        if (widthScale > heightScale)
                        {
                            heightScale = widthScale;
                        }
                        else
                        {
                            widthScale = heightScale;
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
            // Scale appropriately.
            _rootVisual.Scale = new Vector3((float)widthScale, (float)heightScale, 1);

            var xOffset = 0.0;
            var yOffset = 0.0;

            // A size needs to be set because there's an InsetClip applied, and without a size it will clip everything.
            var scaledWidth = _compositionSize.X * widthScale;
            var scaledHeight = _compositionSize.Y * heightScale;
            _rootVisual.Size = new Vector2((float)Math.Min(finalSize.Width / widthScale, _compositionSize.X), (float)Math.Min(finalSize.Height / heightScale, _compositionSize.Y));

            // Center the animation.
            if (Stretch != Stretch.None)
            {
                xOffset = (finalSize.Width - scaledWidth) / 2;
                yOffset = (finalSize.Height - scaledHeight) / 2;
                _rootVisual.Offset = new Vector3((float)xOffset, (float)yOffset, 0);

                if (Stretch == Stretch.UniformToFill)
                {
                    // Adjust the position of the clip.
                    _rootVisual.Clip.Offset = new Vector2((float)(-xOffset / widthScale), (float)(-yOffset / heightScale));
                }
                else
                {
                    _rootVisual.Clip.Offset = Vector2.Zero;
                }
            }

            DebugMeasureAndArrange($"Arrange: final {finalSize} scale: {widthScale}x{heightScale}  offset: {xOffset},{yOffset} clip size: {_rootVisual.Size}");
            return finalSize;
        }

        // Called when the AutoPlay property is updated.
        void HandleAutoPlayPropertyChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (_compositionRoot == null)
                {
                    _queuedCommands.Add(Command.AutoPlay);
                }
                else
                {
                    if (!_requestSeen)
                    {
                        Play();
                    }
                }
            }
            else
            {
                // Ensure there are no auto-play commands enqueued.
                while (_queuedCommands.Remove(Command.AutoPlay)) { }
            }
        }

        // Called when the BackgroundColor properyt is updated.
        void HandleBackgroundColorPropertyChanged(Color oldValue, Color newValue)
        {
            _rootVisual.Brush = Window.Current.Compositor.CreateColorBrush(newValue);
        }

        // Called when the PlaybackRate property is updated.
        void HandlePlaybackRateChanged(double oldValue, double newValue)
        {
            if (_currentPlay != null)
            {
                _currentPlay.Controller.PlaybackRate = (float)newValue;
            }
        }

        // Called when the current ICompositionSource changes its content.
        void HandleCompositionInvalidated(object sender)
        {
            // Get the new content from the source.
            UpdateContent();
        }

        // Called when the Source property is updated.
        void HandleSourcePropertyChanged(ICompositionSource oldValue, ICompositionSource newValue)
        {
            // Clear out the command queue. Any commands that were
            // enqueued before the Source was set are irrelevant.
            ClearCommandQueue();

            // Disconnect from the old source.
            if (oldValue is IDynamicCompositionSource oldDynamicSource)
            {
                oldDynamicSource.CompositionInvalidated -= HandleCompositionInvalidated;
            }

            if (newValue is IDynamicCompositionSource newDynamicSource)
            {
                newDynamicSource.CompositionInvalidated += HandleCompositionInvalidated;
            }

            // Get the new content from the source.
            UpdateContent();
        }

        // Called when the Stretch property is updated.
        void HandleStretchPropertyChanged(Stretch oldValue, Stretch newValue)
        {
            if (_compositionRoot != null)
            {
                DebugMeasureAndArrange("Invalidating measure.");
                InvalidateMeasure();
            }
        }

        // Removes all the enqueued commands and completes any PlayAsync commands.
        void ClearCommandQueue()
        {
            // Copy the commands so that the queue can be emptied before any PlayAsyncs
            // get completed. This is necessary because completing a PlayAsync may cause
            // an immediate callback and reentrance.
            var commands = _queuedCommands.ToArray();
            _queuedCommands.Clear();

            foreach (var command in commands)
            {
                if (command.Type == Command.CommandType.PlayAsync)
                {
                    // Complete the PlayAsync task.
                    ((PlayAsyncCommand)command).CompleteTask();
                }
            }
        }

        // Method called when the current ICompositionSource has new content
        // or the existing content is no longer valid.
        void UpdateContent()
        {
            // Unload the old composition (if any).
            UnloadComposition();

            Visual rootVisual;
            Vector2 size;
            CompositionPropertySet progressPropertySet;
            TimeSpan duration;
            object diagnostics;

            if (Source == null)
            {
                rootVisual = null;
                size = default(Vector2);
                progressPropertySet = null;
                duration = default(TimeSpan);
                diagnostics = null;
            }
            else
            {
                Source.TryCreateInstance(
                    Window.Current.Compositor,
                    out rootVisual,
                    out size,
                    out progressPropertySet,
                    out duration,
                    out diagnostics);
            }

            _compositionRoot = rootVisual;
            _compositionSize = size;

            SetValue(DurationProperty, duration);
            SetValue(DiagnosticsProperty, diagnostics);

            if (rootVisual == null)
            {
                // Load failed. Clear out the queued commands and complete the plays.
                ClearCommandQueue();
            }
            else
            {
                // Connect the content's progress property to our progress property.
                var progressAnimation = Window.Current.Compositor.CreateExpressionAnimation($"my.{c_progressPropertyName}");
                progressAnimation.SetReferenceParameter("my", _progressPropertySet);
                progressPropertySet.StartAnimation("Progress", progressAnimation);

                if (AutoPlay)
                {
                    // Auto-play is enabled. Enqueue an AutoPlay command.
                    _queuedCommands.Add(Command.AutoPlay);
                }

                Debug.Assert(_rootVisual.Children.Count == 0);

                _rootVisual.Children.InsertAtTop(_compositionRoot);

                // The element needs to be measured again for the new content.
                DebugMeasureAndArrange("Invalidating measure.");
                InvalidateMeasure();

                // Play back any commands that were enqueued during loading.
                if (_queuedCommands.Where(cmd => cmd.Type == Command.CommandType.AutoPlay).Any() &&
                    !_queuedCommands.Where(cmd => cmd.Type != Command.CommandType.AutoPlay).Any())
                {
                    // AutoPlay was enabled when loading was enabled or since loading started,
                    // AND there were no other requests. Auto-play.
                    Play();
                }
                else
                {
                    // Process all the commands in the queue. Copy and clear the queue first
                    // in case handling one of the commands causes reentrance.
                    var commands = _queuedCommands.ToArray();
                    _queuedCommands.Clear();
                    foreach (var command in commands)
                    {
                        switch (command.Type)
                        {
                            case Command.CommandType.PlayAsync:
                                {
                                    // Hook up the TaskCompletionSource to complete the 
                                    // original play request when the animation completes.
                                    var playCommand = (PlayAsyncCommand)command;
                                    RunAnimationAsync(playCommand.FromProgress, playCommand.ToProgress, playCommand.Loop, playCommand.Reverse).
                                        GetAwaiter().
                                            OnCompleted(playCommand.CompleteTask);
                                }
                                break;
                            case Command.CommandType.Pause:
                                Pause();
                                break;
                            case Command.CommandType.Resume:
                                Resume();
                                break;
                            case Command.CommandType.Stop:
                                Stop();
                                break;
                            case Command.CommandType.SetProgress:
                                Pause();
                                SetProgress(((SetProgressCommand)command).Progress);
                                break;
                            case Command.CommandType.AutoPlay:
                                // Ignore auto-play - it is handled above.
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                    }
                }
            }

            SetValue(IsCompositionLoadedProperty, _compositionRoot != null);
        }


        // Creates and starts an animation on the progress property. The task completes
        // when the animation is stopped or completes.
        Task RunAnimationAsync(double fromProgress, double toProgress, bool looped, bool reversed)
        {
            Debug.WriteLine($"Play requested of segment from {fromProgress} to {toProgress}");

            // Used to detect reentrance.
            var version = ++_runAnimationAsyncVersion;

            // Cause any other tasks waiting in this method to return. This method may
            // cause reentrance.
            Stop();

            if (version != _runAnimationAsyncVersion)
            {
                // The call was overtaken by another call due to reentrance.
                Debug.WriteLine($"Not playing segment from {fromProgress} to {toProgress} because another segment was requested.");
                return Task.CompletedTask;
            }

            Debug.Assert(_currentPlay == null);

            var from = ClampFloat0to1(fromProgress);
            var to = ClampFloat0to1(toProgress);

            if (from == to)
            {
                // Nothing to play.
                return Task.CompletedTask;
            }

            var duration = Duration * (from < to ? (to - from) : ((1 - from) + to));

            if (duration.TotalMilliseconds < 20)
            {
                // Nothing to play.
                SetProgress(from);
                return Task.CompletedTask;
            }

            Debug.WriteLine($"Playing segment from {fromProgress} to {toProgress}");

            var compositor = Window.Current.Compositor;
            var playAnimation = compositor.CreateScalarKeyFrameAnimation();
            playAnimation.Duration = duration;
            var linearEasing = compositor.CreateLinearEasingFunction();

            if (reversed)
            {
                // Play backwards from toProgress to fromProgress
                playAnimation.InsertKeyFrame(0, to);
                if (from > to)
                {
                    // Play to the beginning.
                    var timeToBeginning = to / (to + (1 - from));
                    playAnimation.InsertKeyFrame(timeToBeginning, 0, linearEasing);
                    // Jump to the end.
                    playAnimation.InsertKeyFrame(timeToBeginning + float.Epsilon, 1, linearEasing);
                }
                playAnimation.InsertKeyFrame(1, from, linearEasing);
            }
            else
            {
                // Play forwards from fromProgress to toProgress
                playAnimation.InsertKeyFrame(0, from);
                if (from > to)
                {
                    // Play to the end
                    var timeToEnd = (1 - from) / ((1 - from) + to);
                    playAnimation.InsertKeyFrame(timeToEnd, 1, linearEasing);
                    // Jump to the beginning
                    playAnimation.InsertKeyFrame(timeToEnd + float.Epsilon, 0, linearEasing);
                }
                playAnimation.InsertKeyFrame(1, to, linearEasing);
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
                : compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            _progressPropertySet.StartAnimation(c_progressPropertyName, playAnimation);

            // Get a controller for the animation and save it in the PlayState.
            var playState = _currentPlay =
                new PlayAsyncState(_progressPropertySet.TryGetAnimationController(c_progressPropertyName));

            // Set the playback rate.
            _currentPlay.Controller.PlaybackRate = (float)PlaybackRate;

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

        void UnloadComposition()
        {
            if (_compositionRoot != null)
            {
                Stop();

                // Set the progress of the CompositionPropertySet to 0. This is important 
                // to ensure that other UI that is bound to this value is reset.
                _progressPropertySet.InsertScalar(c_progressPropertyName, 0);

                _rootVisual.Children.RemoveAll();

                _compositionRoot.Dispose();
                _compositionRoot = null;

                InvalidateArrange();
                SetValue(DurationProperty, null);
                SetValue(DiagnosticsProperty, null);
                SetValue(IsCompositionLoadedProperty, false);
            }
        }

        #region DependencyProperty helpers

        static DependencyProperty RegisterDP<T>(string propertyName, T defaultValue) =>
            DependencyProperty.Register(propertyName, typeof(T), typeof(CompositionPlayer), new PropertyMetadata(defaultValue));

        static DependencyProperty RegisterDP<T>(string propertyName, T defaultValue, Action<CompositionPlayer, T, T> callback) =>
            DependencyProperty.Register(propertyName, typeof(T), typeof(CompositionPlayer),
                new PropertyMetadata(defaultValue, (d, e) => callback(((CompositionPlayer)d), (T)e.OldValue, (T)e.NewValue)));

        #endregion DependencyProperty helpers

        static float ClampFloat0to1(double value) => (float)Math.Min(1, Math.Max(value, 0));

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

        class Command
        {
            protected Command(CommandType type) => Type = type;

            internal static readonly Command Pause = new Command(CommandType.Pause);
            internal static readonly Command Resume = new Command(CommandType.Resume);
            internal static readonly Command Stop = new Command(CommandType.Stop);
            internal static readonly Command AutoPlay = new Command(CommandType.AutoPlay);
            internal CommandType Type { get; }

            internal enum CommandType
            {
                AutoPlay,
                PlayAsync,
                Pause,
                Resume,
                Stop,
                SetProgress,
            }
        }

        sealed class PlayAsyncCommand : Command
        {
            readonly TaskCompletionSource<bool> _taskCompletionSource
                = new TaskCompletionSource<bool>();

            internal PlayAsyncCommand(double fromProgress, double toProgress, bool loop, bool reverse) : base(CommandType.PlayAsync)
            {
                FromProgress = fromProgress;
                ToProgress = toProgress;
                Loop = loop;
                Reverse = reverse;
            }
            internal double FromProgress { get; }
            internal double ToProgress { get; }
            internal bool Loop { get; }
            internal bool Reverse { get; }

            internal void CompleteTask() => _taskCompletionSource.SetResult(true);

            // Gets a Task that will complete when the PlayCommand is completed.
            internal Task Task => _taskCompletionSource.Task;

        }

        sealed class SetProgressCommand : Command
        {
            internal SetProgressCommand(double progress) : base(CommandType.SetProgress)
            {
                Progress = progress;
            }
            internal double Progress { get; }
        }

        [Conditional("DebugMeasureAndArrange")]
        static void DebugMeasureAndArrange(string line) => Debug.WriteLine(line);
    }
}
