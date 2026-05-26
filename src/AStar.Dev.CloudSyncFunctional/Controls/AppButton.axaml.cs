using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A styled button primitive with kind, size, and optional leading and trailing icons.</summary>
public partial class AppButton : UserControl
{
    /// <summary>Identifies the <see cref="Kind"/> styled property.</summary>
    public static readonly StyledProperty<AppButtonKind> KindProperty =
        AvaloniaProperty.Register<AppButton, AppButtonKind>(nameof(Kind), AppButtonKind.Primary);

    /// <summary>Identifies the <see cref="ButtonSize"/> styled property.</summary>
    public static readonly StyledProperty<AppButtonSize> ButtonSizeProperty =
        AvaloniaProperty.Register<AppButton, AppButtonSize>(nameof(ButtonSize), AppButtonSize.Md);

    /// <summary>Identifies the <see cref="Label"/> styled property.</summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<AppButton, string>(nameof(Label), string.Empty);

    /// <summary>Identifies the <see cref="LeadingIcon"/> styled property.</summary>
    public static readonly StyledProperty<Geometry?> LeadingIconProperty =
        AvaloniaProperty.Register<AppButton, Geometry?>(nameof(LeadingIcon));

    /// <summary>Identifies the <see cref="TrailingIcon"/> styled property.</summary>
    public static readonly StyledProperty<Geometry?> TrailingIconProperty =
        AvaloniaProperty.Register<AppButton, Geometry?>(nameof(TrailingIcon));

    /// <summary>Identifies the <see cref="Command"/> styled property.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<AppButton, ICommand?>(nameof(Command));

    /// <summary>Gets or sets the visual style variant.</summary>
    public AppButtonKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Gets or sets the button height preset.</summary>
    public AppButtonSize ButtonSize
    {
        get => GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    /// <summary>Gets or sets the button label text.</summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets the optional icon geometry placed before the label.</summary>
    public Geometry? LeadingIcon
    {
        get => GetValue(LeadingIconProperty);
        set => SetValue(LeadingIconProperty, value);
    }

    /// <summary>Gets or sets the optional icon geometry placed after the label.</summary>
    public Geometry? TrailingIcon
    {
        get => GetValue(TrailingIconProperty);
        set => SetValue(TrailingIconProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the button is clicked.</summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="AppButton"/>.</summary>
    public AppButton() => InitializeComponent();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ApplyAll();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == KindProperty) ApplyKind();
        if (change.Property == ButtonSizeProperty) ApplySize();
        if (change.Property == LabelProperty) ApplyLabel();
        if (change.Property == LeadingIconProperty) ApplyLeadingIcon();
        if (change.Property == TrailingIconProperty) ApplyTrailingIcon();
        if (change.Property == CommandProperty) ApplyCommand();
    }

    private void ApplyAll()
    {
        ApplyKind();
        ApplySize();
        ApplyLabel();
        ApplyLeadingIcon();
        ApplyTrailingIcon();
        ApplyCommand();
    }

    private void ApplyKind()
    {
        if (InnerButton is null) return;

        InnerButton.CornerRadius = new CornerRadius(7);

        switch (Kind)
        {
            case AppButtonKind.Accent:
                SetButtonAppearance("Accent", null, null, 0);
                break;
            case AppButtonKind.Ghost:
                SetButtonAppearance(null, "Ink", "BorderStrong", 1);
                break;
            case AppButtonKind.Subtle:
                SetButtonAppearance(null, "Ink3", null, 0);
                break;
            case AppButtonKind.Danger:
                SetButtonAppearance("Danger", null, null, 0);
                break;
            default:
                SetButtonAppearance("Primary", null, null, 0);
                break;
        }
    }

    private void SetButtonAppearance(string? bgKey, string? fgKey, string? borderKey, int borderThickness)
    {
        if (InnerButton is null) return;

        InnerButton.Background = bgKey is not null && this.TryFindResource(bgKey, out var bg) && bg is IBrush bgBrush
            ? bgBrush
            : Brushes.Transparent;

        InnerButton.Foreground = fgKey is not null && this.TryFindResource(fgKey, out var fg) && fg is IBrush fgBrush
            ? fgBrush
            : Brushes.White;

        InnerButton.BorderThickness = new Thickness(borderThickness);
        InnerButton.BorderBrush = borderKey is not null && this.TryFindResource(borderKey, out var border) && border is IBrush borderBrush
            ? borderBrush
            : Brushes.Transparent;
    }

    private void ApplySize()
    {
        if (InnerButton is null || LabelText is null) return;

        var (height, fontSize) = ButtonSize switch
        {
            AppButtonSize.Sm => (26.0, 12.0),
            AppButtonSize.Lg => (38.0, 13.0),
            _ => (32.0, 12.5)
        };

        InnerButton.Height = height;
        LabelText.FontSize = fontSize;
    }

    private void ApplyLabel()
    {
        if (LabelText is null) return;

        LabelText.Text = Label;
    }

    private void ApplyLeadingIcon()
    {
        if (LeadingIconView is null) return;

        LeadingIconView.Data = LeadingIcon;
        LeadingIconView.IsVisible = LeadingIcon is not null;
    }

    private void ApplyTrailingIcon()
    {
        if (TrailingIconView is null) return;

        TrailingIconView.Data = TrailingIcon;
        TrailingIconView.IsVisible = TrailingIcon is not null;
    }

    private void ApplyCommand()
    {
        if (InnerButton is null) return;

        InnerButton.Command = Command;
    }
}
