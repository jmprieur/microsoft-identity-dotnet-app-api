using System;

namespace FindAppsWithExpiredCerts
{
    public class KeyCredential
    {
        public string customKeyIdentifier { get; set; }
        public DateTime endDateTime { get; set; }
        public string displayName { get; set; }
    }

    public class App
    {
        public string displayName { get; set; }
        public string appId { get; set; }
        public KeyCredential[] keyCredentials { get;set;}
    }

    public class Apps
    {
        public App[] value { get; set; }
    }
}
