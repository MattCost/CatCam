using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using CatCam.EdgeCommon.Services.Clients;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport;
using Microsoft.Extensions.Logging;

namespace CatCam.EdgeCommon.Services.FileUpload;
public class BlobStorageUploadService : IFileUploadService
{
    private readonly ILogger _logger;
    private readonly IDeviceClientWrapper _deviceClient;
    public BlobStorageUploadService(ILogger<BlobStorageUploadService> logger, IDeviceClientWrapper deviceClient)
    {
        _logger = logger;
        _deviceClient = deviceClient;
    }

    public async Task UploadFile(string filename, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Entering Upload File {file}. Calculating Blob Filename.", filename);
        var filenameParts = filename.Split('/');
        string blobName = filenameParts[^1];
        _logger.LogDebug("Parts Count {Count}. Parts {Parts}", filenameParts.Length, filenameParts);
        if (filenameParts.Length >= 2)
        {
            blobName = filenameParts[^2] + "/" + filenameParts[^1];
        }
        _logger.LogDebug("Blob Name {BlobName}", blobName);
        _logger.LogDebug("Waiting for File Write to be complete");
        var startedAt = DateTime.UtcNow;
        var maxDuration = TimeSpan.FromMinutes(5);
        do
        {
            try
            {
                using FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                await DoTheUpload(fileStream, blobName, cancellationToken);
                return;
            }
            catch(IOException ex) when (ex.Message.Contains("because it is being used by another process"))
            {
                await Task.Delay(500,cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while trying to open file {filename}", filename);
                return;
            }
        }
        while(DateTime.UtcNow - startedAt < maxDuration);
        _logger.LogError("Waited {Duration} for access to file, but timed out", maxDuration);
    }

    private async Task DoTheUpload(FileStream fileStreamSource, string blobName, CancellationToken cancellationToken)
    {
        var filename = Path.GetFileName(fileStreamSource.Name);
        _logger.LogDebug("Uploading file {File}", filename);

        try
        {
            var fileUploadSasUriRequest = new FileUploadSasUriRequest
            {
                BlobName = blobName
            };

            _logger.LogTrace("Sending Sas uri request");
            FileUploadSasUriResponse sasUri = await _deviceClient.GetFileUploadSasUriAsync(fileUploadSasUriRequest, cancellationToken);

            _logger.LogTrace("Getting blob uri from response");
            Uri uploadUri = sasUri.GetBlobUri();

            _logger.LogTrace("Generating file stream");
            // using var fileStreamSource = new FileStream(filename, FileMode.Open);
            var blockBlobClient = new BlockBlobClient(uploadUri);
            await blockBlobClient.UploadAsync(fileStreamSource, new BlobUploadOptions(), cancellationToken);
            _logger.LogTrace("Upload Complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while trying to upload file {Filename}", fileStreamSource.Name);
        }

    }
}
