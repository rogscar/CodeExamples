using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SimpleRestExample
{
    public class RestClient
    {
        private readonly string _apiToken;
        private readonly string _baseUrl;

        public RestClient(string apiToken, string baseUrl)
        {
            _apiToken = apiToken;
            _baseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/"; // Ensure URL ends with "/"
        }

        public async Task<(bool Success, JArray Charges, string ErrorMessage)> GetChargesAsync(DateTime dateStart, DateTime dateEnd, int limit)
        {
            try
            {
                // Construct the URL with query parameters, similar to your example
                string url = $"{_baseUrl}charges/?dtstart={dateStart.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.ffffffZ}" +
                            $"&dtend={dateEnd.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.ffffffZ}&limit={limit}";

                // Create HttpClient instance
                using (var client = new HttpClient())
                {
                    // Set headers (Bearer token and JSON content type)
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // Make the GET request asynchronously
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read and parse the JSON response
                    string responseString = await response.Content.ReadAsStringAsync();
                    JArray charges = JArray.Parse(responseString);

                    return (true, charges, null);
                }
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message); 
            }
        }
    }

    // Example usage
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize the client with token and base URL
            var client = new RestClient(
                apiToken: "your-api-token-here",
                baseUrl: "https://api.example.com"
            );

            // Call the method
            var (success, charges, errorMessage) = await client.GetChargesAsync(
                dateStart: DateTime.Now.AddDays(-7),
                dateEnd: DateTime.Now,
                limit: 100
            );

            // Handle the result
            if (success)
            {
                Console.WriteLine("Charges retrieved successfully:");
                Console.WriteLine(charges.ToString());
            }
            else
            {
                Console.WriteLine($"Error Getting Charges: {errorMessage}");
            }
        }
    }
}