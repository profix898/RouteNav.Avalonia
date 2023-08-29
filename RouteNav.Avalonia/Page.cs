using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using RouteNav.Avalonia.Internal;
using System;
using System.Collections.Generic;

namespace RouteNav.Avalonia;

public class Page : ContentControl, ISafeAreaAware, IEquatable<Page>
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<Page, string?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="SafeAreaPadding"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> SafeAreaPaddingProperty = AvaloniaProperty.Register<Page, Thickness>(nameof(SafeAreaPadding));

    public Dictionary<string, string> PageQuery { get; internal set; } = new Dictionary<string, string>();

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

    protected override Type StyleKeyOverride => typeof(Page);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UpdateContentSafeAreaPadding();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SafeAreaPaddingProperty || change.Property == PaddingProperty || change.Property == ContentProperty)
            UpdateContentSafeAreaPadding();
    }

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