using boilersGraphics.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace boilersGraphics.Helpers
{
    internal static class Kurukuru
    {
        private static readonly Cursor Kurukuru0 = GetCursorFromResource("Assets/img/kurukuru_0.cur");
        private static readonly Cursor Kurukuru1 = GetCursorFromResource("Assets/img/kurukuru_1.cur");
        private static readonly Cursor Kurukuru2 = GetCursorFromResource("Assets/img/kurukuru_2.cur");
        private static readonly Cursor Kurukuru3 = GetCursorFromResource("Assets/img/kurukuru_3.cur");
        private static readonly Cursor Kurukuru4 = GetCursorFromResource("Assets/img/kurukuru_4.cur");
        private static readonly Cursor Kurukuru5 = GetCursorFromResource("Assets/img/kurukuru_5.cur");
        private static CancellationTokenSource source = new CancellationTokenSource();
        private static Task task;

        public static void Set(Cursor back)
        {
            if (task is not null && task.Status != TaskStatus.Canceled)
            {
                return;
            }

            var designerCanvas = DesignerCanvas.GetInstance();
            task = Task.Factory.StartNew(async () =>
            {
                var i = 0;
                while (true)
                {
                    var now = DateTime.Now;
                    await App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        switch (i)
                        {
                            case 0:
                                designerCanvas.Cursor = Kurukuru0;
                                i++;
                                break;
                            case 1:
                                designerCanvas.Cursor = Kurukuru1;
                                i++;
                                break;
                            case 2:
                                designerCanvas.Cursor = Kurukuru2;
                                i++;
                                break;
                            case 3:
                                designerCanvas.Cursor = Kurukuru3;
                                i++;
                                break;
                            case 4:
                                designerCanvas.Cursor = Kurukuru4;
                                i++;
                                break;
                            case 5:
                                designerCanvas.Cursor = Kurukuru5;
                                i = 0;
                                break;
                        }
                    });
                    if (source.IsCancellationRequested)
                    {
                        await App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            designerCanvas.Cursor = back;
                        });
                        task = null;
                        break;
                    }
                    await Task.Delay(100);
                }
            }, source.Token);
        }

        public static void Stop()
        {
            source.Cancel();
        }

        private static Cursor GetCursorFromResource(string cursorFilePath)
        {
            return new Cursor(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cursorFilePath));
        }
    }
}
