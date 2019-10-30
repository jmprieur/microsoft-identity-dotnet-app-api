using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FindAppsWithExpiredCerts
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // App configuration
            string tenant = "";
            string clientId = "";
            string[] scopes = new[] { "Directory.AccessAsUser.All" };
            string endpoint = "https://graph.microsoft.com/beta/applications";

            // Create the app.
            var app = PublicClientApplicationBuilder.Create(clientId)
                .WithTenantId(tenant)
                .WithDefaultRedirectUri()
                .Build();

            // Acquire the token interactively (cache not deserialized, not need to attempt to get a token from cache)
            var result = await app.AcquireTokenInteractive(scopes)
                .ExecuteAsync();

            // Call the protected API
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", result.CreateAuthorizationHeader());
            string jsonString = await httpClient.GetStringAsync(endpoint);

            // Exploit data from graph
            dynamic apps = JsonSerializer.Deserialize<dynamic>(jsonString);

            Console.WriteLine("Finding expired certs");


        }
    }
}
