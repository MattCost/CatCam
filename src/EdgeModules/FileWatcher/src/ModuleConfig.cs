namespace CatCam.EdgeModules.FileWatcher;

public class ModuleConfig
{
    public string WatchPath {get;set;} = "/video-storage";
    public bool SendMessage {get;set;} = true;
    public bool AlwaysUpload {get;set;} = false;
    
}