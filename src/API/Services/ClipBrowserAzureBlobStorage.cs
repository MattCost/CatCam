/*

gets clips from azure storage

*/
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatCam.API.Services;

public class ClipBrowserAzureBlobStorage : IClipBrowser
{
    private readonly ILogger _logger;
    private readonly ISecretsManager _secretsManager;
    private BlobServiceClient _blobClient;
    public ClipBrowserAzureBlobStorage(ILogger<ClipBrowserAzureBlobStorage> logger, ISecretsManager secretsManager)
    {
        _logger = logger;
        _secretsManager = secretsManager;

        var connectionString = _secretsManager.GetSecret("STORAGE_ACT_CONNECTION_STRING");
        _blobClient = new BlobServiceClient(connectionString);
    }
    public async Task<IEnumerable<string>> GetClipNames()
    {
        var output = new List<string>();
        var containerClient = _blobClient.GetBlobContainerClient("uploads");
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            output.Add(blobItem.Name);
        }

        return output;
    }

    public async Task<IEnumerable<string>> GetClipNames(string cameraName)
    {
        var output = new List<string>();
        var containerClient = _blobClient.GetBlobContainerClient("uploads");
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix:$"dev-pi-01/{cameraName}"))
        {
            output.Add(blobItem.Name);
        }

        return output;
    }

    public Task<Uri> GetClipSasUri(string cameraName, string clipName)
    {
        var containerClient = _blobClient.GetBlobContainerClient("uploads");
        var blobClient =  containerClient.GetBlobClient($"dev-pi-01/{cameraName}/{clipName}");
        return Task.FromResult(blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, new DateTimeOffset( DateTime.UtcNow + TimeSpan.FromHours(1))));
    }
}