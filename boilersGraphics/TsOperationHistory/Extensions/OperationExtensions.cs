using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Extensions
{
    public static class Operation
    {
        private class EmptyOperation : IOperation
        {
            public string Message { get; set; } = "Empty Operation";
            public void RollForward() { }
            public void Rollback() { }
        }

        /// <summary>
        /// 空のオペレーションを取得する
        /// </summary>
        public static IOperation Empty { get; } = new EmptyOperation();

        /// <summary>
        /// 拡張が自動作成するマージコマンドのマージ間隔デフォルト設定
        /// </summary>
        public static TimeSpan DefaultMergeSpan { get; set; } = TimeSpan.MaxValue;
    }

    /// <summary>
    /// オペレーション拡張機能
    /// </summary>
    public static class OperationExtensions
    {
        /// <summary>
        /// コントローラを通してOperationを実行する、マージはこのタイミングで行われる
        /// マージをしたくない場合は直接 IOperationController.Execute()を呼び出す
        /// </summary>
        public static IOperation ExecuteTo(this IOperation @this, IOperationController controller)
        {
            if (@this is IMergeableOperation mergeableOperation)
                @this = mergeableOperation.Merge(controller);
            return controller.Execute(@this);
        }
        public static IOperation PushTo(this IOperation @this, IOperationController controller)
        {
            if (@this is IMergeableOperation mergeableOperation)
                @this = mergeableOperation.Merge(controller);
            return controller.Push(@this);
        }

        /// <summary>
        /// 前回のオペレーションと結合します
        /// </summary>
        public static IOperation ExecuteAndCombineTop(this IOperation @this, IOperationController controller)
        {
            if (controller.Operations.Any())
            {
                var prev = controller.Pop();
                @this.RollForward();
                var newOperation = prev.CombineOperations(@this).ToCompositeOperation();
                newOperation.Message = @this.Message;
                return controller.Push(newOperation);
            }

            return controller.Execute(@this);
        }

        public static IOperation ExecuteDispose<T>(this T @this, Action regenerateAction) where T : IDisposable
        {
            return new DisposeOperation<T>(@this, regenerateAction);
        }

        /// <summary>
        /// オペレーションを列挙子として結合する
        /// </summary>
        public static IEnumerable<IOperation> CombineOperations(this IOperation @this, params IOperation[] subOperations)
        {
            yield return @this;
            foreach (var operation in subOperations)
                yield return operation;
        }

        /// <summary>
        /// プロパティ名からプロパティ設定オペレーションを作成する
        /// </summary>
        public static IMergeableOperation GenerateSetPropertyOperation<TProperty>(this object @this, string propertyName, TProperty newValue, TimeSpan timeSpan)
        {
            var nullableObj = FastReflection.GetProperty(@this, propertyName);
            TProperty oldValue = nullableObj != null ? (TProperty)nullableObj : default(TProperty);

            return GenerateAutoMergeOperation(@this, propertyName, newValue, oldValue, $"{@this.GetHashCode()}.{propertyName}", timeSpan);
        }

        public static IMergeableOperation GenerateSetStaticPropertyOperation<TProperty>(this Type @class, string propertyName, TProperty newValue, TimeSpan timeSpan)
        {
            var nullableObj = FastReflection.GetStaticProperty(@class, propertyName);
            TProperty oldValue = nullableObj != null ? (TProperty)nullableObj : default(TProperty);

            return GenerateAutoMergeOperation(@class, propertyName, newValue, oldValue, $"{@class.GetHashCode()}.{propertyName}", timeSpan);
        }

        public static IMergeableOperation GenerateSetPropertyOperation<TProperty>(this object @this, string propertyName, TProperty newValue)
        {
            return GenerateSetPropertyOperation(@this, propertyName, newValue, Operation.DefaultMergeSpan);
        }

        public static IMergeableOperation GenerateSetStaticPropertyOperation<TProperty>(this Type @class, string propertyName, TProperty newValue)
        {
            return GenerateSetStaticPropertyOperation(@class, propertyName, newValue, Operation.DefaultMergeSpan);
        }

        public static IMergeableOperation GenerateSetPropertyOperation<T, TProperty>(this T @this, Expression<Func<T, TProperty>> selector, TProperty newValue)
        {
            var propertyName = selector.GetMemberName();
            return GenerateSetPropertyOperation(@this, propertyName, newValue , Operation.DefaultMergeSpan);
        }
        
        public static IMergeableOperation ToOperation<T, TProperty>(this T @this, Expression<Func<T, TProperty>> selector)
        {
            var propertyName = selector.GetMemberName();
            var currentValue = FastReflection.GetProperty<TProperty>(@this, propertyName);
            return GenerateSetPropertyOperation(@this, propertyName, currentValue , Operation.DefaultMergeSpan);
        }

        /// <summary>
        /// マージ可能なオペレーションを作成する
        /// </summary>
        public static IMergeableOperation GenerateAutoMergeOperation<TProperty,TMergeKey>(this object @this,string propertyName, TProperty newValue ,TProperty oldValue, TMergeKey mergeKey,TimeSpan timeSpan)
        {
            return new MergeableOperation<TProperty>(
                x => { FastReflection.SetProperty(@this, propertyName, x); },
                newValue,
                oldValue, new ThrottleMergeJudge<TMergeKey>(mergeKey, timeSpan));
        }

        public static IMergeableOperation GenerateAutoMergeOperation<TProperty, TMergeKey>(this Type @class, string propertyName, TProperty newValue, TProperty oldValue, TMergeKey mergeKey, TimeSpan timeSpan)
        {
            return new MergeableOperation<TProperty>(
                x => { FastReflection.SetStaticProperty(@class, propertyName, x); },
                newValue,
                oldValue, new ThrottleMergeJudge<TMergeKey>(mergeKey, timeSpan));
        }

        /// <summary>
        /// オペレーションに実行後イベントを追加する
        /// </summary>
        public static IOperation AddPostEvent(this IOperation @this, Action action)
        {
            if (@this is IOperationWithEvent triggerOperation)
            {
                triggerOperation.OnExecuted += action;
                return @this;
            }

            return new DelegateOperation(
                () =>
                {
                    @this.RollForward();
                    action.Invoke();
                },
                () =>
                {
                    @this.Rollback();
                    action.Invoke();
                });
        }

        /// <summary>
        /// オペレーション実行前にイベントを追加する
        /// </summary>
        public static IOperation AddPreEvent(this IOperation @this, Action action)
        {
            if (@this is IOperationWithEvent triggerOperation)
            {
                triggerOperation.OnPreviewExecuted += action;
                return @this;
            }

            return new DelegateOperation(
                () =>
                {
                    action.Invoke();
                    @this.RollForward();
                },
                () =>
                {
                    action.Invoke();
                    @this.Rollback();
                });
        }

        /// <summary>
        /// オペレーションが空かどうか
        /// </summary>
        public static bool IsEmpty(this IOperation @this)
        {
            if (@this == Operation.Empty)
                return true;
            if (@this is CompositeOperation compositeOperation)
                return compositeOperation.Operations.Any() is false 
                    || compositeOperation.GetAllOperation().All(x=>x.IsEmpty());
            return false;
        }

        /// <summary>
        /// オペレーションが null または空か
        /// </summary>
        public static bool IsNullOrEmpty(this IOperation @this)
        {
            if (@this is null)
                return true;
            return IsEmpty(@this);
        }


        /// <summary>
        /// オペレーションが空でないかどうか
        /// </summary>
        public static bool IsNotEmpty(this IOperation @this)
        {
            return @this.IsEmpty() is false;
        }
    }

    /// <summary>
    /// リスト操作オペレーション拡張
    /// </summary>
    public static class ListOperationExtensions
    {
        /// <summary>
        /// 値の追加オペレーションを作成する
        /// </summary>
        public static IOperation ToAddOperation<T>(this IList<T> @this, T value)
            => new InsertOperation<T>(@this, value);

        /// <summary>
        /// 値の削除オペレーションを作成する
        /// </summary>
        public static IOperation ToRemoveOperation<T>(this IList<T> @this, T value)
            => new RemoveOperation<T>(@this, value);

        /// <summary>
        /// 値のインデックス指定削除オペレーションを作成する
        /// </summary>
        public static IOperation ToRemoveAtOperation(this IList @this, int index)
            => new RemoveAtOperation(@this, index);

        /// <summary>
        /// 値の複数追加オペレーションを作成する
        /// </summary>
        public static IOperation ToAddRangeOperation<T>(this IList<T> @this, params T[] values)
            => ToAddRangeOperation(@this, values as IEnumerable<T>);

        public static IOperation ToAddRangeOperation<T>(this IList<T> @this, IEnumerable<T> values)
        {
            return values
                .Select(x => new InsertOperation<T>(@this, x))
                .ToCompositeOperation();
        }

        /// <summary>
        /// 値の複数削除オペレーションを作成する
        /// </summary>
        public static IOperation ToRemoveRangeOperation<T>(this IList<T> @this, params T[] values)
            => ToRemoveRangeOperation(@this, values as IEnumerable<T>);

        public static IOperation ToRemoveRangeOperation<T>(this IList<T> @this, IEnumerable<T> values)
        {
            return values
                .Select(x => new RemoveOperation<T>(@this, x))
                .ToCompositeOperation();
        }

        public static IOperation ToClearOperation<T>(this IList<T> @this)
            => new ClearOperation<T>(@this);
    }

    /// <summary>
    /// 階層オペレーション拡張
    /// </summary>
    public static class CompositeOperationExtensions
    {
        /// <summary>
        /// 複数のオペレーションをグループ化して１つのオペレーションに変換する
        /// </summary>
        public static ICompositeOperation ToCompositeOperation(this IEnumerable<IOperation> operations)
        {
            return new CompositeOperation(operations.ToArray());
        }

        /// <summary>
        /// オペレーションを結合して１つのオペレーションに変換する
        /// </summary>
        public static ICompositeOperation Union(this IOperation @this, params IOperation[] operations)
        {
            return new CompositeOperation(@this.CombineOperations(operations).ToArray());
        }

        public static ICompositeOperation Union(this IOperation @this, IEnumerable<IOperation> operations)
        {
            return Union(@this, operations.ToArray());
        }

        /// <summary>
        /// 全てのオペレーションを展開して取得
        /// </summary>
        public static IEnumerable<IOperation> GetAllOperation(this ICompositeOperation compositeOperation)
        {
            var list = new List<IOperation>();

            foreach (var operation in compositeOperation.Operations)
            {
                if (operation is ICompositeOperation compositeOperation2)
                {
                    list.AddRange(compositeOperation2.GetAllOperation());
                }
                else
                {
                    list.Add(operation);
                }
            }

            return list;
        }
    }
}
