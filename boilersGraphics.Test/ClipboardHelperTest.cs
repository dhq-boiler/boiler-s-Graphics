using boilersGraphics.Helpers;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ClipboardHelperTest
    {
        [Test]
        public void Retry_最初の試行で成功すれば1回だけ呼ばれる()
        {
            int callCount = 0;
            var result = ClipboardHelper.Retry(() =>
            {
                callCount++;
                return 42;
            }, retryCount: 5, delayMs: 1);

            Assert.That(result, Is.EqualTo(42));
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Retry_CLIPBRD_E_CANT_OPENを投げ続けると最終的に再throw()
        {
            int callCount = 0;
            Assert.That(() => ClipboardHelper.Retry<int>(() =>
            {
                callCount++;
                throw new COMException("locked", ClipboardHelper.CLIPBRD_E_CANT_OPEN);
            }, retryCount: 3, delayMs: 1), Throws.TypeOf<COMException>());

            Assert.That(callCount, Is.EqualTo(3));
        }

        [Test]
        public void Retry_途中で成功すればその時点で値を返す()
        {
            int callCount = 0;
            var result = ClipboardHelper.Retry(() =>
            {
                callCount++;
                if (callCount < 3)
                    throw new COMException("locked", ClipboardHelper.CLIPBRD_E_CANT_OPEN);
                return "ok";
            }, retryCount: 5, delayMs: 1);

            Assert.That(result, Is.EqualTo("ok"));
            Assert.That(callCount, Is.EqualTo(3));
        }

        [Test]
        public void Retry_CLIPBRD_E_CANT_OPEN以外のCOMExceptionは即throw()
        {
            int callCount = 0;
            Assert.That(() => ClipboardHelper.Retry<int>(() =>
            {
                callCount++;
                // 別の HRESULT (例: E_FAIL = 0x80004005) はリトライ対象外
                throw new COMException("other", unchecked((int)0x80004005));
            }, retryCount: 5, delayMs: 1), Throws.TypeOf<COMException>());

            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Retry_他の例外型は即throw()
        {
            int callCount = 0;
            Assert.That(() => ClipboardHelper.Retry<int>(() =>
            {
                callCount++;
                throw new InvalidOperationException("nope");
            }, retryCount: 5, delayMs: 1), Throws.TypeOf<InvalidOperationException>());

            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Retry_Action版も同様にリトライする()
        {
            int callCount = 0;
            ClipboardHelper.Retry(() =>
            {
                callCount++;
                if (callCount < 2)
                    throw new COMException("locked", ClipboardHelper.CLIPBRD_E_CANT_OPEN);
            }, retryCount: 5, delayMs: 1);

            Assert.That(callCount, Is.EqualTo(2));
        }
    }
}
