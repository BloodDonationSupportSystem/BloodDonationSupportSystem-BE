# 🩸 BDSS - Backend API

> ASP.NET Core Web API for Blood Donation Support System

## 📋 Overview

RESTful Web API built with ASP.NET Core 8.0, implementing Clean Architecture principles to provide comprehensive backend services for blood donation management, including authentication, real-time notifications, and complex business logic.

## 🛠️ Technology Stack

```
Framework:         ASP.NET Core 8.0 (Web API)
Language:          C# 12
ORM:               Entity Framework Core 8.0
Database:          Microsoft SQL Server 2019+
Authentication:    JWT (JSON Web Tokens)
Real-time:         SignalR
Mapping:           AutoMapper
Architecture:      Clean Architecture (Onion)
Patterns:          Repository, Unit of Work, Dependency Injection
Background Jobs:   IHostedService (Background Services)
Email:             SMTP Integration
Validation:        FluentValidation
API Docs:          Swagger/OpenAPI
```

## 🏗️ Project Structure

```
BloodDonationSupportSystem-BE/
│
├── BloodDonationSupportSystem/           # 🌐 Web API Layer
│   ├── Controllers/                      # API Controllers
│   │   ├── AuthController.cs             # Authentication & Authorization
│   │   ├── BaseApiController.cs          # Base Controller with Common Logic
│   │   ├── UsersController.cs            # User Management
│   │   ├── RolesController.cs            # Role Management
│   │   ├── DonationEventsController.cs   # Donation Event Management
│   │   ├── DonationAppointmentRequestsController.cs
│   │   ├── DonorProfilesController.cs    # Donor Profile Management
│   │   ├── BloodInventoriesController.cs # Blood Inventory
│   │   ├── BloodGroupsController.cs      # Blood Group Data
│   │   ├── ComponentTypesController.cs   # Blood Component Types
│   │   ├── BloodCompatibilityController.cs
│   │   ├── BloodRequestsController.cs    # Blood Request Management
│   │   ├── LocationsController.cs        # Donation Center Management
│   │   ├── LocationCapacitiesController.cs
│   │   ├── NotificationsController.cs    # Notification Management
│   │   ├── DonationRemindersController.cs
│   │   ├── BlogPostsController.cs        # Blog/Content Management
│   │   ├── DocumentsController.cs        # Document Management
│   │   ├── DashboardController.cs        # Dashboard Analytics
│   │   └── EmailTestController.cs        # Email Testing (Dev)
│   │
│   ├── Middleware/                       # Custom Middleware
│   │   ├── JwtExceptionMiddleware.cs     # JWT Error Handling
│   │   └── MiddlewareExtensions.cs       # Middleware Registration
│   │
│   ├── Extensions/                       # Extension Methods
│   │   └── SecurityExtensions.cs         # Security Helpers
│   │
│   ├── BackgroundServices/               # Background Tasks
│   │   └── DonationReminderBackgroundService.cs # Scheduled Reminders
│   │
│   ├── Config/                           # Configuration Classes
│   │   ├── AccountLockoutSettings.cs     # Lockout Settings
│   │   └── DonationReminderSettings.cs   # Reminder Configuration
│   │
│   ├── DataSeed/                         # Data Seeding
│   │   └── BloodCompatibilityDataSeed.cs # Initial Data
│   │
│   ├── Properties/
│   │   └── launchSettings.json           # Launch Configuration
│   │
│   ├── appsettings.json                  # App Configuration
│   ├── appsettings.Development.json      # Dev Configuration
│   ├── Program.cs                        # Application Entry Point
│   └── BloodDonationSupportSystem.csproj
│
├── BusinessObjects/                      # 📦 Domain Layer
│   ├── Models/                           # Entity Models (Domain Entities)
│   │   ├── User.cs                       # User Entity
│   │   ├── Role.cs                       # Role Entity
│   │   ├── DonationEvent.cs              # Donation Event
│   │   ├── DonationAppointmentRequest.cs # Appointment
│   │   ├── DonorProfile.cs               # Donor Profile
│   │   ├── BloodInventory.cs             # Blood Stock
│   │   ├── BloodGroup.cs                 # Blood Group
│   │   ├── ComponentType.cs              # Blood Component
│   │   ├── BloodCompatibility.cs         # Compatibility Rules
│   │   ├── BloodRequest.cs               # Blood Request
│   │   ├── Location.cs                   # Donation Center
│   │   ├── LocationCapacity.cs           # Capacity Management
│   │   ├── Notification.cs               # Notification
│   │   ├── DonationReminder.cs           # Reminder
│   │   ├── BlogPost.cs                   # Blog Post
│   │   ├── Document.cs                   # Document
│   │   └── ...                           # Other Entities
│   │
│   ├── Dtos/                             # Data Transfer Objects
│   │   ├── Auth/
│   │   │   ├── LoginDto.cs
│   │   │   ├── RegisterDto.cs
│   │   │   ├── TokenDto.cs
│   │   │   └── RefreshTokenDto.cs
│   │   ├── User/
│   │   │   ├── UserDto.cs
│   │   │   ├── CreateUserDto.cs
│   │   │   └── UpdateUserDto.cs
│   │   ├── DonationEvent/
│   │   │   ├── DonationEventDto.cs
│   │   │   ├── CreateDonationEventDto.cs
│   │   │   └── UpdateDonationEventDto.cs
│   │   ├── Appointment/
│   │   ├── Inventory/
│   │   ├── Request/
│   │   └── ...                           # Feature-specific DTOs
│   │
│   ├── Data/                             # Database Context
│   │   └── BloodDonationDbContext.cs     # EF Core DbContext
│   │
│   ├── Migrations/                       # EF Core Migrations
│   │   └── [Timestamp]_MigrationName.cs
│   │
│   ├── AutoMapperProfiles/               # AutoMapper Profiles
│   │   ├── DashboardMappingProfile.cs
│   │   └── DonationEventProfile.cs
│   │
│   └── BusinessObjects.csproj
│
├── Repositories/                         # 🗄️ Data Access Layer
│   ├── Interface/                        # Repository Interfaces
│   │   ├── IGenericRepository.cs         # Generic CRUD Operations
│   │   ├── IUserRepository.cs
│   │   ├── IDonationEventRepository.cs
│   │   ├── IAppointmentRepository.cs
│   │   ├── IBloodInventoryRepository.cs
│   │   ├── IBloodRequestRepository.cs
│   │   ├── ILocationRepository.cs
│   │   ├── INotificationRepository.cs
│   │   ├── IBlogPostRepository.cs
│   │   └── ...                           # Feature-specific Interfaces
│   │
│   ├── Implementation/                   # Repository Implementations
│   │   ├── GenericRepository.cs          # Base Repository
│   │   ├── UserRepository.cs
│   │   ├── DonationEventRepository.cs
│   │   ├── AppointmentRepository.cs
│   │   ├── BloodInventoryRepository.cs
│   │   ├── BloodRequestRepository.cs
│   │   ├── LocationRepository.cs
│   │   ├── NotificationRepository.cs
│   │   ├── BlogPostRepository.cs
│   │   └── ...                           # Feature-specific Implementations
│   │
│   ├── Base/                             # Base Classes
│   │   └── RepositoryBase.cs             # Common Repository Logic
│   │
│   └── Repositories.csproj
│
├── Services/                             # 💼 Business Logic Layer
│   ├── Interface/                        # Service Interfaces
│   │   ├── IAuthService.cs               # Authentication Service
│   │   ├── IUserService.cs               # User Management Service
│   │   ├── IDonationEventService.cs      # Event Management Service
│   │   ├── IAppointmentService.cs        # Appointment Service
│   │   ├── IBloodInventoryService.cs     # Inventory Service
│   │   ├── IBloodRequestService.cs       # Request Management Service
│   │   ├── ILocationService.cs           # Location Service
│   │   ├── INotificationService.cs       # Notification Service
│   │   ├── IEmailService.cs              # Email Service
│   │   ├── IBlogPostService.cs           # Blog Service
│   │   ├── IDashboardService.cs          # Dashboard/Analytics Service
│   │   └── ...                           # Feature-specific Services
│   │
│   ├── Implementation/                   # Service Implementations
│   │   ├── AuthService.cs
│   │   ├── UserService.cs
│   │   ├── DonationEventService.cs
│   │   ├── AppointmentService.cs
│   │   ├── BloodInventoryService.cs
│   │   ├── BloodRequestService.cs
│   │   ├── LocationService.cs
│   │   ├── NotificationService.cs
│   │   ├── EmailService.cs
│   │   ├── BlogPostService.cs
│   │   ├── DashboardService.cs
│   │   └── ...                           # Feature-specific Implementations
│   │
│   ├── BackgroundServices/               # Background Service Interfaces
│   │   └── IDonationReminderService.cs
│   │
│   └── Services.csproj
│
├── Shared/                               # 🔧 Shared/Cross-cutting
│   ├── Constants/                        # Application Constants
│   │   ├── AppConstants.cs               # General Constants
│   │   ├── ErrorMessages.cs              # Error Message Constants
│   │   └── RoleConstants.cs              # Role Names
│   │
│   ├── Hubs/                             # SignalR Hubs
│   │   └── NotificationHub.cs            # Real-time Notification Hub
│   │
│   ├── Models/                           # Shared Models
│   │   ├── PagedResult.cs                # Pagination Model
│   │   ├── ApiResponse.cs                # Standard API Response
│   │   └── ...                           # Common Models
│   │
│   ├── Utilities/                        # Helper/Utility Classes
│   │   ├── PasswordHasher.cs             # Password Hashing
│   │   ├── JwtTokenGenerator.cs          # JWT Token Generation
│   │   └── ...                           # Other Utilities
│   │
│   └── Shared.csproj
│
├── BDSS_SQL_Script.sql                   # Database Script
├── bdss.bacpac                           # Database Backup
└── BloodDonationSupportSystem.sln        # Solution File
```

