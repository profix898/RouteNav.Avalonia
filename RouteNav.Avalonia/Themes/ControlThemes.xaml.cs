using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace RouteNav.Avalonia.Themes
{
    public class ControlThemes : Styles
    {
        public ControlThemes(IServiceProvider? sp = null)
        {
            AvaloniaXamlLoader.Load(sp, this);
        }
    }
}
