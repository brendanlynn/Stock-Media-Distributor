using System.Windows;
using System.Windows.Controls;

namespace Brendan_Stock_Media_Distributor;
/// <summary>
/// Interaction logic for LocationSelector.xaml
/// </summary>
public partial class LocationSelector : UserControl
{
    private LocationType _locationType;
    public LocationType LocationType
    {
        get => _locationType;
        set
        {
            switch (value)
            {
                case LocationType.None:
                    localRB.IsChecked = false;
                    localGB.IsEnabled = false;
                    cloudRB.IsChecked = false;
                    cloudGB.IsEnabled = false;
                    break;
                case LocationType.Local:
                    localRB.IsChecked = true;
                    localGB.IsEnabled = true;
                    cloudRB.IsChecked = false;
                    cloudGB.IsEnabled = false;
                    break;
                case LocationType.Cloud:
                    localRB.IsChecked = false;
                    localGB.IsEnabled = false;
                    cloudRB.IsChecked = true;
                    cloudGB.IsEnabled = true;
                    break;
                default:
                    throw new ArgumentException();
            }
        }
    }
    public string LocalDirectoryPath
    {
        get => localPath.Text;
        set => localPath.Text = value;
    }
    public string CloudProtocol
    {
        get => cloudProtocol.SelectedIndex switch { 0 => "ftp", 1 => "ftps", _ => "" };
        set => cloudProtocol.SelectedIndex = value switch { "ftp" => 0, "ftps" => 1, _ => -1 };
    }
    public string CloudHostName
    {
        get => cloudHostName.Text;
        set => cloudHostName.Text = value;
    }
    public string CloudDirectoryPath
    {
        get => "/" + cloudPath.Text;
        set => cloudPath.Text = value.StartsWith('/') ? value[1..] : value;
    }
    public string CloudUsername
    {
        get => cloudUsername.Text;
        set => cloudUsername.Text = value;
    }
    public string CloudPassword
    {
        get => cloudPassword.Text;
        set => cloudPassword.Text = value;
    }
    public LocationSelector()
    {
        InitializeComponent();
        cloudProtocol.SelectedIndex = -1;
        localRB.Checked += _LocalRB_Checked;
        cloudRB.Checked += _CloudRB_Checked;
        _locationType = LocationType.None;
        LocationTypeChanged = delegate
        { };
        cloudHostName.TextChanged += _CloudHostName_TextChanged;
        ;
    }

    private void _CloudHostName_TextChanged(object sender, TextChangedEventArgs e)
    {
        string text = cloudHostName.Text;
        if (text.StartsWith("ftp://"))
        {
            cloudHostName.Text = text[6..];
            cloudProtocol.SelectedIndex = 0;
        }
        else if (text.StartsWith("ftps://"))
        {
            cloudHostName.Text = text[7..];
            cloudProtocol.SelectedIndex = 1;
        }
    }

    private void _LocalRB_Checked(object sender, RoutedEventArgs e)
    {
        localGB.IsEnabled = true;
        cloudGB.IsEnabled = false;
        _locationType = LocationType.Local;
        LocationTypeChanged(this, LocationType.Local);
    }
    private void _CloudRB_Checked(object sender, RoutedEventArgs e)
    {
        localGB.IsEnabled = false;
        cloudGB.IsEnabled = true;
        _locationType = LocationType.Cloud;
        LocationTypeChanged(this, LocationType.Cloud);
    }
    public event EventHandler<LocationType> LocationTypeChanged;
}

public enum LocationType
{
    None = -1,
    Local = 0,
    Cloud = 1,
}