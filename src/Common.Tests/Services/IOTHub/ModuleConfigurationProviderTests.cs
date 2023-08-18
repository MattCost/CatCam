using CatCam.Common.Services.IOTHub;
using Microsoft.Extensions.Logging;
using Moq;

namespace CatCam.Common.Tests.Services.IOTHub;

public class ModuleConfigurationProviderTests
{
    [Fact]
    public void GetModulePropertiesJson()
    {
        // Given
        var logger = UnitTestLogger.Create<ModuleConfigurationProvider>();
        var provider = new ModuleConfigurationProvider(logger);
        var properties = new Dictionary<string, object>();
        properties["test1"] = "test1Value";
        properties["test2"] = 42;
        // When
        var output = provider.GetModulePropertiesJson(properties);
        // Then
        Assert.Contains("test1", output);
    }

    [Fact]
    public void GetEdgeHubPropertiesJson()
    {
        var logger = UnitTestLogger.Create<ModuleConfigurationProvider>();
        var provider = new ModuleConfigurationProvider(logger);
        var output = provider.GetEdgeHubPropertiesJson();
        Assert.NotNull(output);
        Assert.Contains("routes", output);
        Assert.Contains("upstream", output);
        Assert.Contains("timeToLiveSecs", output);
    }

    [Fact]
    public void GetEdgeAgentPropertiesJson()
    {
        var logger = UnitTestLogger.Create<ModuleConfigurationProvider>();
        var provider = new ModuleConfigurationProvider(logger);
        var output = provider.GetEdgeAgentPropertiesJson();
        Assert.NotNull(output);

    }
}