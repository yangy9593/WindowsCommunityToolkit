// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Toolkit.Uwp.SampleApp.Models;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.Toolkit.Uwp.SampleApp.SamplePages
{
    /// <summary>
    /// LottieAnimationViewPage sample page
    /// </summary>
    public sealed partial class LottieAnimationViewPage : Page, IXamlRenderListener
    {
        private LottieAnimationView _lottieAnimationView;
        private RangeSelector _rangeSelector;
        private Grid _lottieAnimationViewGrid;

        public LottieAnimationViewPage()
        {
            InitializeComponent();
        }

        public void OnXamlRendered(FrameworkElement control)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            _lottieAnimationView = control.FindChildByName("Container") as LottieAnimationView;
            _lottieAnimationView.Loaded += (s, e) =>
            {
                _rangeSelector = VisualTree.FindDescendant<RangeSelector>(Window.Current.Content as Frame);
                UpdateRangeSelector();
            };

            _lottieAnimationViewGrid = control.FindChildByName("LottieAnimationViewGrid") as Grid;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Shell.Current.RegisterNewCommand("Resume/Pause", ResumePauseButton_Click);
            Shell.Current.RegisterNewCommand("Open File", OpenFileButton_Click);
            Shell.Current.RegisterNewCommand("Background White", BackgroundWhiteButton_Click);
            Shell.Current.RegisterNewCommand("Background Black", BackgroundBlackButton_Click);
        }

        private void UpdateRangeSelector()
        {
            if (_rangeSelector != null)
            {
                _rangeSelector.Minimum = _lottieAnimationView.StartFrame;
                _rangeSelector.Maximum = _lottieAnimationView.EndFrame;

                var sample = DataContext as Sample;
                var propDict = sample.PropertyDescriptor.Expando as IDictionary<string, object>;
                (propDict["MinFrame"] as ValueHolder).Value = _rangeSelector.Minimum;
                (propDict["MaxFrame"] as ValueHolder).Value = _rangeSelector.Maximum;
            }
        }

        private void ResumePauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lottieAnimationView.IsAnimating)
            {
                _lottieAnimationView.PauseAnimation();
            }
            else
            {
                if (_lottieAnimationView.TimesRepeated >= _lottieAnimationView.RepeatCount)
                {
                    _lottieAnimationView.PlayAnimation();
                }
                else
                {
                    _lottieAnimationView.ResumeAnimation();
                }
            }
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".json");
            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                await _lottieAnimationView.SetAnimationAsync(new StreamReader(await file.OpenStreamForReadAsync()));
                UpdateRangeSelector();
                _lottieAnimationView.PlayAnimation();
            }
        }

        private void BackgroundWhiteButton_Click(object sender, RoutedEventArgs e)
        {
            _lottieAnimationViewGrid.Background = new SolidColorBrush(Colors.White);
        }

        private void BackgroundBlackButton_Click(object sender, RoutedEventArgs e)
        {
            _lottieAnimationViewGrid.Background = new SolidColorBrush(Colors.Black);
        }
    }
}
