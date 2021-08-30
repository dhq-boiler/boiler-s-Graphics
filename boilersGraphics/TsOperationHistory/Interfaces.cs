using System;
using System.Collections.Generic;

namespace TsOperationHistory
{
    /// <summary>
    /// ロールバック可能な操作のインタフェース
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// メッセージ
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// 実行 / 前進回帰
        /// </summary>
        void RollForward();

        /// <summary>
        /// ロールバック
        /// </summary>
        void Rollback();
    }
    
    /// <summary>
    /// オペレーションを作成する基本的なビルダー
    /// </summary>
    public interface IOperationBuilder
    {
        IOperationBuilder Message(string message);

        /// <summary>
        /// オペレーションの実行・ロールバック後にイベントを追加
        /// </summary>
        IOperationBuilder PostEvent(Action action);

        /// <summary>
        /// オペレーションの実行・ロールバック前にイベントを追加
        /// </summary>
        IOperationBuilder PrevEvent(Action action);

        /// <summary>
        /// オペレーションを作成
        /// </summary>
        IOperation Build();
    }
    
    public interface IMergeJudge
    {
        /// <summary>
        /// マージ可能か判断します。
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        bool CanMerge(IMergeJudge operation);
        
        /// <summary>
        /// ジャッジを更新します。マージCanMergeがfalseを返す場合も呼び出してください。
        /// </summary>
        /// <param name="prevMergeJudge">前回のマージ</param>
        /// <returns></returns>
        IMergeJudge Update(IMergeJudge prevMergeJudge);

        /// <summary>
        /// マージのためのKeyを取得
        /// </summary>
        object GetMergeKey();
    }

    public interface IMergeableOperationBuilder : IOperationBuilder
    {
        IMergeableOperationBuilder SetActionName(string executeAction, string rollbackAction);
    }

    public interface IBuilderFromValues<in T> : IOperationBuilder
    {
        /// <summary>
        /// 前回の値と新しい値を使用してビルダーを作成
        /// </summary>
        IBuilderFromValues<T> Values(T newValue, T prevValue);

        IBuilderFromValues<T> Throttle(object key , TimeSpan timeSpan);
    }

    public interface IBuilderFromNewValue<in T> : IOperationBuilder
    {
        /// <summary>
        /// 新しい値を利用してビルダーを作成
        /// </summary>
        IBuilderFromNewValue<T> NewValue(T newValue);

        IBuilderFromNewValue<T> Throttle(TimeSpan timeSpan);
    }

    /// <summary>
    /// コレクションを操作するオペレーションを作成するインタフェース
    /// </summary>
    public interface ICollectionOperationBuilder<in T>
    {
        /// <summary>
        /// コレクションに要素を追加するオペレーションを作成
        /// </summary>
        IOperation BuildAddOperation(T addValue);

        /// <summary>
        /// コレクションからに要素を削除するオペレーションを作成
        /// </summary>
        IOperation BuildRemoveOperation(T removeValue);

        /// <summary>
        /// コレクションに要素を複数追加するオペレーションを作成
        /// </summary>
        IOperation BuildAddRangeOperation(IEnumerable<T> addValue);

        /// <summary>
        /// コレクションから要素を複数削除するオペレーションを作成
        /// </summary>
        IOperation BuildRemoveRangeOperation(IEnumerable<T> removeValue);

        /// <summary>
        /// コレクションを空にするオペレーションを作成
        /// </summary>
        IOperation BuildClearOperation();
    }

    /// <summary>
    /// コレクション操作を独自定義してオペレーションを作成するビルダー
    /// </summary>
    public interface ICollectionOperationCustomizer<T> : ICollectionOperationBuilder<T>
    {
        /// <summary>
        /// 追加手続きを登録
        /// </summary>
        ICollectionOperationCustomizer<T> RegisterAdd(Action<T> value);
        ICollectionOperationCustomizer<T> RegisterAdd(Action function);

        /// <summary>
        /// 削除手続きを登録
        /// </summary>
        ICollectionOperationCustomizer<T> RegisterRemove(Action<T> function);
        ICollectionOperationCustomizer<T> RegisterRemove(Action function);

        /// <summary>
        /// 空にするための手続きを登録
        /// </summary>
        ICollectionOperationCustomizer<T> RegisterClear(Action function);

        /// <summary>
        /// 空にした要素を復元する手続きを登録
        /// </summary>
        ICollectionOperationCustomizer<T> RegisterRollback(Action function);
    }
    
    /// <summary>
    /// オペレーションを実行するコントローラ
    /// </summary>
    public interface IOperationController
    {
        /// <summary>
        /// 一つ前の処理へ戻れるか
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// 戻った処理をやりなおせるか
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// 先頭のオペレーションをロールバックする
        /// </summary>
        void Undo();

        /// <summary>
        /// ロールバックされたオペレーションをロールフォワードする
        /// </summary>
        void Redo();

        /// <summary>
        /// スタックをクリアする
        /// </summary>
        void Flush();

        /// <summary>
        /// スタックからデータを取り出さずにデータを取得する
        /// </summary>
        IOperation Peek();

        /// <summary>
        /// スタックからデータを取り出す
        /// </summary>
        IOperation Pop();

        /// <summary>
        /// 実行しないでスタックにデータを積む
        /// </summary>
        IOperation Push(IOperation operation);

        /// <summary>
        /// 操作を実行し、スタックに積む
        /// </summary>
        IOperation Execute(IOperation operation);

        /// <summary>
        /// 実行された操作一覧を取得する
        /// </summary>
        IEnumerable<IOperation> Operations { get; }

        /// <summary>
        /// ロールフォワード対象を取得する
        /// </summary>
        IEnumerable<IOperation> RollForwardTargets { get; }

        /// <summary>
        /// スタックが更新されたときにイベントが発生する
        /// </summary>
        event EventHandler<OperationStackChangedEventArgs> StackChanged;

        /// <summary>
        /// オペレーション中かどうか
        /// </summary>
        bool IsOperating { get; }
    }

    public interface IRestore
    {
        void Restore(Action restorePropertiesAction);
    }

    public enum OperationStackChangedEvent
    {
        Undo,
        Redo,
        Push,
        Pop,
        Clear,
    }

    public class OperationStackChangedEventArgs : EventArgs
    {
        public OperationStackChangedEvent EventType { get; set; }
    }
}