using System.Formats.Asn1;
using CatCam.Common.Exceptions;
using CatCam.Common.Models;
using CatCam.Common.Services.Authorization;
using CatCam.Common.Services.EntityProvider;
using CatCam.Common.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCap.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SitesController : EntityProviderBaseController
{
    public SitesController(ILogger<EntityProviderBaseController> logger, IEntityProvider entityProvider) : base(logger, entityProvider)
    {
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<SiteModel>>> Get()
    {
        // var authResult = await _authorizationService.AuthorizeAsync(User, null, SiteOperations.ListSites);
        // if(!authResult.Succeeded) return Forbid();
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetSiteModelsAsync();}, "Unable to get Sites");
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<SiteModel>> Get([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => { return await EntityProvider.GetSiteModelAsync(id);}, "Unable to get Site");
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] SiteModel model)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.UpsertSiteModelAsync(model), "Unable to update Site");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        return await EntityProviderActionHelper( async () => await EntityProvider.DeleteSiteModelAsync(id), "Unable to delete Site");
    }

}


