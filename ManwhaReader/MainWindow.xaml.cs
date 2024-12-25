using System.Windows;
using System.Windows.Input;

namespace ManwhaReader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnSearchImageClicked(object sender, MouseButtonEventArgs e)
    {
        frame.Source = new Uri("Pages/SearchPage.xaml", UriKind.Relative);
    }
}