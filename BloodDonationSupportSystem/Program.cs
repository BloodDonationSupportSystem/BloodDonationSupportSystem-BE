using AutoMapper;
using BusinessObjects.Data;
using BusinessObjects.Models;
using BloodDonationSupportSystem.Middleware;
using BloodDonationSupportSystem.BackgroundServices;
using BloodDonationSupportSystem.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories.Base;
using Repositories.Implementation;
using Repositories.Interface;
using Services.Implementation;
using Services.Interface;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

// Configure Email Settings - using fully qualified name to avoid ambiguity
builder.Services.Configure<Services.Implementation.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure Donation Reminder Settings
builder.Services.Configure<BloodDonationSupportSystem.Config.DonationReminderSettings>(
    builder.Configuration.GetSection("DonationReminderSettings"));

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
            ClockSkew = TimeSpan.Zero // Thi?t l?p ClockSkew thành 0 ?? ng?n ch?n th?i gian xác th?c ???c n?i l?ng
        };
    }
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BDSS"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Configure AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

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

// EmergencyRequest
builder.Services.AddScoped<IEmergencyRequestRepository, EmergencyRequestRepository>();
builder.Services.AddScoped<IEmergencyRequestService, EmergencyRequestService>();

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

// Analytics, Dashboard and Report Services
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Location Capacity Service
builder.Services.AddScoped<ILocationCapacityService, LocationCapacityService>();

// Register background service for donation reminders
builder.Services.AddHostedService<DonationReminderBackgroundService>();

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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

// Add JWT exception middleware
app.UseJwtExceptionHandler();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
