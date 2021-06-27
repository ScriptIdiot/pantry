using System;
using System.Runtime.InteropServices;

namespace CMSTPBypass
{
    class NativeMethods
    {
        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)] public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
