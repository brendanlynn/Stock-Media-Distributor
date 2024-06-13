using System.Windows.Controls;

namespace Brendan_Stock_Media_Distributor;
/// <summary>
/// Interaction logic for OptionsDisplay.xaml
/// </summary>
public partial class OptionsDisplay : UserControl
{
    public bool Options_Illustration
    {
        get => illustrationBox.IsChecked is true;
        set => illustrationBox.IsChecked = value;
    }
    public bool Options_MatureContent
    {
        get => matureBox.IsChecked is true;
        set => matureBox.IsChecked = value;
    }
    public bool Options_EditorialUseOnly
    {
        get => editorialBox.IsChecked is true;
        set => editorialBox.IsChecked = value;
    }
    public OptionsDisplay()
    {
        InitializeComponent();
    }
}