## ✨ Key Features

### 🔐 Authentication & Authorization
- **JWT Token-based Authentication** - Secure, stateless authentication
- **Refresh Token Mechanism** - Token renewal without re-login
- **Role-based Authorization** - Admin, Staff, Member roles
- **Account Lockout** - Automatic lockout after 5 failed attempts (15 min)
- **Password Security** - Strong password policies and hashing

### 👥 User Management
- **User CRUD Operations** - Complete user lifecycle management
- **Role Assignment** - Flexible role-based access control
- **Profile Management** - User and donor profile updates
- **Activity Tracking** - Login history and audit trails

### 📅 Donation Event Management
- **Event Creation** - Staff can create donation campaigns
- **Location-based Events** - Events tied to specific centers
- **Capacity Management** - Control registration limits
- **Event Status Tracking** - Draft, Active, Completed, Cancelled

### 🩸 Donation Workflow
- **Multi-step Process** - Registration → Screening → Collection → Storage
- **Status Tracking** - Track donation progress in real-time
- **Medical Screening** - Health questionnaire and approval
- **Sample Management** - Blood sample collection and processing

### 📦 Blood Inventory Management
- **Stock Tracking** - Real-time inventory levels
- **Component Management** - Whole blood, plasma, platelets, RBC
- **Blood Group Tracking** - A+, A-, B+, B-, AB+, AB-, O+, O-
- **Expiration Alerts** - Notifications for expiring blood units
- **Location-based Inventory** - Per-center stock management

