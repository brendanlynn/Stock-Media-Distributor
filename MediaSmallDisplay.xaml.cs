using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Brendan_Stock_Media_Distributor;

/// <summary>
/// Interaction logic for MediaSmallDisplay.xaml
/// </summary>
public partial class MediaSmallDisplay : UserControl
{
    private readonly FrameworkElement boundingBox;

    public MediaDetails MD { get; }
    private readonly DispatcherTimer timer;
    private bool _playing = false;
    private bool _loaded = false;
    public MediaSmallDisplay(ulong Id, FrameworkElement BoundingBox)
        : this(MediaDetails.Load(Id) ?? throw new Exception(), BoundingBox) { }
    public MediaSmallDisplay(MediaDetails Details, FrameworkElement BoundingBox)
    {
        MD = Details;
        boundingBox = BoundingBox;

        InitializeComponent();

        timer = new()
        {
            Interval = new(0, 0, 0, 0, 25)
        };

        timer.Tick += _Timer_Tick;
        MouseEnter += _MediaSmallDisplay_MouseEnter;
        MouseLeave += _MediaSmallDisplay_MouseLeave;
        mediaDisplay.MediaEnded += _MediaDisplay_MediaEnded;
        mediaDisplay.MediaOpened += _MediaDisplay_MediaOpened;
        MouseLeftButtonUp += _MediaSmallDisplay_MouseLeftButtonUp;

        idLabel.Content = Details.Id.ToString();
    }
    private void _MediaSmallDisplay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Clicked(this, MD.Id);
    }

    public event EventHandler<ulong> Clicked;
    private void _Timer_Tick(object? sender, EventArgs e)
    {
        if (mediaDisplay.HasVideo)
        {
            Duration d = mediaDisplay.NaturalDuration;
            if (d.HasTimeSpan)
            {
                progressBarMoving.Width = progressBar.ActualWidth * mediaDisplay.Position.TotalMilliseconds / d.TimeSpan.TotalMilliseconds;
            }
        }
    }
    private void _MediaDisplay_MediaOpened(object sender, RoutedEventArgs e)
    {
        dimensionsLabel.Content = $"{(MD.Image_Width != -1 ? MD.Image_Width : mediaDisplay.NaturalVideoWidth)}x{(MD.Image_Height != -1 ? MD.Image_Height : mediaDisplay.NaturalVideoHeight)}";
        Duration d = mediaDisplay.NaturalDuration;
        if (d.HasTimeSpan)
        {
            progressBar.Fill = Brushes.Orange;
            runtimeLabel.Content = d.ToString();
        }
    }
    private void _MediaDisplay_MediaEnded(object sender, RoutedEventArgs e)
    {
        mediaDisplay.Position = new TimeSpan(0, 0, 0, 0, 0);
        mediaDisplay.Play();
    }
    private void _MediaSmallDisplay_MouseEnter(object sender, MouseEventArgs e)
    {
        mediaDisplay.Play();
        _playing = true;
        timer.Start();
    }
    private void _MediaSmallDisplay_MouseLeave(object sender, MouseEventArgs e)
    {
        mediaDisplay.Stop();
        _playing = false;
        progressBarMoving.Width = 0;
        timer.Stop();
    }
    public bool IsUserVisible()
    {
        return Helpers.IsUserVisible(this, boundingBox);
    }

    public void MediaLoad()
    {
        mediaDisplay.Source = new(MD.GetThumbnailPath());
        mediaDisplay.Play();
        mediaDisplay.Stop();
        if (mediaDisplay.NaturalDuration.HasTimeSpan)
        {
            progressBar.Fill = Brushes.Orange;
        }
        else
        {
            progressBar.Fill = Brushes.Red;
        }

        progressBarMoving.Width = 0;
        _loaded = true;
    }
    public void MediaUnload()
    {
        mediaDisplay.Close();
        mediaDisplay.Source = null;
        progressBar.Fill = Brushes.Black;
        progressBarMoving.Width = 0;
        _loaded = false;
    }
    public bool ConditionalLoadUnload()
    {
        if (IsUserVisible())
        {
            ConditionalLoad();
            return true;
        }
        else
        {
            ConditionalUnload();
            return false;
        }
    }
    public void ConditionalLoad()
    {
        if (!_loaded)
        {
            MediaLoad();
        }
    }
    public void ConditionalUnload()
    {
        if (_loaded)
        {
            MediaUnload();
        }
    }
}
