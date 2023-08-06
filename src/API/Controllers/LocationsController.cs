using System.Formats.Asn1;
using CatCam.Common.Exceptions;
using CatCam.Common.Models;
using CatCam.Common.Services.EntityProvider;
using CatCam.Common.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCap.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{

    private readonly ILogger<LocationsController> _logger;
    private readonly IEntityProvider _entityProvider;

    public LocationsController(ILogger<LocationsController> logger, IEntityProvider entityProvider)
    {
        _logger = logger;
        _entityProvider = entityProvider;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<LocationModel>>> Get()
    {
        try
        {
            return Ok(await _entityProvider.GetLocationModelsAsync());
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get Locations.");
            return Problem();
        }
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<LocationModel>> Get([FromRoute] Guid id)
    {
        try
        {
            return Ok(await _entityProvider.GetLocationModelAsync(id));
        }
        catch(EntityNotFoundException)
        {
            return NotFound();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get Location Id {Id}.", id);
            return Problem();
        }
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] LocationModel model)
    {
        try
        {
            await _entityProvider.UpsertLocationModelAsync(model);
            return Ok();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to create Location.");
            return Problem();
        }
    }

}


