using CatCam.EdgeCommon.Messages.Exceptions;
using CatCam.EdgeCommon.Modules;

namespace CatCam.EdgeModules.VideoCaptureSharp;

public class VideoCaptureConfiguration : EdgeModuleConfigurationBase
{
    public string CameraUrl { get; set; } = "http://192.168.50.22:8080/video";
    public string CameraName { get; set; } = "cameraName";
    public bool EnableSaving { get; set; } = true;

    
    /// <summary>
    /// How long is motion detected before we save it
    /// </summary>
    public int MinMotionDurationSeconds { get; set; } = 2;
    
    /// <summary>
    /// How long before motion starts to include in the final clip
    /// </summary>
    public int PreMotionCaptureDurationSeconds {get;set;} = 2;
    
    /// <summary>
    /// How long after motion ends to include in the final clip
    /// </summary>
    public int PostMotionCaptureDurationSeconds { get; set; } = 5;

    public int MinContourArea { get; set; } = 1000;

    public string TempPath {get;set;} = "/tmp/capture";
    public string SavePath { get; set; } = "/data/";
    public string FilenameTimestampFormat { get; set; } = "yyyy_MM_dd_HH_mm_ss";
    public string FilenameExtension { get; set; } = ".avi";

    public bool IncludeTimestamp { get;set;} = true;
    public bool DrawBoundingBoxes { get;set; } = false;
    public bool DrawOverallBoundingBox { get; set;} = false;
    
    public override void Validate()
    {
        if(string.IsNullOrEmpty(CameraUrl))
        {
            throw new InvalidPropertyException($"CameraUrl is invalid");
        }
    }
}
