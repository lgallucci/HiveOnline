using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace HiveServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var port = 7777;
            var portText = Environment.GetEnvironmentVariable("HIVE_PORT");
            if (!string.IsNullOrWhiteSpace(portText) && int.TryParse(portText, out var configuredPort))
                port = configuredPort;

            if (args.Length > 0 && int.TryParse(args[0], out var cliPort))
                port = cliPort;

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(new GameServer(port));
                    services.AddHostedService(sp => sp.GetRequiredService<GameServer>());
                })
                .Build();

            await host.RunAsync();
        }
    }
}
