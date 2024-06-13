using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Brendan_Stock_Media_Distributor;
/// <summary>
/// Interaction logic for Settings.xaml
/// </summary>
public partial class Settings : Window
{
    private int backupSelectedOption = -1;
    public int BackupSelectedOption
    {
        get => backupSelectedOption;
        set
        {
            if (value is < -1 or > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            backupSelectedOption = value;
            switch (backupSelectedOption)
            {
                default:
                    noBackupRadio.IsChecked = true;
                    backupLocationSelector.LocationType = LocationType.None;
                    break;
                case 0:
                    noBackupRadio.IsChecked = false;
                    backupLocationSelector.LocationType = LocationType.Local;
                    break;
                case 1:
                    noBackupRadio.IsChecked = false;
                    backupLocationSelector.LocationType = LocationType.Cloud;
                    break;
            }
        }
    }
    public Settings()
    {
        InitializeComponent();
        RevertSettings();
        noBackupRadio.Checked += _NoBackupRadio_Checked;
        backupLocationSelector.LocationTypeChanged += _BackupLocationSelector_LocationTypeChanged;
        Closing += _Settings_Closing;
        revertButton.Click += _RevertButton_Click;
        saveButton.Click += _SaveButton_Click;
        resetButton.Click += _ResetButton_Click;
        googleVertexAIAccessToken.TextChanged += _GoogleVertexAIServiceKeyJson_TextChanged;
        localPath.TextChanged += _LocalPath_TextChanged;
        everypixelLabsClientID.TextChanged += _EverypixelLabsClientID_TextChanged;
        everypixelLabsSecret.TextChanged += _EverypixelLabsSecret_TextChanged;
    }

    private void _EverypixelLabsSecret_TextChanged(object sender, TextChangedEventArgs e)
    {
        everypixelLabsSecret.BorderBrush = SettingsObject.IsValidEverypixelLabsSecret(everypixelLabsSecret.Text)
            ? Brushes.Gray
            : Brushes.Red;
    }

    private void _EverypixelLabsClientID_TextChanged(object sender, TextChangedEventArgs e)
    {
        everypixelLabsClientID.BorderBrush = SettingsObject.IsValidEverypixelLabsClientID(everypixelLabsClientID.Text)
            ? Brushes.Gray
            : Brushes.Red;
    }

    private void _LocalPath_TextChanged(object sender, TextChangedEventArgs e)
    {
        string t = localPath.Text;
        int ss = localPath.SelectionStart;
        int sl = localPath.SelectionLength;
        int se = ss + sl;
        int n_ss = ss;
        int n_sl = sl;

        bool lws = false;
        StringBuilder n_t = new();
        bool Keep(int I)
        {
            char c = t[I];
            if (c == '\\')
            {
                if (lws)
                {
                    return false;
                }
                else
                {
                    lws = true;
                    return true;
                }
            }
            lws = false;
            if (c == ':')
            {
                return I == 1;
            }

            return !Shared.PathDisallowedChars.Contains(c);
        }
        for (int i = 0; i < ss; i++)
        {
            char c = t[i];
            if (Keep(i))
            {
                _ = n_t.Append(c);
            }
            else
            {
                n_ss--;
            }
        }
        for (int i = ss; i < se; i++)
        {
            char c = t[i];
            if (Keep(i))
            {
                _ = n_t.Append(c);
            }
            else
            {
                n_sl--;
            }
        }
        for (int i = se; i < t.Length; i++)
        {
            char c = t[i];
            if (Keep(i))
            {
                _ = n_t.Append(c);
            }
        }
        if (n_t.Length == 0)
        {
            localPath.Text = "";
            localPath.SelectionStart = 0;
            localPath.SelectionLength = 0;
            return;
        }
        if (n_t[^1] != '\\')
        {
            _ = n_t.Append('\\');
        }

        localPath.Text = n_t.ToString();
        localPath.SelectionStart = n_ss;
        localPath.SelectionLength = n_sl;
    }

    private void _GoogleVertexAIServiceKeyJson_TextChanged(object sender, TextChangedEventArgs e)
    {
        googleVertexAIAccessToken.BorderBrush = SettingsObject.IsValidGoogleVertexAIAccessToken(googleVertexAIAccessToken.Text)
            ? Brushes.Gray
            : Brushes.Red;
    }

    private void _ResetButton_Click(object sender, RoutedEventArgs e)
    {
        SetSettingsObject(new());
    }

    private void _SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
    }

