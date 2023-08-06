using CatCam.Common.Models;

namespace CatCam.Common.Services.EntityProvider;

public interface IEntityProvider
{
    Task<SiteModel> GetSiteModelAsync(Guid siteId);
    Task<IEnumerable<SiteModel>> GetSiteModelsAsync();
    Task UpsertSiteModelAsync(SiteModel model);
    Task DeleteSiteModelAsync(Guid siteId);

    Task<LocationModel> GetLocationModelAsync(Guid locationId);
    Task<IEnumerable<LocationModel>> GetLocationModelsAsync();
    Task UpsertLocationModelAsync(LocationModel model);
    Task DeleteLocationModelAsync(Guid locationId);

}