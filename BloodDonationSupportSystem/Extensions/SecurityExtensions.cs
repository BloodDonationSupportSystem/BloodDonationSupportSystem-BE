using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace BloodDonationSupportSystem.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? 
                        principal.FindFirst("sub");
            
            return claim != null && Guid.TryParse(claim.Value, out Guid userId) 
                ? userId 
                : Guid.Empty;
        }

        public static string GetUsername(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string GetUserRole(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirst(ClaimTypes.Role)?.Value;
        }

        public static bool IsInRole(this ClaimsPrincipal principal, string role)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.HasClaim(ClaimTypes.Role, role);
        }
    }

    public static class HttpContextExtensions
    {
        public static Guid GetUserId(this HttpContext context)
        {
            return context.User.GetUserId();
        }

        public static string GetUsername(this HttpContext context)
        {
            return context.User.GetUsername();
        }

        public static string GetUserEmail(this HttpContext context)
        {
            return context.User.GetUserEmail();
        }

        public static string GetUserRole(this HttpContext context)
        {
            return context.User.GetUserRole();
        }

        public static bool IsInRole(this HttpContext context, string role)
        {
            return context.User.IsInRole(role);
        }
    }
}