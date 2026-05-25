using Avalonia.Controls;

namespace AStar.Dev.OneDriveFunctional;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}