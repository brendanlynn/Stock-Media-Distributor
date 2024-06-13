using System.Windows.Controls;
using static Brendan_Stock_Media_Distributor.MediaDetails;

namespace Brendan_Stock_Media_Distributor;

/// <summary>
/// Interaction logic for CategoryDisplay.xaml
/// </summary>
public partial class CategoryDisplay : UserControl
{
    public int Category_Shutterstock_Primary_Index
    {
        get => shutterstock_primaryCategoryComboBox.SelectedIndex;
        set => shutterstock_primaryCategoryComboBox.SelectedIndex = value;
    }
    public string? Category_Shutterstock_Primary_Value
    {
        get
        {
            int i = Category_Shutterstock_Primary_Index;
            if (i == -1)
            {
                return null;
            }

            return Category_Shutterstock_Categories[i];
        }
        set
        {
            if (value is null)
            {
                Category_Shutterstock_Primary_Index = -1;
            }
            else
            {
                Category_Shutterstock_Primary_Index = Category_Shutterstock_Categories.IndexOf(value);
            }
        }
    }
    public int Category_Shutterstock_Secondary_Index
    {
        get => shutterstock_secondaryCategoryComboBox.SelectedIndex - 1;
        set => shutterstock_secondaryCategoryComboBox.SelectedIndex = value + 1;
    }
    public string? Category_Shutterstock_Secondary_Value
    {
        get
        {
            int i = Category_Shutterstock_Secondary_Index;
            if (i == -1)
            {
                return null;
            }

            return Category_Shutterstock_Categories[i];
        }
        set
        {
            if (value is null)
            {
                Category_Shutterstock_Secondary_Index = -1;
            }
            else
            {
                Category_Shutterstock_Secondary_Index = Category_Shutterstock_Categories.IndexOf(value);
            }
        }
    }
    public int Category_AdobeStock_Index
    {
        get => adobeStock_categoryComboBox.SelectedIndex;
        set => adobeStock_categoryComboBox.SelectedIndex = value;
    }
    public string? Category_AdobeStock_Value
    {
        get
        {
            int i = Category_AdobeStock_Index;
            if (i == -1)
            {
                return null;
            }

            return Category_AdobeStock_Categories[i];
        }
        set
        {
            if (value is null)
            {
                Category_AdobeStock_Index = -1;
            }
            else
            {
                Category_AdobeStock_Index = Category_AdobeStock_Categories.IndexOf(value);
            }
        }
    }
    public CategoryDisplay()
    {
        InitializeComponent();
        foreach (string category in Category_Shutterstock_Categories)
        {
            _ = shutterstock_primaryCategoryComboBox.Items.Add(category);
        }

        _ = shutterstock_secondaryCategoryComboBox.Items.Add("None");
        foreach (string category in Category_Shutterstock_Categories)
        {
            _ = shutterstock_secondaryCategoryComboBox.Items.Add(category);
        }
        Category_Shutterstock_Secondary_Index = -1;

        foreach (string category in Category_AdobeStock_Categories)
        {
            _ = adobeStock_categoryComboBox.Items.Add(category);
        }
    }
}
