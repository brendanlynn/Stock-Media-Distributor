using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Brendan_Stock_Media_Distributor;
/// <summary>
/// Interaction logic for MediaDetailedDisplay.xaml
/// </summary>
public partial class MediaDetailedDisplay : UserControl
{
    private readonly DispatcherTimer timer;
    public MediaDetailedDisplay()
    {

        InitializeComponent();

        timer = new()
        {
            Interval = new(0, 0, 0, 0, 25)
        };

        timer.Tick += _Timer_Tick;
        mediaDisplay.MediaEnded += _MediaDisplay_MediaEnded;
        mediaDisplay.MediaOpened += _MediaDisplay_MediaOpened;
    }
    public void ClearInfo()
    {
        idLabel.Content = "----";
        mediaDisplay.Close();
        fileSizeLabel.Content = "----";
        fileNameLabel.Text = "----";
        dimensionsLabel.Content = "----";
        samplingLabel.Content = "----";
        codecLabel.Content = "----";
        runtimeLabel.Content = "----";
    }
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
        MediaDetails md = Windows.mainWindow.MediaMetadata[Windows.mainWindow.Id];
        dimensionsLabel.Content = $"{(md.Image_Width != -1 ? md.Image_Width : mediaDisplay.NaturalVideoWidth)}x{(md.Image_Height != -1 ? md.Image_Height : mediaDisplay.NaturalVideoHeight)}";
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
    public void DisplayMedia(ulong Id)
    {
        DisplayMedia(Id, MediaDetails.Load(Id) ?? throw new Exception());
    }

    public void DisplayMedia(ulong Id, MediaDetails MediaDetails)
    {
        runtimeLabel.Content = "----";
        fileNameLabel.Text = MediaDetails.File_Name;
        FileInfo fi = new(MediaDetails.File_FilePath);
        fileSizeLabel.Content = $"{fi.Length:n0} bytes";
        idLabel.Content = Id.ToString();
        progressBarMoving.Width = 0;
        mediaDisplay.Source = new(MediaDetails.File_FilePath);
        mediaDisplay.Play();
        if (mediaDisplay.NaturalDuration.HasTimeSpan)
        {
            progressBar.Fill = Brushes.Orange;
        }
        else
        {
            progressBar.Fill = Brushes.Red;
        }

        timer.Start();
    }
}
