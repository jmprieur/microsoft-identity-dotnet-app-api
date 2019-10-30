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
        // App configuration
        static private string tenant = "";
        static private string clientId = "";

        // Graph app API
        static private string[] scopes = new[] { "Directory.AccessAsUser.All", "User.ReadBasic.All" };
        static private string endpoint = "https://graph.microsoft.com/beta/applications";

        static async Task Main(string[] args)
        {
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
            string appsJsonString = await httpClient.GetStringAsync(endpoint);

            // Get apps from Graph
            Apps apps = JsonSerializer.Deserialize<Apps>(appsJsonString);

            // Find expired certificates
            var appsWithExpiredCerts = apps.value.Select(a => new { app = a, 
                                                                    owners=GetOwners(httpClient, a).Result,
                                                                    expiredCerts = a.keyCredentials
                                                                                    .Where(k => k.endDateTime < DateTime.Now) })
                                                 .Where(a => a.expiredCerts.Any());

            // Display the apps with their expired certificates
            foreach (var appWithExpiredCerts in appsWithExpiredCerts)
            {
                var a = appWithExpiredCerts.app;
                Console.WriteLine($"App displayName={a.displayName} AppId={a.appId} Owners='{appWithExpiredCerts.owners}' ");
                foreach(var expiredCert in appWithExpiredCerts.expiredCerts)
                {
                    Console.WriteLine($"- EXPIRED Certificate '{expiredCert.displayName}' Id={expiredCert.customKeyIdentifier}");
                }
            }


        }


        private static async Task<string> GetOwners(HttpClient httpClient, App app)
        {
            // Find their owners
            string ownerUri = $"{endpoint}/{app.id}/owners";
            string ownersJsonString = await httpClient.GetStringAsync(ownerUri);
            Users users = JsonSerializer.Deserialize<Users>(ownersJsonString);
            return string.Join(",", users.value.Select(u => u.userPrincipalName));
        }
    }
}
