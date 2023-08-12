using CatCam.EdgeCommon.Services.Clients;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport;
using Microsoft.Extensions.Logging;

namespace CatCat.EdgeCommon.Services.Clients;

public class DeviceClientWrapper : IDeviceClientWrapper
{
    private readonly DeviceClient _deviceClient;
    private readonly ILogger _logger;

    public DeviceClientWrapper(ILogger<DeviceClientWrapper> logger)
    {
        _logger = logger;
       var connectionString = Environment.GetEnvironmentVariable("IOT-HUB-CONNECTION-STRING");
        _deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
    }

    public async Task<FileUploadSasUriResponse> GetFileUploadSasUriAsync(FileUploadSasUriRequest request, CancellationToken cancellationToken = default)
    {
        return await _deviceClient.GetFileUploadSasUriAsync(request, cancellationToken);
    }
}