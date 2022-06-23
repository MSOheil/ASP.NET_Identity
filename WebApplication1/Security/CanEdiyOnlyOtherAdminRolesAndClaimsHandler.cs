using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication1.Security
{
    public class CanEdiyOnlyOtherAdminRolesAndClaimsHandler:
        AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ManageAdminRolesAndClaimsRequirement requirement)
        {

           var authFilterContext = context.Resource as AuthorizationFilterContext;
           if(authFilterContext is null)
               return Task.CompletedTask;

            string loggedInAdminId =
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminInBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];
            if(context.User.IsInRole("Admin")&&
                context.User.HasClaim(claim=>claim.Type=="Edit Role" && claim.Value == "true")&&
                adminInBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
