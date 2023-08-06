using CatCam.Common.Services.Storage;

namespace CatCam.Common.Models;

public class Camera
{
    public Guid Id { get; set; }
    public Guid Location { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool EnableSaving { get; set; } = true;
    public string IotHubDeviceName { get; set; } = string.Empty;

}

public class CameraUpdateModel
{
    public Guid? Location { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
    public bool? EnableSaving { get; set; }
    public string? IotHubDeviceName { get; set; }
}