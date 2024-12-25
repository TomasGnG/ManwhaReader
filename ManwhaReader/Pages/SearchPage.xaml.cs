using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ManwhaReader.Core.Interfaces;

namespace ManwhaReader.Pages;

public partial class SearchPage : Page
{
    public SearchPage()
    {
        InitializeComponent();
        InitializeSearchProviders();
        
        resultsList.SelectionChanged += OnResultListItemSelected;
        
        searchTextBox.Focus();
    }
    
    private IManwhaProvider Provider { get; set; }

    private void InitializeSearchProviders()
    {
        GetProviders().ForEach(x =>
        {
            providerList.Items.Add(new ProviderSearchComboBoxItem
            {
                ImageSource = new BitmapImage(new Uri(x.ImageUrl, UriKind.Relative)),
                ProviderName = x.Name
            });
        });
        
        providerList.SelectedIndex = 0;
    }

    private async void OnSearchFieldKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            Dispatcher.Invoke(() => Cursor = Cursors.Wait);
            
            if (e.Key != Key.Enter)
                return;

            await ShowResults();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine(ex);
        }
        finally
        {
            Cursor = Cursors.Arrow;
        }
    }

    private async Task ShowResults()
    {
        var results = (await Provider.Search(searchTextBox.Text)).ToList();

        if(!resultCountPanel.IsVisible)
            resultCountPanel.Visibility = Visibility.Visible;

        resultCountTextBlock.Text = results.Count.ToString();
        
        resultsList.Items.Clear();
        results.ForEach(x =>
        {
            BitmapImage bmp;
            using (var ms = new MemoryStream(x.ImageData))
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
            }
            
            resultsList.Items.Add(new ProviderSearchComboBoxItem
            {
                ImageSource = bmp,
                ProviderName = x.Title
            });
        });
    }

    private void OnResultListItemSelected(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void OnProviderListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = (IProviderSearchComboBoxItem) providerList.SelectedItem;
        Provider = GetProviderByName(selected.ProviderName);
        e.Handled = true;
    }

    private static List<IManwhaProvider> GetProviders() => AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(s => s.GetTypes())
        .Where(p => typeof(IManwhaProvider).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
        .Select(Activator.CreateInstance)
        .Cast<IManwhaProvider>()
        .ToList();
    
    private IManwhaProvider GetProviderByName(string providerName) => GetProviders().FirstOrDefault(x => x.Name == providerName)!;
}