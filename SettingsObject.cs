using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Brendan_Stock_Media_Distributor;
public sealed partial class SettingsObject
    : IEquatable<SettingsObject>
{
    private static readonly char[] invalidPathChars = Path.GetInvalidPathChars();
    private static readonly string settingsDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Brendan Stock Media Distributor\\";
    private static readonly string settingsFile = settingsDirectory + "settings.json";

    #region Save
    [JsonIgnore]
    private string saveLocalPath = DefaultSaveLocalPath;
    public string SaveLocalPath
    {
        get => saveLocalPath;
        set
        {
            if (saveLocalPath == value)
            {
                return;
            }

            if (!IsValidSaveLocalPath(value))
            {
                throw new ArgumentException();
            }

            if (!value.EndsWith('\\'))
            {
                value += "\\";
            }

            saveLocalPath = value;
        }
    }
    public static bool IsValidSaveLocalPath(string value)
    {
        return IsValidPath(value);
    }

    public static readonly string DefaultSaveLocalPath = $"C:\\Users\\{Environment.UserName}\\Brendan Stock Media Distributor\\";

    public void CleanSave() { }
    public static void CopySave(SettingsObject Source, SettingsObject Destination)
    {
        Destination.saveLocalPath = Source.saveLocalPath;
    }

    public static bool EqualSave(SettingsObject Value0, SettingsObject Value1)
    {
        return Value0.saveLocalPath == Value1.saveLocalPath;
    }
    #endregion Save

    #region Backup
    [JsonIgnore]
    private int backupMaster = -1;
    public int BackupMaster
    {
        get => backupMaster;
        set
        {
            if (backupMaster == value)
            {
                return;
            }

            if (!IsValidBackupMaster(value))
            {
                throw new ArgumentOutOfRangeException();
            }

            backupMaster = value;
        }
    }
    public static bool IsValidBackupMaster(int value)
    {
        return value is >= -1 and <= 1;
    }

    [JsonIgnore]
    private string backupLocalPath = "";
    public string BackupLocalPath
    {
        get => backupLocalPath;
        set
        {
            if (backupLocalPath == value)
            {
                return;
            }

            if (!IsValidBackupLocalPath(value))
            {
                throw new ArgumentException();
            }

            backupLocalPath = value;
        }
    }
    public static bool IsValidBackupLocalPath(string value)
    {
        return IsValidPath(value);
    }

    [JsonIgnore]
    private string backupCloudProtocol = "";
    public string BackupCloudProtocol
    {
        get => backupCloudProtocol;
        set
        {
            if (backupCloudProtocol == value)
            {
                return;
            }

            if (!IsValidBackupCloudProtocol(value))
            {
                throw new ArgumentOutOfRangeException();
            }

            backupCloudProtocol = value;
        }
    }
    public static bool IsValidBackupCloudProtocol(string value)
    {
        return value is "ftp" or "ftps";
    }

    [JsonIgnore]
    private string backupCloudHostName = "";
    public string BackupCloudHostName
    {
        get => backupCloudHostName;
        set
        {
            if (backupCloudHostName == value)
            {
                return;
            }

            (bool isValid, string? hostname, string? protocol) = IsValidHostName(value);
            if (!isValid)
            {
                throw new ArgumentException();
            }

            backupCloudHostName = hostname!;
            if (protocol is not null)
            {
                backupCloudProtocol = protocol[..^3];
            }
        }
    }
    public static bool IsValidBackupCloudHostName(string value)
    {
        return IsValidHostName(value).IsValid;
    }

    [JsonIgnore]
    private string backupCloudPath = "";
    public string BackupCloudPath
    {
        get => backupCloudPath;
        set
        {
            if (backupCloudPath == value)
            {
                return;
            }

            if (!IsValidBackupCloudPath(value))
            {
                throw new ArgumentOutOfRangeException();
            }

            backupCloudPath = value;
        }
    }
    public static bool IsValidBackupCloudPath(string value)
    {
        return IsValidCloudPath(value);
    }

    public string BackupCloudUsername { get; set; } = "";

    public string BackupCloudPassword { get; set; } = "";

    public void CleanBackup()
    {
        if (backupMaster != 1)
        {
            backupLocalPath = "";
        }

        if (backupMaster != 2)
        {
            backupCloudProtocol = "";
            backupCloudHostName = "";
            backupCloudPath = "";
            BackupCloudUsername = "";
            BackupCloudPassword = "";
        }
    }
    public static void CopyBackup(SettingsObject Source, SettingsObject Destination)
    {
        Destination.backupMaster = Source.backupMaster;
        Destination.backupLocalPath = Source.backupLocalPath;
        Destination.backupCloudProtocol = Source.backupCloudProtocol;
        Destination.backupCloudHostName = Source.backupCloudHostName;
        Destination.backupCloudPath = Source.backupCloudPath;
        Destination.BackupCloudUsername = Source.BackupCloudUsername;
        Destination.BackupCloudPassword = Source.BackupCloudPassword;
    }
    public static bool EqualBackup(SettingsObject Value0, SettingsObject Value1)
    {
        if (Value0.backupMaster != Value1.backupMaster)
        {
            return false;
        }

        switch (Value0.backupMaster)
        {
            case 0:
                return Value0.backupLocalPath == Value1.backupLocalPath;
            case 1:
                if (Value0.backupCloudProtocol != Value1.backupCloudProtocol)
                {
                    return false;
                }

                if (Value0.backupCloudHostName != Value1.backupCloudHostName)
                {
                    return false;
                }

                if (Value0.backupCloudPath != Value1.backupCloudPath)
                {
                    return false;
                }

                if (Value0.BackupCloudUsername != Value1.BackupCloudUsername)
                {
                    return false;
                }

                if (Value0.BackupCloudPassword != Value1.BackupCloudPassword)
                {
                    return false;
                }

                return true;
        }
        return true;
    }
    #endregion

    #region GoogleVertexAI
    [JsonIgnore]
    private string googleVertexAIProjectId = "";
    public string GoogleVertexAIProjectId
    {
        get => googleVertexAIProjectId;
        set
        {
            if (googleVertexAIProjectId == value)
            {
                return;
            }

            if (!IsValidGoogleVertexAIProjectId(value))
            {
                throw new ArgumentException();
            }

            googleVertexAIProjectId = value;
        }
    }
    public static bool IsValidGoogleVertexAIProjectId(string value)
    {
        return !value.Contains(' ');
    }

    [JsonIgnore]
    private string googleVertexAIAccessToken = "";
    public string GoogleVertexAIAccessToken
    {
        get => googleVertexAIAccessToken;
        set
        {
            if (googleVertexAIAccessToken == value)
            {
                return;
            }

            if (!IsValidGoogleVertexAIAccessToken(value))
            {
                throw new ArgumentException();
            }

            googleVertexAIAccessToken = value;
        }
    }
    public static bool IsValidGoogleVertexAIAccessToken(string value)
    {
        return !value.Contains(' ');
    }

    public void CleanGoogleVertexAI() { }
    public static void CopyGoogleVertexAI(SettingsObject Source, SettingsObject Destination)
    {
        Destination.googleVertexAIProjectId = Source.googleVertexAIProjectId;
        Destination.googleVertexAIAccessToken = Source.googleVertexAIAccessToken;
    }

    public static bool EqualGoogleVertexAI(SettingsObject Source, SettingsObject Destination)
    {
        return Destination.googleVertexAIProjectId == Source.googleVertexAIProjectId
            && Destination.googleVertexAIAccessToken == Source.googleVertexAIAccessToken;
    }
    #endregion GoogleVertexAI

    #region EverypixelLabs
    [JsonIgnore]
    private string everypixelLabsClientID = "";
    public string EverypixelLabsClientID
    {
        get => everypixelLabsClientID;
        set
        {
            if (everypixelLabsClientID == value)
            {
                return;
            }

            if (!IsValidEverypixelLabsClientID(value))
            {
                throw new ArgumentException();
            }

            everypixelLabsClientID = value;
        }
    }
    public static bool IsValidEverypixelLabsClientID(string value)
    {
        return (!value.Contains(' ')) && (value.Length > 0);
    }

    [JsonIgnore]
    private string everypixelLabsSecret = "";
    public string EverypixelLabsSecret
    {
        get => everypixelLabsSecret;
        set
        {
            if (everypixelLabsSecret == value)
            {
                return;
            }

            if (!IsValidEverypixelLabsSecret(value))
            {
                throw new ArgumentException();
            }

            everypixelLabsSecret = value;
        }
    }
    public static bool IsValidEverypixelLabsSecret(string value)
    {
        return (!value.Contains(' ')) && (value.Length > 0);
    }

    public void CleanEverypixelLabs() { }
    public static void CopyEverypixelLabs(SettingsObject Source, SettingsObject Destination)
    {
        Destination.everypixelLabsClientID = Source.everypixelLabsClientID;
        Destination.everypixelLabsSecret = Source.everypixelLabsSecret;
    }
    public static bool EqualEverypixelLabs(SettingsObject Value0, SettingsObject Value1)
    {
        return Value0.everypixelLabsClientID == Value1.everypixelLabsClientID
            && Value0.everypixelLabsSecret == Value1.everypixelLabsSecret;
    }
    #endregion EveryPixelLabs

    #region CostOptimizations
    [JsonIgnore]
    private GenerateFromOptions titleGenerateFrom = GenerateFromOptions.Image;
    public GenerateFromOptions TitleGenerateFrom
    {
        get => titleGenerateFrom;
        set
        {
            if (titleGenerateFrom == value)
            {
                return;
            }

            if (!IsValidTitleGenerateFrom(value))
            {
                throw new ArgumentException();
            }

            titleGenerateFrom = value;
        }
    }
    public static bool IsValidTitleGenerateFrom(GenerateFromOptions value)
    {
        return value switch
        {
            GenerateFromOptions.Image or
            GenerateFromOptions.Captions_Description => true,
            _ => false,
        };
    }
    [JsonIgnore]
    private GenerateFromOptions descriptionGenerateFrom = GenerateFromOptions.Image;
    public GenerateFromOptions DescriptionGenerateFrom
    {
        get => descriptionGenerateFrom;
        set
        {
            if (descriptionGenerateFrom == value)
            {
                return;
            }

            if (!IsValidDescriptionGenerateFrom(value))
            {
                throw new ArgumentException();
            }

            descriptionGenerateFrom = value;
        }
    }
    public static bool IsValidDescriptionGenerateFrom(GenerateFromOptions value)
    {
        return value switch
        {
            GenerateFromOptions.Image => true,
            _ => false,
        };
    }
    [JsonIgnore]
    private GenerateFromOptions categoriesGenerateFrom = GenerateFromOptions.Image;
    public GenerateFromOptions CategoriesGenerateFrom
    {
        get => categoriesGenerateFrom;
        set
        {
            if (categoriesGenerateFrom == value)
            {
                return;
            }

            if (!IsValidCategoriesGenerateFrom(value))
            {
                throw new ArgumentException();
            }

            categoriesGenerateFrom = value;
        }
    }
    public static bool IsValidCategoriesGenerateFrom(GenerateFromOptions value)
    {
        return value switch
        {
            GenerateFromOptions.Image or
            GenerateFromOptions.Captions_Title or
            GenerateFromOptions.Captions_Description or
            GenerateFromOptions.Keywords => true,
            _ => false,
        };
    }
    [JsonIgnore]
    private GenerateFromOptions keywordsGenerateFrom = GenerateFromOptions.Image;
    public GenerateFromOptions KeywordsGenerateFrom
    {
        get => keywordsGenerateFrom;
        set
        {
            if (keywordsGenerateFrom == value)
            {
                return;
            }

            if (!IsValidKeywordsGenerateFrom(value))
            {
                throw new ArgumentException();
            }

            keywordsGenerateFrom = value;
        }
    }
    public static bool IsValidKeywordsGenerateFrom(GenerateFromOptions value)
    {
        return value switch
        {
            GenerateFromOptions.Image => true,
            _ => false,
        };
    }
    public void CleanCostOptimizations() { }
    public static void CopyCostOptimizations(SettingsObject Source, SettingsObject Destination)
    {
        Destination.titleGenerateFrom = Source.titleGenerateFrom;
        Destination.descriptionGenerateFrom = Source.descriptionGenerateFrom;
        Destination.categoriesGenerateFrom = Source.categoriesGenerateFrom;
        Destination.keywordsGenerateFrom = Source.keywordsGenerateFrom;
    }
    public static bool EqualCostOptimizations(SettingsObject Value0, SettingsObject Value1)
    {
        return Value0.titleGenerateFrom == Value1.titleGenerateFrom
            && Value0.descriptionGenerateFrom == Value1.descriptionGenerateFrom
            && Value0.categoriesGenerateFrom == Value1.categoriesGenerateFrom
            && Value0.keywordsGenerateFrom == Value1.keywordsGenerateFrom;
    }
    #endregion CostOptimizations

    public void Load()
    {
        if (File.Exists(settingsFile))
        {
            string json = File.ReadAllText(settingsFile, Encoding.UTF8);
            JsonConvert.PopulateObject(json, this);
        }
    }
    public void Save()
    {
        Clean();
        string json = JsonConvert.SerializeObject(this);
        if (!Directory.Exists(settingsDirectory))
        {
            _ = Directory.CreateDirectory(settingsDirectory);
        }
        File.WriteAllText(settingsFile, json, Encoding.UTF8);
    }
    public void Clean()
    {
        CleanSave();
        CleanBackup();
        CleanGoogleVertexAI();
        CleanEverypixelLabs();
        CleanCostOptimizations();
    }
    public static void Copy(SettingsObject Source, SettingsObject Destination)
    {
        CopySave(Source, Destination);
        CopyBackup(Source, Destination);
        CopyGoogleVertexAI(Source, Destination);
        CopyEverypixelLabs(Source, Destination);
        CopyCostOptimizations(Source, Destination);
    }
    public static bool Equal(SettingsObject Value0, SettingsObject Value1)
    {
        return EqualSave(Value0, Value1)
            && EqualBackup(Value0, Value1)
            && EqualGoogleVertexAI(Value0, Value1)
            && EqualEverypixelLabs(Value0, Value1)
            && EqualCostOptimizations(Value0, Value1);
    }

    private static bool IsValidPath(string Path)
    {
        for (int i = 0; i < invalidPathChars.Length; i++)
        {
            if (Path.Contains(invalidPathChars[i]))
            {
                return false;
            }
        }

        return System.IO.Path.IsPathRooted(Path);
    }
    private static (bool IsValid, string? HostName, string? Protocol) IsValidHostName(string PotentialHostName)
    {
        Match match = HostNameRegex().Match(PotentialHostName);
        if (match.Success)
        {
            string? hostname = match.Groups["hostname"].Success ? match.Groups["hostname"].Value : null;
            string? protocol = match.Groups["protocol"].Success ? match.Groups["protocol"].Value : null;
            return (true, hostname, protocol);
        }
        else
        {
            return (false, null, null);
        }
    }
    [GeneratedRegex(@"^(?:(?<protocol>ftps?://)?(?<hostname>[a-zA-Z0-9\-.]+\.[a-zA-Z]{2,}))$")]
    private static partial Regex HostNameRegex();
    private static bool IsValidCloudPath(string Path)
    {
        if (Path == "/")
        {
            return true;
        }

        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }

        if (!Path.StartsWith('/'))
        {
            return false;
        }

        if (Path.EndsWith('/'))
        {
            return false;
        }

        bool lastWasSlash = true;
        for (int i = 1; i < Path.Length; i++)
        {
            char c = Path[i];
            if (c == '/')
            {
                if (lastWasSlash)
                {
                    return false;
                }

                lastWasSlash = true;
            }
            else
            {
                lastWasSlash = false;
                if (!char.IsLetterOrDigit(c) && c != '.')
                {
                    return false;
                }
            }
        }

        return true;
    }

    public override bool Equals(object? Value)
    {
        return Equals(Value as SettingsObject);
    }

    public bool Equals(SettingsObject? Value)
    {
        return Value is not null && Equal(this, Value);
    }

    public static bool operator ==(SettingsObject Value0, SettingsObject Value1)
    {
        return Equal(Value0, Value1);
    }

    public static bool operator !=(SettingsObject Value0, SettingsObject Value1)
    {
        return !Equal(Value0, Value1);
    }

    public static SettingsObject LoadObject()
    {
        SettingsObject so = new();
        so.Load();
        return so;
    }
    public void CopyTo(SettingsObject Destination)
    {
        Copy(this, Destination);
    }

    public SettingsObject Clone()
    {
        SettingsObject n = new();
        CopyTo(n);
        return n;
    }
}

public enum GenerateFromOptions
{
    Image,
    Captions_Title,
    Captions_Description,
    Categories,
    Keywords,
}