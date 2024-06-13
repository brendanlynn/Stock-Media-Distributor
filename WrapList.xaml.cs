using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Brendan_Stock_Media_Distributor;

/// <summary>
/// Interaction logic for WrapList.xaml
/// </summary>
public partial class WrapList : UserControl
{
    public IList<string> Items { get; }

    public int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => throw new NotImplementedException();
    }
    public WrapList()
    {
        InitializeComponent();
        WrappedListItemsList wlil = new();

        wlil.AddEvent += _Wlil_AddEvent;
        wlil.InsertEvent += _Wlil_InsertEvent;
        wlil.RemoveAtEvent += _Wlil_RemoveAtEvent;
        wlil.RemoveEvent += _Wlil_RemoveEvent;
        wlil.ClearEvent += _Wlil_ClearEvent;

        Items = wlil;
    }

    private void _Wlil_AddEvent(object? sender, string e)
    {
        wrapPanel.Children.Add(MakeElement(wrapPanel.Children.Count, e));
    }

    private void _Wlil_InsertEvent(object? sender, (int Index, string Item) e)
    {
        wrapPanel.Children.Insert(e.Index, MakeElement(e.Index, e.Item));
        if (_selectedIndex >= e.Index)
        {
            _selectedIndex++;
        }
    }
    private void _Wlil_RemoveAtEvent(object? sender, int e)
    {
        if (e == _selectedIndex)
        {
            _selectedIndex = -1;
            _ll = null;
        }
        else if (_selectedIndex > e)
        {
            _selectedIndex--;
        }

        wrapPanel.Children.RemoveAt(e);
    }
    private void _Wlil_RemoveEvent(object? sender, (int Index, string Item) e)
    {
        _Wlil_RemoveAtEvent(sender, e.Index);
    }

    private void _Wlil_ClearEvent(object? sender, EventArgs e)
    {
        wrapPanel.Children.Clear();
        _ll = null;
        _selectedIndex = -1;
    }

    private UIElement MakeElement(int Index, string Content)
    {
        Label l = new()
        {
            Content = Content,
            Background = Brushes.LightGray,
            Margin = new Thickness(2)
        };
        l.MouseLeftButtonUp += (object? sender, MouseButtonEventArgs e) => _Element_Clicked(sender as Label);
        return l;
    }

    private Label? _ll = null;
    private void _Element_Clicked(Label? Sender)
    {
        if (_ll is not null)
        {
            _ll.Background = Brushes.LightGray;
        }

        if (Sender is null)
        {
            _selectedIndex = -1;
            return;
        }
        _selectedIndex = wrapPanel.Children.IndexOf(Sender);
        Sender.Background = Brushes.LightBlue;
        _ll = Sender;
    }

    private class WrappedListItemsList
        : IList<string>
    {
        private readonly List<string> items;
        public WrappedListItemsList()
        {
            items = [];
            AddEvent = delegate
            { };
            InsertEvent = delegate
            { };
            RemoveAtEvent = delegate
            { };
            RemoveEvent = delegate
            { };
            ClearEvent = delegate
            { };
        }
        public WrappedListItemsList(IEnumerable<string> Items)
        {
            items = [.. Items];
            AddEvent = delegate
            { };
            InsertEvent = delegate
            { };
            RemoveAtEvent = delegate
            { };
            RemoveEvent = delegate
            { };
            ClearEvent = delegate
            { };
        }

        public event EventHandler<string> AddEvent;
        public event EventHandler<(int Index, string Item)> InsertEvent;
        public event EventHandler<int> RemoveAtEvent;
        public event EventHandler<(int Index, string Item)> RemoveEvent;
        public event EventHandler ClearEvent;

        public void Add(string Item)
        {
            items.Add(Item);
            AddEvent(this, Item);
        }
        public void Insert(int Index, string Item)
        {
            items.Insert(Index, Item);
            InsertEvent(this, (Index, Item));
        }
        public void RemoveAt(int Index)
        {
            items.RemoveAt(Index);
            RemoveAtEvent(this, Index);
        }
        public bool Remove(string Item)
        {
            int idx = items.IndexOf(Item);

            if (idx == -1)
            {
                return false;
            }

            items.RemoveAt(idx);
            RemoveEvent(this, (idx, Item));

            return true;
        }
        public void Clear()
        {
            items.Clear();
            ClearEvent(this, EventArgs.Empty);
        }

        public int Count => items.Count;

        public string this[int Index]
        {
            get => items[Index];
            set => items[Index] = value;
        }
        public bool Contains(string Item)
        {
            return items.Contains(Item);
        }

        public int IndexOf(string Item)
        {
            return items.IndexOf(Item);
        }

        public void CopyTo(string[] Array, int Index)
        {
            items.CopyTo(Array, Index);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }

        bool ICollection<string>.IsReadOnly => false;
    }
}
