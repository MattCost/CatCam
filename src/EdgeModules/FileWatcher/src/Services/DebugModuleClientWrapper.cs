using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace CatCam.EdgeModules.FileWatcher.Services;

public class DebugModuleClientWrapper : IModuleClientWrapper
{
    private readonly ILogger _logger;
    public DebugModuleClientWrapper(ILogger<DebugModuleClientWrapper> logger)
    {
        _logger = logger;
    }
    public Task CloseAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => _logger.LogDebug("Closing Client"), cancellationToken);
    }

    public Task<Twin> GetTwinAsync(CancellationToken cancellationToken)
    {
        var debugConfig = new ModuleConfig
        {
            WatchPath = "/home/matt/data/whatever",
            Filters = new List<string> { "*.txt" },
            SendMessage = true,
            AlwaysUpload = true
        };

        var json = System.Text.Json.JsonSerializer.Serialize(debugConfig);
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

    public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler)
    {
        _logger.LogDebug("Setting Connection Status Change Handler.");
    }

    public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext, CancellationToken cancellationToken)
    {
        return Task.Run(() => _logger.LogDebug("Setting Desired Property Update Callback handler"), cancellationToken);
    }
}