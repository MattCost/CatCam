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
public abstract class EntityProviderBaseController : ControllerBase
{

    private readonly ILogger<EntityProviderBaseController> _logger;
    protected ILogger Logger { get { return _logger; } }
    
    private readonly IEntityProvider _entityProvider;

    protected IEntityProvider EntityProvider { get { return _entityProvider; } }

    public EntityProviderBaseController(ILogger<EntityProviderBaseController> logger, IEntityProvider entityProvider)
    {
        _logger = logger;
        _entityProvider = entityProvider;
    }

    protected async Task<ActionResult<T>> EntityProviderActionHelper<T>( Func<Task<T>> action, string errorMessage)
    {
        try
        {
            return Ok(await action());
        }
        catch(EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entity not found.");
            return NotFound();
        }
        catch(EntityLockedException ex)
        {
            _logger.LogWarning(ex, "Entity locked");
            return new StatusCodeResult(StatusCodes.Status423Locked);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected Exception. Error message: {Message}", errorMessage);
            return Problem(errorMessage);
        }

    }

    protected async Task<ActionResult> EntityProviderActionHelper( Func<Task> action, string errorMessage)
    {
        try
        {
            await action();
            return Ok();
        }
        catch(EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entity not found.");
            return NotFound();
        }
        catch(EntityLockedException ex)
        {
            _logger.LogWarning(ex, "Entity locked");
            return new StatusCodeResult(StatusCodes.Status423Locked);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpected Exception. Error message: {Message}", errorMessage);
            return Problem(errorMessage);
        }

    }

}


