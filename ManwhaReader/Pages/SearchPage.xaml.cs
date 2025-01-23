using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ManwhaReader.Core.Interfaces;

namespace ManwhaReader.Pages;

public partial class SearchPage : Page
{
#pragma warning disable CS8618, CS9264
    public SearchPage()
#pragma warning restore CS8618, CS9264
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
            if (string.IsNullOrWhiteSpace(Dispatcher.Invoke(() => searchTextBox.Text)))
                return;

            if (e.Key != Key.Enter)
                return;
            
            if(busyIndicator.IsBusy)
                return;

            busyIndicator.IsBusy = true;
            e.Handled = true;
            await ShowResults();
            busyIndicator.IsBusy = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine(ex);
            e.Handled = false;
            busyIndicator.IsBusy = false;
        }
    }

    private async Task ShowResults()
    {
        var results = (await Provider.Search(searchTextBox.Text, loadImagesCheckBox.IsChecked ?? true)).ToList();

        if(!resultCountPanel.IsVisible)
            resultCountPanel.Visibility = Visibility.Visible;

        resultCountTextBlock.Text = results.Count.ToString();

        var list = new List<IProviderSearchComboBoxItem>();
        
        results.ForEach(x =>
        {
            var bmp = new BitmapImage();

            if (loadImagesCheckBox.IsChecked ?? true)
            {
                using var ms = new MemoryStream(x.ImageData);
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
            }
            
            list.Add(new ProviderSearchComboBoxItem
            {
                ImageSource = bmp,
                ProviderName = x.Title
            });
        });
        
        resultsList.ItemsSource = list;
    }

    private async void OnResultListItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            e.Handled = true;
            
            if(resultsList.SelectedItem == null)
                return;
            
            busyIndicator.IsBusy = true;
            var selected = (IProviderSearchComboBoxItem)resultsList.SelectedItem;

            var manwha = await Provider.GetManwhaByTitle(selected.ProviderName);

            NavigationService?.Navigate(new ManwhaPage(manwha));
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine(exception);
        }
        finally
        {
            resultsList.SelectedItem = null;
            busyIndicator.IsBusy = false;
        }
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
    
    private static IManwhaProvider GetProviderByName(string providerName) => GetProviders().FirstOrDefault(x => x.Name == providerName)!;
}