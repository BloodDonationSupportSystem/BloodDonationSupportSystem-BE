using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Models;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Middleware
{
    public class JwtExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtExceptionMiddleware> _logger;

        public JwtExceptionMiddleware(RequestDelegate next, ILogger<JwtExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogError(ex, "Token expired: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, "Token has expired");
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Token validation failed: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, "Invalid token");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ApiResponse(statusCode, message ?? exception.Message);
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(response, jsonOptions);
            
            await context.Response.WriteAsync(json);
        }
    }
}