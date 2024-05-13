using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using RouteNav.Avalonia.Routing;
using RouteNav.Avalonia.Stacks;

namespace RouteNav.Avalonia.Controls
{
    /// <summary>
    /// A button control that functions as a navigateable hyperlink.
    /// </summary>
    [PseudoClasses(pcVisited)]
    public class HyperlinkButton : Button, IRouteItem
    {
        #region Launcher
        
        public static ILauncher Launcher { get; set; } = new HyperlinkLauncher();

        #endregion
        
        // See: https://www.w3schools.com/cssref/sel_visited.php
        private const string pcVisited = ":visited";

        /// <summary>
        /// Defines the <see cref="RouteUri"/> property.
        /// </summary>
        public static readonly StyledProperty<Uri?> RouteUriProperty = AvaloniaProperty.Register<HyperlinkButton, Uri?>(nameof(RouteUri), defaultValue: null);
        
        /// <summary>
        /// Defines the <see cref="TargetProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<NavigationTarget> TargetProperty = AvaloniaProperty.Register<HyperlinkButton, NavigationTarget>(nameof(Target), NavigationTarget.Self);
        
        /// <summary>
        /// Defines the <see cref="IsVisited"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsVisitedProperty = AvaloniaProperty.Register<HyperlinkButton, bool>(nameof(IsVisited), defaultValue: false);
        
        /// <summary>
        /// Defines the <see cref="TrackIsVisited"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> TrackIsVisitedProperty = AvaloniaProperty.Register<HyperlinkButton, bool>(nameof(TrackIsVisited), defaultValue: false);

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperlinkButton"/> class.
        /// </summary>
        public HyperlinkButton()
        {
        }
        
        /// <summary>
        /// Gets or sets the Uniform Resource Identifier (URI) navigated to when the <see cref="HyperlinkButton"/> is clicked.
        /// </summary>
        /// <remarks>
        /// The URI may be any website or file location that can be launched using the <see cref="ILauncher"/> service.
        /// <br></br>
        /// For compatibility with both V11.1 <see cref="Avalonia.Controls.HyperlinkButton"/> and <see cref="IRouteItem"/> (in this library), both <see cref="RouteUri"/> and <see cref="NavigateUri"/> are provided. Use any one of them.
        /// </remarks>
        public Uri? NavigateUri
        {
            get { return GetValue(RouteUriProperty); }
            set { SetValue(RouteUriProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="NavigateUri"/> has been visited.
        /// </summary>
        public bool IsVisited
        {
            get { return GetValue(IsVisitedProperty); }
            set { SetValue(IsVisitedProperty, value); }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IsVisited"/> flag should be set upon visiting the Uri (default: false).
        /// </summary>
        public bool TrackIsVisited
        {
            get { return GetValue(TrackIsVisitedProperty); }
            set { SetValue(TrackIsVisitedProperty, value); }
        }
        
        #region Overrides of StyledElement

        protected override Type StyleKeyOverride => typeof(global::Avalonia.Controls.HyperlinkButton);

        #endregion
        
        #region Implementation of IRouteItem

        /// <summary>
        /// Gets or sets the Uniform Resource Identifier (URI) navigated to when the <see cref="HyperlinkButton"/> is clicked.
        /// </summary>
        /// <remarks>
        /// For compatibility with both V11.1 <see cref="Avalonia.Controls.HyperlinkButton"/> and <see cref="IRouteItem"/> (in this library), both <see cref="RouteUri"/> and <see cref="NavigateUri"/> are provided. Use any one of them.
        /// </remarks>
        public Uri? RouteUri
        {
            get { return GetValue(RouteUriProperty); }
            set { SetValue(RouteUriProperty, value); }
        }
        
        /// <summary>Set <see cref="RouteUri"/> via route path. Both relative paths (e.g. 'myPage' relative to current stack) and
        ///          absolute paths (e.g. '/myStack/myPage') are supported. The leading '/' denotes an absolute path.</summary>
        public string RoutePath
        {
            set { SetValue(RouteUriProperty, value.StartsWith("/") ? new Uri(Navigation.BaseRouteUri, value.TrimEnd('/')) : new Uri(value.TrimEnd('/'), UriKind.Relative)); }
        }
        
        public NavigationTarget Target
        {
            get { return GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
        
        public void NavigateToRoute()
        {
            if (RouteUri == null)
                return;
            
            Dispatcher.UIThread.Post(async () =>
            {
                var success = await Launcher.LaunchUriAsync(RouteUri);
                if (success && TrackIsVisited)
                    SetCurrentValue(IsVisitedProperty, true);
            });
        }

        #endregion

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsVisitedProperty)
                PseudoClasses.Set(pcVisited, change.GetNewValue<bool>());
        }

        /// <inheritdoc/>
        protected override void OnClick()
        {
            base.OnClick();

            NavigateToRoute();
        }
    }
}
