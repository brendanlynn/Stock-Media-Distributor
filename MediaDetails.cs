using Newtonsoft.Json;
using System.Collections.Immutable;
using System.IO;
using static Brendan_Stock_Media_Distributor.Shared;

namespace Brendan_Stock_Media_Distributor;
public sealed class MediaDetails
{
    public static ImmutableArray<string> Category_Shutterstock_Categories { get; } = [
        "Abstract",
        "Animals/Wildlife",
        "Arts",
        "Backgrounds/Textures",
        "Beauty/Fashion",
        "Business/Finance",
        "Celebrities",
        "Education",
        "Food/Drink",
        "Healthcare/Medical",
        "Holidays",
        "Industrial",
        "Interiors",
        "Miscellaneous",
        "Nature",
        "Objects",
        "Parks/Outdoor",
        "People",
        "Religeon",
        "Science",
        "Signs/Symbols",
        "Sports/Recreation",
        "Technology",
        "Transportation",
        "Vintage",
    ];
    public static ImmutableArray<string> Category_AdobeStock_Categories { get; } = [
        "Animals",
        "Buildings and Architecture",
        "Business",
        "Drinks",
        "The Environment",
        "States of Mind",
        "Food",
        "Graphic Resources",
        "Hobbies and Leisure",
        "Industry",
        "Landscape",
        "Lifestyle",
        "People",
        "Plants and Flowers",
        "Culture and Religion",
        "Science",
        "Social Issues",
        "Sports",
        "Technology",
        "Transport",
        "Travel",
    ];

    public MediaDetails(ulong Id, string File_LocalName, string? File_LocalThumbnailName, MediaType File_MediaType, string File_Name, int Image_Width, int Image_Height, string Captions_Title, string Captions_Description, int Category_Shutterstock_Primary_Index, int Category_Shutterstock_Secondary_Index, int Category_AdobeStock_Index, List<string> Keywords)
    {
        this.Id = Id;
        this.File_LocalName = File_LocalName;
        this.File_LocalThumbnailName = File_LocalThumbnailName;
        this.File_MediaType = File_MediaType;
        this.File_Name = File_Name;
        this.Image_Width = Image_Width;
        this.Image_Height = Image_Height;
        this.Captions_Title = Captions_Title;
        this.Captions_Description = Captions_Description;
        this.Category_Shutterstock_Primary_Index = Category_Shutterstock_Primary_Index;
        this.Category_Shutterstock_Secondary_Index = Category_Shutterstock_Secondary_Index;
        this.Category_AdobeStock_Index = Category_AdobeStock_Index;
        this.Keywords = Keywords;
    }
    public ulong Id { get; }
    [JsonIgnore]
    public string Id_Path => Path.Combine(CurrentSettings.SaveLocalPath, Id.ToString());

    [JsonIgnore]
    public string File_FilePath => Path.Combine(Id_Path, File_LocalName);
    [JsonIgnore]
    public string? File_ThumbnailFilePath
        => File_LocalThumbnailName is null
         ? null
         : Path.Combine(Id_Path, File_LocalThumbnailName);
    public string File_LocalName { get; }
    public string? File_LocalThumbnailName { get; }
    public MediaType File_MediaType { get; }
    public string File_Name { get; set; }

    public int Image_Width { get; }
    public int Image_Height { get; }

    public string Captions_Title { get; set; }
    public string Captions_Description { get; set; }
    public int Category_Shutterstock_Primary_Index { get; set; }
    public int Category_Shutterstock_Secondary_Index { get; set; }
    public int Category_AdobeStock_Index { get; set; }
    public List<string> Keywords { get; }
    public bool Options_Illustration { get; set; }
    public bool Options_MatureContent { get; set; }
    public bool Options_EditorialUseOnly { get; set; }

    public static MediaDetails? Load(string DirectoryPath)
    {
        try
        {
            string jsonPath = GetJsonPath(DirectoryPath);
            string jsonText = File.ReadAllText(jsonPath);
            return JsonConvert.DeserializeObject<MediaDetails>(jsonText);
        }
        catch
        {
            return null;
        }
    }
    public static MediaDetails? Load(ulong Id)
    {
        return Load(IdToPath(Id));
    }

