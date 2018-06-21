using System.Windows;
using System.Windows.Media;

namespace grapher.Extensions
{
    internal static class Extensions
    {
        /*
         * https://stackoverflow.com/questions/10279092/how-to-get-children-of-a-wpf-container-by-type
         */
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static T GetParentOfType<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            if (obj == null) return null;

            while (!(obj is T))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (obj == null) return null;

            return (T)obj;
        }

        public static double DpiXFactor(this Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            else
            {
                return 1.0;
            }
        }

        public static double DpiYFactor(this Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                return source.CompositionTarget.TransformToDevice.M22;
            }
            else
            {
                return 1.0;
            }
        }
    }
}
