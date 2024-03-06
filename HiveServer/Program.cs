using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HiveServer
{
    class Program
    {
        static async void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<GameServer>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
