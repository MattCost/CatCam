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
public class LocationsController : EntityProviderBaseController
{
    public LocationsController(ILogger<EntityProviderBaseController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<LocationModel>>> Get()
    {
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetLocationModelsAsync();}, "Unable to get locations");
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<LocationModel>> Get([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetLocationModelAsync(id);} , "Unable to get location");
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] LocationModel model)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.UpsertLocationModelAsync(model), "Unable to create or update location");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteLocationModelAsync(id), "Unable to delete location.");
    }
}


