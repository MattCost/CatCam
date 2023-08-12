using Microsoft.Extensions.Logging;

namespace CatCam.EdgeCommon.Services.FileUpload;

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
        _logger.LogDebug("Entering Upload File {file}. Waiting for file write to be complete", filename);
        var filenameParts = filename.Split('/');
        string blobName = filenameParts[filenameParts.Length - 1];
        _logger.LogDebug("Parts Count {Count}. Parts {Parts}", filenameParts.Length, filenameParts);
        if(filenameParts.Length >=2)
        {
            blobName = filenameParts[filenameParts.Length- 2] + "/" + filenameParts[filenameParts.Length-1];
        }
        _logger.LogDebug("Blob Name {BlobName}", blobName);


        bool exclusiveAccess = false;
        do
        {
            try
            {
                using(FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    _logger.LogTrace("Uploading file {File}", filename);
                    await Task.Delay(500);
                    _logger.LogTrace("Upload complete");
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