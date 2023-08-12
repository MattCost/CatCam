using CatCam.EdgeCommon.Messages.Telemetry;
using CatCam.EdgeCommon.Modules;
using CatCam.EdgeCommon.Services.Clients;
using CatCam.EdgeCommon.Services.FileUpload;

namespace CatCam.EdgeModules.FileWatcher;

public class FileWatcherEdgeModule : EdgeModuleBase<FileWatcherConfiguration>
{
    private readonly IFileUploadService _fileUploadService;
    private FileSystemWatcher? _fileWatcher;

    public FileWatcherEdgeModule(ILogger<EdgeModuleBase<FileWatcherConfiguration>> logger, IModuleClientWrapper moduleClient, IFileUploadService fileUploadService) : base(logger, moduleClient)
    {
        _fileUploadService = fileUploadService;
    }

    public override async Task StartProcessing(CancellationToken token)
    {
        Logger.LogTrace("Entering Start Processing...");
        await base.StartProcessing(token);

        Logger.LogDebug("Watching files in {Path}", Configuration.WatchPath);
        _fileWatcher = new FileSystemWatcher(Configuration.WatchPath);

        // Was recording with *.tmp, then doing a rename, but Gstream didn't like the file extension
        // _fileWatcher.Renamed += FileRenamedEvent;
        // Writing video to temp folder, th        _fileWatcher = new FileSystemWatcher(_activeConfig.WatchPath);        
        _fileWatcher.Created += FileCreatedEvent;
        _fileWatcher.IncludeSubdirectories = true;
        _fileWatcher.EnableRaisingEvents = true;

        Logger.LogTrace("Start Processing complete");
    }

    public override async Task StopProcessing(CancellationToken token)
    {
        Logger.LogTrace("Entering Stop Processing...");
        await base.StopProcessing(token);
        if (_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Renamed -= FileRenamedEvent;
            _fileWatcher.Dispose();
            _fileWatcher = null;
        }
        await Task.CompletedTask;
        Logger.LogTrace("Stop Processing complete");
    }

    private void FileCreatedEvent(object sender, FileSystemEventArgs e)
    {
        Logger.LogDebug("File '{FileName}' Created at {FullPath}", e.Name, e.FullPath);
        DealWithEvent(sender, e);
    }

    private void FileRenamedEvent(object sender, FileSystemEventArgs e)
    {
        Logger.LogDebug("File '{FileName}' Renamed at {FullPath}", e.Name, e.FullPath);
        DealWithEvent(sender, e);
    }

    private void DealWithEvent(object sender, FileSystemEventArgs e)
    {
        if (Configuration.SendMessage)
        {
            var message = new ClipCreated
            {
                DateTime = DateTime.UtcNow,
                FileName = e.Name ?? string.Empty,
                FullPath = e.FullPath
            };

            ModuleClient.SendMessage("upstream", message, CancellationTokenSource.Token);
        }

        if (Configuration.AlwaysUpload)
        {
            Logger.LogDebug("Calling File Upload Service for '{Name}'...", e.Name);
            _fileUploadService.UploadFile(e.FullPath, CancellationTokenSource.Token);
        }
    }
}