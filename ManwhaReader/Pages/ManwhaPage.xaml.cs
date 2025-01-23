using System.IO;
using System.Net.Http;
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
        title.Text = Manwha.Name;
        tags.ItemsSource = Manwha.TagsAsEnumerable();
        status.Text = Manwha.Status;
        description.Text = Manwha.Description!;
        SetThumbnailAsync();
    }

    private async void SetThumbnailAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");
            
            var bytes = await client.GetByteArrayAsync(Manwha.ThumbnailUrl);

            using var ms = new MemoryStream(bytes);
            var bitmap = new BitmapImage();
            
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            Dispatcher.Invoke(() => thumbnail.Source = bitmap);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine(e);
        }
        
        
    }

    private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
    {
        NavigationService?.GoBack();
        NavigationService?.RemoveBackEntry();
    }
}