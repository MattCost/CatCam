namespace CatCam.API.Services;

public interface IClipBrowser
{
    IEnumerable<string> GetFiles(string path);
}