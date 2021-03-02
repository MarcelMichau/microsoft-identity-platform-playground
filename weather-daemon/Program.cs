﻿using System;
using Microsoft.Identity.Client;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Headers;

namespace weather_daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        private static async Task RunAsync()
        {
            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

            IConfidentialClientApplication app =
              ConfidentialClientApplicationBuilder.Create(config.ClientId)
                  .WithClientSecret(config.ClientSecret)
                  .WithAuthority(new Uri(config.Authority))
                  .Build();

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator
            string[] scopes = new string[] { config.ApiScope };

            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token acquired \n");
                Console.ResetColor();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Scope provided is not supported");
                Console.ResetColor();
            }

            if (result != null)
            {
                var httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
                if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);

                HttpResponseMessage response = await httpClient.GetAsync($"{config.ApiBaseAddress}/WeatherForecast");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var results = JsonDocument.Parse(json);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Display(results.RootElement.EnumerateArray());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to call the Web Api: {response.StatusCode}");
                    string content = await response.Content.ReadAsStringAsync();

                    // Note that if you got reponse.Code == 403 and reponse.content.code == "Authorization_RequestDenied"
                    // this is because the tenant admin as not granted consent for the application to call the Web API
                    Console.WriteLine($"Content: {content}");
                }
                Console.ResetColor();

            }
        }

        private static void Display(JsonElement.ArrayEnumerator results)
        {
            Console.WriteLine("Web Api result: \n");

            foreach (JsonElement element in results)
            {
                var date = DateTime.MinValue;
                var temperatureC = 0;
                var temperatureF = 0;
                var summary = "";

                if (element.TryGetProperty("date", out JsonElement dateElement))
                {
                    date = dateElement.GetDateTime();
                }

                if (element.TryGetProperty("temperatureC", out JsonElement temperatureCElement))
                {
                    temperatureC = temperatureCElement.GetInt32();
                }

                if (element.TryGetProperty("temperatureF", out JsonElement temperatureFElement))
                {
                    temperatureF = temperatureFElement.GetInt32();
                }

                if (element.TryGetProperty("summary", out JsonElement summaryElement))
                {
                    summary = summaryElement.GetString();
                }

                Console.WriteLine($"Weather result - Date: {date}, Temperature C: {temperatureC}, Temperature F: {temperatureF}, Summary: {summary}");
            }
        }
    }
}