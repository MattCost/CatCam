using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport;

namespace CatCam.EdgeModules.FileWatcher.Services;
public class BlobStorageUploadService : IFileUploadService
{
    private readonly DeviceClient _deviceClient;
    private readonly ILogger _logger;
    public BlobStorageUploadService(ILogger<BlobStorageUploadService> logger)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("IOT-HUB-CONNECTION-STRING");
        _deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
    }

    public async Task UploadFile(string filename, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Entering Upload File {file}. Waiting for exclusive access to file", filename);
        // TODO add some long running timeout? Videos could go on a while.
        while(true)
        {
            try
            {
                using FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                await DoTheUpload(fileStream, cancellationToken);
                return;
            }
            catch(IOException)
            {
                await Task.Delay(100,cancellationToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception while trying to open file {filename}", filename);
                return;
            }
        } 
    }
    
    private async Task DoTheUpload(FileStream fileStreamSource, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Uploading file {File}", fileStreamSource.Name);
        
        try
        {
            var fileUploadSasUriRequest = new FileUploadSasUriRequest
            {
                BlobName = fileStreamSource.Name
            };

            _logger.LogTrace("Sending Sas uri request");
            FileUploadSasUriResponse sasUri = await _deviceClient.GetFileUploadSasUriAsync(fileUploadSasUriRequest, cancellationToken);
            
            _logger.LogTrace("Getting blob uri from response");
            Uri uploadUri = sasUri.GetBlobUri();

            _logger.LogTrace("Generating file stream");
            // using var fileStreamSource = new FileStream(filename, FileMode.Open);
            var blockBlobClient = new BlockBlobClient(uploadUri);
            await blockBlobClient.UploadAsync(fileStreamSource, new BlobUploadOptions(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while trying to upload file {Filename}", fileStreamSource.Name);
        }

    }
}