    private void _RevertButton_Click(object sender, RoutedEventArgs e)
    {
        RevertSettings();
    }

    public (SettingsObject? Settings, string? ErrorMessage) GetSettingsObject()
    {
        string newSaveLocalPath = localPath.Text;
        int newBackupMaster = BackupSelectedOption;
        string newBackupLocalPath = backupLocationSelector.LocalDirectoryPath;
        string newBackupCloudProtocol = backupLocationSelector.CloudProtocol;
        string newBackupCloudHostName = backupLocationSelector.CloudHostName;
        string newBackupCloudPath = backupLocationSelector.CloudDirectoryPath;
        string newBackupCloudUsername = backupLocationSelector.CloudUsername;
        string newBackupCloudPassword = backupLocationSelector.CloudPassword;
        string newGoogleVertexAIProjectId = googleVertexAIProjectId.Text;
        string newGoogleVertexAIAccessToken = googleVertexAIAccessToken.Text;
        string newEverypixelLabsClientID = everypixelLabsClientID.Text;
        string newEverypixelLabsSecret = everypixelLabsSecret.Text;
        GenerateFromOptions newTitleGenerateFrom = titleFromWhat.Text switch
        {
            "Description" => GenerateFromOptions.Captions_Description,
            "Categories" => GenerateFromOptions.Categories,
            "Keywords" => GenerateFromOptions.Keywords,
            _ => GenerateFromOptions.Image,
        };
        GenerateFromOptions newDescriptionGenerateFrom = descriptionFromWhat.Text switch
        {
            "Title" => GenerateFromOptions.Captions_Title,
            "Categories" => GenerateFromOptions.Categories,
            "Keywords" => GenerateFromOptions.Keywords,
            _ => GenerateFromOptions.Image,
        };
        GenerateFromOptions newCategoriesGenerateFrom = categoriesFromWhat.Text switch
        {
            "Title" => GenerateFromOptions.Captions_Title,
            "Description" => GenerateFromOptions.Captions_Description,
            "Keywords" => GenerateFromOptions.Keywords,
            _ => GenerateFromOptions.Image,
        };
        GenerateFromOptions newKeywordsGenerateFrom = keywordsFromWhat.Text switch
        {
            "Title" => GenerateFromOptions.Captions_Title,
            "Description" => GenerateFromOptions.Captions_Description,
            "Categories" => GenerateFromOptions.Categories,
            _ => GenerateFromOptions.Image,
        };

        SettingsObject s = new();

        try
        {
            s.SaveLocalPath = newSaveLocalPath;
        }
        catch
        {
            return (null, "Save.Local.Path is invalid.");
        }

        try
        {
            s.BackupMaster = newBackupMaster;
        }
        catch
        {
            return (null, "Backup.Master is invalid.");
        }
        switch (newBackupMaster)
        {
            case 0:
                try
                {
                    s.BackupLocalPath = newBackupLocalPath;
                }
                catch
                {
                    return (null, "Backup.Local.Path is invalid.");
                }
                break;
            case 1:
                try
                {
                    s.BackupCloudProtocol = newBackupCloudProtocol;
                }
                catch
                {
                    return (null, "Backup.Cloud.Protocol is invalid.");
                }
                try
                {
                    s.BackupCloudHostName = newBackupCloudHostName;
                }
                catch
                {
                    return (null, "Backup.Cloud.HostName is invalid.");
                }
                try
                {
                    s.BackupCloudPath = newBackupCloudPath;
                }
                catch
                {
                    return (null, "Backup.Cloud.Path is invalid.");
                }
                try
                {
                    s.BackupCloudUsername = newBackupCloudUsername;
                }
                catch
                {
                    return (null, "Backup.Cloud.Username is invalid.");
                }
                try
                {
                    s.BackupCloudPassword = newBackupCloudPassword;
                }
                catch
                {
                    return (null, "Backup.Cloud.Password is invalid.");
                }
                break;
        }

        try
        {
            s.GoogleVertexAIProjectId = newGoogleVertexAIProjectId;
        }
        catch
        {
            return (null, "GoogleVertexAI.ProjectId is invalid.");
        }

        try
        {
            s.GoogleVertexAIAccessToken = newGoogleVertexAIAccessToken;
        }
        catch
        {
            return (null, "GoogleVertexAI.AccessToken is invalid.");
        }

        try
        {
            s.EverypixelLabsClientID = newEverypixelLabsClientID;
        }
        catch
        {
            return (null, "EverypixelLabs.CientID is invalid.");
        }
        try
        {
            s.EverypixelLabsSecret = newEverypixelLabsSecret;
        }
        catch
        {
            return (null, "EverypixelLabs.Secret is invalid.");
        }

        try
        {
            s.TitleGenerateFrom = newTitleGenerateFrom;
        }
        catch
        {
            return (null, "CostOptimizations.TitleGenerateFrom is invalid.");
        }
        try
        {
            s.DescriptionGenerateFrom = newDescriptionGenerateFrom;
        }
        catch
        {
            return (null, "CostOptimizations.DescriptionGenerateFrom is invalid.");
        }
        try
        {
            s.CategoriesGenerateFrom = newCategoriesGenerateFrom;
        }
        catch
        {
            return (null, "CostOptimizations.CategoriesGenerateFrom is invalid.");
        }
        try
        {
            s.KeywordsGenerateFrom = newKeywordsGenerateFrom;
        }
        catch
        {
            return (null, "CostOptimizations.KeywordsGenerateFrom is invalid.");
        }

        return (s, null);
    }
    public void SetSettingsObject(SettingsObject Settings)
    {
        localPath.Text = Settings.SaveLocalPath;
        BackupSelectedOption = Settings.BackupMaster;
        backupLocationSelector.LocalDirectoryPath = Settings.BackupLocalPath;
        backupLocationSelector.CloudProtocol = Settings.BackupCloudProtocol;
        backupLocationSelector.CloudHostName = Settings.BackupCloudHostName;
        backupLocationSelector.CloudDirectoryPath = Settings.BackupCloudPath;
        backupLocationSelector.CloudUsername = Settings.BackupCloudUsername;
        backupLocationSelector.CloudPassword = Settings.BackupCloudPassword;
        googleVertexAIProjectId.Text = Settings.GoogleVertexAIProjectId;
        googleVertexAIAccessToken.Text = Settings.GoogleVertexAIAccessToken;
        everypixelLabsClientID.Text = Settings.EverypixelLabsClientID;
        everypixelLabsSecret.Text = Settings.EverypixelLabsSecret;
        titleFromWhat.Text = Settings.TitleGenerateFrom switch
        {
            GenerateFromOptions.Captions_Title => "Title",
            GenerateFromOptions.Captions_Description => "Description",
            GenerateFromOptions.Categories => "Categories",
            GenerateFromOptions.Keywords => "Keywords",
            _ => "Image",
        };
        descriptionFromWhat.Text = Settings.DescriptionGenerateFrom switch
        {
            GenerateFromOptions.Captions_Title => "Title",
            GenerateFromOptions.Captions_Description => "Description",
            GenerateFromOptions.Categories => "Categories",
            GenerateFromOptions.Keywords => "Keywords",
            _ => "Image",
        };
        categoriesFromWhat.Text = Settings.CategoriesGenerateFrom switch
        {
            GenerateFromOptions.Captions_Title => "Title",
            GenerateFromOptions.Captions_Description => "Description",
            GenerateFromOptions.Categories => "Categories",
            GenerateFromOptions.Keywords => "Keywords",
            _ => "Image",
        };
        keywordsFromWhat.Text = Settings.KeywordsGenerateFrom switch
        {
            GenerateFromOptions.Captions_Title => "Title",
            GenerateFromOptions.Captions_Description => "Description",
            GenerateFromOptions.Categories => "Categories",
            GenerateFromOptions.Keywords => "Keywords",
            _ => "Image",
        };
    }
    public void SaveSettings()
    {
        void SetEnabled(bool IsEnabled)
        {
            this.IsEnabled = IsEnabled;
            Windows.mainWindow.IsEnabled = IsEnabled;
        }
        SetEnabled(false);
        (SettingsObject? ns, string? errorMessage) = GetSettingsObject();
        if (ns is null)
        {
            _ = MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK);
            SetEnabled(true);
            return;
        }
        SettingsObject cs = Shared.CurrentSettings;
        if (ns == cs)
        {
            _ = MessageBox.Show("No settings were changed.", "Information", MessageBoxButton.OK);
            RevertSettings();
            SetEnabled(true);
            return;
        }

