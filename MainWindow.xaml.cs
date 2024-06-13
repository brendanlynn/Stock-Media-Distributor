using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static Brendan_Stock_Media_Distributor.Shared;
using static Brendan_Stock_Media_Distributor.Windows;

namespace Brendan_Stock_Media_Distributor;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private object? pastSender;

    public bool AllowImages => letImages.IsChecked;
    public bool AllowVideos => letVideos.IsChecked;

    public Dictionary<ulong, MediaDetails> MediaMetadata { get; set; }

    private SplashScreen _ss;
    private readonly ManualResetEvent _ss_mre;
    private readonly DateTime _ss_t0;
    private readonly List<MediaSmallDisplay> _MSDitems = [];
    private Range _MSDShownRange = new(0, 0);
    private readonly DispatcherTimer _itemsReloadTimer;

    public ulong Id { get; private set; }

    public MainWindow()
    {
        _ss_mre = new(false);

        _ss_t0 = DateTime.Now;

        Thread newWindowThread = new(new ThreadStart(ThreadStartingPoint));
        newWindowThread.SetApartmentState(ApartmentState.STA);
        newWindowThread.IsBackground = true;
        newWindowThread.Start();

        void ThreadStartingPoint()
        {
            _ss = new()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            _ss.Show();
            _ = _ss_mre.Set();
            Dispatcher.Run();
        }

        #region BasicInit
        mainWindow = this;
        bool anotherExists = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()!.Location)).Length > 1;
        if (anotherExists)
        {
            Application.Current.Shutdown();
        }

        CurrentSettings = SettingsObject.LoadObject();
        Init();
        InitializeComponent();
        _itemsReloadTimer = new()
        {
            Interval = new(0, 0, 0, 0, 50)
        };
        #endregion BasicInit

        #region EventHandlerAssignments
        settingsItem.Click += _SettingsItem_Click;
        Closing += _MainWindow_Closing;
        letImages.Checked += _FilterChangedHandler;
        letImages.Unchecked += _FilterChangedHandler;
        letVideos.Checked += _FilterChangedHandler;
        letVideos.Unchecked += _FilterChangedHandler;
        addFiles.Click += _AddFiles_Click;
        addDirectory.Click += _AddDirectory_Click;
        itemsPanel.SizeChanged += _ItemsPanel_SizeChanged;
        itemsViewer.ScrollChanged += _ItemsViewer_ScrollChanged;
        _itemsReloadTimer.Tick += _ItemsReloadTimer_Tick;
        saveButton.Click += _SaveButton_Click;
        revertButton.Click += _RevertButton_Click;
        clearAllButton.Click += _ClearAllButton_Click;
        captionsDisplay.titleGenerateButton.Click += _TitleGenerateButton_Click;
        captionsDisplay.descriptionGenerateButton.Click += _DescriptionGenerateButton_Click;
        captionsDisplay.generateButton.Click += _GenerateButton_Click;
        categoryDisplay.generateButton.Click += _GenerateButton_Click1;
        keywordsDisplay.generateButton.Click += _GenerateButton_Click2;
        generateAllButton.Click += _GenerateAllButton_Click;
        deleteButton.Click += _DeleteButton_Click;
        copyToButton.Click += _CopyToButton_Click;
        exportButton_shutterstock.Click += _ExportButton_Shutterstock_Click;
        exportButton_adobeStock.Click += _ExportButton_AdobeStock_Click;
        exportButton.Click += _ExportButton_Click;
        PreviewKeyUp += _MainWindow_PreviewKeyUp;
        #endregion EventHandlerAssignments

        LoadMediaMetadata();
        RefreshItemsDisplay();

        Loaded += _MainWindow_Loaded;
    }

    private void _MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Shift)
        {
            SaveInfo();
            e.Handled = true;
        }
    }
    #region EventHandlers
    private void _ExportButton_Shutterstock_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsEnabled = false;
            settings.IsEnabled = false;
            string? dp = ChoosePathForExport();
            if (dp is null)
            {
                return;
            }

            MediaDetails md = MediaMetadata[Id];

            string csv_shutterstock = MakeCSV_Shutterstock(md);

            string fn_csv_shutterstock = NameCSV_Shutterstock(md.Id);
            string fn_file = NameExportFile(md);

            fn_csv_shutterstock = Path.Combine(dp, fn_csv_shutterstock);
            fn_file = Path.Combine(dp, fn_file);

            try
            {
                md.CopyMediaTo(fn_file, false);
            }
            catch { }

            File.WriteAllText(fn_csv_shutterstock, csv_shutterstock, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occured: {ex.Message}");
        }
        finally
        {
            IsEnabled = true;
            settings.IsEnabled = true;
        }
    }
    private void _ExportButton_AdobeStock_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsEnabled = false;
            settings.IsEnabled = false;
            string? dp = ChoosePathForExport();
            if (dp is null)
            {
                return;
            }

            MediaDetails md = MediaMetadata[Id];

            string csv_adobeStock = MakeCSV_AdobeStock(md);

            string fn_csv_adobeStock = NameCSV_AdobeStock(md.Id);
            string fn_file = NameExportFile(md);

            fn_csv_adobeStock = Path.Combine(dp, fn_csv_adobeStock);
            fn_file = Path.Combine(dp, fn_file);

            try
            {
                md.CopyMediaTo(fn_file, false);
            }
            catch { }

            File.WriteAllText(fn_csv_adobeStock, csv_adobeStock, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occured: {ex.Message}");
        }
        finally
        {
            IsEnabled = true;
            settings.IsEnabled = true;
        }
    }
    private void _ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsEnabled = false;
            settings.IsEnabled = false;
            string? dp = ChoosePathForExport();
            if (dp is null)
            {
                return;
            }

            MediaDetails md = MediaMetadata[Id];

            string csv_shutterstock = MakeCSV_Shutterstock(md);
            string csv_adobeStock = MakeCSV_AdobeStock(md);

            string fn_csv_shutterstock = NameCSV_Shutterstock(md.Id);
            string fn_csv_adobeStock = NameCSV_AdobeStock(md.Id);
            string fn_file = NameExportFile(md);

            fn_csv_shutterstock = Path.Combine(dp, fn_csv_shutterstock);
            fn_csv_adobeStock = Path.Combine(dp, fn_csv_adobeStock);
            fn_file = Path.Combine(dp, fn_file);

            try
            {
                md.CopyMediaTo(fn_file, false);
            }
            catch { }

            File.WriteAllText(fn_csv_shutterstock, csv_shutterstock, Encoding.UTF8);
            File.WriteAllText(fn_csv_adobeStock, csv_adobeStock, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occured: {ex.Message}");
        }
        finally
        {
            IsEnabled = true;
            settings.IsEnabled = true;
        }
    }
    private void _CopyToButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            MediaDetails md = MediaMetadata[Id];
            string ext = Path.GetExtension(md.File_LocalName);
            SaveFileDialog sfd = new()
            {
                Title = "Choose External Copy-to Location",
                AddExtension = true,
                Filter = $"{ext[1..].ToUpper()} File|*{ext}",
            };
            if (sfd.ShowDialog() is true)
            {
                md.CopyMediaTo(sfd.FileName);
            }
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occured: {ex.Message}");
        }
        finally
        {
            IsEnabled = true;
            settings.IsEnabled = true;
        }
    }
    private void _DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult r = MessageBox.Show($"Are you sure you want to permanently delete image #{Id}?", "Permanent Deletion Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
        if (r == MessageBoxResult.Yes)
        {
            try
            {
                MediaDetails.Delete(Id);
            }
            catch { }
            _ = MediaMetadata.Remove(Id);
            ClearSelection();
            for (int i = 0; i < _MSDitems.Count; i++)
            {
                if (_MSDitems[i].MD.Id == Id)
                {
                    _MSDitems.RemoveAt(i);
                    itemsPanel.Children.RemoveAt(i);
                    break;
                }
            }
            itemsCountLabel.Content = $"{itemsPanel.Children.Count} item(s) found";
        }
    }
    private async void _GenerateAllButton_Click(object sender, RoutedEventArgs e)
    {
        Task StartGenerationTaskFromInt(int Index)
        {
            return Index switch
            {
                0 => Generate_Captions_Title(),
                1 => Generate_Captions_Description(),
                2 => Generate_Categories(),
                3 => Generate_Keywords(),
                _ => throw new Exception(),
            };
        }
        HashSet<int> w_title = CurrentSettings.TitleGenerateFrom == GenerateFromOptions.Image ? [] : [(int)CurrentSettings.TitleGenerateFrom - 1];
        HashSet<int> w_description = CurrentSettings.DescriptionGenerateFrom == GenerateFromOptions.Image ? [] : [(int)CurrentSettings.DescriptionGenerateFrom - 1];
        HashSet<int> w_categories = CurrentSettings.CategoriesGenerateFrom == GenerateFromOptions.Image ? [] : [(int)CurrentSettings.CategoriesGenerateFrom - 1];
        HashSet<int> w_keywords = CurrentSettings.KeywordsGenerateFrom == GenerateFromOptions.Image ? [] : [(int)CurrentSettings.KeywordsGenerateFrom - 1];
        await RunLongSafe(async () =>
        {
            List<HashSet<int>> execOrder = Helpers.GetFunctionExecutionOrder([w_title, w_description, w_categories, w_keywords]);
            foreach (HashSet<int> execs in execOrder)
            {
                Task[] ts = new Task[execs.Count];
                IEnumerator<int> execE = execs.GetEnumerator();
                for (int i = 0; i < ts.Length; ++i)
                {
                    if (!execE.MoveNext())
                    {
                        throw new Exception();
                    }

                    ts[i] = StartGenerationTaskFromInt(execE.Current);
                }
                for (int i = 0; i < ts.Length; ++i)
                {
                    await ts[i];
                }
            }
        });
    }
    private async void _GenerateButton_Click2(object sender, RoutedEventArgs e)
    {
        await RunLongSafe(Generate_Keywords);
    }

    private async void _GenerateButton_Click1(object sender, RoutedEventArgs e)
    {
        await RunLongSafe(Generate_Categories);
    }

    private async void _GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        await RunLongSafe(async () =>
        {
            Task a_title = Generate_Captions_Title();
            Task a_description = Generate_Captions_Description();
            await a_title;
            await a_description;
        });
    }
    private async void _TitleGenerateButton_Click(object sender, RoutedEventArgs e)
    {
        await RunLongSafe(Generate_Captions_Title);
    }

    private async void _DescriptionGenerateButton_Click(object sender, RoutedEventArgs e)
    {
        await RunLongSafe(Generate_Captions_Description);
    }

    private void _ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        ResetInfo();
    }

    private void _RevertButton_Click(object sender, RoutedEventArgs e)
    {
        RevertInfo();
    }

    private void _SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveInfo();
    }

    private void _ItemsReloadTimer_Tick(object? sender, EventArgs e)
    {
        _itemsReloadTimer.Stop();
        ConditionLoadUnloadForAllItems();
    }
    private void _ItemsViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        RestartItemsReloadTimerProgress();
    }

    private void _ItemsPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RestartItemsReloadTimerProgress();
    }

    private void _MainWindow_Loaded(object sender, RoutedEventArgs e)
    {

        DateTime t1 = DateTime.Now;
        int td = (int)(t1 - _ss_t0).TotalMilliseconds;
        if (td < 5000)
        {
            Thread.Sleep(5000 - td);
        }

        _ = _ss_mre.WaitOne();

        _ss.Dispatcher.Invoke(_ss.Close);

        _ = Activate();
    }
    private void _MediaSmallDisplay_Clicked(object? sender, ulong Id)
    {
        if (InfoHasChanged())
        {
            MessageBoxResult r = ShowDialogForUnsavedChangesToMetadata();
            switch (r)
            {
                case MessageBoxResult.Yes:
                    SaveInfo();
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    return;
            }
        }

        if (pastSender is MediaSmallDisplay msd0)
        {
            msd0.BorderBrush = Brushes.Black;
            msd0.border.BorderThickness = new(0);
        }
        if (sender is MediaSmallDisplay msd1)
        {
            msd1.BorderBrush = Brushes.Blue;
            msd1.border.BorderThickness = new(4);
            pastSender = sender;
        }
        ShowInfo(Id);
    }
    private void _AddFiles_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsEnabled = false;
            settings.IsEnabled = false;

            OpenFileDialog ofd = new()
            {
                Title = "Select Media to Add",
                Filter = "Media|*.jpg;*.png;*.mp4|Images|*.jpg;*.png|Videos|*.mp4",
                Multiselect = true,
            };
            if (ofd.ShowDialog() is not true)
            {
                return;
            }

            string[] fns = ofd.FileNames;

            foreach (string fn in fns)
            {
                if (!File.Exists(fn))
                {
                    continue;
                }
                try
                {
                    (ulong name, MediaDetails details) = MediaDetails.AddExternalFile(fn);
                    MediaMetadata.Add(name, details);
                }
                catch { }
            }
            RefreshItemsDisplay();
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occured: {ex.Message}");
        }
        finally
        {
            IsEnabled = true;
            settings.IsEnabled = true;
        }
    }
    private void _AddDirectory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsEnabled = false;
            settings.IsEnabled = false;

            OpenFolderDialog ofd = new()
            {
                Title = "Select Folder(s) With Media to Add",
                Multiselect = true,
            };
            if (ofd.ShowDialog() is not true)
            {
                return;
            }

            string[] fns = ofd.FolderNames;

            Queue<string> directories = [];
            foreach (string fn in fns)
            {
                if (!Directory.Exists(fn))
                {
                    continue;
                }

                directories.Enqueue(fn);
            }

            List<string> files = [];

            while (directories.Count > 0)
            {
                string fn = directories.Dequeue();
                string[] fls = Directory.GetFiles(fn);
                string[] sds = Directory.GetDirectories(fn);

                files.AddRange(fls);

                foreach (string s in sds)
                {
                    directories.Enqueue(s);
                }
            }

            foreach (string file in files)
            {
                try
                {
                    (ulong name, MediaDetails details) = MediaDetails.AddExternalFile(file);
                    MediaMetadata.Add(name, details);
                }
                catch { }
            }

            RefreshItemsDisplay();
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occured: {ex.Message}");
        }
        finally
        {
            IsEnabled = true;
            settings.IsEnabled = true;
        }
    }
    private void _FilterChangedHandler(object sender, RoutedEventArgs e)
    {
        RefreshItemsDisplay();
    }

    private void _MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (InfoHasChanged())
        {
            MessageBoxResult r = ShowDialogForUnsavedChangesToMetadata();
            switch (r)
            {
                case MessageBoxResult.Yes:
                    SaveInfo();
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    e.Cancel = true;
                    return;
            }
        }
        if (settings.TryClose())
        {
            settings.preclosed = true;
            Application.Current.Shutdown();
            return;
        }
        e.Cancel = true;
    }
    private void _SettingsItem_Click(object sender, RoutedEventArgs e)
    {
        settings.RevertSettings();
        _ = OpenWindow(settings);
    }
    #endregion EventHandlers
    public async Task Generate_Keywords()
    {
        MediaDetails md = MediaMetadata[Id];
        string[] keywords = md.File_MediaType switch
        {
            MediaType.Image => await MediaKeywording.EverypixelLabs_Keywords_Image(md.GetThumbnailPath()),
            MediaType.Video => await MediaKeywording.EverypixelLabs_Keywords_Video(md.GetThumbnailPath()),
            _ => throw new NotSupportedException("Cannot keyword audio."),
        };
        keywordsDisplay.Keywords.Clear();
        keywordsDisplay.Keywords.AddRange(keywords);
        keywordsDisplay.RefreshDisplay();
    }
    private void RestartItemsReloadTimerProgress()
    {
        _itemsReloadTimer.Stop();
        _itemsReloadTimer.Start();
    }
    public void ConditionLoadUnloadForAllItems()
    {
        const int initInc = 32;

        int idx;

        idx = Search(initInc, 0);
        if (idx != -1)
        {
            goto FoundIdx;
        }

        for (int inc = initInc; inc > 1; inc >>= 1)
        {
            int ofst = inc >> 1;
            idx = Search(inc, ofst);
            if (idx != -1)
            {
                goto FoundIdx;
            }
        }

        return;

        int Search(int Increment, int Offset)
        {
            for (int i = Offset; i < _MSDitems.Count; i += Increment)
            {
                if (_MSDitems[i].ConditionalLoadUnload())
                {
                    return i;
                }
            }

            return -1;
        }

        FoundIdx:

        for (int i = _MSDShownRange.Start.Value; i < int.Min(_MSDShownRange.End.Value, _MSDitems.Count - 1); i++)
        {
            _ = _MSDitems[i].ConditionalLoadUnload();
        }

        int si;
        for (si = idx; si >= 0; si--)
        {
            if (!_MSDitems[si].ConditionalLoadUnload())
            {
                break;
            }
        }

        si++;

        int ei;
        for (ei = idx; ei < _MSDitems.Count; ei++)
        {
            if (!_MSDitems[ei].ConditionalLoadUnload())
            {
                break;
            }
        }

        _MSDShownRange = new(si, ei);
    }
    public void ShowInfo(ulong Id)
    {
        this.Id = Id;
        MediaDetails md = MediaMetadata[Id];
        detailedDisplay.DisplayMedia(Id, md);

        captionsDisplay.Caption_Title = md.Captions_Title;
        captionsDisplay.Caption_Description = md.Captions_Description;

        categoryDisplay.Category_Shutterstock_Primary_Index = md.Category_Shutterstock_Primary_Index;
        categoryDisplay.Category_Shutterstock_Secondary_Index = md.Category_Shutterstock_Secondary_Index;
        categoryDisplay.Category_AdobeStock_Index = md.Category_AdobeStock_Index;

        List<string> otherKeywordList = keywordsDisplay.Keywords;
        otherKeywordList.Clear();
        otherKeywordList.AddRange(md.Keywords);
        keywordsDisplay.RefreshDisplay();

        optionsDisplay.Options_Illustration = md.Options_Illustration;
        optionsDisplay.Options_MatureContent = md.Options_MatureContent;
        optionsDisplay.Options_EditorialUseOnly = md.Options_EditorialUseOnly;

        rightPane.IsEnabled = true;
    }
    private static MessageBoxResult ShowDialogForUnsavedChangesToMetadata()
    {
        return MessageBox.Show("There are still unsaved changes to metadata. Would you like to save changes?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.No);
    }
    public bool InfoHasChanged()
    {
        if (!rightPane.IsEnabled)
        {
            return false;
        }

        MediaDetails md = MediaMetadata[Id];

        if (captionsDisplay.Caption_Title != md.Captions_Title ||
            captionsDisplay.Caption_Description != md.Captions_Description)
        {
            return true;
        }

        if (categoryDisplay.Category_Shutterstock_Primary_Index != md.Category_Shutterstock_Primary_Index ||
            categoryDisplay.Category_Shutterstock_Secondary_Index != md.Category_Shutterstock_Secondary_Index ||
            categoryDisplay.Category_AdobeStock_Index != md.Category_AdobeStock_Index)
        {
            return true;
        }

        if (optionsDisplay.Options_Illustration != md.Options_Illustration ||
            optionsDisplay.Options_MatureContent != md.Options_MatureContent ||
            optionsDisplay.Options_EditorialUseOnly != md.Options_EditorialUseOnly)
        {
            return true;
        }

        List<string> oldKeywords = md.Keywords;
        List<string> newKeywords = keywordsDisplay.Keywords;
        if (oldKeywords.Count != newKeywords.Count)
        {
            return true;
        }
        for (int i = 0; i < oldKeywords.Count; ++i)
        {
            if (oldKeywords[i] != newKeywords[i])
            {
                return true;
            }
        }

        return false;
    }
    public void RevertInfo()
    {
        ShowInfo(Id);
    }

    public void SaveInfo()
    {
        MediaDetails md = MediaMetadata[Id];

        md.Captions_Title = captionsDisplay.Caption_Title;
        md.Captions_Description = captionsDisplay.Caption_Description;

        md.Category_Shutterstock_Primary_Index = categoryDisplay.Category_Shutterstock_Primary_Index;
        md.Category_Shutterstock_Secondary_Index = categoryDisplay.Category_Shutterstock_Secondary_Index;
        md.Category_AdobeStock_Index = categoryDisplay.Category_AdobeStock_Index;

        md.Keywords.Clear();
        md.Keywords.AddRange(keywordsDisplay.Keywords);

        md.Options_Illustration = optionsDisplay.Options_Illustration;
        md.Options_MatureContent = optionsDisplay.Options_MatureContent;
        md.Options_EditorialUseOnly = optionsDisplay.Options_EditorialUseOnly;

        MediaMetadata[Id] = md;
        md.Save();
    }
    public void ResetInfo()
    {
        captionsDisplay.Caption_Title = "";
        captionsDisplay.Caption_Description = "";

        categoryDisplay.Category_Shutterstock_Primary_Index = -1;
        categoryDisplay.Category_Shutterstock_Secondary_Index = -1;
        categoryDisplay.Category_AdobeStock_Index = -1;

        keywordsDisplay.Keywords.Clear();
        keywordsDisplay.RefreshDisplay();

        optionsDisplay.Options_Illustration = false;
        optionsDisplay.Options_MatureContent = false;
        optionsDisplay.Options_EditorialUseOnly = false;
    }
    public async Task RunLongSafe(Func<Task> Action)
    {
        try
        {
            IsEnabled = false;
            settings.IsEnabled = false;
            await Action();
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show("An error occured: " + ex.Message);
        }
        finally
        {
            IsEnabled = true;
            settings.IsEnabled = true;
        }
    }
    public async Task Generate_Captions_Title()
    {
        string fp = MediaMetadata[Id].GetThumbnailPath();
        switch (CurrentSettings.TitleGenerateFrom)
        {
            case GenerateFromOptions.Captions_Description:
                string desc = captionsDisplay.Caption_Description;
                if (string.IsNullOrWhiteSpace(desc))
                {
                    _ = MessageBox.Show("Cannot generate title from description. There is no description.", "Title Generation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string prompt =
                    $"Here is a description for an image in JSON format: {JsonConvert.SerializeObject(desc)}. " +
                    "Summarize the description in one short sentence with only spaces and lowercase letters. " +
                    "If you need commas or periods, your sentence is too long. Remember to start with lowercase \"a\" or \"an\". " +
                    "Output ONLY the description. Do not format it in JSON.";
                captionsDisplay.Caption_Title = await Prompt(prompt);
                break;
            default:
                (string? text, string? error) = await ImageCaptioning.CaptionImage(fp);
                if (text is null)
                {
                    _ = MessageBox.Show(error ?? "An error occured.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                captionsDisplay.Caption_Title = text;
                break;
        }
    }
    public async Task Generate_Captions_Description()
    {
        const string prompt = "Generate a detailed description of the image. The length of the description should not exceed 200 characters. Return ONLY the description.";
        captionsDisplay.Caption_Description = await PromptWithImage(prompt);
    }
    public static async Task<string> Prompt(string Prompt)
    {
        return (await GeminiVision.Prompt(Prompt)).Trim();
    }
    public async Task<string> PromptWithImage(string Prompt)
    {
        string fp = MediaMetadata[Id].GetThumbnailPath();
        FileInfo fi = new(fp);
        string mimeType = GetMimeType(fi.Extension) ?? throw new Exception("File extension unrecognized.");
        string text = await GeminiVision.Prompt(Prompt, fp, mimeType);
        return text.Trim();
    }
    public async Task Generate_Categories()
    {
        string p1 = CurrentSettings.CategoriesGenerateFrom switch
        {
            GenerateFromOptions.Captions_Title
                => "Here is a JSON-formatted string of the title of an image: "
                 + JsonConvert.SerializeObject(captionsDisplay.Caption_Title)
                 + ". ",
            GenerateFromOptions.Captions_Description
                => "Here is a JSON-formatted string of the description of an image: "
                 + JsonConvert.SerializeObject(captionsDisplay.Caption_Description)
                 + ". ",
            GenerateFromOptions.Keywords
                => "Here is a JSON-formatted list of keywords for an image: "
                 + JsonConvert.SerializeObject(keywordsDisplay.Keywords)
                 + ". ",
            _
                => "",
        };
        string p2 = CurrentSettings.CategoriesGenerateFrom switch
        {
            GenerateFromOptions.Captions_Title => "title",
            GenerateFromOptions.Captions_Description => "description",
            GenerateFromOptions.Keywords => "keywords",
            _ => "image",
        };
        string prompt_shutterstock =
            p1 +
            "Here is a JSON-formatted list of potential categories: " +
            JsonConvert.SerializeObject(MediaDetails.Category_Shutterstock_Categories) +
            $". If you think only one category matches the {p2} provided, output ONLY that one " +
            $"category. If you think two categories closely match the {p2} " +
            "provided, output ONLY those two categories, in the format: \"<primary category>,<secondary category>\" -- NOT in JSON " +
            $"format. If you think more than two categories closely match the {p2}, pick only the most relevant" +
            " two.";
        string prompt_adobeStock =
            p1 +
            "Here is a JSON-formatted list of potential categories: " +
            "[\"" + string.Join("\", \"", MediaDetails.Category_AdobeStock_Categories) + "\"]" +
            $". Find the most relevant category for the provided {p2} and output ONLY that category, " +
            "as it was given, without surrounding quotes.";

        string result_shutterstock;
        string result_adobeStock;

        if (CurrentSettings.CategoriesGenerateFrom == GenerateFromOptions.Image)
        {
            result_shutterstock = await PromptWithImage(prompt_shutterstock);
            result_adobeStock = await PromptWithImage(prompt_adobeStock);
        }
        else
        {
            result_shutterstock = await Prompt(prompt_shutterstock);
            result_adobeStock = await Prompt(prompt_adobeStock);
        }

        string[] shutterstock_categories = result_shutterstock.Split(',');
        switch (shutterstock_categories.Length)
        {
            case 1:
                categoryDisplay.Category_Shutterstock_Primary_Value = shutterstock_categories[0];
                categoryDisplay.Category_Shutterstock_Secondary_Value = "the answer to life, the universe, and everything is 42";
                break;
            case 2:
                categoryDisplay.Category_Shutterstock_Primary_Value = shutterstock_categories[0];
                categoryDisplay.Category_Shutterstock_Secondary_Value = shutterstock_categories[1];
                break;
            default:
                throw new Exception("AI returned invalid output.");
        }

        categoryDisplay.Category_AdobeStock_Value = result_adobeStock;
    }
    public void ClearSelection()
    {
        detailedDisplay.ClearInfo();
        ResetInfo();
        rightPane.IsEnabled = false;
    }
    public void RefreshItemsDisplay()
    {
        void Add(MediaSmallDisplay msd)
        {
            _ = itemsPanel.Children.Add(msd);
            msd.Clicked += _MediaSmallDisplay_Clicked;
        }
        foreach (object? child in itemsPanel.Children)
        {
            if (child is MediaSmallDisplay msd)
            {
                try
                {
                    msd.ConditionalUnload();
                }
                catch { }
            }
        }
        itemsPanel.Children.Clear();
        _MSDitems.Clear();
        int total = 0;
        if (AllowImages || AllowVideos)
        {
            foreach (KeyValuePair<ulong, MediaDetails> pair in MediaMetadata)
            {
                switch (pair.Value.File_MediaType)
                {
                    case MediaType.Image:
                        if (!AllowImages)
                        {
                            continue;
                        }

                        break;
                    case MediaType.Video:
                        if (!AllowVideos)
                        {
                            continue;
                        }

                        break;
                }
                MediaSmallDisplay msd = new(pair.Value, itemsViewer);
                Add(msd);
                _MSDitems.Add(msd);
                total++;
            }
        }
        else
        {
            foreach (KeyValuePair<ulong, MediaDetails> pair in MediaMetadata)
            {
                MediaSmallDisplay msd = new(pair.Value, itemsViewer);
                Add(msd);
                _MSDitems.Add(msd);
                total++;
            }
        }

        itemsCountLabel.Content = $"{total} item(s) found";
        ConditionLoadUnloadForAllItems();
    }
    public void LoadMediaMetadata()
    {
        MediaMetadata = MediaDetails.LoadAll();
    }

    public void LoadMediaMetadata(ulong Id)
    {
        MediaMetadata[Id] = MediaDetails.Load(Id) ?? throw new Exception();
    }

    public void SaveMediaMetadata(ulong Id)
    {
        MediaMetadata[Id].Save();
    }

    public static string? ChoosePathForExport()
    {
        OpenFolderDialog ofd = new()
        {
            Title = "Select Export Location"
        };
        if (ofd.ShowDialog() is not true)
        {
            return null;
        }
        return ofd.FolderName;
    }
    public static string MakeCSV_Shutterstock(MediaDetails MediaDetails)
    {
        string filename = MediaDetails.Id.ToString() + Path.GetExtension(MediaDetails.File_LocalName);

        string description = MediaDetails.Captions_Description;

        string keywords = string.Join(',', MediaDetails.Keywords);

        string categories = MediaDetails.Category_Shutterstock_Categories[MediaDetails.Category_Shutterstock_Primary_Index];
        int ssi = MediaDetails.Category_Shutterstock_Secondary_Index;
        if (ssi != -1)
        {
            categories += MediaDetails.Category_Shutterstock_Categories[ssi];
        }

        string illustration = MediaDetails.Options_Illustration ? "yes" : "no";
        string matureContent = MediaDetails.Options_MatureContent ? "yes" : "no";
        string editorial = MediaDetails.Options_EditorialUseOnly ? "yes" : "no";

        string[,] sCsv = {
            { "Filename", "Description", "Keywords", "Categories", "Editorial", "Mature content", "illustration", },
            { filename, description, keywords, categories, editorial, matureContent, illustration, },
        };
        return Helpers.CSVize(sCsv);
    }
    public static string MakeCSV_AdobeStock(MediaDetails MediaDetails)
    {
        string filename = MediaDetails.Id.ToString() + Path.GetExtension(MediaDetails.File_LocalName);

        string title = MediaDetails.Captions_Title;

        string keywords = string.Join(',', MediaDetails.Keywords);

        string category = MediaDetails.Category_AdobeStock_Categories[MediaDetails.Category_AdobeStock_Index];

        string[,] sCsv = {
            { "Filename", "Title", "Keywords", "Category", },
            { filename, title, keywords, category, },
        };
        return Helpers.CSVize(sCsv);
    }
    public static string NameExportFile(MediaDetails MediaDetails)
    {
        return MediaDetails.Id.ToString() + Path.GetExtension(MediaDetails.File_LocalName);
    }

    public static string NameCSV_Shutterstock(ulong Id)
    {
        return $"{Id}_shutterstock.csv";
    }

    public static string NameCSV_AdobeStock(ulong Id)
    {
        return $"{Id}_adobeStock.csv";
    }
}