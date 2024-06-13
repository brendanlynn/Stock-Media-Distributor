using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Brendan_Stock_Media_Distributor;

public static class MediaKeywording
{

    private static async Task<string?> PostFileOrURLToURL(string ClientID, string Secret, Uri URI, string Location)
    {
        static int AccessProtocol(string Location)
        {
            string[] parts = Location.Split(@"://");
            switch (parts.Length)
            {
                case 1:
                    char[] invalids = Path.GetInvalidPathChars();
                    foreach (char c in invalids)
                    {
                        if (Location.Contains(c))
                        {
                            return -1;
                        }
                    }

                    return 0;
                case 2:
                    return parts[0] switch
                    {
                        "http" => 1,
                        "https" => 2,
                        "ftp" => 3,
                        "ftps" => 4,
                        "ssh" => 5,
                        "sftp" => 6,
                        _ => -1,
                    };
                default:
                    return -1;
            }
        }
        try
        {
            int protocol = AccessProtocol(Location);

            if (protocol < 0)
            {
                return null;
            }

            HttpClient httpClient = new();

            if (protocol > 0)
            {
                URI = URI.Query switch
                {
                    "" or "?" => new Uri(URI.AbsolutePath + "?url=" + Uri.EscapeDataString(Location)),
                    _ => new Uri(URI.PathAndQuery + "&url=" + Uri.EscapeDataString(Location)),
                };
            }

            string credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{ClientID}:{Secret}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            Task<HttpResponseMessage> aResponse;
            if (protocol == 0)
            {
                if (!Path.IsPathRooted(Location))
                {
                    Location = Path.Combine(Directory.GetCurrentDirectory(), Location);
                }

                MultipartFormDataContent formData = [];
                byte[] data = File.ReadAllBytes(Location);
                formData.Add(new ByteArrayContent(data), "data", Path.GetFileName(Location));

                aResponse = httpClient.PostAsync(URI, formData);
            }
            else
            {
                aResponse = httpClient.GetAsync(URI);
            }

            HttpResponseMessage response = await aResponse;

            Task<string> aResult = response.Content.ReadAsStringAsync();
            string result = await aResult;
            return result;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<string?> ToEndpointWithAuth(string Endpoint, string Source)
    {
        return await PostFileOrURLToURL(Shared.CurrentSettings.EverypixelLabsClientID, Shared.CurrentSettings.EverypixelLabsSecret, new(Endpoint), Source);
    }
    public static async Task<Keyword[]?> GetKeywordsFromES(string Endpoint, string Source)
    {
        string? s = await ToEndpointWithAuth(Endpoint, Source) ?? "[]";
        if (s is null)
        {
            return null;
        }
        return Newtonsoft.Json.JsonConvert.DeserializeObject<KeywordResponse>(s)?.keywords;
    }
    public static async IAsyncEnumerable<string> GetKeywordsAboveX(string Endpoint, string Source, double LowerBound)
    {
        Keyword[]? keywords = await GetKeywordsFromES(Endpoint, Source);
        if (keywords is null)
        {
            yield break;
        }

        foreach (Keyword k in keywords)
        {
            if (k.score >= LowerBound)
            {
                yield return k.keyword;
            }
        }
    }
    public static async Task<string[]> EnumerateIAsyncEnumerable(IAsyncEnumerable<string> items)
    {
        List<string> list = [];
        IAsyncEnumerator<string> e = items.GetAsyncEnumerator();
        while (await e.MoveNextAsync())
        {
            list.Add(e.Current);
        }

        return [.. list];
    }
    private const int dkc = 50;
    private const double mc = 0.05;
    public static async Task<string[]> EverypixelLabs_Keywords_Image(string Source)
    {
        return await EnumerateIAsyncEnumerable(GetKeywordsAboveX($"https://api.everypixel.com/v1/keywords?num_keywords={dkc}", Source, mc));
    }
    public static async Task<string[]> EverypixelLabs_Keywords_Video(string Source)
    {
        return await EnumerateIAsyncEnumerable(GetKeywordsAboveX($"https://api.everypixel.com/v1/video_keywords?num_keywords={dkc}", Source, mc));
    }
    public class KeywordResponse
    {
        public Keyword[] keywords;
        public string status;
        public KeywordResponse(Keyword[] keywords, string status)
        {
            this.keywords = keywords;
            this.status = status;
        }
    }
    public class Keyword
    {
        public string keyword;
        public double score;
        public Keyword(string keyword, double score)
        {
            this.keyword = keyword;
            this.score = score;
        }
    }
}