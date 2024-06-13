using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Brendan_Stock_Media_Distributor;
public static class GeminiVision
{
    public static string Endpoint => @"https://us-central1-aiplatform.googleapis.com/v1/projects/" + Shared.CurrentSettings.GoogleVertexAIProjectId + "/locations/us-central1/publishers/google/models/gemini-1.0-pro:streamGenerateContent";
    public static string VisionEndpoint => @"https://us-central1-aiplatform.googleapis.com/v1/projects/" + Shared.CurrentSettings.GoogleVertexAIProjectId + "/locations/us-central1/publishers/google/models/gemini-1.0-pro-vision:streamGenerateContent";
    public static async Task<string> Prompt(string Prompt)
    {
        // Prepare the request body
        Dictionary<string, object> requestBody = new()
        {
            ["contents"] = new Dictionary<string, object>()
            {
                ["role"] = "user",
                ["parts"] = new List<object>
                {
                    new Dictionary<string, object>()
                    {
                        ["text"] = Prompt,
                    },
                },
            },
        };

        // Create the HTTP client
        using HttpClient client = new();

        // Set the request headers
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken()); // Replace with your access token retrieval logic
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Send the request
        StringContent content = new(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(Endpoint, content);

        // Check for successful response
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error calling Vertex AI API: {await response.Content.ReadAsStringAsync()}");
        }

        // Parse the response and extract the caption
        string responseString = await response.Content.ReadAsStringAsync();
        JToken t0 = JToken.Parse(responseString);

        StringBuilder sb = new();
        try
        {
            JArray responses = (JArray)t0;
            foreach (JToken t1 in responses)
            {
                JObject? response_ = (JObject?)t1;
                JToken? t2 = response_?["candidates"];
                JArray? canidates = (JArray?)t2;
                JToken? t3 = canidates?[0];
                JObject? canidate = (JObject?)t3;
                JToken? t4 = canidate?["content"];
                JObject? content_ = (JObject?)t4;
                JToken? t5 = content_?["parts"];
                JArray? parts = (JArray?)t5;
                JToken? t6 = parts?[0];
                JObject? part = (JObject?)t6;
                JToken? t7 = part?["text"];
                JValue? text = (JValue?)t7;
                string? st = text?.ToString();
                _ = sb.Append(st);
            }
            return sb.ToString();
        }
        catch
        {
            return responseString;
        }
    }
    public static async Task<string> Prompt(string Prompt, string MediaFilePath, string MimeType)
    {
        if (string.IsNullOrEmpty(MediaFilePath))
        {
            throw new ArgumentNullException(nameof(MediaFilePath));
        }

        if (!File.Exists(MediaFilePath))
        {
            throw new FileNotFoundException("Image file not found.", MediaFilePath);
        }

        // Read the image content as base64 encoded string
        byte[] imageBytes = await File.ReadAllBytesAsync(MediaFilePath);
        string imageBase64 = Convert.ToBase64String(imageBytes);

        // Prepare the request body
        Dictionary<string, object> requestBody = new()
        {
            ["contents"] = new Dictionary<string, object>()
            {
                ["role"] = "user",
                ["parts"] = new List<object>
                {
                    new Dictionary<string, object>()
                    {
                        ["text"] = Prompt,
                    },
                    new Dictionary<string, object>()
                    {
                        ["inlineData"] = new Dictionary<string, object>()
                        {
                            ["mimeType"] = MimeType,
                            ["data"] = imageBase64,
                        },
                    },
                },
            },
        };

        // Create the HTTP client
        using HttpClient client = new();

        // Set the request headers
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken()); // Replace with your access token retrieval logic
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Send the request
        StringContent content = new(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(VisionEndpoint, content);

        // Check for successful response
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Error calling Vertex AI API: {await response.Content.ReadAsStringAsync()}");
        }

        // Parse the response and extract the caption
        string responseString = await response.Content.ReadAsStringAsync();
        JToken t0 = JToken.Parse(responseString);

        StringBuilder sb = new();
        try
        {
            JArray responses = (JArray)t0;
            foreach (JToken t1 in responses)
            {
                JObject? response_ = (JObject?)t1;
                JToken? t2 = response_?["candidates"];
                JArray? canidates = (JArray?)t2;
                JToken? t3 = canidates?[0];
                JObject? canidate = (JObject?)t3;
                JToken? t4 = canidate?["content"];
                JObject? content_ = (JObject?)t4;
                JToken? t5 = content_?["parts"];
                JArray? parts = (JArray?)t5;
                JToken? t6 = parts?[0];
                JObject? part = (JObject?)t6;
                JToken? t7 = part?["text"];
                JValue? text = (JValue?)t7;
                string? st = text?.ToString();
                _ = sb.Append(st);
            }
            return sb.ToString();
        }
        catch
        {
            return responseString;
        }
    }

    // Replace this with your access token retrieval logic (e.g., using Google Cloud Application Default Credentials)
    public static string GetAccessToken()
    {
        // Implement access token retrieval logic here
        return Shared.CurrentSettings.GoogleVertexAIAccessToken;
    }
}