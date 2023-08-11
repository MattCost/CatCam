using System.Text.Json.Nodes;
using CatCam.Common.Exceptions;
using CatCam.Common.Services.Secrets;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;

namespace CatCam.Common.Services.IOTHub;

public class IOTHubService : IIOTHubService
{
    private readonly ILogger _logger;
    private readonly ISecretsManager _secretsManager;

    private ServiceClient? _serviceClient = null;
    private ServiceClient ServiceClient
    {
        get
        {
            return _serviceClient ??= CreateServiceClient();
        }
    }

    private RegistryManager? _registryManager = null;
    private RegistryManager RegistryManager
    {
        get
        {
            return _registryManager ??= CreateRegistryManager();
        }
    }

    public IOTHubService(ILogger<IOTHubService> logger, ISecretsManager secretsManager)
    {
        _logger = logger;
        _secretsManager = secretsManager;
    }

    private RegistryManager CreateRegistryManager()
    {
        var connString = _secretsManager.GetSecret("IOT_HUB_CONNECTION_STRING");
        return RegistryManager.CreateFromConnectionString(connString);
    }

    private ServiceClient CreateServiceClient()
    {
        var connString = _secretsManager.GetSecret("IOT_HUB_CONNECTION_STRING");
        return ServiceClient.CreateFromConnectionString(connString);
    }
    public async Task<IEnumerable<string>> GetDeviceIds()
    {
        var output = new List<string>();
        await RegistryManager.OpenAsync();
        _logger.LogTrace("=== Querying twins ===");

        string queryText = $"SELECT deviceId FROM devices";
        _logger.LogDebug("Query: {Query}", queryText);

        IQuery query = RegistryManager.CreateQuery(queryText);

        while (query.HasMoreResults)
        {
            var jsonS = await query.GetNextAsJsonAsync();
            foreach (var json in jsonS)
            {
                _logger.LogDebug("Json: {Json}", json);
                var node = JsonNode.Parse(json);
                if (node != null && node["deviceId"] != null)
                {
                    output.Add(node["deviceId"]!.GetValue<string>());
                }
            }
        }

        return output;

        //     foreach (Twin twin in twins)
        //     {
        //         output.Add(twin);
        //         _logger.LogDebug("DeviceId {DeviceId} IsEdge {IsEdge}. Is Connected {Connected}.", twin.DeviceId, twin.Capabilities.IotEdge, twin.ConnectionState);
        //         if (!string.IsNullOrWhiteSpace(twin.DeviceScope))
        //         {
        //             _logger.LogDebug("Device scope: {DeviceScope}", twin.DeviceScope);
        //         }
        //         if (twin.ParentScopes?.Any() ?? false)
        //         {
        //             _logger.LogDebug("Parent scope: {ParentScopes}", twin.ParentScopes[0]);
        //         }
        //     }
        // }

        // return output;
    }
    public async Task<Twin> GetDeviceTwin(string iotHubDeviceId)
    {
        await RegistryManager.OpenAsync();
        var twin = await RegistryManager.GetTwinAsync(iotHubDeviceId);
        var desired = twin.Properties.Desired; 
        _logger.LogDebug("Desired: {Desired}.", desired);
        _logger.LogDebug("Desired string: {Desired}.", desired.ToJson());
        return twin;
    }

    public async Task<Twin> GetModuleTwin(string iotHubDeviceId, string moduleName)
    {
        await RegistryManager.OpenAsync();
        var twin = await RegistryManager.GetTwinAsync(iotHubDeviceId, moduleName);
        var desired = twin?.Properties.Desired ?? throw new EntityNotFoundException($"{iotHubDeviceId}/{moduleName} not found.");
        _logger.LogDebug("Desired: {Desired}.", desired);
        _logger.LogDebug("Desired string: {Desired}.", desired.ToJson());
        return twin;
    }

    public Task DeployToDevice(string iotHubDeviceId, string deploymentJson)
    {
        throw new NotImplementedException();
    }

    public async Task SetDeviceDesiredProperty(string iotHubDeviceId, string propertyName, object? value) => await SetDeviceDesiredProperties(iotHubDeviceId, new Dictionary<string, object?> { { propertyName, value } });
    public async Task RemoveDeviceDesiredProperty(string iotHubDeviceId, string propertyName) => await SetDeviceDesiredProperty(iotHubDeviceId, propertyName, null);

    public async Task SetDeviceDesiredProperties(string iotHubDeviceId, Dictionary<string, object?> properties)
    {
        _logger.LogDebug("Device {Device}. Setting properties {Properties}", iotHubDeviceId, properties);
        await RegistryManager.OpenAsync();
        try
        {
            _logger.LogTrace("Fetching current twin for {Device}", iotHubDeviceId);
            var twin = await RegistryManager.GetTwinAsync(iotHubDeviceId);
            _logger.LogTrace("Sending patch {Patch}", properties.GetTwinPatch());
            await RegistryManager.UpdateTwinAsync(iotHubDeviceId, properties.GetTwinPatch(), twin.ETag);
            _logger.LogTrace("Properties updated!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting desired Properties");
        }
    }

    public async Task SetModuleDesiredProperties(string iotHubDeviceId, string moduleName, Dictionary<string, object?> properties)
    {
        _logger.LogDebug("Device {Device}. Setting properties {Properties}", iotHubDeviceId, properties);
        await RegistryManager.OpenAsync();
        try
        {
            _logger.LogTrace("Fetching current twin for {Device}/{Module}", iotHubDeviceId, moduleName);
            var twin = await RegistryManager.GetTwinAsync(iotHubDeviceId, moduleName);
            _logger.LogTrace("Sending patch {Patch}", properties.GetTwinPatch());
            await RegistryManager.UpdateTwinAsync(iotHubDeviceId, moduleName, properties.GetTwinPatch(), twin.ETag);
            _logger.LogTrace("Properties updated!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting desired Properties");
        }
    }

    public async Task SetModuleDesiredProperty(string iotHubDeviceId, string moduleName, string propertyName, object? value) => await SetModuleDesiredProperties(iotHubDeviceId, moduleName, new Dictionary<string, object?> { { propertyName, value } });
    public async Task RemoveModuleDesiredProperty(string iotHubDeviceId, string moduleName, string propertyName) => await SetModuleDesiredProperty(iotHubDeviceId, moduleName, propertyName, null);
}
