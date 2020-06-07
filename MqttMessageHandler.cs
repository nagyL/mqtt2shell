using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace Mqtt2Shell
{
    public class MqttMessageHandler : IDisposable
    {
        private readonly ShellAdapterFactory shellAdapterFactory;
        private readonly IOptions<MqttClientSettings> options;
        private readonly ILogger<MqttMessageHandler> logger;
        private IManagedMqttClient mqttClient;
        private bool disposedValue;

        public MqttMessageHandler(ShellAdapterFactory shellAdapterFactory, IOptions<MqttClientSettings> options, ILogger<MqttMessageHandler> logger)
        {
            this.shellAdapterFactory = shellAdapterFactory;
            this.options = options;
            this.logger = logger;
        }

        public async Task StartAsync()
        {
            var settings = options.Value;

            var mqttClientOptions =
                new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(
                        new MqttClientOptionsBuilder()
                            .WithClientId(settings.ClientId ?? Guid.NewGuid().ToString())
                            .WithTcpServer(settings.TcpServer)
                            .WithCredentials(settings.UserName, settings.Password))
                    .Build();

            mqttClient = new MqttFactory().CreateManagedMqttClient();

            await mqttClient.SubscribeAsync(
                new MqttTopicFilterBuilder()
                    .WithTopic(settings.Topic)
                    .Build());

            mqttClient.UseApplicationMessageReceivedHandler(e =>
                HandleMqttMessage(e.ApplicationMessage));

            await mqttClient.StartAsync(mqttClientOptions);
        }

        public void HandleMqttMessage(MqttApplicationMessage mqttMessage)
        {
            var messagePayload =
                mqttMessage
                    .ConvertPayloadToString();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(FormatMqttMessage(mqttMessage));
            }
            else
            {
                logger.LogInformation($"Receiving message: '{messagePayload}'.");
            }

            var shellAdapter = shellAdapterFactory.Create();
            shellAdapter.Execute(messagePayload);
        }

        public async Task StopAsync()
        {
            await mqttClient.StopAsync();
        }

        private string FormatMqttMessage(MqttApplicationMessage applicationMessage)
        {
            return
                new StringBuilder()
                    .AppendLine("Receiving message")
                    .AppendLine($"+ Topic = {applicationMessage.Topic}")
                    .AppendLine($"+ Payload = {applicationMessage.ConvertPayloadToString()}")
                    .AppendLine($"+ QoS = {applicationMessage.QualityOfServiceLevel}")
                    .AppendLine($"+ Retain = {applicationMessage.Retain}")
                    .ToString();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (mqttClient != null)
                    {
                        mqttClient.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class MqttClientSettings
    {
        public string ClientId { get; set; }
        public string TcpServer { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Topic { get; set; }
    }}