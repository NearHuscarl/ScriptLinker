using System;
using System.Windows;

namespace ScriptLinker.Behaviors
{
    enum Magin
    {
        Left,
        Top,
        Right,
        Bottom,
    }

    class Margin
    {
        public static readonly DependencyProperty RightProperty = DependencyProperty.RegisterAttached(
            "Right",
            typeof(double),
            typeof(Margin),
            new UIPropertyMetadata(OnRightPropertyChanged));

        public static double GetRight(FrameworkElement element)
        {
            return (double)element.GetValue(RightProperty);
        }

        public static void SetRight(FrameworkElement element, double value)
        {
            element.SetValue(RightProperty, value);
        }

        private static void OnRightPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs args)
        {
            ChangeMarginProperty(dpo, args, Magin.Right);
        }

        public static readonly DependencyProperty LeftProperty = DependencyProperty.RegisterAttached(
            "Left",
            typeof(double),
            typeof(Margin),
            new UIPropertyMetadata(OnLeftPropertyChanged));

        public static double GetLeft(FrameworkElement element)
        {
            return (double)element.GetValue(LeftProperty);
        }

        public static void SetLeft(FrameworkElement element, double value)
        {
            element.SetValue(LeftProperty, value);
        }

        private static void OnLeftPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs args)
        {
            ChangeMarginProperty(dpo, args, Magin.Left);
        }

        public static readonly DependencyProperty TopProperty = DependencyProperty.RegisterAttached(
           "Top",
           typeof(double),
           typeof(Margin),
           new UIPropertyMetadata(OnTopPropertyChanged));

        public static double GetTop(FrameworkElement element)
        {
            return (double)element.GetValue(TopProperty);
        }

        public static void SetTop(FrameworkElement element, double value)
        {
            element.SetValue(TopProperty, value);
        }

        private static void OnTopPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs args)
        {
            ChangeMarginProperty(dpo, args, Magin.Top);
        }

        public static readonly DependencyProperty BottomProperty = DependencyProperty.RegisterAttached(
           "Bottom",
           typeof(double),
           typeof(Margin),
           new UIPropertyMetadata(OnBottomPropertyChanged));

        public static double GetBottom(FrameworkElement element)
        {
            return (double)element.GetValue(BottomProperty);
        }

        public static void SetBottom(FrameworkElement element, double value)
        {
            element.SetValue(BottomProperty, value);
        }

        private static void OnBottomPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs args)
        {
            ChangeMarginProperty(dpo, args, Magin.Bottom);
        }

        private static void ChangeMarginProperty(DependencyObject dpo, DependencyPropertyChangedEventArgs args, Magin marginSide)
        {
            var element = dpo as FrameworkElement;

            if (element != null)
            {
                double value = (double)args.NewValue;
                if (true)
                {
                    var margin = element.Margin;

                    switch (marginSide)
                    {
                        case Magin.Left:
                            margin.Left = value;
                            break;
                        case Magin.Top:
                            margin.Top = value;
                            break;
                        case Magin.Right:
                            margin.Right = value;
                            break;
                        case Magin.Bottom:
                            margin.Bottom = value;
                            break;
                    }

                    element.Margin = margin;
                }
            }
        }
    }
}
