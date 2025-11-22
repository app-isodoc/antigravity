using System.Security.Claims;
using Isodoc.Web.Services;

namespace Isodoc.Web.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService currentTenantService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            if (roleClaim != null && roleClaim.Value == "SuperAdmin")
            {
                currentTenantService.SetSuperAdmin();
            }
            else
            {
                var clientIdClaim = context.User.FindFirst("ClientId");
                if (clientIdClaim != null && Guid.TryParse(clientIdClaim.Value, out var clientId))
                {
                    currentTenantService.SetTenant(clientId);
                }
            }
        }

        await _next(context);
    }
}
