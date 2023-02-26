using System.Runtime.InteropServices;

namespace boilersGraphics.Helpers
{
    internal static class NativeMethods
    {
        [DllImport(dllName: "BgffThumbnailProvider.dll")]
        internal static extern int DllRegisterServer();

        [DllImport(dllName: "BgffThumbnailProvider.dll")]
        internal static extern int DllUnregisterServer();
    }
}
