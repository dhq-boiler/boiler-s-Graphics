using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Helpers;

/// <summary>
/// Windows クリップボードへのアクセスは、他プロセスが OLE クリップボードを保持している間
/// CLIPBRD_E_CANT_OPEN (HRESULT 0x800401D0) を投げる。
/// SetDataObject の直後に GetDataObject を呼ぶテストや、外部アプリ (RDP, IME, Office 等) が
/// クリップボードを触っているケースで散発的に発生するため、短い間隔で複数回リトライする。
/// </summary>
internal static class ClipboardHelper
{
    internal const int CLIPBRD_E_CANT_OPEN = unchecked((int)0x800401D0);
    internal const int DefaultRetryCount = 10;
    internal const int DefaultRetryDelayMs = 50;

    public static IDataObject GetDataObject()
        => Retry(() => Clipboard.GetDataObject());

    public static bool ContainsImage()
        => Retry(() => Clipboard.ContainsImage());

    public static BitmapSource GetImage()
        => Retry(() => Clipboard.GetImage());

    public static void SetDataObject(object data, bool copy)
        => Retry(() => Clipboard.SetDataObject(data, copy));

    public static void SetImage(BitmapSource image)
        => Retry(() => Clipboard.SetImage(image));

    public static object GetData(IDataObject obj, string format)
        => Retry(() => obj.GetData(format));

    public static bool GetDataPresent(IDataObject obj, string format)
        => Retry(() => obj.GetDataPresent(format));

    internal static T Retry<T>(Func<T> action, int retryCount = DefaultRetryCount, int delayMs = DefaultRetryDelayMs)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                return action();
            }
            catch (COMException ex) when (ex.HResult == CLIPBRD_E_CANT_OPEN && i < retryCount - 1)
            {
                Thread.Sleep(delayMs);
            }
        }
        // ループを抜ける = 最終試行で投げた例外が伝搬している。ここに到達するのは retryCount==0 のときのみ
        return action();
    }

    internal static void Retry(Action action, int retryCount = DefaultRetryCount, int delayMs = DefaultRetryDelayMs)
    {
        Retry<object>(() => { action(); return null; }, retryCount, delayMs);
    }
}
