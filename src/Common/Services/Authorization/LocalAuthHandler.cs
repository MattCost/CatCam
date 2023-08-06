using Microsoft.AspNetCore.Authorization;

namespace CatCam.Common.Services.Authorization;

public class LocalAuthHandler : IAuthorizationHandler
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