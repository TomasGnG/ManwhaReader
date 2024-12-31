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

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
            return;

        if (frame.Content is not ManwhaPage)
            return;
        
        e.Handled = true;
        frame.NavigationService.GoBack();
        frame.NavigationService.RemoveBackEntry();
    }
}