using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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

            // Get apps from Graph
            Apps apps = JsonSerializer.Deserialize<Apps>(jsonString);

            // Find expired certificates
            var appsWithExpiredCerts = apps.value.Select(a => new { app = a, expiredCerts = a.keyCredentials.Where(k => k.endDateTime < DateTime.Now) })
                                                 .Where(a => a.expiredCerts.Any());

            // Display the apps with their expired certificates
            foreach(var appWithExpiredCerts in appsWithExpiredCerts)
            {
                var a = appWithExpiredCerts.app;
                Console.WriteLine($"App displayName={a.displayName} AppId={a.appId} ");
                foreach(var expiredCert in appWithExpiredCerts.expiredCerts)
                {
                    Console.WriteLine($"- EXPIRED Certificate '{expiredCert.displayName}' Id={expiredCert.customKeyIdentifier}");
                }
            }


        }
    }
}
