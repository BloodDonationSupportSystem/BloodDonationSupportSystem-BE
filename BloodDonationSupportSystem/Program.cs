using AutoMapper;
using BloodDonationSupportSystem.BackgroundServices;
using BloodDonationSupportSystem.Config;
using BloodDonationSupportSystem.Middleware;
using BusinessObjects.AutoMapperProfiles;
using BusinessObjects.Data;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.Base;
using Repositories.Implementation;
using Repositories.Interface;
using Services.BackgroundServices;
using Services.Implementation;
using Services.Interface;
using Shared.Hubs;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.Azure.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

// Configure Email Settings - using fully qualified name to avoid ambiguity
builder.Services.Configure<Services.Implementation.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure Donation Reminder Settings
builder.Services.Configure<BloodDonationSupportSystem.Config.DonationReminderSettings>(
    builder.Configuration.GetSection("DonationReminderSettings"));

// Add Rate Limiting to prevent brute force attacks
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Global rate limit
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Strict rate limit for authentication endpoints
    options.AddPolicy("AuthRateLimit", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Add JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
    if (jwtConfig != null)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
            ClockSkew = TimeSpan.Zero // Thi?t l?p ClockSkew th�nh 0 ?? ng?n ch?n th?i gian x�c th?c ???c n?i l?ng
        };

        // Configure SignalR JWT authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    }
});

// Add SignalR with Azure SignalR Service (when configured)
var signalRBuilder = builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Use Azure SignalR Service if connection string is configured
var azureSignalRConnectionString = builder.Configuration["Azure:SignalR:ConnectionString"];
if (!string.IsNullOrEmpty(azureSignalRConnectionString))
{
    signalRBuilder.AddAzureSignalR(azureSignalRConnectionString);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BDSS"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Add Memory Cache for performance optimization
builder.Services.AddMemoryCache();

// Configure AutoMapper
builder.Services.AddAutoMapper(config => 
{
    config.AddProfile<MappingProfile>();
    config.AddProfile<DashboardMappingProfile>();
    config.AddProfile<DonationEventProfile>();
    // Add any other mapping profiles here
});

// Register repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Cache Service
builder.Services.AddScoped<Services.Interfaces.ICacheService, MemoryCacheService>();

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Real-time Notification Service - register with the specific hub context
builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();

// Location 
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();

// BloodGroup
builder.Services.AddScoped<IBloodGroupRepository, BloodGroupRepository>();
builder.Services.AddScoped<IBloodGroupService, BloodGroupService>();

// ComponentType
builder.Services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();

// Blood Compatibility Service
builder.Services.AddScoped<IBloodCompatibilityService, BloodCompatibilityService>();

// Document Seed Service
builder.Services.AddScoped<IDocumentSeedService, DocumentSeedService>();

// Role
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

// User
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// BloodRequest
builder.Services.AddScoped<IBloodRequestRepository, BloodRequestRepository>();
builder.Services.AddScoped<IBloodRequestService, BloodRequestService>();

// DonationEvent
builder.Services.AddScoped<IDonationEventRepository, DonationEventRepository>();
builder.Services.AddScoped<IDonationEventService, DonationEventService>();

// BloodInventory
builder.Services.AddScoped<IBloodInventoryRepository, BloodInventoryRepository>();
builder.Services.AddScoped<IBloodInventoryService, BloodInventoryService>();

// BlogPost
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IBlogPostService, BlogPostService>();

// Document
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// DonorProfile
builder.Services.AddScoped<IDonorProfileRepository, DonorProfileRepository>();
builder.Services.AddScoped<IDonorProfileService, DonorProfileService>();

// Notification
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// RefreshToken
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Donation Reminder Service
builder.Services.AddScoped<IDonationReminderService, DonationReminderService>();
builder.Services.AddScoped<IDonorReminderSettingsRepository, DonorReminderSettingsRepository>();

// DonationAppointmentRequest
builder.Services.AddScoped<IDonationAppointmentRequestRepository, DonationAppointmentRequestRepository>();
builder.Services.AddScoped<IDonationAppointmentRequestService, DonationAppointmentRequestService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

// Location Capacity Service
builder.Services.AddScoped<ILocationCapacityService, LocationCapacityService>();

// Register background service for donation reminders (DISABLED for testing)
// builder.Services.AddHostedService<DonationReminderBackgroundService>();

// Đăng ký background service cho việc cập nhật trạng thái máu hết hạn (DISABLED for testing)
// builder.Services.AddHostedService<BloodInventoryExpirationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blood Donation Support System API", Version = "v1" });
    
    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSignalR", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://blood-donation-support-system-v.vercel.app"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // This is required for SignalR with authentication
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger for both Development and Production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blood Donation API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at root URL
});

if (!app.Environment.IsDevelopment())
{
    // Production security settings
    // Enable HSTS (HTTP Strict Transport Security) - forces HTTPS
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add security headers
app.Use(async (context, next) =>
{
    // Prevent MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // Prevent clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // Enable XSS filtering
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // Control referrer information
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Content Security Policy - adjust as needed for your application
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' wss: ws:;");
    
    // Prevent caching of sensitive data
    if (context.Request.Path.StartsWithSegments("/api/Auth"))
    {
        context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
        context.Response.Headers.Append("Pragma", "no-cache");
    }
    
    await next();
});

// Use Rate Limiting
app.UseRateLimiter();

// Use CORS
app.UseCors("AllowSignalR");

// Add JWT exception middleware
app.UseJwtExceptionHandler();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
