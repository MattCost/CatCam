using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;

namespace CatCam.Common.Services.IOTHub;

public class ModuleConfigurationProvider
{
    private readonly ILogger<ModuleConfigurationProvider> _logger;
    public ModuleConfigurationProvider(ILogger<ModuleConfigurationProvider> logger)
    {
        _logger = logger;
    }

    public object GetEdgeAgentModule()
    {
        var module = new Dictionary<string, object>();
        module["type"] = "docker";
        var settingsDict = new Dictionary<string, object>();
        settingsDict["image"] = "mcr.microsoft.com/azureiotedge-agent:1.4";
        module["settings"] = settingsDict;
        var envVarDict = new Dictionary<string, object>();
        envVarDict["SendRuntimeQualityTelemetry"] = new { value = false};
        module["env"] = envVarDict;
        return module;
    }

    public object GetEdgeHubModule()
    {
        var module = new Dictionary<string, object>();
        module["type"] = "docker";
        module["status"] = "running";
        module["restartPolicy"] = "always";
        var settingsDict = new Dictionary<string, object>();
        settingsDict["image"] = "mcr.microsoft.com/azureiotedge-hub:1.4";
        settingsDict["createOptions"] = "{json string here}";

        module["settings"] = settingsDict;
        return module;
    }

    public string GetEdgeAgentPropertiesJson()
    {
        var properties = new Dictionary<string, object>();
        properties["schemaVersion"] = "1.1";

        var runtimeDict = new Dictionary<string, object>();
        properties["runtime"] = runtimeDict;

        runtimeDict["type"] = "docker";
        var settingsDict = new Dictionary<string, object>();
        runtimeDict["settings"] = settingsDict;

        var regDict = new Dictionary<string, object>();
        settingsDict["registryCredentials"] = regDict;
        regDict["catcamacr"] = new { address = "catcamacr.azurecr.io", password = "hunter2", username = "edge-deployments" };


        var systemModulesDict = new Dictionary<string, object>();
        systemModulesDict["edgeAgent"] = GetEdgeAgentModule();
        systemModulesDict["edgeHub"] =  GetEdgeHubModule();

        properties["systemModules"] = systemModulesDict;

        var modulesDict = new Dictionary<string, object>();

        properties["modules"] = modulesDict;

        return GetModulePropertiesJson(properties);
    }

    public string GetEdgeHubPropertiesJson()
    {
        var properties = new Dictionary<string, object>();
        properties["schemaVersion"] = "1.1.";
        properties["storeAndForwardConfiguration"] = new { timeToLiveSecs = 7200 };
        var routes = new Dictionary<string, object>();
        routes["upstream"] = new { route = "FROM /messages/* INTO $upstream" };
        properties["routes"] = routes;

        return GetModulePropertiesJson(properties);

    }

    public string GetModuleJson(string moduleName, string source, string containerOptions, List<string> envVars)
    {
        return string.Empty;
    }

    public string GetModulePropertiesJson(Dictionary<string, object> properties)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(properties);
        _logger.LogDebug("Properties as json: {Json}", json);
        var output = $@"""properties.desired"" : {json}";

        _logger.LogDebug("Output: {Output}", output);
        return output;

    }

}