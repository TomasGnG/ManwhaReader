using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ManwhaReader.Core;

namespace ManwhaReader.Pages;

public partial class SearchPage : Page
{
    public SearchPage()
    {
        InitializeComponent();
        InitializeSearchProviders();
    }

    private void InitializeSearchProviders()
    {
        var type = typeof(ISearchCompletion);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<ISearchCompletion>()
            .ToList();
        
        types.ForEach(x =>
        {
            providerList.Items.Add(new ProviderSearchComboBoxItem
            {
                ImageSource = new BitmapImage(new Uri(x.ProviderImagePath, UriKind.Relative)),
                ProviderName = x.ProviderName
            });
        });
        
        providerList.SelectedIndex = 0;
    }

    private void OnSearchFieldKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        
        ShowResults();
    }

    private async void ShowResults()
    {
        var results = (await new ThunderscansSearchCompletion().Search(searchTextBox.Text)).ToList();

        if(!resultCountPanel.IsVisible)
            resultCountPanel.Visibility = Visibility.Visible;

        resultCountTextBlock.Text = results.Count.ToString();
        
        resultsList.Items.Clear();
        results.ForEach(x =>
        {
            resultsList.Items.Add(new ProviderSearchComboBoxItem
            {
                ImageSource = new BitmapImage(new Uri(x.ImageUrl)),
                ProviderName = x.Title
            });
        });
        // searchResultsTextBlock.Text = string.Empty;
        // results.ForEach(x => searchResultsTextBlock.Text += x + "\n");
    }
}