        if (ns.SaveLocalPath != cs.SaveLocalPath)
        {
            MessageBoxResult r0 = MessageBox.Show("Save path was changed. Do you want to copy all files, overwriting all contents of the new directory? If so, that may take some time.", "Selection", MessageBoxButton.YesNoCancel, MessageBoxImage.None, MessageBoxResult.Cancel);
            switch (r0)
            {
                case MessageBoxResult.Yes:
                    Directory.Delete(ns.SaveLocalPath, true);
                    Helpers.CopyDirectory(cs.SaveLocalPath, ns.SaveLocalPath, true);
                    break;
                case MessageBoxResult.No:
                    MessageBoxResult r1 = MessageBox.Show("Do you want to overwrite all files in the new directory?", "Selection", MessageBoxButton.YesNoCancel);
                    switch (r1)
                    {
                        case MessageBoxResult.Yes:
                            Directory.Delete(ns.SaveLocalPath, true);
                            _ = Directory.CreateDirectory(ns.SaveLocalPath);
                            break;
                        case MessageBoxResult.No:
                            break;
                        default:
                            SetEnabled(true);
                            return;
                    }
                    break;
                default:
                    SetEnabled(true);
                    return;
            }
        }
        ns.CopyTo(cs);
        cs.Save();
        RevertSettings();
        _ = MessageBox.Show("Changes successfully saved.", "Information", MessageBoxButton.OK);
        SetEnabled(true);
    }
    public void RevertSettings()
    {
        SetSettingsObject(Shared.CurrentSettings);
    }

    public bool TryClose()
    {
        if (!IsVisible)
        {
            return true;
        }

        SettingsObject? s = GetSettingsObject().Settings;
        if (s is null || (s != Shared.CurrentSettings))
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Some settings are unsaved. Close anyway?", "Unsaved Settings", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }
            }
            catch
            { }
        }

        return true;
    }
    public bool preclosed = false;
    private void _Settings_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (preclosed)
        {
            return;
        }

        e.Cancel = true;
        bool c = TryClose();
        if (c)
        {
            Hide();
            return;
        }
    }
    private void _NoBackupRadio_Checked(object sender, RoutedEventArgs e)
    {
        BackupSelectedOption = -1;
        backupLocationSelector.LocationType = LocationType.None;
    }
    private void _BackupLocationSelector_LocationTypeChanged(object? sender, LocationType e)
    {
        BackupSelectedOption = (int)e;
        noBackupRadio.IsChecked = false;
    }
}
