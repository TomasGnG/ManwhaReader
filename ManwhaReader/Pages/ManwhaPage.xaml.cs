using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ManwhaReader.Core.DatabaseObjects;

namespace ManwhaReader.Pages;

public partial class ManwhaPage : Page
{
    public ManwhaPage(Manwha manwha)
    {
        ArgumentNullException.ThrowIfNull(manwha);

        InitializeComponent();
        
        Manwha = manwha;
        SetContent();
    }
    
    private Manwha Manwha { get; }
    
    private void SetContent()
    {
        thumbnail.Source = new BitmapImage(new Uri(Manwha.ThumbnailUrl));
        title.Text = Manwha.Name;
        tags.ItemsSource = Manwha.Tags;
        status.Text = Manwha.Status;
        description.Text = Manwha.Description!;
    }

    private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
    {
        NavigationService?.GoBack();
        NavigationService?.RemoveBackEntry();
    }
}