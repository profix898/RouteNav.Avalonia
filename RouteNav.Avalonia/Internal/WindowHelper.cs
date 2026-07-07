using System;
using System.Runtime.InteropServices;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace RouteNav.Avalonia.Internal;

/// <summary>Platform-specific window helper methods.</summary>
public static class WindowHelper
{
    /// <summary>Applies a dialog-style window frame on platforms that support it.</summary>
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

    /// <summary>Gets a Win32 window style value.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hwnd, int index);

    /// <summary>Sets a Win32 window style value.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    #endregion
}
