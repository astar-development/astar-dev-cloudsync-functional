using AStar.Dev.CloudSyncFunctional.Workspace;
using Avalonia.Controls;
using Avalonia.Input;

namespace AStar.Dev.CloudSyncFunctional;

/// <summary>The main application window containing the titlebar, sidebar, and main pane.</summary>
public partial class MainWindow : Window
{
    /// <summary>Initializes the main window with the provided workspace ViewModel.</summary>
    /// <param name="viewModel">The workspace ViewModel resolved from DI.</param>
    public MainWindow(WorkspaceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        WireChrome();
    }

    /// <summary>Initializes the main window with a default design-time workspace ViewModel.</summary>
    public MainWindow() : this(new WorkspaceViewModel())
    {
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
