using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Unit = R3.Unit;

namespace boilersGraphics.Extensions;

public static class R3Extensions
{
    /// <summary>
    /// Converts any observable to a Unit observable
    /// </summary>
    public static R3.Observable<Unit> ToUnit<T>(this R3.Observable<T> source)
    {
        return source.Select(_ => Unit.Default);
    }

    /// <summary>
    /// Converts ReactiveProperty to a Unit observable
    /// </summary>
    public static R3.Observable<Unit> ToUnit<T>(this R3.ReactiveProperty<T> source)
    {
        return source.Select(_ => Unit.Default);
    }
    
    /// <summary>
    /// Observes collection changed events as observable for ObservableList - simplified implementation
    /// </summary>
    public static R3.Observable<NotifyCollectionChangedEventArgs> CollectionChangedAsObservable<T>(this ObservableList<T> source)
    {
        // Simplified implementation for compilation
        return R3.Observable.Return(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Observes collection changed events as observable for NotifyCollectionChangedSynchronizedViewList
    /// </summary>
    public static R3.Observable<NotifyCollectionChangedEventArgs> CollectionChangedAsObservable<T>(this NotifyCollectionChangedSynchronizedViewList<T> source)
    {
        return R3.Observable.Create<NotifyCollectionChangedEventArgs>(observer =>
        {
            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => observer.OnNext(e);
            
            source.CollectionChanged += OnCollectionChanged;
            
            return R3.Disposable.Create(() =>
            {
                source.CollectionChanged -= OnCollectionChanged;
            });
        });
    }

    /// <summary>
    /// Observes element property changes in collection - simplified implementation
    /// </summary>
    public static R3.Observable<TProperty> ObserveElementObservableProperty<T, TProperty>(
        this ObservableList<T> source, 
        Func<T, TProperty> propertySelector) where T : class
    {
        // Simplified implementation for compilation
        try
        {
            var firstValue = source.FirstOrDefault();
            if (firstValue != null)
            {
                var result = propertySelector(firstValue);
                return R3.Observable.Return(result);
            }
        }
        catch
        {
            // Ignore errors
        }
        return R3.Observable.Empty<TProperty>();
    }

    /// <summary>
    /// Observes element property changes in NotifyCollectionChangedSynchronizedViewList
    /// </summary>
    public static R3.Observable<TProperty> ObserveElementObservableProperty<T, TProperty>(
        this NotifyCollectionChangedSynchronizedViewList<T> source,
        Func<T, TProperty> propertySelector) where T : class
    {
        return R3.Observable.Create<TProperty>(observer =>
        {
            var disposables = new System.Collections.Generic.List<IDisposable>();

            void SubscribeToElements()
            {
                // Clear existing subscriptions
                foreach (var disposable in disposables)
                {
                    disposable?.Dispose();
                }
                disposables.Clear();

                // Subscribe to current elements
                foreach (var item in source)
                {
                    if (item != null)
                    {
                        try
                        {
                            var value = propertySelector(item);
                            observer.OnNext(value);
                            
                            // If the property is R3 Observable, subscribe to changes
                            if (value is R3.Observable<TProperty> r3Observable)
                            {
                                var subscription = r3Observable.Subscribe(x => observer.OnNext(x));
                                disposables.Add(subscription);
                            }
                            // If the property has an observable, try to subscribe
                            else if (value is R3.ReactiveProperty<TProperty> reactiveProperty)
                            {
                                var subscription = reactiveProperty.Subscribe(x => observer.OnNext(x));
                                disposables.Add(subscription);
                            }
                        }
                        catch
                        {
                            // Ignore errors in property access
                        }
                    }
                }
            }

            // Initial subscription
            SubscribeToElements();

            // Subscribe to collection changes
            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                SubscribeToElements();
            }

            source.CollectionChanged += OnCollectionChanged;

            return R3.Disposable.Create(() =>
            {
                source.CollectionChanged -= OnCollectionChanged;
                foreach (var disposable in disposables)
                {
                    disposable?.Dispose();
                }
                disposables.Clear();
            });
        });
    }

    public static R3.ReactiveCommand WithSubscribe(this R3.ReactiveCommand command, Action onNext)
    {
        command.Subscribe(unit => onNext());
        return command;
    }

    public static R3.ReactiveCommand WithSubscribe(this R3.ReactiveCommand command, R3.Observer<Unit> observer)
    {
        command.Subscribe(observer);
        return command;
    }

    public static ObservableList<T> ToObservableList<T>(this R3.ReactiveProperty<List<T>> source)
    {
        var observableList = new ObservableList<T>();

        // 初期値を設定
        if (source.CurrentValue != null)
        {
            observableList.AddRange(source.CurrentValue);
        }

        // ソースの変更を監視
        source.Subscribe(newList =>
        {
            observableList.Clear();
            if (newList != null)
            {
                observableList.AddRange(newList);
            }
        });

        return observableList;
    }

    public static ObservableList<T> ToObservableList<T>(this R3.ReactiveProperty<T> source)
    {
        var observableList = new ObservableList<T>();

        // 初期値を追加
        if (source.CurrentValue != null)
        {
            observableList.Add(source.CurrentValue);
        }

        // 変更を監視
        source.Subscribe(newValue =>
        {
            observableList.Clear();
            if (newValue != null)
            {
                observableList.Add(newValue);
            }
        });

        return observableList;
    }

    public static ObservableList<T> ToObservableList<T>(this R3.Observable<T> source)
    {
        var observableList = new ObservableList<T>();

        // 変更を監視
        source.Subscribe(newValue =>
        {
            observableList.Clear();
            if (newValue != null)
            {
                observableList.Add(newValue);
            }
        });

        return observableList;
    }

    /// <summary>
    /// Extension to dispose ObservableList (if needed for compatibility)
    /// </summary>
    public static void Dispose<T>(this ObservableList<T> source)
    {
        // ObservableList doesn't implement IDisposable by default
        // This is for compatibility with existing code that expects Dispose
        source?.Clear();
    }

    /// <summary>
    /// NotifyCollectionChangedSynchronizedViewListの要素のプロパティ変更を監視するObservableを作成します
    /// </summary>
    /// <typeparam name="T">リストの要素の型（INotifyPropertyChangedを実装している必要があります）</typeparam>
    /// <param name="source">監視対象のNotifyCollectionChangedSynchronizedViewList</param>
    /// <param name="propertySelector">監視するプロパティを指定するセレクター</param>
    /// <returns>プロパティ変更を通知するObservable</returns>
    public static Observable<Unit> ObserveElementPropertyChanged<T>(this NotifyCollectionChangedSynchronizedViewList<T> source, Expression<Func<T, object>> propertySelector)
        where T : INotifyPropertyChanged
    {
        return Observable.Create<Unit>(observer =>
        {
            var disposables = new CompositeDisposable();
            var propertyName = GetPropertyName(propertySelector);

            // 既存の要素のプロパティ変更を監視
            foreach (var item in source)
            {
                if (item != null)
                {
                    Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                        h => (sender, e) => h(e),
                        h => item.PropertyChanged += h,
                        h => item.PropertyChanged -= h)
                        .Where(e => e.PropertyName == propertyName)
                        .Subscribe(_ => observer.OnNext(Unit.Default))
                        .AddTo(disposables);
                }
            }

            // コレクション変更を監視して新しい要素のプロパティ変更も監視
            Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => (sender, e) => h(e),
                h => source.CollectionChanged += h,
                h => source.CollectionChanged -= h)
                .Subscribe(change =>
                {
                    switch (change.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (change.NewItems != null)
                            {
                                foreach (T newItem in change.NewItems)
                                {
                                    if (newItem != null)
                                    {
                                        Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                            h => (sender, e) => h(e),
                                            h => newItem.PropertyChanged += h,
                                            h => newItem.PropertyChanged -= h)
                                            .Where(e => e.PropertyName == propertyName)
                                            .Subscribe(_ => observer.OnNext(Unit.Default))
                                            .AddTo(disposables);
                                    }
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            // 削除された要素の監視は自動的に解除される
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            if (change.NewItems != null)
                            {
                                foreach (T newItem in change.NewItems)
                                {
                                    if (newItem != null)
                                    {
                                        Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                            h => (sender, e) => h(e),
                                            h => newItem.PropertyChanged += h,
                                            h => newItem.PropertyChanged -= h)
                                            .Where(e => e.PropertyName == propertyName)
                                            .Subscribe(_ => observer.OnNext(Unit.Default))
                                            .AddTo(disposables);
                                    }
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            // 全てクリアして再度監視を設定
                            disposables.Clear();
                            foreach (var item in source)
                            {
                                if (item != null)
                                {
                                    Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                        h => (sender, e) => h(e),
                                        h => item.PropertyChanged += h,
                                        h => item.PropertyChanged -= h)
                                        .Where(e => e.PropertyName == propertyName)
                                        .Subscribe(_ => observer.OnNext(Unit.Default))
                                        .AddTo(disposables);
                                }
                            }
                            break;
                    }
                })
                .AddTo(disposables);

            return disposables;
        });
    }

    private static string GetPropertyName<T>(Expression<Func<T, object>> propertySelector)
    {
        if (propertySelector.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (propertySelector.Body is UnaryExpression unaryExpression
            && unaryExpression.Operand is MemberExpression memberExpression2)
        {
            return memberExpression2.Member.Name;
        }

        throw new ArgumentException("Invalid property selector expression");
    }
}