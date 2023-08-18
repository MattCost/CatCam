using System.Formats.Asn1;
using CatCam.Common.Exceptions;
using CatCam.Common.Models;
using CatCam.Common.Services.EntityProvider;
using CatCam.Common.Services.IOTHub;
using CatCam.Common.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices.Shared;

namespace CatCap.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{

    private readonly ILogger<DevicesController> _logger;
    private readonly IIOTHubService _iOTHubClient;

    public DevicesController(ILogger<DevicesController> logger, IIOTHubService iOTHubClient)
    {
        _logger = logger;
        _iOTHubClient = iOTHubClient;
    }
    
    [HttpGet("deviceIds")]
    public async Task<IActionResult> GetDeviceIds()
    {
        return Ok(await _iOTHubClient.GetDeviceIds());
    }

    [HttpGet("{iotDeviceId}")]
    public async Task<IActionResult> GetDeviceTwin([FromRoute] string iotDeviceId)
    {
        var twin = await _iOTHubClient.GetDeviceTwin(iotDeviceId);

        return Ok(twin.ToJson(Newtonsoft.Json.Formatting.Indented));
    }

    [HttpPatch("{iotDeviceId}/properties")]
    public async Task<IActionResult> SetDesiredProperties([FromRoute] string iotDeviceId, [FromBody] Dictionary<string, object?> desiredProperties)
    {
        await _iOTHubClient.SetDeviceDesiredProperties(iotDeviceId, desiredProperties);
        return Ok();
    }
    
    [HttpDelete("{iotDeviceId}/properties/{propertyName}")]
    public async Task<IActionResult> DeleteDeviceProperty([FromRoute] string iotDeviceId, [FromRoute] string propertyName)
    {
        await _iOTHubClient.SetDeviceDesiredProperty(iotDeviceId, propertyName, null);
        return Ok();
    }

    [HttpGet("{iotDeviceId}/modules/{moduleName}")]
    public async Task<IActionResult> GetModuleTwin([FromRoute] string iotDeviceId, [FromRoute] string moduleName)
    {
        try
        {
            var twin = await _iOTHubClient.GetModuleTwin(iotDeviceId, moduleName);
            return Ok( twin.ToJson(Newtonsoft.Json.Formatting.Indented));
            
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Unable to get module twin");
            return Problem();
        }
    }

    [HttpPatch("{iotDeviceId}/modules/{moduleName}/properties")]
    public async Task<IActionResult> SetModuleDesiredProperties([FromRoute] string iotDeviceId, [FromRoute] string moduleName, [FromBody] Dictionary<string, object?> desiredProperties)
    {
        await _iOTHubClient.SetModuleDesiredProperties(iotDeviceId, moduleName, desiredProperties);
        return Ok();
    }

    [HttpDelete("{iotDeviceId}/modules/{moduleName}/properties/{propertyName}")]
    public async Task<IActionResult> DeleteModuleProperty([FromRoute] string iotDeviceId, [FromRoute] string moduleName, [FromRoute] string propertyName)
    {
        await _iOTHubClient.SetModuleDesiredProperty(iotDeviceId, moduleName, propertyName, null);
        return Ok();
    }

    [HttpGet("{iotDeviceId}/currentConfig")]
    public async Task<IActionResult> GetCurrentConfiguration([FromRoute] string iotDeviceId)
    {
        await _iOTHubClient.AddModule(iotDeviceId, "test", new object{});
        return Ok();
    }

}
