using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CatCam.EdgeCommon.Services.Clients;

public class DebugModuleClientWrapper<TConfig> : IModuleClientWrapper where TConfig : class
{
    private readonly ILogger _logger;
    private readonly TConfig _config;
    public DebugModuleClientWrapper(ILogger<DebugModuleClientWrapper<TConfig>> logger, IOptions<TConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => _logger.LogDebug("Closing Client"), cancellationToken);
    }

    public Task<Twin> GetTwinAsync(CancellationToken cancellationToken)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_config);
        var Properties = new TwinProperties
        {
            Desired = new TwinCollection(json)
        };
        return Task.FromResult(new Twin(Properties));
    }

    public Task OpenAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => _logger.LogDebug("Opening Client"), cancellationToken);
    }

    public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken)
    {

        return Task.Run(() => _logger.LogDebug("Module Client sending msg to {Output}", outputName), cancellationToken);
    }

    public async Task SendMessage(string channel, object message, CancellationToken cancellationToken)
    {
        var msgJson = System.Text.Json.JsonSerializer.Serialize(message);
        _logger.LogDebug("Sending message {Message} to {Channel}", msgJson, channel);
        await Task.Yield();
    }

    public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler)
    {
        _logger.LogDebug("Setting Connection Status Change Handler.");
    }

    public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext, CancellationToken cancellationToken)
    {
        return Task.Run(() => _logger.LogDebug("Setting Desired Property Update Callback handler"), cancellationToken);
    }

    public Task SetInputMessageHandlerAsync(string name, MessageHandler messageHandler, object userContext)
    {
        throw new NotImplementedException();
    }
}