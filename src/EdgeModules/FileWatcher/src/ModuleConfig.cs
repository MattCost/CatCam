namespace CatCam.EdgeModules.FileWatcher;

public class ModuleConfig
{
    public string WatchPath {get;set;} = "/data";
    public List<string> Filters {get;set;} = new List<string>{"*.avi", "*.mjpeg"};
    public bool SendMessage {get;set;} = true;
    public bool AlwaysUpload {get;set;} = true;
    
}