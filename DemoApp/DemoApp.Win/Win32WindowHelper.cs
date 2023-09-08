using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace DemoApp.Win;

internal static class Win32WindowHelper
{
    #region Consts

    private const int GWL_EXSTYLE = -20;

    private const int WS_EX_DLGMODALFRAME = 0x00000001;

    #endregion

    #region DllImport

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    #endregion

    public static void SetDialogModalFrame(this Window window)
    {
        var wndHandle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (wndHandle != IntPtr.Zero)
        {
            var exStyle = GetWindowLong(wndHandle, GWL_EXSTYLE);
            exStyle |= WS_EX_DLGMODALFRAME;
            SetWindowLong(wndHandle, GWL_EXSTYLE, exStyle);
        }
    }
}
