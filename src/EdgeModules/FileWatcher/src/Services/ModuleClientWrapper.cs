using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;

namespace CatCam.EdgeModules.FileWatcher.Services;

public class ModuleClientWrapper : IModuleClientWrapper
{
    private readonly ModuleClient _moduleClient;

    public ModuleClientWrapper()
    {
        MqttTransportSettings mqttSetting = new(TransportType.Mqtt_Tcp_Only);
        ITransportSettings[] settings = { mqttSetting };

        // Open a connection to the Edge runtime
        _moduleClient = ModuleClient.CreateFromEnvironmentAsync(settings).Result;
    }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
        return _moduleClient.CloseAsync(cancellationToken);
    }

    public Task<Twin> GetTwinAsync(CancellationToken cancellationToken)
    {
        return _moduleClient.GetTwinAsync(cancellationToken);
    }

    public Task OpenAsync(CancellationToken cancellationToken)
    {
        return _moduleClient.OpenAsync(cancellationToken);
    }

    public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken)
    {
        return _moduleClient.SendEventAsync(outputName, message, cancellationToken);
    }

    public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler)
    {
        _moduleClient.SetConnectionStatusChangesHandler(statusChangesHandler);
    }

    public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext, CancellationToken cancellationToken)
    {
        return _moduleClient.SetDesiredPropertyUpdateCallbackAsync(callback, userContext, cancellationToken);
    }
}