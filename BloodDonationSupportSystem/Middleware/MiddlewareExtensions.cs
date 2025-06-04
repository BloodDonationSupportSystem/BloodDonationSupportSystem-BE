using Microsoft.AspNetCore.Builder;

namespace BloodDonationSupportSystem.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtExceptionMiddleware>();
        }
    }
}