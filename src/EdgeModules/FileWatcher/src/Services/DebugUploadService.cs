namespace CatCam.EdgeModules.FileWatcher.Services;

public class DebugUploadService : IFileUploadService
{
    private ILogger _logger;
    public DebugUploadService(ILogger<DebugUploadService> logger)
    {
        _logger = logger;
    }
    public async Task UploadFile(string filename, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Entering Upload File {file}. Waiting for exclusive access to file", filename);
        bool exclusiveAccess = false;
        do
        {
            try
            {
                using(FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    _logger.LogTrace("Uploading file {File}", filename);
                }
                return;
            }
            catch(IOException)
            {
                // _logger.LogDebug(ex, "unable to lock file {file}", filename);
                await Task.Delay(100,cancellationToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception while trying to open file {filename}", filename);
                return;
            }
        }
        while(!exclusiveAccess);
        // TODO add some long running timeout? Videos could go on a while.
    }
}