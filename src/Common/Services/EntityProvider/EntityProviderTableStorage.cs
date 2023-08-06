using Azure.Data.Tables;
using CatCam.Common.Exceptions;
using CatCam.Common.Models;
using CatCam.Common.Services.Secrets;
using Microsoft.Extensions.Logging;

namespace CatCam.Common.Services.EntityProvider;

public class EntityProviderTableStorage : IEntityProvider
{
    protected readonly ISecretsManager _secretsManager;
    private static string TableName => "Entities";
    private static string Sites => nameof(Sites);
    private static string Locations => nameof(Locations);
    protected readonly ILogger<EntityProviderTableStorage> _logger;
    protected TableClient TableClient { get; private set; }
    public EntityProviderTableStorage(ILogger<EntityProviderTableStorage> logger, ISecretsManager secretsManager)
    {
        _logger = logger;
        _secretsManager = secretsManager;

        var connectionString = _secretsManager.GetSecret("STORAGE_ACT_CONNECTION_STRING");
        TableClient = new TableClient(connectionString, TableName);

        _secretsManager = secretsManager;
    }

    #region PublicInterface
    public async Task<SiteModel> GetSiteModelAsync(Guid siteId) => await GetEntityAsync<SiteModel>(siteId.ToString(), Sites);
    public async Task<IEnumerable<SiteModel>> GetSiteModelsAsync() => await GetAllAsync<SiteModel>(Sites);
    public async Task UpsertSiteModelAsync(SiteModel model) => await UpsertAsync(model, Sites, model.Id.ToString());
    public async Task DeleteSiteModelAsync(Guid siteId)
    {
        var locations = await GetLocationsForSite(siteId);
        if (locations.Any())
        {
            throw new Exception("Site has child locations");
        }
        await DeleteAsync(Sites, siteId.ToString());
    }

    public async Task<LocationModel> GetLocationModelAsync(Guid locationId) => await GetEntityAsync<LocationModel>(locationId.ToString(), Locations);

    public async Task<IEnumerable<LocationModel>> GetLocationModelsAsync() => await GetAllAsync<LocationModel>(Locations);

    public async Task UpsertLocationModelAsync(LocationModel model)
    {
        var parentSiteId = model.SiteId;
        try
        {
            var parentSite = await GetSiteModelAsync(parentSiteId);
        }
        catch (EntityNotFound)
        {
            throw new Exception("Invalid Parent Site Id");
        }
        catch (Exception)
        {
            throw new Exception("Unable to verify parent Site Id");
        }

        await UpsertAsync(model, Locations, model.Id.ToString());
    }

    public async Task DeleteLocationModelAsync(Guid locationId)
    {
        //no delete if child entities
        await DeleteAsync(Locations, locationId.ToString());
    }

    #endregion


    #region PrivateMethods
    private static TModel CreateFromTableEntity<TModel>(TableEntity tableEntity) where TModel : class, new()
    {
        var output = new TModel();
        var properties = typeof(TModel).GetProperties();
        foreach (var property in properties)
        {
            if (!tableEntity.ContainsKey(property.Name))
                continue;

            var value = IsSupportedType(property.PropertyType) ?
                tableEntity[property.Name] :
                System.Text.Json.JsonSerializer.Deserialize(tableEntity[property.Name].ToString() ?? "{}", property.PropertyType);

            property.SetValue(output, value);
        }
        return output;
    }
    private async Task<TModel> GetEntityAsync<TModel>(string rowKey, string partitionKey) where TModel : class, new()
    {
        _logger.LogTrace("Getting Partition:Row {Partition}:{Row} from Table {Table}", partitionKey, rowKey, TableName);
        try
        {
            var tableEntity = await TableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
            return CreateFromTableEntity<TModel>(tableEntity);

        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Partition:Row {Partition}:{Row} not found in Table {Table}", partitionKey, rowKey, TableName);
            throw new EntityNotFound($"{TableName}:{partitionKey}:{rowKey} not found");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    private async Task<IEnumerable<TModel>> GetAllAsync<TModel>(string partitionKey) where TModel : class, new()
    {
        try
        {
            await Task.CompletedTask;
            var queryFilter = $"PartitionKey eq '{partitionKey}'";
            var queryResults = TableClient.QueryAsync<TableEntity>(filter: queryFilter).ToBlockingEnumerable();
            var output = queryResults.Select(CreateFromTableEntity<TModel>).ToList();
            return output;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Table {TableName} not found", TableName);
            throw new EntityNotFound($"{TableName} not found");
        }

        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    private static bool IsSupportedType(Type type)
    {
        if (type == typeof(string)) return true;
        if (type == typeof(Guid)) return true;
        if (type == typeof(bool)) return true;
        if (type == typeof(DateTime)) return true;
        if (type == typeof(double)) return true;
        if (type == typeof(int)) return true;
        if (type == typeof(long)) return true;

        return false;
    }
    private static Dictionary<string, object> ModelToDict<TModel>(TModel model)
    {
        var dict = new Dictionary<string, object>();

        foreach (var property in typeof(TModel).GetProperties())
        {
            dict[property.Name] = IsSupportedType(property.PropertyType) ?
                property.GetValue(model) ?? new object() :
                System.Text.Json.JsonSerializer.Serialize(property.GetValue(model));
        }

        return dict;
    }
    private async Task UpsertAsync<TModel>(TModel model, string partitionKey, string rowKey)
    {
        try
        {
            var tableEntity = new TableEntity(ModelToDict(model))
            {
                RowKey = rowKey,
                PartitionKey = partitionKey
            };
            await TableClient.UpsertEntityAsync(tableEntity);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Table store returned 404");
            throw new EntityNotFound("Table not found?");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    private async Task DeleteAsync(string partitionKey, string rowKey)
    {
        try
        {

            await TableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Partition:Row {Partition}:{Row} not found in Table {Table}", partitionKey, rowKey, TableName);
            throw new EntityNotFound($"{TableName}:{partitionKey}:{rowKey} not found");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }

    #endregion

    private async Task<IEnumerable<LocationModel>> GetLocationsForSite(Guid siteId)
    {
        try
        {
            await Task.CompletedTask;
            var queryFilter = $"PartitionKey eq '{Locations}' and SiteId eq '{siteId}'";
            var queryResults = TableClient.QueryAsync<TableEntity>(filter: queryFilter).ToBlockingEnumerable();
            var output = queryResults.Select(CreateFromTableEntity<LocationModel>).ToList();
            return output;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Table {TableName} not found", TableName);
            throw new EntityNotFound($"{TableName} not found");
        }

        catch (Azure.RequestFailedException ex) when (ex.Status == 400)
        {
            _logger.LogError(ex, "Bad Request generated");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception");
            throw;
        }
    }
}