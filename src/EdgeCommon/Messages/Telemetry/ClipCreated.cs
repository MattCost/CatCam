namespace CatCam.EdgeCommon.Messages.Telemetry;

public class ClipCreated
{
    public DateTime DateTime {get; set;} = DateTime.UtcNow;
    public string FileName {get;set;} = string.Empty;
    public string FullPath {get;set;} = string.Empty;

}