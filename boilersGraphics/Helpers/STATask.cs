using System;
using System.Threading;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers;

// STAで実行されるTaskオブジェクトを生成する
public static class STATask
{
    public static Task Run<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        var thread = new Thread(() =>
        {
            try
            {
                tcs.SetResult(func());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task;
    }

    public static Task Run(Action act)
    {
        return Run(() =>
        {
            act();
            return true;
        });
    }
}