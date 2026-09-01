using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using RouteNav.Avalonia.Dialogs;
using RouteNav.Avalonia.Internal;
using RouteNav.Avalonia.Platform;

namespace RouteNav.Avalonia;

/// <summary>Base RouteNav page type, with title, safe-area and dialog-size metadata.</summary>
public class Page : ContentControl, ISafeAreaAware, IEquatable<Page>
{
    /// <summary>
    /// Defines the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<Page, string?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="SafeAreaPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<Page, Thickness>(nameof(SafeAreaPadding));

    /// <summary>
    /// Defines the <see cref="DialogSizeHint" /> property.
    /// </summary>
    public static readonly StyledProperty<DialogSize?> DialogSizeHintProperty = AvaloniaProperty.Register<Page, DialogSize?>(nameof(DialogSizeHint), DialogSize.Large);

    /// <summary>
    /// Defines the <see cref="SizeScaleHint" /> property.
    /// </summary>
    public static readonly StyledProperty<Size?> SizeScaleHintProperty = AvaloniaProperty.Register<Page, Size?>(nameof(SizeScaleHint));

    /// <summary>
    /// Defines the <see cref="MinSizeHint" /> property.
    /// </summary>
    public static readonly StyledProperty<Size?> MinSizeHintProperty = AvaloniaProperty.Register<Page, Size?>(nameof(MinSizeHint));

    /// <summary>
    /// Defines the <see cref="MaxSizeHint" /> property.
    /// </summary>
    public static readonly StyledProperty<Size?> MaxSizeHintProperty = AvaloniaProperty.Register<Page, Size?>(nameof(MaxSizeHint));

    /// <summary>Gets the query parameters supplied to this page via its route URI.</summary>
    public Dictionary<string, string> PageQuery { get; internal set; } = new Dictionary<string, string>();

    /// <summary>Gets the route URI this page was resolved for, or <c>null</c> when the page was created without a route (e.g. via a page factory).</summary>
    public Uri? RouteUri { get; internal set; }

    /// <summary>
    /// Gets or sets the title of the page
    /// </summary>
    public string? Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    /// <summary>
    /// Gets or sets the safe area padding of the page (from inset manager)
    /// </summary>
    public Thickness SafeAreaPadding
    {
        get { return GetValue(SafeAreaPaddingProperty); }
        set { SetValue(SafeAreaPaddingProperty, value); }
    }

    /// <summary>
    /// Gets or sets the size hint in case the page is displayed as dialog
    /// </summary>
    public DialogSize? DialogSizeHint
    {
        get { return GetValue(DialogSizeHintProperty); }
        set { SetValue(DialogSizeHintProperty, value); }
    }

    /// <summary>
    /// Gets or sets the scale used to derive unset dialog size axes from the parent, in case the page is displayed as dialog
    /// </summary>
    public Size? SizeScaleHint
    {
        get { return GetValue(SizeScaleHintProperty); }
        set { SetValue(SizeScaleHintProperty, value); }
    }

    /// <summary>
    /// Gets or sets the minimum dialog size in case the page is displayed as dialog
    /// </summary>
    public Size? MinSizeHint
    {
        get { return GetValue(MinSizeHintProperty); }
        set { SetValue(MinSizeHintProperty, value); }
    }

    /// <summary>
    /// Gets or sets the maximum dialog size in case the page is displayed as dialog
    /// </summary>
    public Size? MaxSizeHint
    {
        get { return GetValue(MaxSizeHintProperty); }
        set { SetValue(MaxSizeHintProperty, value); }
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Page);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UpdateContentSafeAreaPadding();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SafeAreaPaddingProperty || change.Property == PaddingProperty || change.Property == ContentProperty)
            UpdateContentSafeAreaPadding();
    }

    /// <summary>Applies the remaining safe-area padding to the hosted content.</summary>
    protected virtual void UpdateContentSafeAreaPadding()
    {
        if (Content != null && Presenter != null)
        {
            if (Presenter.Child is ISafeAreaAware safeAreaAwareChild)
                safeAreaAwareChild.SafeAreaPadding = Padding.GetRemainingSafeAreaPadding(SafeAreaPadding);
            else
                Presenter.Padding = Presenter.Padding.ApplySafeAreaPadding(Padding.GetRemainingSafeAreaPadding(SafeAreaPadding));
        }
    }

    #region Implementation of IEquatable<Page>

    /// <inheritdoc />
    public bool Equals(Page? other)
    {
        if (other == null)
            return false;

        if (GetType() != other.GetType())
            return false;

        return PageQuery.EqualsContent(other.PageQuery);
    }

    #endregion
}
