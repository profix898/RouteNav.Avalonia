﻿using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace RouteNav.Avalonia.Themes;

public sealed class RouteNavStyles : Styles
{
    public RouteNavStyles(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}