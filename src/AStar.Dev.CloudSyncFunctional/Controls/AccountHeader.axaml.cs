using System.Windows.Input;
using AStar.Dev.CloudSyncFunctional.Accounts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AStar.Dev.CloudSyncFunctional.Controls;

/// <summary>A header strip displaying account identity, provider, sync status, and action buttons.</summary>
public partial class AccountHeader : UserControl
{
    /// <summary>Identifies the <see cref="AccountName"/> styled property.</summary>
    public static readonly StyledProperty<string> AccountNameProperty =
        AvaloniaProperty.Register<AccountHeader, string>(nameof(AccountName), string.Empty);

    /// <summary>Identifies the <see cref="Email"/> styled property.</summary>
    public static readonly StyledProperty<string> EmailProperty =
        AvaloniaProperty.Register<AccountHeader, string>(nameof(Email), string.Empty);

    /// <summary>Identifies the <see cref="Kind"/> styled property.</summary>
    public static readonly StyledProperty<ProviderKind> KindProperty =
        AvaloniaProperty.Register<AccountHeader, ProviderKind>(nameof(Kind));

    /// <summary>Identifies the <see cref="Status"/> styled property.</summary>
    public static readonly StyledProperty<SyncStatus> StatusProperty =
        AvaloniaProperty.Register<AccountHeader, SyncStatus>(nameof(Status));

    /// <summary>Identifies the <see cref="PauseCommand"/> styled property.</summary>
    public static readonly StyledProperty<ICommand?> PauseCommandProperty =
        AvaloniaProperty.Register<AccountHeader, ICommand?>(nameof(PauseCommand));

    /// <summary>Identifies the <see cref="SettingsCommand"/> styled property.</summary>
    public static readonly StyledProperty<ICommand?> SettingsCommandProperty =
        AvaloniaProperty.Register<AccountHeader, ICommand?>(nameof(SettingsCommand));

    /// <summary>Identifies the <see cref="MoreCommand"/> styled property.</summary>
    public static readonly StyledProperty<ICommand?> MoreCommandProperty =
        AvaloniaProperty.Register<AccountHeader, ICommand?>(nameof(MoreCommand));

    /// <summary>Gets or sets the display name of the account.</summary>
    public string AccountName
    {
        get => GetValue(AccountNameProperty);
        set => SetValue(AccountNameProperty, value);
    }

    /// <summary>Gets or sets the email address for the account.</summary>
    public string Email
    {
        get => GetValue(EmailProperty);
        set => SetValue(EmailProperty, value);
    }

    /// <summary>Gets or sets the cloud provider kind.</summary>
    public ProviderKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Gets or sets the current sync status.</summary>
    public SyncStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the Pause button is clicked.</summary>
    public ICommand? PauseCommand
    {
        get => GetValue(PauseCommandProperty);
        set => SetValue(PauseCommandProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the Settings button is clicked.</summary>
    public ICommand? SettingsCommand
    {
        get => GetValue(SettingsCommandProperty);
        set => SetValue(SettingsCommandProperty, value);
    }

    /// <summary>Gets or sets the command invoked when the More (⋯) button is clicked.</summary>
    public ICommand? MoreCommand
    {
        get => GetValue(MoreCommandProperty);
        set => SetValue(MoreCommandProperty, value);
    }

    /// <summary>Initializes a new instance of <see cref="AccountHeader"/>.</summary>
    public AccountHeader() => InitializeComponent();

    /// <summary>Returns the label and tone for the status pill based on the given sync status.</summary>
    /// <param name="status">The sync status to map.</param>
    /// <returns>A tuple of (label, tone) appropriate for <see cref="StatusPill"/>.</returns>
    public static (string Label, Tone Tone) GetStatusPillConfig(SyncStatus status) =>
        status switch
        {
            SyncStatus.Ok      => ("All synced", Tone.Good),
            SyncStatus.Syncing => ("Syncing", Tone.Primary),
            SyncStatus.Warn    => ("Warning", Tone.Warn),
            SyncStatus.Paused  => ("Paused", Tone.Neutral),
            _                  => ("Unknown", Tone.Neutral)
        };

    /// <summary>Returns the human-readable display name for the given provider kind.</summary>
    /// <param name="kind">The provider kind to map.</param>
    /// <returns>The display name string.</returns>
    public static string GetProviderDisplayName(ProviderKind kind) =>
        kind switch
        {
            ProviderKind.OneDrive    => "OneDrive",
            ProviderKind.GoogleDrive => "Google Drive",
            ProviderKind.Dropbox     => "Dropbox",
            _                        => "Unknown"
        };

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        updateBorder();
        updateProviderMark();
        updateNameText();
        updateProviderNameText();
        updateStatusPill();
        updateEmailText();
        updateButtons();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == AccountNameProperty) updateNameText();
        if (change.Property == EmailProperty) updateEmailText();
        if (change.Property == KindProperty) { updateProviderMark(); updateProviderNameText(); }
        if (change.Property == StatusProperty) updateStatusPill();
        if (change.Property == PauseCommandProperty || change.Property == SettingsCommandProperty || change.Property == MoreCommandProperty) updateButtons();
    }

    private void updateBorder()
    {
        if (ContainerBorder is null) return;

        if (this.TryFindResource("Border", out var res) && res is IBrush borderBrush)
            ContainerBorder.BorderBrush = borderBrush;
    }

    private void updateProviderMark()
    {
        if (ProviderMarkControl is null) return;

        ProviderMarkControl.Kind = Kind;
        ProviderMarkControl.Size = 42;
    }

    private void updateNameText()
    {
        if (NameText is null) return;

        NameText.Text = AccountName;

        if (this.TryFindResource("Ink", out var res) && res is IBrush brush)
            NameText.Foreground = brush;
    }

    private void updateProviderNameText()
    {
        if (ProviderNameText is null) return;

        ProviderNameText.Text = GetProviderDisplayName(Kind);

        if (this.TryFindResource("Ink3", out var res) && res is IBrush brush)
            ProviderNameText.Foreground = brush;
    }

    private void updateStatusPill()
    {
        if (StatusPillControl is null) return;

        var (label, tone) = GetStatusPillConfig(Status);
        StatusPillControl.Label = label;
        StatusPillControl.Tone = tone;
    }

    private void updateEmailText()
    {
        if (EmailText is null) return;

        EmailText.Text = Email;

        if (this.TryFindResource("Ink3", out var res) && res is IBrush brush)
            EmailText.Foreground = brush;
    }

    private void updateButtons()
    {
        if (PauseButton is null || SettingsButton is null || MoreButton is null) return;

        PauseButton.Command = PauseCommand;
        SettingsButton.Command = SettingsCommand;
        MoreButton.Command = MoreCommand;
    }
}
