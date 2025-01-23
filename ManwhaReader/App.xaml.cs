using System.Windows;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

namespace ManwhaReader;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        const string connectionString = "XpoProvider=SQLite;Data Source=Data.db3;Read Only=false";
        
        XpoDefault.DataLayer = XpoDefault.GetDataLayer(connectionString, AutoCreateOption.DatabaseAndSchema);
        XpoDefault.IdentityMapBehavior = IdentityMapBehavior.Strong;
    }
}