using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;

namespace CatCam.EdgeModules.FileWatcher.Services;

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

    public Task CloseAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Closing module client connection");
        return _moduleClient.CloseAsync(cancellationToken);
    }

    public Task<Twin> GetTwinAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting twin");
        return _moduleClient.GetTwinAsync(cancellationToken);
    }

    public Task OpenAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Opening module client connection");
        return _moduleClient.OpenAsync(cancellationToken);
    }

    public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending Event to Output {Output}", outputName);
        return _moduleClient.SendEventAsync(outputName, message, cancellationToken);
    }

    public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler)
    {
        _moduleClient.SetConnectionStatusChangesHandler(statusChangesHandler);
    }

    public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Setting Desired Property callback handler");
        return _moduleClient.SetDesiredPropertyUpdateCallbackAsync(callback, userContext, cancellationToken);
    }
}