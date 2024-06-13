using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;

namespace Brendan_Stock_Media_Distributor;
public static class Helpers
{
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        DirectoryInfo dir = new(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        }

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        _ = Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            _ = file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
    public static bool IsUserVisible(FrameworkElement element, FrameworkElement container)
    {
        if (!element.IsVisible)
        {
            return false;
        }

        Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
        Rect rect = new(0.0, 0.0, container.ActualWidth, container.ActualHeight);
        return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
    }
    public static string CSVize(string Value)
    {
        if (Value.Contains('\r') || Value.Contains('\n'))
        {
            throw new ArgumentException();
        }
        if (Value.Contains(',') || Value.Contains('"'))
        {
            Value = Value.Replace("\"", "\"\"");
            return "\"" + Value + "\"";
        }
        else
        {
            return Value;
        }
    }
    public static string CSVize(string[] Values)
    {
        StringBuilder r = new();
        for (int i = 0; i < Values.Length; ++i)
        {
            if (i != 0)
            {
                _ = r.Append(',');
            }
            _ = r.Append(CSVize(Values[i]));
        }
        return r.ToString();
    }
    public static string CSVize(string[,] Values)
    {
        StringBuilder r = new();
        for (int i = 0; i < Values.GetLength(0); i++)
        {
            for (int j = 0; j < Values.GetLength(1); j++)
            {
                if (j != 0)
                {
                    _ = r.Append(',');
                }
                _ = r.Append(CSVize(Values[i, j]));
            }
            _ = r.AppendLine();
        }
        return r.ToString();
    }
    public static List<HashSet<int>> GetFunctionExecutionOrder(List<HashSet<int>> Dependencies)
    {
        List<HashSet<int>> r = GetReverseFunctionExecutionOrder(Dependencies);
        r.Reverse();
        return r;
    }
    public static List<HashSet<int>> GetReverseFunctionExecutionOrder(List<HashSet<int>> Dependencies)
    {
        if (Dependencies.Count == 0)
        {
            return [];
        }

        int i;
        for (i = 0; i < Dependencies.Count; ++i)
        {
            HashSet<int> hs = Dependencies[i];
            foreach (int v in hs)
            {
                if (v == i)
                {
                    throw new ArgumentException();
                }
                if (v < 0 || v >= Dependencies.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        HashSet<int> oash = [];
        List<HashSet<int>> r = [[.. Enumerable.Range(0, Dependencies.Count)]];
        i = 0;
        while (true)
        {
            HashSet<int> chs = r[i++];
            HashSet<int> nhs = [];
            foreach (int v in chs)
            {
                nhs.UnionWith(Dependencies[v]);
            }
            if (nhs.Count == 0)
            {
                break;
            }
            if (chs.Count == nhs.Count)
            {
                throw new Exception("Circular reference detected.");
            }
            chs.ExceptWith(nhs);
            oash.UnionWith(chs);
            r.Add(nhs);
        }
        return r;
    }
}
