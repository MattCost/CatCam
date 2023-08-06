namespace CatCam.Common.Services.Storage;

public interface IClipBrowser
{
    Task<IEnumerable<string>> GetClipNames();
    Task<IEnumerable<string>> GetClipNames(string cameraName);
    Task<Uri> GetClipSasUri(string cameraName, string clipName);

}