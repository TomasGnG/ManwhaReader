using System.Windows;
using System.Windows.Controls;

namespace ManwhaReader;

public partial class BusyIndicator : UserControl
{
    public BusyIndicator()
    {
        InitializeComponent();
    }
    
    public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(BusyIndicator), new PropertyMetadata(false, OnIsBusyChanged));

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    
    private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BusyIndicator busyIndicator)
        {
            busyIndicator.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}