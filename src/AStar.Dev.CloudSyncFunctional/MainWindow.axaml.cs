using Avalonia.Controls;

namespace AStar.Dev.CloudSyncFunctional;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}