    private void Save(string DirectoryPath)
    {
        if (!Directory.Exists(DirectoryPath))
        {
            _ = Directory.CreateDirectory(DirectoryPath);
        }

        string jsonPath = GetJsonPath(DirectoryPath);
        string jsonText = JsonConvert.SerializeObject(this);
        File.WriteAllText(jsonPath, jsonText);
    }
    private void Save(ulong Id)
    {
        Save(IdToPath(Id));
    }
    public void Save()
    {
        Save(Id);
    }

    public static Dictionary<ulong, MediaDetails> LoadAll()
    {
        SettingsObject s = CurrentSettings;
        DirectoryInfo di = new(s.SaveLocalPath);
        if (!di.Exists)
        {
            return [];
        }

        DirectoryInfo[] dis = di.GetDirectories();

        Dictionary<ulong, MediaDetails> output = [];

        foreach (DirectoryInfo d in dis)
        {
            if (!ulong.TryParse(d.Name, out ulong name))
            {
                continue;
            }

            MediaDetails? md = Load(d.FullName);
            if (md is not null)
            {
                output.Add(name, md);
            }
        }

        return output;
    }

    private static string GetJsonPath(string DirectoryPath)
    {
        return Path.Combine(DirectoryPath, "meta.json");
    }

    private static string GetJsonPath(ulong Id)
    {
        return GetJsonPath(IdToPath(Id));
    }

    private static string IdToPath(ulong ID)
    {
        return Path.Combine(CurrentSettings.SaveLocalPath, ID.ToString());
    }

    public static bool IdExists(ulong Id)
    {
        return Directory.Exists(IdToPath(Id));
    }

    public static KeyValuePair<ulong, MediaDetails> AddExternalFile(string Path)
    {
        FileInfo fi = new(Path);
        MediaType mediaType = fi.Extension switch
        {
            ".jpg" or ".png" => MediaType.Image,
            ".mp4" => MediaType.Video,
            _ => throw new Exception("Unrecognized extension."),
        };
        string originalName = fi.Name;
        ulong id;
        do
        {
            id = (ulong)Random.Shared.NextInt64();
        }
        while (IdExists(id));
        string path = IdToPath(id);
        _ = Directory.CreateDirectory(path);
        string Fullpathifier(string Name) => System.IO.Path.Combine(path, Name);
        string localName = "data" + fi.Extension;
        string filePath = Fullpathifier(localName);
        fi = fi.CopyTo(filePath);
        int width = -1;
        int height = -1;
        string? localThumbnailPath;
        System.Drawing.Bitmap? b = null;
        System.Drawing.Bitmap? b2 = null;
        try
        {
            b = new(filePath);
            width = b.Width;
            height = b.Height;
            if (b.Width > 500 || b.Height > 500)
            {
                double s = 500d / ((b.Width > b.Height) ? b.Width : b.Height);
                b2 = new(b, new((int)(b.Width * s), (int)(b.Height * s)));
                localThumbnailPath = "thumbnail.jpg";
                b2.Save(Fullpathifier(localThumbnailPath));
            }
            else
            {
                localThumbnailPath = null;
            }
        }
        catch
        {
            localThumbnailPath = null;
        }
        finally
        {
            b?.Dispose();
            b2?.Dispose();
        }
        MediaDetails md = new(id, localName, localThumbnailPath, mediaType, originalName, width, height, "", "", -1, -1, -1, []);
        md.Save(id);
        return new(id, md);
    }
    public static void Delete(ulong Id)
    {
        string dp = IdToPath(Id);
        Directory.Delete(dp, true);
        Directory.Delete(dp);
    }
    public void Delete()
    {
        Delete(Id);
    }
    public void CopyMediaTo(string Destination)
    {
        FileInfo fi = new(File_FilePath);
        _ = fi.CopyTo(Destination);
    }
    public void CopyMediaTo(string Destination, bool Overwrite)
    {
        FileInfo fi = new(File_FilePath);
        _ = fi.CopyTo(Destination, Overwrite);
    }
    public string GetThumbnailPath()
    {
        return File_ThumbnailFilePath ?? File_FilePath;
    }
}