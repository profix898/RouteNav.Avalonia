using System;
using System.Runtime.InteropServices;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Internal;

public static class WindowHelper
{
    public static void SetDialogStyle(this AvaloniaWindow window)
    {
        if (OperatingSystem.IsWindows())
        {
            const int GWL_EXSTYLE = -20;
            const int WS_EX_DLGMODALFRAME = 0x00000001;

            var wndHandle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (wndHandle != IntPtr.Zero)
            {
                var exStyle = GetWindowLong(wndHandle, GWL_EXSTYLE);
                exStyle |= WS_EX_DLGMODALFRAME;
                SetWindowLong(wndHandle, GWL_EXSTYLE, exStyle);
            }
        }
    }

    #region Win32

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    #endregion
}
