using System.IO;
using Microsoft.Extensions.Configuration;

namespace GrpcDemo.Client.Applibs
{
    public static class ConfigHelper
    {
        private static IConfiguration _config;

        public static string GrpcServerUrl = Config["GrpcService:Server"];

        public static IConfiguration Config
        {
            get
            {
                if (_config == null)
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", true, true)
                        .AddEnvironmentVariables();

                    _config = builder.Build();
                }

                return _config;
            }
        }
    }
}