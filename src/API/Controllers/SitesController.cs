using System.Formats.Asn1;
using CatCam.Common.Exceptions;
using CatCam.Common.Models;
using CatCam.Common.Services.EntityProvider;
using CatCam.Common.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCap.API.Controllers;

// [Authorize]
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class SitesController : ControllerBase
{

    private readonly ILogger<SitesController> _logger;
    private readonly IEntityProvider _entityProvider;

    public SitesController(ILogger<SitesController> logger, IEntityProvider entityProvider)
    {
        _logger = logger;
        _entityProvider = entityProvider;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<SiteModel>>> Get()
    {
        try
        {
            return Ok(await _entityProvider.GetSiteModelsAsync());
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get Sites.");
            return Problem();
        }
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<SiteModel>> Get([FromRoute] Guid id)
    {
        try
        {
            return Ok(await _entityProvider.GetSiteModelAsync(id));
        }
        catch(EntityNotFound)
        {
            return NotFound();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get Sites.");
            return Problem();
        }
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] SiteModel model)
    {
        try
        {
            await _entityProvider.UpsertSiteModelAsync(model);
            return Ok();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get Sites.");
            return Problem();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            await _entityProvider.DeleteSiteModelAsync(id);
            return Ok();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to delete site Id {Id}", id);
            return Problem();
        }
    }

}


