using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;

namespace CatCam.EdgeCommon.Services.Clients;

public class ModuleClientWrapper : IModuleClientWrapper
{
    private readonly ModuleClient _moduleClient;
    private readonly ILogger _logger;

    public ModuleClientWrapper(ILogger<ModuleClientWrapper> logger)
    {
        _logger = logger;
        MqttTransportSettings mqttSetting = new(TransportType.Mqtt_Tcp_Only);
        ITransportSettings[] settings = { mqttSetting };

        // Open a connection to the Edge runtime
        _moduleClient = ModuleClient.CreateFromEnvironmentAsync(settings).Result;
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Closing module client connection");
        await _moduleClient.CloseAsync(cancellationToken);
    }

    public async Task<Twin> GetTwinAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting twin");
        return await _moduleClient.GetTwinAsync(cancellationToken);
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Opening module client connection");
        await _moduleClient.OpenAsync(cancellationToken);
    }

    public async Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending Event to Output {Output}", outputName);
        await _moduleClient.SendEventAsync(outputName, message, cancellationToken);
    }

    public async Task SendMessage(string channel, object message, CancellationToken cancellationToken)
    {
        var msgJson = System.Text.Json.JsonSerializer.Serialize(message);
        var msgBytes = Encoding.UTF8.GetBytes(msgJson);
        var iotMessage = new Message(msgBytes)
        {
            ContentEncoding = "utf-8",
            ContentType = "application/json"
        };
        
        await _moduleClient.SendEventAsync(channel, iotMessage, cancellationToken);
    }

    public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler)
    {
        _moduleClient.SetConnectionStatusChangesHandler(statusChangesHandler);
    }

    public async Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Setting Desired Property callback handler");
        await _moduleClient.SetDesiredPropertyUpdateCallbackAsync(callback, userContext, cancellationToken);
    }

    public async Task SetInputMessageHandlerAsync(string name, MessageHandler messageHandler, object userContext)
    {
        await _moduleClient.SetInputMessageHandlerAsync(name, messageHandler, userContext);
    }
}