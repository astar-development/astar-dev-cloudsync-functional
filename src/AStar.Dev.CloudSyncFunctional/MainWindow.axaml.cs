using System;
using System.Linq;
using AStar.Dev.CloudSyncFunctional.Accounts;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;

namespace AStar.Dev.CloudSyncFunctional;

/// <summary>The main application window containing the titlebar, sidebar, and main pane.</summary>
public partial class MainWindow : Window
{
    /// <summary>Initializes the main window, wires up chrome buttons, and subscribes to workspace changes.</summary>
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainWindowViewModel();
        DataContext = vm;

        WireChrome();
        SetWorkspaceSubtitle(vm);
        AccountCountText.Text = vm.Workspace.Accounts.Count.ToString();
        WireAccountSelection(vm);
    }

    private void WireChrome()
    {
        TitleBarGrid.AddHandler(PointerPressedEvent, OnTitleBarPressed);
        MinimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;
        MaximizeButton.Click += (_, _) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        CloseButton.Click += (_, _) => Close();
    }

    private void OnTitleBarPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed && !e.Handled)
            BeginMoveDrag(e);
    }

    private void SetWorkspaceSubtitle(MainWindowViewModel vm)
    {
        var totalTb = vm.Workspace.Accounts.Sum(a => a.TotalBytes) / 1_099_511_627_776.0;
        WorkspaceSubtitleText.Text = $"{vm.Workspace.Accounts.Count} accounts · {totalTb:F1} TB total";
    }

    private void WireAccountSelection(MainWindowViewModel vm)
    {
        AccountsListBox.SelectionChanged += (_, _) =>
        {
            if (AccountsListBox.SelectedItem is AccountViewModel clicked &&
                !ReferenceEquals(clicked, vm.Workspace.SelectedAccount))
                vm.Workspace.SelectedAccount = clicked;
        };

        vm.Workspace.WhenAnyValue(x => x.SelectedAccount)
            .Subscribe(UpdateMainPane);
    }

    private void UpdateMainPane(AccountViewModel? account)
    {
        if (account is null) return;

        AccountsListBox.SelectedItem = account;
        AccountHeaderControl.AccountName = account.Name;
        AccountHeaderControl.Email = account.Email;
        AccountHeaderControl.Kind = account.Kind;
        AccountHeaderControl.Status = account.Status;
        FolderItemsControl.ItemsSource = account.Folders;
    }
}
