using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace RouteNav.Avalonia.Themes;

/// <summary>Loads the RouteNav default styles and control themes.</summary>
public sealed class RouteNavStyles : Styles
{
    /// <summary>Initializes a new instance of the <see cref="RouteNavStyles" /> class.</summary>
    public RouteNavStyles(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}
