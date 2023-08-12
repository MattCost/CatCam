using CatCam.EdgeCommon.Modules;

namespace CatCam.EdgeModules.FileWatcher;

public class FileWatcherConfiguration : EdgeModuleConfigurationBase
{
    public string WatchPath {get;set;} = "/data";
    public List<string> Filters {get;set;} = new List<string>{"*.avi", "*.mjpeg"};
    public bool SendMessage {get;set;} = true;
    public bool AlwaysUpload {get;set;} = true;

    public override void Validate()
    {
        if(string.IsNullOrEmpty(WatchPath))
        {
            throw new Exception();
        }

        if(Filters is null || Filters.Count == 0)
        {
            throw new Exception();
        }
    }
}