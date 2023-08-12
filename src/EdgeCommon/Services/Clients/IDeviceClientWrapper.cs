using Microsoft.Azure.Devices.Client.Transport;

namespace CatCam.EdgeCommon.Services.Clients;

public interface IDeviceClientWrapper
{
    public Task<FileUploadSasUriResponse> GetFileUploadSasUriAsync(FileUploadSasUriRequest request, CancellationToken cancellationToken = default);


}