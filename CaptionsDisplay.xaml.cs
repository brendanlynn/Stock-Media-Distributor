using System.Windows.Controls;

namespace Brendan_Stock_Media_Distributor;

/// <summary>
/// Interaction logic for CaptionsDisplay.xaml
/// </summary>
public partial class CaptionsDisplay : UserControl
{
    public string Caption_Title
    {
        get => titleText.Text;
        set => titleText.Text = value;
    }
    public string Caption_Description
    {
        get => descriptionText.Text;
        set => descriptionText.Text = value;
    }
    public CaptionsDisplay()
    {
        InitializeComponent();
        titleText.TextChanged += _TitleText_TextChanged;
        descriptionText.TextChanged += _DescriptionText_TextChanged;
    }
    private void _TitleText_TextChanged(object sender, TextChangedEventArgs e)
    {
        titleInfoLabel.Content = $"{titleText.Text.Length} chars";
    }

    private void _DescriptionText_TextChanged(object sender, TextChangedEventArgs e)
    {
        descriptionInfoLabel.Content = $"{descriptionText.Text.Length} chars";
    }
}
