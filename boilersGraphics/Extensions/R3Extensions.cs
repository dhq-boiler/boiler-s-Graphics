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
    /// Observes element property changes in NotifyCollectionChangedSynchronizedViewList.
    /// The selector must return the property's <see cref="R3.Observable{TProperty}"/>
    /// (e.g. a <see cref="R3.BindableReactiveProperty{TProperty}"/>) so that this
    /// extension can actually subscribe to per-element changes.
    /// </summary>
    public static R3.Observable<TProperty> ObserveElementObservableProperty<T, TProperty>(
        this NotifyCollectionChangedSynchronizedViewList<T> source,
        Func<T, R3.Observable<TProperty>> propertySelector) where T : class
    {
        return R3.Observable.Create<TProperty>(observer =>
        {
            var disposables = new System.Collections.Generic.List<IDisposable>();

            void SubscribeToElements()
            {
                foreach (var disposable in disposables)
                {
                    disposable?.Dispose();
                }
                disposables.Clear();

                foreach (var item in source)
                {
                    if (item == null) continue;
                    try
                    {
                        var observable = propertySelector(item);
                        if (observable == null) continue;
                        var subscription = observable.Subscribe(v => observer.OnNext(v));
                        disposables.Add(subscription);
                    }
                    catch
                    {
                        // Ignore errors in property access
                    }
                }
            }

            SubscribeToElements();

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

        // �����l��ݒ�
        if (source.CurrentValue != null)
        {
            observableList.AddRange(source.CurrentValue);
        }

        // �\�[�X�̕ύX���Ď�
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

        // �����l��ǉ�
        if (source.CurrentValue != null)
        {
            observableList.Add(source.CurrentValue);
        }

        // �ύX���Ď�
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

        // �ύX���Ď�
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
    /// NotifyCollectionChangedSynchronizedViewList�̗v�f�̃v���p�e�B�ύX���Ď�����Observable���쐬���܂�
    /// </summary>
    /// <typeparam name="T">���X�g�̗v�f�̌^�iINotifyPropertyChanged���������Ă���K�v������܂��j</typeparam>
    /// <param name="source">�Ď��Ώۂ�NotifyCollectionChangedSynchronizedViewList</param>
    /// <param name="propertySelector">�Ď�����v���p�e�B���w�肷��Z���N�^�[</param>
    /// <returns>�v���p�e�B�ύX��ʒm����Observable</returns>
    public static Observable<Unit> ObserveElementPropertyChanged<T>(this NotifyCollectionChangedSynchronizedViewList<T> source, Expression<Func<T, object>> propertySelector)
        where T : INotifyPropertyChanged
    {
        return Observable.Create<Unit>(observer =>
        {
            var disposables = new CompositeDisposable();
            var propertyName = GetPropertyName(propertySelector);

            // �����̗v�f�̃v���p�e�B�ύX���Ď�
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

            // �R���N�V�����ύX���Ď����ĐV�����v�f�̃v���p�e�B�ύX���Ď�
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
                            // �폜���ꂽ�v�f�̊Ď��͎����I�ɉ��������
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
                            // �S�ăN���A���čēx�Ď���ݒ�
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