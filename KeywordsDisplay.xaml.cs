using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Brendan_Stock_Media_Distributor;

/// <summary>
/// Interaction logic for KeywordsDisplay.xaml
/// </summary>
public partial class KeywordsDisplay : UserControl
{
    public List<string> Keywords { get; }

    public KeywordsDisplay()
    {
        Keywords = [];

        InitializeComponent();

        addButton.Click += _AddButton_Click;
        inputText.KeyUp += _KeywordsDisplay_KeyUp;
        clearButton.Click += _ClearButton_Click;
        removeButton.Click += _RemoveButton_Click;
    }

    private void _KeywordsDisplay_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddHander();
        }
    }

    private void _RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (Keywords.Count == 0)
        {
            return;
        }

        int idx = keywordsDisplay.SelectedIndex;
        if (idx == -1)
        {
            return;
        }

        Keywords.RemoveAt(idx);
        keywordsDisplay.Items.RemoveAt(idx);

        keywordsCountLabel.Content = Keywords.Count;
    }

    private void _ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Keywords.Clear();
        keywordsDisplay.Items.Clear();

        keywordsCountLabel.Content = "0";
    }

    private void _AddButton_Click(object sender, RoutedEventArgs e)
    {
        AddHander();
    }

    private void AddHander()
    {
        string inputText = this.inputText.Text;
        if (string.IsNullOrWhiteSpace(inputText))
        {
            this.inputText.Text = "";
            this.inputText.Focus();
            return;
        }

        inputText = inputText.Trim();

        while (inputText.Contains("  "))
        {
            inputText = inputText.Replace("  ", " ");
        }

        inputText = inputText.ToLower();

        if (Keywords.Contains(inputText))
        {
            this.inputText.Text = "";
            this.inputText.Focus();
            return;
        }

        Keywords.Add(inputText);
        keywordsDisplay.Items.Add(inputText);

        this.inputText.Text = "";

        keywordsCountLabel.Content = Keywords.Count;

        this.inputText.Focus();
    }

    public void RefreshDisplay()
    {
        keywordsDisplay.Items.Clear();
        foreach (string kw in Keywords)
        {
            keywordsDisplay.Items.Add(kw);
        }

        keywordsCountLabel.Content = Keywords.Count;
    }
}
