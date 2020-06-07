using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mqtt2Shell.Adapters.Windows;
using NLog.Extensions.Logging;

namespace Mqtt2Shell
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args);
            var host = hostBuilder.Build();

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder =
                Host
                    .CreateDefaultBuilder(args)
                    .UseWindowsService()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config
                            .AddJsonFile("appsettings.user.json", true, true)
                            .AddEnvironmentVariables();
                    })                    
                    .ConfigureServices((hostingContext, services) =>
                    {
                        services.Configure<MqttClientSettings>(hostingContext.Configuration.GetSection("MqttClientSettings"));
                        services.Configure<WindowsShellAdapterSettings>(hostingContext.Configuration.GetSection("WindowsShellAdapterSettings"));
                        services.AddHostedService<ServiceCtl>();
                        services.AddTransient<MqttMessageHandler>();
                        services.AddTransient<ShellAdapterFactory>();
                        services.AddTransient<WindowsShellAdapter>();
                    })
                    .ConfigureLogging(builder => 
                    {
                        builder.ClearProviders();
                        builder.AddNLog("nlog.config");
                    });

            return hostBuilder;
        }
    }
}
