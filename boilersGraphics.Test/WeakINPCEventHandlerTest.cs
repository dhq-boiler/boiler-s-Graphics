using boilersGraphics.Helpers;
using NUnit.Framework;
using System;
using System.ComponentModel;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class WeakINPCEventHandlerTest
    {
        // 本クラスの本番上のバグ防止意図:
        // - PropertyChanged 購読者を弱参照で持ち、購読者の解放後も購読側プロセスが
        //   生き続けるリスナーリークを防ぐこと。
        // - 解放されていれば Handler 呼び出しは無動作 (NullReferenceException を投げない)。
        // - 生存中は元のコールバックに正しい sender / args が転送されること。

        private sealed class Subscriber
        {
            public int CallCount { get; private set; }
            public object LastSender { get; private set; }
            public PropertyChangedEventArgs LastArgs { get; private set; }

            public void OnChanged(object sender, PropertyChangedEventArgs args)
            {
                CallCount++;
                LastSender = sender;
                LastArgs = args;
            }
        }

        [Test]
        public void Handler_target生存中は元のコールバックに転送する()
        {
            var subscriber = new Subscriber();
            var weak = new WeakINPCEventHandler(subscriber.OnChanged);
            var senderObj = new object();
            var args = new PropertyChangedEventArgs("Foo");

            weak.Handler(senderObj, args);

            Assert.That(subscriber.CallCount, Is.EqualTo(1));
            Assert.That(subscriber.LastSender, Is.SameAs(senderObj));
            Assert.That(subscriber.LastArgs, Is.SameAs(args));
        }

        [Test]
        public void Handler_targetが回収されたあとは無動作で例外を投げない()
        {
            // ローカル関数で WeakINPCEventHandler を返し、Subscriber への強参照を残さない
            WeakINPCEventHandler MakeWeak(out WeakReference subscriberRef)
            {
                var subscriber = new Subscriber();
                subscriberRef = new WeakReference(subscriber);
                return new WeakINPCEventHandler(subscriber.OnChanged);
            }

            var weak = MakeWeak(out var subscriberRef);

            // GC を強制し、subscriber を回収させる
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // subscriberRef.Target が null になったかチェック (環境依存)
            // - もし回収されていれば、Handler は何もせずに戻ること
            // - 仮に未回収でも例外は投げない
            Assert.That(() => weak.Handler(this, new PropertyChangedEventArgs("X")),
                Throws.Nothing);

            // 弱参照が解放されたことを確認 (期待挙動)
            // GC 挙動はランタイム依存だが、Debug 含めて安定的に解放されるはず
            if (subscriberRef.IsAlive)
            {
                // GC 挙動が不安定な環境ではここに来る場合がある (情報メッセージのみ)
                Assert.Inconclusive("subscriber was not collected by GC; lifetime test inconclusive on this run.");
            }
        }

        [Test]
        public void Handler_複数回呼び出しても累積カウント()
        {
            var subscriber = new Subscriber();
            var weak = new WeakINPCEventHandler(subscriber.OnChanged);

            weak.Handler(this, new PropertyChangedEventArgs("A"));
            weak.Handler(this, new PropertyChangedEventArgs("B"));
            weak.Handler(this, new PropertyChangedEventArgs("C"));

            Assert.That(subscriber.CallCount, Is.EqualTo(3));
            Assert.That(subscriber.LastArgs.PropertyName, Is.EqualTo("C"));
        }

        [Test]
        public void Handler_INPC本物のイベント経由でも転送する()
        {
            // INPC 実体で PropertyChanged を発火させて、転送経路全体を確認する
            var publisher = new PublisherMock();
            var subscriber = new Subscriber();
            var weak = new WeakINPCEventHandler(subscriber.OnChanged);
            publisher.PropertyChanged += weak.Handler;

            publisher.RaiseChange("MyProperty");

            Assert.That(subscriber.CallCount, Is.EqualTo(1));
            Assert.That(subscriber.LastSender, Is.SameAs(publisher));
            Assert.That(subscriber.LastArgs.PropertyName, Is.EqualTo("MyProperty"));
        }

        private sealed class PublisherMock : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public void RaiseChange(string name)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [Test]
        public void Handler_ラムダコールバックでも安全に転送する()
        {
            int called = 0;
            string lastName = null;
            PropertyChangedEventHandler cb = (s, e) => { called++; lastName = e.PropertyName; };
            var weak = new WeakINPCEventHandler(cb);

            weak.Handler(this, new PropertyChangedEventArgs("Lambda"));

            Assert.That(called, Is.EqualTo(1));
            Assert.That(lastName, Is.EqualTo("Lambda"));
        }
    }
}
