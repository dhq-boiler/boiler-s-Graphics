using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NLog;
using Prism.Services.Dialogs;
using SharpDX;

namespace boilersGraphics.Extensions;

public static class Extensions
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /*
     * https://stackoverflow.com/questions/10279092/how-to-get-children-of-a-wpf-container-by-type
     */
    public static T GetChildOfType<T>(this DependencyObject depObj)
        where T : DependencyObject
    {
        if (depObj == null) return null;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            var result = child as T ?? GetChildOfType<T>(child);
            if (result != null) return result;
        }

        return null;
    }

    public static IEnumerable<T> EnumerateChildOfType<T>(this DependencyObject depObj)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            var result = (child as IEnumerable<T> ?? EnumerateChildOfType<T>(child)).ToList();
            if (result != null)
                for (var j = 0; j < result.Count(); j++)
                {
                    var item = result.ElementAt(j);
                    if (item != null)
                        yield return item;
                }

            var result2 = child as T ?? GetChildOfType<T>(child);
            if (result2 != null)
                yield return result2;
        }
    }

    public static Rect BoundsRelativeTo(this FrameworkElement element, Visual relativeTo)
    {
        var slot = LayoutInformation.GetLayoutSlot(element);
        return element.TransformToVisual(relativeTo).TransformBounds(slot);
    }

    //public static Rect BoundsRelativeTo(this FrameworkElement child, Visual parent)
    //{
    //    GeneralTransform gt = child.TransformToAncestor(parent);
    //    return gt.TransformBounds(new Rect(0, 0, child.ActualWidth, child.ActualHeight));
    //}

    /// <summary>
    ///     VisualTreeを親側にたどって、
    ///     指定した型の要素を探す
    /// </summary>
    public static T FindAncestor<T>(this DependencyObject depObj)
        where T : DependencyObject
    {
        while (depObj != null)
        {
            if (depObj is T target) return target;
            depObj = VisualTreeHelper.GetParent(depObj);
        }

        return null;
    }

    public static Visual FindRoot(this Visual depObj)
    {
        var parent = depObj;
        while (parent != null)
        {
            parent = VisualTreeHelper.GetParent(depObj) as Visual;
            if (parent != null)
                depObj = parent;
        }

        return depObj;
    }

    public static async IAsyncEnumerable<T> FindVisualChildrenAsync<T>(this DependencyObject depObj, int digCount = 0)
        where T : DependencyObject
    {
        var sw = new Stopwatch();
        sw.Start();
        logger.Debug($"Begin FindVisualChildren(digCount={digCount})");
        if (depObj is null)
        {
            sw.Stop();
            logger.Debug($"Finish FindVisualChildren(digCount={digCount}) elapsed={sw.ElapsedMilliseconds}ms");
            yield break;
        }

        if (depObj is ContentPresenter cp2)
        {
            //dig ContentPresenter.Content
            var content = cp2.Content as DependencyObject;
            if (content is T) yield return (T)content;
            if (content is not null)
            {
                var list = await FindVisualChildrenAsync<DependencyObject>(content, digCount + 1).ToListAsync();
                foreach (var childOfChild2 in list)
                    if (childOfChild2 != null && childOfChild2 is T)
                        yield return (T)childOfChild2;
            }

            //dig ContentPresenter.ContentTemplate
            if (cp2.ContentTemplate != null)
            {
                var loadObj2 = cp2.ContentTemplate.LoadContent();
                if (loadObj2 is T) yield return (T)loadObj2;
                var list = await FindVisualChildrenAsync<DependencyObject>(loadObj2, digCount + 1).ToListAsync();
                foreach (var childOfChild2 in list)
                    if (childOfChild2 != null && childOfChild2 is T)
                        yield return (T)childOfChild2;
            }
        }
        else if (depObj is DialogWindow dw)
        {
            //dig DialogWindow.Template
            var loadObj = dw.Template.LoadContent();
            if (loadObj is not null)
            {
                var list = await FindVisualChildrenAsync<DependencyObject>(loadObj, digCount + 1).ToListAsync();
                foreach (var childOfChild in list)
                {
                    if (childOfChild is ContentPresenter cp)
                    {
                        //dig ContentPresenter.Content
                        var content = cp.Content as DependencyObject;
                        if (content is T) yield return (T)content;
                        if (content is not null)
                        {
                            var list2 = await FindVisualChildrenAsync<DependencyObject>(content, digCount + 1)
                                .ToListAsync();
                            foreach (var childOfChild2 in list2)
                                if (childOfChild2 != null && childOfChild2 is T)
                                    yield return (T)childOfChild2;
                        }

                        //dig ContentPresenter.ContentTemplate
                        if (cp.ContentTemplate != null)
                        {
                            var loadObj2 = cp.ContentTemplate.LoadContent();
                            if (loadObj2 is T) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = await FindVisualChildrenAsync<DependencyObject>(loadObj2, digCount + 1)
                                    .ToListAsync();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T)
                                        yield return (T)childOfChild2;
                            }
                        }
                    }

                    if (childOfChild is Control c)
                        //dig Control.Template
                        if (c.Template != null)
                        {
                            var loadObj2 = c.Template.LoadContent();
                            if (loadObj2 is T) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = await FindVisualChildrenAsync<DependencyObject>(loadObj2, digCount + 1)
                                    .ToListAsync();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T)
                                        yield return (T)childOfChild2;
                            }
                        }

                    if (childOfChild is ContentControl cc)
                    {
                        //dig ContentControl.Template
                        if (cc.Template != null)
                        {
                            var loadObj2 = cc.Template.LoadContent();
                            if (loadObj2 is T) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = await FindVisualChildrenAsync<DependencyObject>(loadObj2, digCount + 1)
                                    .ToListAsync();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T)
                                        yield return (T)childOfChild2;
                            }
                        }

                        //dig ContentControl.ContentTemplate
                        if (cc.ContentTemplate != null)
                        {
                            var loadObj2 = cc.ContentTemplate.LoadContent();
                            if (loadObj2 is T) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = await FindVisualChildrenAsync<DependencyObject>(loadObj2, digCount + 1)
                                    .ToListAsync();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T)
                                        yield return (T)childOfChild2;
                            }
                        }
                    }

                    if (childOfChild != null && childOfChild is T) yield return (T)childOfChild;
                }
            }

            //dig Prism.Services.Dialogs.DialogWindow.ContentTemplate
            if (dw.ContentTemplate != null)
            {
                loadObj = dw.ContentTemplate.LoadContent();
                if (loadObj is not null)
                {
                    var list2 = await FindVisualChildrenAsync<DependencyObject>(loadObj, digCount + 1).ToListAsync();
                    foreach (var childOfChild in list2)
                        if (childOfChild != null && childOfChild is T)
                            yield return (T)childOfChild;
                }
            }
        }

        //dig Children
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            if (child != null && child is T)
            {
                yield return (T)child;

                var list = await FindVisualChildrenAsync<T>(child, digCount + 1).ToListAsync();
                foreach (var childOfChild in list) yield return childOfChild;
            }

            if (child != null && child is ContentPresenter cp)
            {
                //dig ContentPresenter.Content
                var content = cp.Content as DependencyObject;
                if (content is T) yield return (T)content;
                if (content is not null)
                {
                    var list = await FindVisualChildrenAsync<DependencyObject>(content, digCount + 1).ToListAsync();
                    foreach (var childOfChild2 in list)
                        if (childOfChild2 != null && childOfChild2 is T)
                            yield return (T)childOfChild2;
                }

                //dig ContentPresenter.ContentTemplate
                if (cp.ContentTemplate != null)
                {
                    var dependencyObject = cp.ContentTemplate.LoadContent();
                    if (dependencyObject is T) yield return (T)dependencyObject;
                    var list = await FindVisualChildrenAsync<T>(dependencyObject, digCount + 1).ToListAsync();
                    foreach (var childOfChild in list) yield return childOfChild;
                }
            }
        }

        sw.Stop();
        logger.Debug($"Finish FindVisualChildren(digCount={digCount}) elapsed={sw.ElapsedMilliseconds}ms");
    }

    public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj, int digCount = 0)
        where T : DependencyObject
    {
        var sw = new Stopwatch();
        sw.Start();
        logger.Debug($"Begin FindVisualChildren(digCount={digCount})");
        if (depObj is null)
        {
            sw.Stop();
            logger.Debug($"Finish FindVisualChildren(digCount={digCount}) elapsed={sw.ElapsedMilliseconds}ms");
            yield break;
        }

        if (depObj is ContentPresenter cp2)
        {
            //dig ContentPresenter.Content
            var content = cp2.Content as DependencyObject;
            if (content is T) yield return (T)content;
            if (content is not null)
            {
                var list = FindVisualChildren<DependencyObject>(content, digCount + 1).ToList();
                foreach (var childOfChild2 in list)
                    if (childOfChild2 != null && childOfChild2 is T)
                        yield return (T)childOfChild2;
            }

            //dig ContentPresenter.ContentTemplate
            if (cp2.ContentTemplate != null)
            {
                var loadObj2 = cp2.ContentTemplate.LoadContent();
                if (loadObj2 is T) yield return (T)loadObj2;
                var list = FindVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                foreach (var p in list)
                {
                    foreach (var o in EnumVisualChildren<T>(p)) yield return o;
                }
            }
        }
        else if (depObj is DialogWindow dw)
        {
            //dig DialogWindow.Template
            var loadObj = dw.Template.LoadContent();
            if (loadObj is not null)
            {
                var list = FindVisualChildren<DependencyObject>(loadObj, digCount + 1).ToList();
                foreach (var childOfChild in list)
                {
                    if (childOfChild is ContentPresenter cp)
                    {
                        //dig ContentPresenter.Content
                        var content = cp.Content as DependencyObject;
                        if (content is T) yield return (T)content;
                        if (content is not null)
                        {
                            var list2 = FindVisualChildren<DependencyObject>(content, digCount + 1).ToList();
                            foreach (var childOfChild2 in list2)
                                if (childOfChild2 != null && childOfChild2 is T)
                                    yield return (T)childOfChild2;
                        }

                        //dig ContentPresenter.ContentTemplate
                        if (cp.ContentTemplate != null)
                        {
                            var loadObj2 = cp.ContentTemplate.LoadContent();
                            if (loadObj2 is T) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = FindVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T)
                                        yield return (T)childOfChild2;
                            }
                        }
                    }

                    if (childOfChild is Control c)
                        //dig Control.Template
                        if (c.Template != null)
                        {
                            var loadObj2 = c.Template.LoadContent();
                            if (loadObj2 is T) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = FindVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T)
                                        yield return (T)childOfChild2;
                            }
                        }

                    if (childOfChild is ContentControl cc)
                    {
                        //dig ContentControl.Template
                        if (cc.Template != null)
                        {
                            var loadObj2 = cc.Template.LoadContent();
                            if (loadObj2 is T) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = FindVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T)
                                        yield return (T)childOfChild2;
                            }
                        }

                        //dig ContentControl.ContentTemplate
                        if (cc.ContentTemplate != null)
                        {
                            var loadObj2 = cc.ContentTemplate.LoadContent();
                            if (loadObj2 is T) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = FindVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T)
                                        yield return (T)childOfChild2;
                            }
                        }
                    }

                    if (childOfChild != null && childOfChild is T) yield return (T)childOfChild;
                }
            }

            //dig Prism.Services.Dialogs.DialogWindow.ContentTemplate
            if (dw.ContentTemplate != null)
            {
                loadObj = dw.ContentTemplate.LoadContent();
                if (loadObj is not null)
                {
                    var list2 = FindVisualChildren<DependencyObject>(loadObj, digCount + 1).ToList();
                    foreach (var childOfChild in list2)
                        if (childOfChild != null && childOfChild is T)
                            yield return (T)childOfChild;
                }
            }
        }

        //dig Children
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            if (child != null && child is T)
            {
                yield return (T)child;

                var list = FindVisualChildren<T>(child, digCount + 1).ToList();
                foreach (var childOfChild in list) yield return childOfChild;
            }
            else if (child != null && child is ContentPresenter cp)
            {
                //dig ContentPresenter.Content
                var content = cp.Content as DependencyObject;
                if (content is T) yield return (T)content;
                if (content is not null)
                {
                    var list = FindVisualChildren<DependencyObject>(content, digCount + 1).ToList();
                    foreach (var childOfChild2 in list)
                        if (childOfChild2 != null && childOfChild2 is T)
                            yield return (T)childOfChild2;
                }

                //dig ContentPresenter.ContentTemplate
                if (cp.ContentTemplate != null)
                {
                    var dependencyObject = cp.ContentTemplate.LoadContent();
                    if (dependencyObject is T) yield return (T)dependencyObject;
                    var list = FindVisualChildren<T>(dependencyObject, digCount + 1).ToList();
                    foreach (var childOfChild in list) yield return childOfChild;
                }
            }
            else if (child != null)
            {
                var list = FindVisualChildren<T>(child, digCount + 1).ToList();
                foreach (var childOfChild in list) yield return childOfChild;
            }
        }

        sw.Stop();
        logger.Debug($"Finish FindVisualChildren(digCount={digCount}) elapsed={sw.ElapsedMilliseconds}ms");
    }

    public static IEnumerable<T> EnumVisualChildren<T>(this DependencyObject pp, int digCount = 0) where T : DependencyObject
    {
        if (pp is Control _control)
        {
            if (_control.Template is not null)
            {
                //dig Control.Template
                var templateContent = _control.Template.LoadContent();
                var list = EnumVisualChildren<DependencyObject>(templateContent, digCount + 1).ToList();
                foreach (var p in list)
                {
                    foreach (var o in EnumVisualChildren<T>(p))
                    {
                        if (o is not ContentPresenter && o is not Control)
                        {
                            yield return o;
                        }
                    }
                }
            }

            if (_control is ItemsControl ic)
            {
                if (ic.ItemTemplate is not null)
                {
                    var itemTemplateContent = ic.ItemTemplate.LoadContent();
                    var list = EnumVisualChildren<DependencyObject>(itemTemplateContent, digCount + 1).ToList();
                    foreach (var p in list)
                    {
                        foreach (var o in EnumVisualChildren<T>(p))
                        {
                            if (o is not ContentPresenter && o is not Control)
                            {
                                yield return o;
                            }
                        }
                    }
                }
            }
        }

        if (pp is ContentPresenter cp2)
        {
            //dig ContentPresenter.Content
            var content = cp2.Content as DependencyObject;
            if (content is T && content is not ContentPresenter) yield return (T)content;
            if (content is not null)
            {
                var list = EnumVisualChildren<DependencyObject>(content, digCount + 1).ToList();
                foreach (var childOfChild2 in list)
                    if (childOfChild2 != null && childOfChild2 is T && childOfChild2 is not ContentPresenter && childOfChild2 is not Control)
                        yield return (T)childOfChild2;
            }

            //dig ContentPresenter.ContentTemplate
            if (cp2.ContentTemplate != null)
            {
                var loadObj2 = cp2.ContentTemplate.LoadContent();
                if (loadObj2 is T && loadObj2 is not ContentPresenter) yield return (T)loadObj2;
                var list = EnumVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                foreach (var p in list)
                {
                    foreach (var o in EnumVisualChildren<T>(p))
                    {
                        if (o is not ContentPresenter && o is not Control)
                        {
                            yield return o;
                        }
                    }
                }
            }
        }

        if (pp is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is T dependencyObject)
                {
                    if (dependencyObject is not ContentPresenter)
                    {
                        yield return dependencyObject;
                    }

                    foreach (var item in EnumVisualChildren<T>(dependencyObject))
                    {
                        if (item is not ContentPresenter && item is not Control)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        if (pp is DialogWindow dw)
        {
            //dig DialogWindow.Template
            var loadObj = dw.Template.LoadContent();
            if (loadObj is not null)
            {
                var list = EnumVisualChildren<DependencyObject>(loadObj, digCount + 1).ToList();
                foreach (var childOfChild in list)
                {
                    if (childOfChild is ContentPresenter cp)
                    {
                        //dig ContentPresenter.Content
                        var content = cp.Content as DependencyObject;
                        if (content is T && content is not ContentPresenter) yield return (T)content;
                        if (content is not null)
                        {
                            var list2 = EnumVisualChildren<DependencyObject>(content, digCount + 1).ToList();
                            foreach (var childOfChild2 in list2)
                                if (childOfChild2 != null && childOfChild2 is T && childOfChild2 is not ContentPresenter && childOfChild2 is not Control)
                                    yield return (T)childOfChild2;
                        }

                        //dig ContentPresenter.ContentTemplate
                        if (cp.ContentTemplate != null)
                        {
                            var loadObj2 = cp.ContentTemplate.LoadContent();
                            if (loadObj2 is T && loadObj2 is not ContentPresenter && loadObj2 is not Control) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = EnumVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T && childOfChild2 is not ContentPresenter && childOfChild2 is not Control)
                                        yield return (T)childOfChild2;
                            }
                        }
                    }

                    if (childOfChild is Control c)
                        //dig Control.Template
                        if (c.Template != null)
                        {
                            var loadObj2 = c.Template.LoadContent();
                            if (loadObj2 is T && loadObj2 is not ContentPresenter && loadObj2 is not Control) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = EnumVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T && childOfChild2 is not ContentPresenter && childOfChild2 is not Control)
                                        yield return (T)childOfChild2;
                            }
                        }

                    if (childOfChild is ContentControl cc)
                    {
                        //dig ContentControl.Template
                        if (cc.Template != null)
                        {
                            var loadObj2 = cc.Template.LoadContent();
                            if (loadObj2 is T && loadObj2 is not ContentPresenter && loadObj2 is not Control) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = EnumVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T && childOfChild2 is not ContentPresenter && childOfChild2 is not Control)
                                        yield return (T)childOfChild2;
                            }
                        }

                        //dig ContentControl.ContentTemplate
                        if (cc.ContentTemplate != null)
                        {
                            var loadObj2 = cc.ContentTemplate.LoadContent();
                            if (loadObj2 is T && loadObj2 is not ContentPresenter && loadObj2 is not Control) yield return (T)loadObj2;
                            if (loadObj2 is not null)
                            {
                                var list2 = EnumVisualChildren<DependencyObject>(loadObj2, digCount + 1).ToList();
                                foreach (var childOfChild2 in list2)
                                    if (childOfChild2 != null && childOfChild2 is T && childOfChild2 is not ContentPresenter && childOfChild2 is not Control)
                                        yield return (T)childOfChild2;
                            }
                        }
                    }

                    if (childOfChild != null && childOfChild is T && childOfChild is not ContentPresenter && childOfChild is not Control) yield return (T)childOfChild;
                }
            }

            //dig Prism.Services.Dialogs.DialogWindow.ContentTemplate
            if (dw.ContentTemplate != null)
            {
                loadObj = dw.ContentTemplate.LoadContent();
                if (loadObj is not null)
                {
                    var list2 = EnumVisualChildren<DependencyObject>(loadObj, digCount + 1).ToList();
                    foreach (var childOfChild in list2)
                        if (childOfChild != null && childOfChild is T && childOfChild is not ContentPresenter && childOfChild is not Control)
                            yield return (T)childOfChild;
                }
            }
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(pp); i++)
        {
            var child = VisualTreeHelper.GetChild(pp, i);
            if (child != null && child is T)
            {
                yield return (T)child;

                var list = EnumVisualChildren<T>(child, digCount + 1).ToList();
                foreach (var childOfChild in list) yield return childOfChild;
            }
            else if (child != null && child is ContentPresenter cp)
            {
                //dig ContentPresenter.Content
                var content = cp.Content as DependencyObject;
                if (content is T) yield return (T)content;
                if (content is not null)
                {
                    var list = EnumVisualChildren<DependencyObject>(content, digCount + 1).ToList();
                    foreach (var childOfChild2 in list)
                        if (childOfChild2 != null && childOfChild2 is T)
                            yield return (T)childOfChild2;
                }

                //dig ContentPresenter.ContentTemplate
                if (cp.ContentTemplate != null)
                {
                    var dependencyObject = cp.ContentTemplate.LoadContent();
                    if (dependencyObject is T) yield return (T)dependencyObject;
                    var list = EnumVisualChildren<T>(dependencyObject, digCount + 1).ToList();
                    foreach (var childOfChild in list) yield return childOfChild;
                }
            }
            else if (child != null)
            {
                var list = EnumVisualChildren<T>(child, digCount + 1).ToList();
                foreach (var childOfChild in list) yield return childOfChild;
            }
        }

        if (pp is IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                var list = EnumVisualChildren<T>(item, digCount + 1).ToList();
                foreach (var child in list)
                {
                    yield return child;
                }
            }
        }

        //foreach (var o in EnumVisualChildren<T>(pp)) yield return o;
        if (pp is T)
            yield return (T)pp;
    }

    public static IEnumerable<FrameworkElement> GetChildren(this FrameworkElement parent)
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;

            var children = GetChildren(child).ToList();
            foreach (var child2 in children)
                yield return child2;

            if (child != null)
                yield return child;
        }
    }

    public static T FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
    {
        // Confirm parent and childName are valid. 
        if (parent == null) return null;

        T foundChild = null;

        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            var childType = child as T;
            if (childType == null)
            {
                // recursively drill down the tree
                foundChild = FindChild<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child. 
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                var frameworkElement = child as FrameworkElement;
                // If the child's name is set for search
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    // if the child's name is of the request name
                    foundChild = (T)child;
                    break;
                }
            }
            else
            {
                // child element found.
                foundChild = (T)child;
                break;
            }
        }

        return foundChild;
    }

    public static async IAsyncEnumerable<T> GetCorrespondingViewsAsync<T>(this FrameworkElement parent,
        object dataContext, bool parentInclude = false)
        where T : FrameworkElement
    {
        if (parentInclude && parent.DataContext == dataContext)
            if (parent is T)
                yield return parent as T;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            var result = default(IEnumerable<T>);
            if (child is IEnumerable<T> enumerable)
                result = enumerable;
            else
                result = await FindVisualChildrenAsync<T>(child).ToListAsync();
            //var result = (child as IEnumerable<T>) ?? FindVisualChildren<T>(child);
            if (result != null)
                foreach (var item in result)
                    if (item != null && item.DataContext == dataContext)
                        yield return item;
            var result2 = child as T;
            if (result2 is not null && result2.DataContext == dataContext)
            {
                yield return result2;
            }
            else
            {
                var result3 = await FindVisualChildrenAsync<T>(result2).ToListAsync();
                if (result3 is not null)
                    foreach (var item in result3)
                        if (item is not null && item.DataContext == dataContext)
                            yield return item;
            }
            //var result2 = (child as T) ?? GetChildOfType<T>(child);
            //if (result2 != null && result2.DataContext == dataContext)
            //    yield return result2;
        }
    }

    public static IEnumerable<T> GetDescendantsViews<T>(this FrameworkElement parent, bool parentInclude = false)
        where T : FrameworkElement
    {
        if (parentInclude)
            if (parent is T)
                yield return parent as T;

        //for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        foreach (var child in EnumVisualChildren<T>(parent, 1))
        {
            //var child = VisualTreeHelper.GetChild(parent, i);

            var result = default(IEnumerable<T>);
            if (child is IEnumerable<T> enumerable)
                result = enumerable;
            else
                result = EnumVisualChildren<T>(child).ToList();
            if (result != null)
                foreach (var item in result)
                    if (item != null)
                        yield return item;
            var result2 = child as T;
            if (result2 is not null)
            {
                yield return result2;
            }
            else
            {
                var result3 = EnumVisualChildren<T>(result2).ToList();
                if (result3 is not null)
                    foreach (var item in result3)
                        if (item is not null)
                            yield return item;
            }
        }
    }

    public static IEnumerable<T> GetCorrespondingViews<T>(this FrameworkElement parent, object dataContext,
        bool parentInclude = false)
        where T : FrameworkElement
    {
        if (parentInclude && parent.DataContext == dataContext)
            if (parent is T)
                yield return parent as T;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            var result = default(IEnumerable<T>);
            if (child is IEnumerable<T> enumerable)
                result = enumerable;
            else
                result = FindVisualChildren<T>(child).ToList();
            //var result = (child as IEnumerable<T>) ?? FindVisualChildren<T>(child);
            if (result != null)
                foreach (var item in result)
                    if (item != null && item.DataContext == dataContext)
                        yield return item;
            var result2 = child as T;
            if (result2 is not null && result2.DataContext == dataContext)
            {
                yield return result2;
            }
            else
            {
                var result3 = FindVisualChildren<T>(result2).ToList();
                if (result3 is not null)
                    foreach (var item in result3)
                        if (item is not null && item.DataContext == dataContext)
                            yield return item;
            }
        }
    }

    public static IEnumerable<FrameworkElement> GetViewsHavingDataContext(this FrameworkElement parent,
        bool parentInclude = false)
    {
        if (parentInclude && !(parent.DataContext is null))
            if (parent is FrameworkElement)
                yield return parent;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = child as IEnumerable<FrameworkElement> ?? EnumerateChildOfType<FrameworkElement>(child);
            if (result != null)
                foreach (var item in result)
                    if (item != null && !(item.DataContext is null))
                        yield return item;
            var result2 = child as FrameworkElement ?? GetChildOfType<FrameworkElement>(child);
            if (result2 != null && !(result2.DataContext is null))
                yield return result2;
        }
    }

    public static T GetParentOfType<T>(this DependencyObject obj)
        where T : DependencyObject
    {
        if (obj == null) return null;

        while (obj != null && !(obj is T)) obj = VisualTreeHelper.GetParent(obj);

        if (obj == null) return null;

        return (T)obj;
    }

    public static DependencyObject GetParentOfType(this DependencyObject obj, string name)
    {
        if (obj == null) return null;

        while (obj != null && obj is FrameworkElement && !(obj as FrameworkElement).Name.Equals(name))
            obj = VisualTreeHelper.GetParent(obj);

        if (obj == null) return null;

        return obj;
    }

    public static double DpiXFactor(this Visual visual)
    {
        var source = PresentationSource.FromVisual(visual);
        if (source != null)
            return source.CompositionTarget.TransformToDevice.M11;
        return 1.0;
    }

    public static double DpiYFactor(this Visual visual)
    {
        var source = PresentationSource.FromVisual(visual);
        if (source != null)
            return source.CompositionTarget.TransformToDevice.M22;
        return 1.0;
    }

    public static double SumWidthExceptInfinity(this IEnumerable<PathGeometry> geometries, GlyphTypeface glyphTypeface,
        int fontSize)
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

    public static double SumHeightExceptInfinity(this IEnumerable<PathGeometry> geometries, GlyphTypeface glyphTypeface,
        int fontSize)
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
        var ret = 0d;
        foreach (var @char in str)
        {
            ushort glyphIndex;
            glyphTypeface.CharacterToGlyphMap.TryGetValue(@char, out glyphIndex);
            var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, fontSize);
            var pg = geometry.GetOutlinedPathGeometry();
            ret += pg.Bounds.Width;
        }

        return ret / str.Count();
    }

    public static double GetAvgHeight(this GlyphTypeface glyphTypeface, int fontSize)
    {
        const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var ret = 0d;
        foreach (var @char in str)
        {
            ushort glyphIndex;
            glyphTypeface.CharacterToGlyphMap.TryGetValue(@char, out glyphIndex);
            var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, fontSize);
            var pg = geometry.GetOutlinedPathGeometry();
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

    public static IEnumerable<SelectableDesignerItemViewModelBase> WithPickupChildren(
        this IEnumerable<SelectableDesignerItemViewModelBase> selected,
        IEnumerable<SelectableDesignerItemViewModelBase> all)
    {
        foreach (var item in selected)
        {
            if (item is GroupItemViewModel)
            {
                var group = item as GroupItemViewModel;
                var id = group.ID;
                var idmatch = all.Where(x => x.ParentID == id);
                foreach (var idmatchitem in idmatch) yield return idmatchitem;
            }

            yield return item;
        }
    }

    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException("source");
        return new ObservableCollection<T>(source);
    }

    public static IEnumerable<SelectableDesignerItemViewModelBase> Items(this ObservableCollection<Layer> layers)
    {
        return layers.SelectMany(x => x.Children).Select(x => (x as LayerItem).Item.Value);
    }

    public static T GetParent<T>(this DependencyObject obj)
    {
        var parent = VisualTreeHelper.GetParent(obj);
        return parent switch
        {
            null => default,
            T ret => ret,
            _ => parent.GetParent<T>()
        };
    }

    public static IEnumerable<DependencyObject> Children(this DependencyObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException("obj");

        var count = VisualTreeHelper.GetChildrenCount(obj);
        if (count == 0)
            yield break;

        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            yield return child;
        }
    }

    public static IEnumerable<DependencyObject> Descendants(this DependencyObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException("obj");

        foreach (var child in obj.Children())
        {
            yield return child;
            foreach (var grandChild in child.Descendants())
                yield return grandChild;
        }
    }

    public static IEnumerable<T> Children<T>(this DependencyObject obj)
        where T : DependencyObject
    {
        return obj.Children().OfType<T>();
    }

    //--- 特定の型の子孫要素を取得
    public static IEnumerable<T> Descendants<T>(this DependencyObject obj)
        where T : DependencyObject
    {
        return obj.Descendants().OfType<T>();
    }

    //https://stackoverflow.com/questions/41608665/linq-recursive-parent-child
    public static IEnumerable<T2> SelectRecursive<T1, T2>(this IEnumerable<T1> source,
        Func<T2, IEnumerable<T2>> selector) where T1 : class where T2 : class
    {
        foreach (var parent in source)
        {
            yield return parent as T2;

            var children = selector(parent as T2);
            foreach (var child in SelectRecursive(children, selector))
                yield return child;
        }
    }

    public static bool HasAsAncestor(this LayerTreeViewItemBase layerItem, LayerTreeViewItemBase ancestor)
    {
        var temp = layerItem;
        while (temp.Parent.Value != null)
        {
            if (temp == ancestor)
                return true;
            temp = temp.Parent.Value;
        }

        return false;
    }

    //http://stackoverflow.com/questions/7319952/how-to-get-listbox-itemspanel-in-code-behind
    public static T GetVisualChild<T>(this DependencyObject parent) where T : Visual
    {
        var child = default(T);

        var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < numVisuals; i++)
        {
            var v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;
            if (child == null) child = GetVisualChild<T>(v);
            if (child != null) break;
        }

        return child;
    }
}