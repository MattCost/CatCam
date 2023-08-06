using Microsoft.AspNetCore.Authorization;

namespace CatCam.Common.Services.Authorization;

public class AlwaysAllowAuthHandler : IAuthorizationHandler
{
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        await Task.CompletedTask;
        foreach(var req in context.PendingRequirements)
        {
            context.Succeed(req);
        }
    }
}