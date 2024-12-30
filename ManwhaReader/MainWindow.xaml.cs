using System.Windows;
using System.Windows.Input;
using ManwhaReader.Pages;

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

    private SearchPage SearchPage { get; set; } = new();

    private void OnSearchImageClicked(object sender, MouseButtonEventArgs e)
    {
        frame.Navigate(SearchPage);
    }
}