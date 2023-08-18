using Microsoft.Azure.Devices.Shared;

namespace CatCam.Common.Services.IOTHub;

public interface IIOTHubService
{
    public Task<IEnumerable<string>> GetDeviceIds(); 
    public Task<Twin> GetDeviceTwin(string iotHubDeviceId);
    public Task<Twin> GetModuleTwin(string iotHubDeviceId, string moduleName);

    public Task DeployToDevice(string iotHubDeviceId, string deploymentJson);

    public Task SetDeviceDesiredProperty(string iotHubDeviceId, string propertyName, object? value);
    public Task SetDeviceDesiredProperties(string iotHubDeviceId, Dictionary<string, object?> properties);
    public Task RemoveDeviceDesiredProperty(string iotHubDeviceId, string propertyName);

    public Task SetModuleDesiredProperty(string iotHubDeviceID, string moduleName, string propertyName, object? value);
    public Task RemoveModuleDesiredProperty(string iotHubDeviceId, string moduleName, string propertyName);
    public Task SetModuleDesiredProperties(string iotHubDeviceId, string moduleName, Dictionary<string, object?> properties);
    

    public Task AddModule(string iotHubDeviceId, string moduleName, object moduleContent);

}