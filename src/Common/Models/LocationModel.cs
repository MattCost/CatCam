using CatCam.Common.Services.Storage;

namespace CatCam.Common.Models;

public class LocationModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
}

public class LocationUpdateModel
{
    public string? Name { get; set; } = string.Empty;
}