### 🔔 Notification System
- **In-app Notifications** - Real-time alerts via SignalR
- **Email Notifications** - SMTP integration for emails
- **Eligibility Reminders** - Automatic reminders after 90 days
- **Emergency Alerts** - Urgent blood request notifications
- **Appointment Reminders** - Scheduled appointment notifications

### 🏥 Blood Request Management
- **Emergency Requests** - Urgent blood requirement requests
- **Request Matching** - Match requests with available inventory
- **Status Tracking** - Pending, Approved, Fulfilled, Rejected
- **Priority Handling** - Emergency vs. regular requests

### 📊 Dashboard & Analytics
- **Admin Dashboard** - System-wide statistics
- **Staff Dashboard** - Location-specific metrics
- **Member Dashboard** - Personal donation history
- **Custom Reports** - Donation trends, inventory levels, user activity

### 🗺️ Location Management
- **Donation Centers** - Manage multiple facilities
- **Capacity Configuration** - Set daily/event capacities
- **Operating Hours** - Schedule management
- **Address & Contact Info** - Complete location details

### 📰 Content Management
- **Blog Posts** - Educational content and news
- **Document Library** - Policies, forms, and resources
- **Rich Content** - HTML content support

### ⏰ Background Services
- **Scheduled Tasks** - Daily reminder processing (8:00 AM)
- **Email Queue** - Asynchronous email sending
- **Data Cleanup** - Automated maintenance tasks

## 🏛️ Architecture Patterns

### Clean Architecture (Onion Architecture)
```
┌─────────────────────────────────────┐
│         Presentation Layer          │
│     (Controllers, Middleware)       │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│       Business Logic Layer          │
│     (Services, Validators)          │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│        Data Access Layer            │
│   (Repositories, EF Core)           │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│          Domain Layer               │
│   (Entities, Interfaces)            │
└─────────────────────────────────────┘
```

