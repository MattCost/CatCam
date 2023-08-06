using Microsoft.AspNetCore.Authorization;

namespace CatCam.Common.Services.Authorization;

public class SiteAuthHandler : AuthorizationHandler<SiteOperations>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SiteOperations requirement)
    {
        await Task.CompletedTask;
        
        if ( context.User.IsInRole("SystemAdmin") )
        {
            context.Succeed(requirement);
            return; 
        }
        
        if( requirement == SiteOperations.ListSites)
        {

        }

        if( requirement == SiteOperations.CreateSites)
        {

        }

        if( requirement == SiteOperations.DeleteSites)
        {

        }
    }
}