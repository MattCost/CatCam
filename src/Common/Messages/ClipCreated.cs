namespace CatCam.Common.Messages;

public class ClipCreated
{
    public DateTime DateTime {get; set;} = DateTime.UtcNow;
    public string FileName {get;set;} = string.Empty;

}