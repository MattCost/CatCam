using CatCam.Common.Services.Storage;

namespace CatCam.Common.Models;

public class SiteModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SiteUpdateModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }

}