using CatCam.EdgeModules.FileWatcher.Services;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using System.Text;

namespace CatCam.EdgeModules.FileWatcher;

internal class ModuleBackgroundService : BackgroundService
{
    private readonly ILogger<ModuleBackgroundService> _logger;
    private CancellationToken _cancellationToken;
    private ModuleConfig _activeConfig = new();
    private FileSystemWatcher? _fileWatcher;
    private readonly IModuleClientWrapper _moduleClient;
    private readonly IFileUploadService _fileUploadService;

    public ModuleBackgroundService(ILogger<ModuleBackgroundService> logger, IModuleClientWrapper moduleClient, IFileUploadService fileUploadService)
    {
        _logger = logger;
        _moduleClient = moduleClient;
        _fileUploadService = fileUploadService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        // From the sample, see what it does
        // Reconnect is not implented because we'll let docker restart the process when the connection is lost
        _moduleClient.SetConnectionStatusChangesHandler((status, reason) => 
            _logger.LogWarning("Connection changed: Status: {status} Reason: {reason}", status, reason));

        await _moduleClient.OpenAsync(cancellationToken);

        _logger.LogInformation("IoT Hub module client initialized. Getting settings.");

        var moduleTwin = await _moduleClient.GetTwinAsync(cancellationToken);
        await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired,_moduleClient);

        // Register callbacks
        await _moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, _moduleClient, _cancellationToken);
        // Need any message handlers?

        // Need a watchdog of some sort... if we throw an ex, it doesn't seem to kill the console app, nor log anything
    }

    private async Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
    {
        ModuleConfig? newConfig;
        try
        {
            newConfig = System.Text.Json.JsonSerializer.Deserialize<ModuleConfig>(desiredProperties.ToJson());
            if(newConfig == null)
            {
                _logger.LogWarning("Unable to extract ModuleConfig from Desired Properties. Ignoring");
                return;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception when trying to extract ModuleConfig from Desired Properties. Ingoring");
            return;
        }

        _logger.LogTrace("Stopping Processing");
        await StopProcessingAsync(_cancellationToken);

        _activeConfig = newConfig;

        _logger.LogTrace("Starting Processing");
        await StartProcessingAsync(_cancellationToken);
    }

    private async Task StopProcessingAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Entering Stop Processing...");
        if(_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Created -= FileCreatedEvent;
            _fileWatcher.Dispose();
        }
        await Task.CompletedTask;
        _logger.LogTrace("Stop Processing complete");
    }

    private async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Entering Start Processing...");

        _logger.LogDebug("Watching files in {Path}", _activeConfig.WatchPath);

        _fileWatcher = new FileSystemWatcher(_activeConfig.WatchPath);        

        _fileWatcher.Created += FileCreatedEvent;
        // _fileWatcher.Changed += FileChangedEvent;
        _fileWatcher.EnableRaisingEvents = true;

        await Task.CompletedTask;
        _logger.LogTrace("Start Processing complete");
    }
    
    private void FileCreatedEvent(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File '{FileName}' Created at {FullPath}", e.Name, e.FullPath);
        DealWithEvent(sender, e);
    }

    private void FileChangedEvent(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File '{FileName}' Changed at {FullPath}", e.Name, e.FullPath);
        DealWithEvent(sender, e);
    }

    private void DealWithEvent(object sender, FileSystemEventArgs e)
    {
        if(_activeConfig.SendMessage)
        {
            var message = new CatCam.Common.Messages.ClipCreated
            {
                DateTime = DateTime.UtcNow,
                FileName = e.Name ?? string.Empty,
                FullPath = e.FullPath
            };

            SendIOTMessage(message).Wait(_cancellationToken);
        }

        if(_activeConfig.AlwaysUpload)
        {
            _logger.LogDebug("Calling File Upload Service for '{Name}'...", e.Name);
            _fileUploadService.UploadFile(e.FullPath, _cancellationToken);
        }
    }

    private async Task SendIOTMessage(object message, string channel = "upstream")
    {
        if(_moduleClient == null)
        {
            throw new InvalidOperationException("Module client is null. Can't SendIOTMessage");
        }
        var json = System.Text.Json.JsonSerializer.Serialize(message);
        _logger.LogDebug("Sending IOT Message {Json}", json);
        var msgBytes = Encoding.UTF8.GetBytes(json);
        var iotMessage = new Message(msgBytes)
        {
            ContentEncoding = "utf-8",
            ContentType = "application/json"
        };
        await _moduleClient.SendEventAsync(channel, iotMessage, _cancellationToken);
    }
}
