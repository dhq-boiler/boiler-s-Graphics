using System;
using System.Collections;
using System.Collections.Generic;
using ZLinq;

namespace boilersGraphics.Helpers;

/// <summary>
///     キーと複数の値のコレクションを表します
/// </summary>
public class MultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, List<TValue>>>
{
    private readonly Dictionary<TKey, List<TValue>> mDictionary = new();

    /// <summary>
    ///     指定したキーに関連付けられている複数の値を取得または設定します
    /// </summary>
    public List<TValue> this[TKey key]
    {
        get => mDictionary[key];
        set => mDictionary[key] = value;
    }

    /// <summary>
    ///     キーを格納しているコレクションを取得します
    /// </summary>
    public Dictionary<TKey, List<TValue>>.KeyCollection Keys => mDictionary.Keys;

    /// <summary>
    ///     複数の値を格納しているコレクションを取得します
    /// </summary>
    public Dictionary<TKey, List<TValue>>.ValueCollection Values => mDictionary.Values;

    /// <summary>
    ///     格納されているキーと値のペアの数を取得します
    /// </summary>
    public int Count => mDictionary.Count;

    IEnumerator<KeyValuePair<TKey, List<TValue>>> IEnumerable<KeyValuePair<TKey, List<TValue>>>.GetEnumerator()
    {
        return new MultiDictionaryEnumerator(mDictionary.AsValueEnumerable().ToList());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new MultiDictionaryEnumerator(mDictionary.AsValueEnumerable().ToList());
    }

    /// <summary>
    ///     指定したキーと値をディクショナリに追加します
    /// </summary>
    public void Add(TKey key, TValue value)
    {
        if (!mDictionary.ContainsKey(key)) mDictionary.Add(key, new List<TValue>());
        mDictionary[key].Add(value);
    }

    /// <summary>
    ///     指定したキーと複数の値をディクショナリに追加します
    /// </summary>
    public void Add(TKey key, params TValue[] values)
    {
        foreach (var n in values) Add(key, n);
    }

    /// <summary>
    ///     指定したキーと複数の値をディクショナリに追加します
    /// </summary>
    public void Add(TKey key, IEnumerable<TValue> values)
    {
        foreach (var n in values) Add(key, n);
    }

    /// <summary>
    ///     指定したキーを持つ値を削除します
    /// </summary>
    public bool Remove(TKey key, TValue value)
    {
        return mDictionary[key].Remove(value);
    }

    /// <summary>
    ///     指定したキーを持つ複数の値を削除します
    /// </summary>
    public bool Remove(TKey key)
    {
        return mDictionary.Remove(key);
    }

    /// <summary>
    ///     すべてのキーと複数の値を削除します
    /// </summary>
    public void Clear()
    {
        mDictionary.Clear();
    }

    /// <summary>
    ///     指定したキーと値が格納されているかどうかを判断します
    /// </summary>
    public bool Contains(TKey key, TValue value)
    {
        return mDictionary[key].Contains(value);
    }

    /// <summary>
    ///     指定したキーが格納されているかどうかを判断します
    /// </summary>
    public bool ContainsKey(TKey key)
    {
        return mDictionary.ContainsKey(key);
    }

    /// <summary>
    ///     反復処理する列挙子を返します
    /// </summary>
    public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator()
    {
        foreach (var n in mDictionary) yield return n;
    }

    public class MultiDictionaryEnumerator : IEnumerator<KeyValuePair<TKey, List<TValue>>>
    {
        private bool disposedValue;
        private List<KeyValuePair<TKey, List<TValue>>> list;

        private int position = -1;

        public MultiDictionaryEnumerator(List<KeyValuePair<TKey, List<TValue>>> list)
        {
            this.list = list;
        }

        public KeyValuePair<TKey, List<TValue>> Current
        {
            get
            {
                try
                {
                    return list[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            position++;
            return position < list.Count;
        }

        public void Reset()
        {
            position = -1;
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) list.Clear();

                list = null;
                disposedValue = true;
            }
        }
    }
}