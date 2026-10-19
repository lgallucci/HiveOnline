using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HiveServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var generateIndex = Array.FindIndex(args, a => a.Equals("--generate-cert", StringComparison.OrdinalIgnoreCase));
            if (generateIndex >= 0)
            {
                TlsCertificate.Generate(args.Length > generateIndex + 1 ? args[generateIndex + 1] : "hive-server.pfx");
                return;
            }

            var port = 7777;
            var portText = Environment.GetEnvironmentVariable("HIVE_PORT");
            if (!string.IsNullOrWhiteSpace(portText) && int.TryParse(portText, out var configuredPort))
                port = configuredPort;

            var testMode = args.Any(a => a.Equals("--test", StringComparison.OrdinalIgnoreCase)) ||
                string.Equals(Environment.GetEnvironmentVariable("HIVE_TEST_MODE"), "1", StringComparison.Ordinal);

            var portArg = args.FirstOrDefault(a => int.TryParse(a, out _));
            if (portArg != null)
                port = int.Parse(portArg);

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(new GameServer(port) { TestMode = testMode, Certificate = TlsCertificate.LoadFromEnvironment() });
                    services.AddHostedService(sp => sp.GetRequiredService<GameServer>());
                })
                .Build();

            await host.RunAsync();
        }
    }
}
