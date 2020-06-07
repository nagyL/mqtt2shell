using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Mqtt2Shell
{
    public class ServiceCtl : BackgroundService
    {
        private readonly MqttMessageHandler mqttMessageHandler;

        public ServiceCtl(MqttMessageHandler mqttMessageHandler)
        {
            this.mqttMessageHandler = mqttMessageHandler;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await mqttMessageHandler.StartAsync();
            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await mqttMessageHandler.StopAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}