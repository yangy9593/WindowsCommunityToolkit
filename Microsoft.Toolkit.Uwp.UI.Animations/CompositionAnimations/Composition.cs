using System;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;

namespace Microsoft.Toolkit.Uwp.UI.Animations
{
    public static class Composition
    {
        public static double GetListViewStaggerDelayDelta(DependencyObject obj)
        {
            return (double)obj.GetValue(ListViewStaggerDelayDeltaProperty);
        }

        public static void SetListViewStaggerDelayDelta(DependencyObject obj, double value)
        {
            obj.SetValue(ListViewStaggerDelayDeltaProperty, value);
        }

        // Using a DependencyProperty as the backing store for ListViewStaggerDelayDelta.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListViewStaggerDelayDeltaProperty =
            DependencyProperty.RegisterAttached("ListViewStaggerDelayDelta", typeof(double), typeof(Composition), new PropertyMetadata(double.NaN, ListItemLoadedAnimationPropertyChanged));

        public static AnimationCollection GetListItemLoadedAnimation(DependencyObject obj)
        {
            var collection = (AnimationCollection)obj.GetValue(ListItemLoadedAnimationProperty);

            if (collection == null)
            {
                collection = new AnimationCollection();
                obj.SetValue(ListItemLoadedAnimationProperty, collection);
            }

            return collection;
        }

        public static void SetListItemLoadedAnimation(DependencyObject obj, AnimationCollection value)
        {
            obj.SetValue(ListItemLoadedAnimationProperty, value);
        }

        // Using a DependencyProperty as the backing store for ListItemLoadedAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListItemLoadedAnimationProperty =
            DependencyProperty.RegisterAttached("ListItemLoadedAnimation", typeof(AnimationCollection), typeof(Composition), new PropertyMetadata(null, ListItemLoadedAnimationPropertyChanged));

        private static void ListItemLoadedAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Windows.UI.Xaml.Controls.ListViewBase listView)
            {
                listView.ChoosingItemContainer -= ListViewBase_ChoosingItemContainer;
                listView.ChoosingItemContainer += ListViewBase_ChoosingItemContainer;
            }
        }

        private static void ListViewBase_ChoosingItemContainer(Windows.UI.Xaml.Controls.ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            // Do we already have an ItemContainer? If so, we're done here.
            if (args.ItemContainer != null)
            {
                return;
            }

            if (sender.ItemsPanelRoot is ItemsStackPanel || sender.ItemsPanelRoot is ItemsWrapGrid)
            {
                SelectorItem containerItem = null;
                if (sender is ListView)
                {
                    containerItem = new ListViewItem();
                }
                else if (sender is GridView)
                {
                    containerItem = new GridViewItem();
                }

                if (containerItem != null)
                {
                    // Wire up stagger animations on items
                    containerItem.Loaded += ContainerItem_Loaded;

                    args.ItemContainer = containerItem;
                }
            }
            else
            {
                sender.ChoosingItemContainer -= ListViewBase_ChoosingItemContainer;
            }
        }

        private static void ContainerItem_Loaded(object sender, RoutedEventArgs e)
        {
            SelectorItem itemContainer = sender as SelectorItem;
            itemContainer.Loaded -= ContainerItem_Loaded;

            var listViewBase = itemContainer.FindAscendant<Windows.UI.Xaml.Controls.ListViewBase>();
            int firstVisibleIndex, lastVisibleIndex;

            if (listViewBase.ItemsPanelRoot is ItemsStackPanel isp)
            {
                firstVisibleIndex = isp.FirstVisibleIndex;
                lastVisibleIndex = isp.LastCacheIndex;
            }
            else if (listViewBase.ItemsPanelRoot is ItemsWrapGrid iwg)
            {
                firstVisibleIndex = iwg.FirstVisibleIndex;
                lastVisibleIndex = iwg.LastVisibleIndex;
            }
            else
            {
                return;
            }

            var itemIndex = listViewBase.IndexFromContainer(itemContainer);
            var delay = GetListViewStaggerDelayDelta(listViewBase);

            if (double.IsNaN(delay))
            {
                delay = DEFAULT_DELAY_DELTA;
            }

            var relativeIndex = itemIndex - firstVisibleIndex;

            if (itemIndex >= 0 && itemIndex >= firstVisibleIndex && itemIndex <= lastVisibleIndex)
            {
                var frame = itemContainer.FindAscendant<Frame>();
                if (frame != null)
                {
                    if ((frame.ForwardStack.Count > 0 && frame.ForwardStack[frame.ForwardStack.Count - 1].Parameter == itemContainer.Content) ||
                        (frame.BackStack.Count > 0 && frame.BackStack[frame.BackStack.Count - 1].Parameter == itemContainer.Content))
                    {
                        return;
                    }
                }

                var element = itemContainer.ContentTemplateRoot;
                var itemVisual = ElementCompositionPreview.GetElementVisual(element);
                var staggerDelay = TimeSpan.FromMilliseconds(relativeIndex * delay);

                var animations = GetListItemLoadedAnimation(listViewBase) ?? GetDefaultListItemAnimationCollection();
                var visual = ElementCompositionPreview.GetElementVisual(element);
                var compositor = visual.Compositor;

                foreach (var animation in animations)
                {
                    var compositionAnimation = animation.GetCompositionAnimation(compositor);
                    (compositionAnimation as KeyFrameAnimation).DelayTime += staggerDelay;
                    (compositionAnimation as KeyFrameAnimation).DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
                    visual.StartAnimation(animation.Target, compositionAnimation);
                }
            }
        }

        private const double DEFAULT_DELAY_DELTA = 40d;

        private static AnimationCollection GetDefaultListItemAnimationCollection()
        {
            return new AnimationCollection()
                {
                    new OpacityAnimation() { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(500) }
                };
        }
    }
}
