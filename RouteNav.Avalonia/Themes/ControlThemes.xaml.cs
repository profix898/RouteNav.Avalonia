using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace RouteNav.Avalonia.Themes
{

    /// <summary>
    /// Includes the labs control themes in an application.
    /// </summary>
    public class ControlThemes : Styles
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlThemes"/> class.
        /// </summary>
        /// <param name="sp">The parent's service provider.</param>
        public ControlThemes(IServiceProvider? sp = null)
        {
            AvaloniaXamlLoader.Load(sp, this);
        }
    }
}
