﻿using Avalonia;
using Avalonia.ReactiveUI;
using DemoApp;
using System.Runtime.Versioning;
using Avalonia.Web;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static void Main(string[] args) => BuildAvaloniaApp()
        .UseReactiveUI()
        .SetupBrowserApp("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}