### Repository Pattern
- **Generic Repository** - Base CRUD operations
- **Specific Repositories** - Domain-specific queries
- **Unit of Work** - Transaction management

### Dependency Injection
- **Constructor Injection** - All services injected via DI
- **Scoped Services** - Per-request lifecycle
- **Singleton Services** - Application-wide instances

## 🔒 Security Features

- **JWT Authentication** - Bearer token validation
- **Password Hashing** - BCrypt/PBKDF2 hashing
- **CORS Policy** - Configured for frontend origin
- **SQL Injection Protection** - Parameterized queries with EF Core
- **XSS Protection** - Input sanitization
- **Account Lockout** - Brute force protection

## 📡 API Endpoints

### Authentication
```
POST   /api/auth/register              # User registration
POST   /api/auth/login                 # User login
POST   /api/auth/refresh-token         # Refresh access token
POST   /api/auth/logout                # Logout
GET    /api/auth/profile               # Get current user
PUT    /api/auth/profile               # Update profile
```

### Donation Events
```
GET    /api/donationevents             # List events (paginated)
GET    /api/donationevents/{id}        # Get event details
POST   /api/donationevents             # Create event (Staff/Admin)
PUT    /api/donationevents/{id}        # Update event
DELETE /api/donationevents/{id}        # Delete event
POST   /api/donationevents/{id}/register # Register for event
```

### Blood Inventory
```
GET    /api/bloodinventories           # List inventory
GET    /api/bloodinventories/{id}      # Get inventory item
POST   /api/bloodinventories           # Add inventory
PUT    /api/bloodinventories/{id}      # Update inventory
DELETE /api/bloodinventories/{id}      # Remove inventory
GET    /api/bloodinventories/statistics # Inventory stats
```

### Appointments
```
GET    /api/donationappointmentrequests # List appointments
GET    /api/donationappointmentrequests/{id} # Get appointment
POST   /api/donationappointmentrequests # Create appointment
PUT    /api/donationappointmentrequests/{id} # Update appointment
DELETE /api/donationappointmentrequests/{id} # Cancel appointment
```

### Full API documentation available at: `/swagger`

## 🗄️ Database

### Entity Framework Core
- **Code-First Approach** - Migrations-based schema management
- **Fluent API** - Explicit entity configuration
- **Relationships** - One-to-Many, Many-to-Many configured
- **Indexes** - Performance optimization

### Key Tables
```
- Users (User accounts and profiles)
- Roles (User roles)
- DonationEvents (Donation campaigns)
- DonationAppointmentRequests (Appointments)
- DonorProfiles (Donor information)
- BloodInventories (Blood stock)
- BloodGroups (Blood types)
- ComponentTypes (Blood components)
- BloodCompatibility (Compatibility rules)
- BloodRequests (Blood requests)
- Locations (Donation centers)
- LocationCapacities (Capacity settings)
- Notifications (User notifications)
- DonationReminders (Scheduled reminders)
- BlogPosts (Blog content)
- Documents (Document library)
```

## ⚙️ Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "BDSS": "Server=...;Database=BloodDonationDB;..."
  },
  "JwtConfig": {
    "Secret": "...",
    "Issuer": "...",
    "Audience": "...",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "...",
    "Password": "..."
  },
  "AccountLockoutSettings": {
    "MaxFailedAttempts": 5,
    "LockoutDurationMinutes": 15
  },
  "DonationReminderSettings": {
    "DonationIntervalDays": 90,
    "ScheduledRunTime": "08:00:00",
    "EnableEmailReminders": true
  }
}
```

## 📦 Dependencies

### Main Packages
- `Microsoft.EntityFrameworkCore` - ORM
- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server provider
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT auth
- `AutoMapper` - Object mapping
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI
- `Microsoft.AspNetCore.SignalR` - Real-time communication

## 🚀 Performance Optimizations

- **Async/Await** - Non-blocking I/O operations
- **Pagination** - Large dataset handling
- **Eager Loading** - Optimize related entity queries
- **Caching** - Response caching where appropriate
- **Connection Pooling** - Efficient database connections

## 📝 Notes

- Requires .NET 8.0 SDK or higher
- SQL Server 2019 or higher recommended
- SignalR for WebSocket support (real-time notifications)
- Background services run as Hosted Services
