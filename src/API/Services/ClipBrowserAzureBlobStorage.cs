/*

gets clips from azure storage

*/
namespace CatCam.API.Services;

public class ClipBrowserAzureBlobStorage : IClipBrowser
{
    public IEnumerable<string> GetFiles(string path)
    {
        throw new NotImplementedException();
    }
}