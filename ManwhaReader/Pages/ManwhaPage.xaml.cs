using System.Windows.Controls;
using ManwhaReader.Core.DatabaseObjects;

namespace ManwhaReader.Pages;

public partial class ManwhaPage : Page
{
    public ManwhaPage(Manwha manwha)
    {
        InitializeComponent();
        
        Manwha = manwha;
    }

    private Manwha Manwha { get; }
}