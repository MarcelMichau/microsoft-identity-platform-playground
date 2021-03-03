using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.IO;

namespace WeatherDaemon
{
    public class AuthenticationConfig
    {
        public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string Authority => string.Format(CultureInfo.InvariantCulture, Instance, Tenant);
        public string ClientSecret { get; set; }
        public string ApiBaseAddress { get; set; }
        public string ApiScope { get; set; }
        public static AuthenticationConfig ReadFromJsonFile(string path)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path);

            var configuration = builder.Build();
            return configuration.Get<AuthenticationConfig>();
        }
    }
}