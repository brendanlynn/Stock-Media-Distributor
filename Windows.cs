using System.Windows;

namespace Brendan_Stock_Media_Distributor;
public static class Windows
{
    public static MainWindow mainWindow;
    public static Settings settings;
    public static void Init()
    {
        settings = new();
    }

    public static bool OpenWindow(Window Window)
    {
        if (Window.WindowState == WindowState.Minimized)
        {
            Window.WindowState = WindowState.Normal;
        }
        else if (Window.Visibility == Visibility.Visible)
        {
            return Window.Activate();
        }
        else
        {
            Window.Show();
        }

        return true;
    }
}