using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ScriptLinker.Behaviors
{
    public class ScrollViewerBehavior
    {
        public static readonly DependencyProperty ScrollToTopOnSourceChangedProperty =
            DependencyProperty.RegisterAttached
            (
                "ScrollToTopOnSourceChanged",
                typeof(bool),
                typeof(ScrollViewerBehavior),
                new UIPropertyMetadata(false, OnScrollToTopPropertyChanged)
            );

        public static bool GetScrollToTopOnSourceChanged(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToTopOnSourceChangedProperty);
        }
        public static void SetScrollToTopOnSourceChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToTopOnSourceChangedProperty, value);
        }
        private static void OnScrollToTopPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = dpo as ItemsControl;
            if (itemsControl == null)
                return;

            var dependencyPropertyDescriptor =
                    DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
            if (dependencyPropertyDescriptor != null)
            {
                if ((bool)e.NewValue == true)
                {
                    if ((bool)e.NewValue == true)
                    {
                        dependencyPropertyDescriptor.AddValueChanged(itemsControl, ItemsSourceChanged);
                    }
                    else
                    {
                        dependencyPropertyDescriptor.RemoveValueChanged(itemsControl, ItemsSourceChanged);
                    }
                }
            }
        }
        static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ItemsControl itemsControl = sender as ItemsControl;
            EventHandler eventHandler = null;
            eventHandler = new EventHandler(delegate
            {
                if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(itemsControl) as ScrollViewer;
                    scrollViewer.ScrollToTop();
                    itemsControl.ItemContainerGenerator.StatusChanged -= eventHandler;
                }
            });
            itemsControl.ItemContainerGenerator.StatusChanged += eventHandler;
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
