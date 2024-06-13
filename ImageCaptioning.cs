using Brendan_Stock_Media_Distributor;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public static class ImageCaptioning
{
    private static string Endpoint => "https://us-central1-aiplatform.googleapis.com/v1/projects/" + Shared.CurrentSettings.GoogleVertexAIProjectId + "/locations/us-central1/publishers/google/models/imagetext:predict";

    public static async Task<(string? Caption, string? ErrorMessage)> CaptionImage(string imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                throw new ArgumentNullException(nameof(imagePath));
            }

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Image file not found.", imagePath);
            }

            // Read the image content as base64 encoded string
            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            string imageBase64 = Convert.ToBase64String(imageBytes);

            // Prepare the request body
            Dictionary<string, object> requestBody = new()
            {
                ["instances"] = new List<object>()
            {
                new Dictionary<string, object>()
                {
                    ["image"] = new Dictionary<string, object>()
                    {
                        ["bytesBase64Encoded"] = imageBase64
                    }
                }
            },
                ["parameters"] = new Dictionary<string, object>()
                {
                    ["sampleCount"] = 1,
                    ["language"] = "en"
                }
            };

            // Create the HTTP client
            using HttpClient client = new();

            // Set the request headers
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken()); // Replace with your access token retrieval logic
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Send the request
            StringContent content = new(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(Endpoint, content);

            // Check for successful response
            if (!response.IsSuccessStatusCode)
            {
                return (null, $"Error calling Vertex AI API. Status code {response.StatusCode}. The API returned: {await response.Content.ReadAsStringAsync()}");
            }
            // Parse the response and extract the caption
            string responseString = await response.Content.ReadAsStringAsync();
            try
            {
                IDictionary<string, IList<string>>? responseData = JsonSerializer.Deserialize<IDictionary<string, IList<string>>>(responseString);

                IList<string>? captions = responseData?["predictions"];
                if (captions?.Count > 0)
                {
                    return (captions[0], null);
                }

                return (null, $"Error calling Vertex AI API. The API returned: {await response.Content.ReadAsStringAsync()}");
            }
            catch
            {
                return (null, $"Error calling Vertex AI API. The API returned: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            return (null, $"The following unrecognized exception occured: {ex.Message}");
        }
    }

    // Replace this with your access token retrieval logic (e.g., using Google Cloud Application Default Credentials)
    private static string GetAccessToken()
    {
        // Implement access token retrieval logic here
        return Shared.CurrentSettings.GoogleVertexAIAccessToken;
    }
}