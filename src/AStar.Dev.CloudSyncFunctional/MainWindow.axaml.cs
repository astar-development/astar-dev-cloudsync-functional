using AStar.Dev.CloudSyncFunctional.Workspace;
using Avalonia.Controls;
using Avalonia.Input;

namespace AStar.Dev.CloudSyncFunctional;

/// <summary>The main application window containing the titlebar, sidebar, and main pane.</summary>
public partial class MainWindow : Window
{
    /// <summary>Initializes the main window, sets the workspace as the data context, and wires chrome controls.</summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new WorkspaceViewModel();
        WireChrome();
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
}
