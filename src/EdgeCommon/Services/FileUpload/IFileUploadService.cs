namespace CatCam.EdgeCommon.Services.FileUpload;


public interface IFileUploadService
{
    Task UploadFile(string filename, CancellationToken cancellationToken);
}