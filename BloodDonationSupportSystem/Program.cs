using BusinessObjects.Data;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Implementation;
using Repositories.Interface;
using Services.Implementation;
using Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BDSS")));

// Register repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Location 
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();

// BloodGroup
builder.Services.AddScoped<IBloodGroupRepository, BloodGroupRepository>();
builder.Services.AddScoped<IBloodGroupService, BloodGroupService>();

// ComponentType
builder.Services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();

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

// RequestMatch
builder.Services.AddScoped<IRequestMatchRepository, RequestMatchRepository>();
builder.Services.AddScoped<IRequestMatchService, RequestMatchService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
