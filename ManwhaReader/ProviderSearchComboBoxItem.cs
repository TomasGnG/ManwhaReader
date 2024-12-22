using System.Windows.Media;

namespace ManwhaReader;

public class ProviderSearchComboBoxItem : IProviderSearchComboBoxItem
{
    public required ImageSource ImageSource { get; init; }
    public required string ProviderName { get; init; }
}