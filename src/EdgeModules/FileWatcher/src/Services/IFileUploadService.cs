namespace CatCam.EdgeModules.FileWatcher.Services;

public interface IFileUploadService
{
    Task UploadFile(string filename, CancellationToken cancellationToken);
}