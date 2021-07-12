using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Extensions
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

        public static IEnumerable<T> EnumerateChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as IEnumerable<T>) ?? EnumerateChildOfType<T>(child);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        if (item != null)
                            yield return item;
                    }
                }
                var result2 = (child as T) ?? GetChildOfType<T>(child);
                if (result2 != null)
                    yield return result2;
            }
        }

        public static IEnumerable<T> GetCorrespondingViews<T>(this FrameworkElement parent, object dataContext, bool parentInclude = false)
            where T : FrameworkElement
        {
            if (parent.DataContext == dataContext)
            {
                if (parent is T)
                    yield return parent as T;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                var result = (child as IEnumerable<T>) ?? EnumerateChildOfType<T>(child);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        if (item != null && item.DataContext == dataContext)
                            yield return item;
                    }
                }
                var result2 = (child as T) ?? GetChildOfType<T>(child);
                if (result2 != null && result2.DataContext == dataContext)
                    yield return result2;
            }
        }

        public static T GetParentOfType<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            if (obj == null) return null;

            while (obj != null && !(obj is T))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (obj == null) return null;

            return (T)obj;
        }

        public static DependencyObject GetParentOfType(this DependencyObject obj, string name)
        {
            if (obj == null) return null;

            while (obj != null && (obj is FrameworkElement && !(obj as FrameworkElement).Name.Equals(name)))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (obj == null) return null;

            return obj;
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

        public static double SumWidthExceptInfinity(this IEnumerable<PathGeometry> geometries, GlyphTypeface glyphTypeface, int fontSize)
        {
            double ret = 0;
            foreach (var pg in geometries)
            {
                var value = pg.Bounds.Width;
                if (double.IsInfinity(value))
                {
                    var spaceWidth = glyphTypeface.GetAvgWidth(fontSize);
                    ret += spaceWidth;
                }
                else
                {
                    ret += value + 5;
                }
            }
            return ret;
        }

        public static double SumHeightExceptInfinity(this IEnumerable<PathGeometry> geometries, GlyphTypeface glyphTypeface, int fontSize)
        {
            double ret = 0;
            foreach (var pg in geometries)
            {
                var value = pg.Bounds.Height;
                if (double.IsInfinity(value))
                {
                    var spaceHeight = glyphTypeface.GetAvgHeight(fontSize);
                    ret += spaceHeight;
                }
                else
                {
                    ret += value + 5;
                }
            }
            return ret;
        }

        public static double GetAvgWidth(this GlyphTypeface glyphTypeface, int fontSize)
        {
            const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            double ret = 0d;
            foreach (var @char in str)
            {
                ushort glyphIndex;
                glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, fontSize);
                PathGeometry pg = geometry.GetOutlinedPathGeometry();
                ret += pg.Bounds.Width;
            }
            return ret / str.Count();
        }

        public static double GetAvgHeight(this GlyphTypeface glyphTypeface, int fontSize)
        {
            const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            double ret = 0d;
            foreach (var @char in str)
            {
                ushort glyphIndex;
                glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, fontSize);
                PathGeometry pg = geometry.GetOutlinedPathGeometry();
                ret += pg.Bounds.Height;
            }
            return ret / str.Count();
        }

        public static Point Shift(this Point target, double x, double y)
        {
            return new Point(target.X + x, target.Y + y);
        }

        public static Point Multiple(this Point target, double x, double y)
        {
            return new Point(target.X * x, target.Y * y);
        }
    }
}
