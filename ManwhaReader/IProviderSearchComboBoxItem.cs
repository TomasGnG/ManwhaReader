using System.Windows.Media;

namespace ManwhaReader;

public interface IProviderSearchComboBoxItem
{
    ImageSource ImageSource { get; }
    string ProviderName { get; }
}