<div align="center">

# 🩸 BDSS Backend

### Blood Donation Support System - Enterprise Backend API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-00D4AA?style=for-the-badge&logo=microsoft&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Swagger](https://img.shields.io/badge/Swagger-API%20Docs-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io/)

*A comprehensive ASP.NET Core backend API for managing blood donation operations, featuring real-time notifications, automated workflows, and intelligent donor management.*

[Live Demo](https://blood-donation-support-system-v.vercel.app/) • [Swagger Docs](https://blood-donation-api-asb9esgvbfhwfhbf.southeastasia-01.azurewebsites.net/swagger) • [Report Bug](#-contributing) • [Request Feature](#-contributing)

</div>

---

## 📋 Table of Contents

- [About The Project](#-about-the-project)
- [Key Features](#-key-features)
- [Tech Stack](#️-tech-stack)
- [System Architecture](#-system-architecture)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Database Schema](#-database-schema)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 About The Project

**BDSS Backend** is a robust, enterprise-grade RESTful Web API built with ASP.NET Core 8.0, implementing Clean Architecture principles to provide comprehensive backend services for the Blood Donation Support System. The platform digitizes and streamlines blood donation management, connecting donors with healthcare facilities while enabling efficient inventory tracking and workflow automation.

### 🎓 Project Details
- **Development Period:** 4 months (5/2025 - 8/2025)
- **Team Size:** 1 Developer (Solo Project)
- **Architecture:** Clean Architecture (Onion Architecture)
- **Development Approach:** API-First Design
- **Lines of Code:** ~15,000+ (Backend only)

### 💡 Problem Statement

Traditional blood donation systems face significant technical challenges:
- Manual data entry and processing leading to errors
- Lack of real-time inventory visibility
- Inefficient coordination between multiple stakeholders
- No automated reminder or notification systems
- Complex business logic requiring custom workflows
- Limited reporting and analytics capabilities

### ✅ Our Solution

BDSS Backend provides a centralized, scalable API platform that:
- Automates donation workflows with state machine patterns
- Enables real-time updates via SignalR WebSockets
- Implements intelligent background services for reminders
- Provides comprehensive data validation and error handling
- Offers RESTful endpoints following industry best practices
- Supports role-based access control for security

---

## ⭐ Key Features

### 🔐 Authentication & Authorization
- **JWT-based authentication** - Secure token generation and validation with refresh tokens
- **Role-based access control** - Admin, Staff, Member roles with granular permissions
- **Account security** - Automatic lockout after 5 failed login attempts
- **Token refresh mechanism** - Seamless session management
- **Password policies** - Strong password validation and hashing (BCrypt)

### 📦 Donation Management
- **Event lifecycle management** - Complete CRUD operations for donation events
- **Appointment system** - Request, approval, and cancellation workflows
- **Donor registration** - Medical history and eligibility tracking
- **Workflow automation** - Multi-step donation process state management
- **Sample tracking** - Blood sample collection and processing
- **Digital records** - Complete audit trail of all donations

### 🩸 Blood Inventory System
- **Real-time inventory tracking** - Current stock levels by blood type and component
- **Component management** - Whole blood, plasma, platelets separation tracking
- **Expiration monitoring** - Automated alerts for expiring blood units
- **Blood compatibility checking** - Cross-matching algorithms for safe transfusion
- **Request fulfillment** - Emergency blood request processing and matching
- **Stock statistics** - Comprehensive inventory analytics and reporting

### 🏢 Location & Capacity Management
- **Multi-location support** - Manage multiple donation centers nationwide
- **Capacity configuration** - Daily capacity limits and slot scheduling
- **Geographic data** - Coordinates and proximity calculations for donor convenience
- **Operating hours** - Flexible scheduling per location
- **Resource allocation** - Staff and equipment assignment

### 🔔 Notification System
- **Real-time notifications** - SignalR-based push notifications to web clients
- **Email integration** - SMTP email sending for confirmations and reminders
- **Automated reminders** - Background service for eligibility notifications (90-day donation cycle)
- **Event-driven messaging** - Notification triggers on order state changes
- **Notification preferences** - User-configurable notification settings
- **Bulk notifications** - Mass messaging for emergency blood requests

### 👥 User Management
- **Profile management** - Complete user CRUD operations with validation
- **Role assignment** - Dynamic role management by administrators
- **Activity tracking** - Audit logs for sensitive operations
- **Staff management** - Staff account creation with permissions
- **Donor profiles** - Extended donor information including medical history
- **Account activation** - Email-based account verification

### 📊 Reporting & Analytics
- **Dashboard metrics** - Real-time statistics aggregation for KPIs
- **Donation history** - Complete audit trail of all donation activities
- **Inventory reports** - Stock level analysis and trend forecasting
- **Performance metrics** - Efficiency tracking for donation centers
- **Export functionality** - Data export to CSV/Excel for analysis
- **Custom reports** - Configurable report generation

### 📝 Content Management
- **Blog system** - Educational content creation and publishing
- **Document management** - Policy and guideline storage
- **Rich text support** - HTML content rendering and editing
- **Category management** - Content organization and filtering
- **SEO optimization** - Meta tags and descriptions

### 🔄 Background Services
- **Scheduled tasks** - Automated donation eligibility reminders every 90 days
- **Data cleanup** - Expired token and notification cleanup jobs
- **Batch processing** - Bulk notification sending and email queuing
- **Health monitoring** - System health checks and performance alerts
- **Inventory alerts** - Low stock warnings and expiration notifications

---

## 🛠️ Tech Stack

### Backend Framework
| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 LTS | Core framework |
| ASP.NET Core | 8.0 | Web API framework |
| C# | 12 | Programming language |
| Entity Framework Core | 8.0 | ORM and data access |

### Database & Persistence
| Technology | Purpose |
|------------|---------|
| Microsoft SQL Server 2019+ | Primary relational database |
| EF Core Migrations | Database schema versioning |
| LINQ | Type-safe query composition |
| SQL Scripts | Database initialization and seeding |

### Security & Authentication
| Technology | Purpose |
|------------|---------|
| JWT (JSON Web Tokens) | Stateless authentication |
| BCrypt.Net | Password hashing algorithm |
| ASP.NET Core Identity (Custom) | User management foundation |
| Authorization Policies | Role-based access control |

### Real-time Communication
| Technology | Purpose |
|------------|---------|
| SignalR | WebSocket-based real-time updates |
| SignalR Hubs | Notification broadcasting to connected clients |
| Connection Management | User session tracking |

### Architecture & Patterns
| Pattern | Implementation |
|---------|----------------|
| Clean Architecture | Onion Architecture structure with clear separation |
| Repository Pattern | Data access abstraction layer |
| Unit of Work | Transaction management and atomicity |
| Dependency Injection | Built-in ASP.NET Core DI container |
| CQRS-lite | Command/Query separation for complex operations |
| Service Layer | Business logic encapsulation |

### Background Processing
| Technology | Purpose |
|------------|---------|
| IHostedService | Background task execution framework |
| Timers | Scheduled job execution (daily reminders) |
| Task Parallel Library | Async/await patterns for performance |

### Validation & Mapping
| Technology | Purpose |
|------------|---------|
| AutoMapper | Object-to-object mapping (DTOs ↔ Entities) |
| FluentValidation | Request validation rules |
| Data Annotations | Model validation attributes |

### API Documentation
| Technology | Purpose |
|------------|---------|
| Swagger/OpenAPI | Interactive API documentation |
| Swashbuckle | Swagger generation for ASP.NET Core |
| XML Comments | API endpoint descriptions |

### External Services
| Service | Purpose |
|---------|---------|
| SMTP (Gmail/Outlook) | Transactional email delivery |
| Cloudinary (optional) | Image and file storage CDN |

### Development Tools
| Tool | Purpose |
|------|---------|
| Visual Studio 2022 | Primary IDE |
| SQL Server Management Studio | Database management |
| Postman | API testing and debugging |
| Git | Version control |

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CLIENT APPLICATIONS                             │
├──────────────────┬──────────────────┬──────────────────┬───────────────────┤
│   Web Admin      │   Web Member     │   Web Staff      │   Mobile Apps     │
│   (Next.js)      │   (Next.js)      │   (Next.js)      │   (Future)        │
└────────┬─────────┴────────┬─────────┴────────┬─────────┴─────────┬─────────┘
         │                  │                  │                   │
         └──────────────────┴────────┬─────────┴───────────────────┘
                                     │
                           ┌─────────▼─────────┐
                           │   API Gateway     │
                           │  (ASP.NET Core)   │
                           │  Middleware Stack │
                           └─────────┬─────────┘
                                     │
         ┌───────────────────────────┼───────────────────────────┐
         │                           │                           │
┌────────▼────────┐        ┌─────────▼─────────┐       ┌────────▼────────┐
│  REST API       │        │  SignalR Hubs     │       │ Authentication  │
│  Controllers    │        │  (WebSocket)      │       │  Middleware     │
│  • Auth         │        │  • Notifications  │       │  • JWT Tokens   │
│  • Users        │        │  • Real-time      │       │  • RBAC         │
│  • Donations    │        │    Updates        │       │  • Lockout      │
│  • Inventory    │        └─────────┬─────────┘       └────────┬────────┘
│  • Locations    │                  │                          │
└────────┬────────┘                  │                          │
         │                           │                          │
         └───────────────────────────┼──────────────────────────┘
                                     │
                           ┌─────────▼─────────┐
                           │  Service Layer    │
                           │ (Business Logic)  │
                           │  • Validation     │
                           │  • Workflows      │
                           │  • Calculations   │
                           └─────────┬─────────┘
                                     │
         ┌───────────────────────────┼───────────────────────────┐
         │                           │                           │
┌────────▼────────┐        ┌─────────▼─────────┐       ┌────────▼────────┐
│  Repository     │        │  Background       │       │   External      │
│  Layer          │        │  Services         │       │   Services      │
│  • UnitOfWork   │        │  • Reminders      │       │  • SMTP Email   │
│  • Repositories │        │  • Cleanup Jobs   │       │  • Cloudinary   │
└────────┬────────┘        └─────────┬─────────┘       └─────────────────┘
         │                           │                          
         │                           │                          
         ▼                           ▼                          
┌─────────────────────────────────────────────────────────────┐
│                   Data Access Layer                          │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Entity Framework Core 8.0                            │ │
│  │  • DbContext                                          │ │
│  │  • Migrations                                         │ │
│  │  • Change Tracking                                    │ │
│  └───────────────────────┬───────────────────────────────┘ │
└────────────────────────────┼─────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│              Microsoft SQL Server Database                   │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Tables (20+)                                         │ │
│  │  • Users                • Roles                       │ │
│  │  • DonorProfiles       • DonationEvents              │ │
│  │  • BloodInventories    • Appointments                │ │
│  │  • Locations           • Notifications               │ │
│  │  • BlogPosts           • Transactions                │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│              📱 Presentation Layer                           │
│  • Controllers (API Endpoints)                              │
│  • SignalR Hubs (Real-time)                                 │
│  • Middleware (Auth, Error Handling, CORS)                  │
│  • DTOs (Data Transfer Objects)                             │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│              🎯 Application Layer                            │
│  • Services (Business Logic)                                │
│  • Validators (FluentValidation)                            │
│  • AutoMapper Profiles                                      │
│  • Background Services                                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│              💼 Domain Layer                                 │
│  • Entities (Domain Models)                                 │
│  • Repository Interfaces                                    │
│  • Business Rules                                           │
│  • Domain Events                                            │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│              🗄️ Infrastructure Layer                         │
│  • Repository Implementations                               │
│  • DbContext (EF Core)                                      │
│  • External Service Integration                             │
│  • Email Service                                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 🚀 Getting Started

### Prerequisites

```bash
# Required
- .NET SDK 8.0 or later
- Microsoft SQL Server 2019+ (or SQL Server Express)
- Visual Studio 2022 / Visual Studio Code / JetBrains Rider

# Optional
- SQL Server Management Studio (SSMS)
- Postman or similar API testing tool
```

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/BDSS.git
   cd BDSS/BloodDonationSupportSystem-BE
   ```

2. **Configure Database Connection**
   
   Update `appsettings.json` or `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=BDSS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Configure JWT Settings**
   ```json
   {
     "Jwt": {
       "Key": "YourSuperSecretKeyHere_AtLeast32Characters!",
       "Issuer": "BDSS_API",
       "Audience": "BDSS_Clients",
       "TokenValidityInMinutes": 60,
       "RefreshTokenValidityInDays": 7
     }
   }
   ```

4. **Configure Email Settings (Optional)**
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@gmail.com",
       "SenderName": "BDSS System",
       "Username": "your-email@gmail.com",
       "Password": "your-app-password",
       "EnableSsl": true
     }
   }
   ```

5. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

6. **Apply Database Migrations**
   ```bash
   # From the BloodDonationSupportSystem project directory
   dotnet ef database update --project ../BusinessObjects
   
   # Or run the included SQL script
   # Execute BDSS_SQL_Script.sql in SSMS
   ```

7. **Seed Initial Data (Optional)**
   
   The application includes a data seeder that runs on first startup to create:
   - Default admin account
   - Blood groups (A+, A-, B+, B-, AB+, AB-, O+, O-)
   - Component types (Whole Blood, Plasma, Platelets, RBC)
   - Sample locations

8. **Run the Application**
   ```bash
   # Development mode
   dotnet run --project BloodDonationSupportSystem
   
   # Or press F5 in Visual Studio
   ```

9. **Access Swagger UI**
   
   Navigate to: `https://localhost:5222/swagger`

### Default Credentials

After initial seed:
```
Admin Account:
Email: admin@bdss.com
Password: Admin@123

Staff Account:
Email: staff@bdss.com
Password: Staff@123
```

⚠️ **Important:** Change these credentials in production!

---

## 📚 API Documentation

### Base URL
- **Development:** `https://localhost:5222/api`
- **Production:** `https://your-domain.com/api`

### Authentication

All protected endpoints require a JWT Bearer token:
```http
Authorization: Bearer <your_jwt_token>
```

### API Modules

#### Authentication (`/api/auth`)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/auth/register` | Register new member account | ❌ |
| POST | `/auth/login` | Login and receive JWT token | ❌ |
| POST | `/auth/refresh-token` | Refresh expired token | ✅ |
| POST | `/auth/logout` | Logout and invalidate token | ✅ |
| GET | `/auth/profile` | Get current user profile | ✅ |
| PUT | `/auth/profile` | Update user profile | ✅ |
| POST | `/auth/change-password` | Change account password | ✅ |
| POST | `/auth/forgot-password` | Request password reset | ❌ |
| POST | `/auth/reset-password` | Reset password with token | ❌ |

#### Users (`/api/users`)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/users` | List all users (paginated) | Admin, Staff |
| GET | `/users/{id}` | Get user by ID | Admin, Staff |
| POST | `/users` | Create new user | Admin |
| PUT | `/users/{id}` | Update user | Admin |
| DELETE | `/users/{id}` | Delete user | Admin |
| PUT | `/users/{id}/lock` | Lock/unlock user account | Admin |
| PUT | `/users/{id}/role` | Assign role to user | Admin |

#### Donation Events (`/api/donationevents`)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/donationevents` | List all events | All (filtered by role) |
| GET | `/donationevents/{id}` | Get event details | All |
| POST | `/donationevents` | Create donation event | Admin, Staff |
| PUT | `/donationevents/{id}` | Update event | Admin, Staff |
| DELETE | `/donationevents/{id}` | Delete event | Admin |
| POST | `/donationevents/{id}/register` | Register for event | Member |
| GET | `/donationevents/upcoming` | Get upcoming events | All |
| GET | `/donationevents/location/{locationId}` | Events by location | All |

#### Appointments (`/api/appointmentrequests`)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/appointmentrequests` | List appointments | Staff, Admin |
| GET | `/appointmentrequests/{id}` | Get appointment details | Owner, Staff, Admin |
| POST | `/appointmentrequests` | Create appointment request | Member |
| PUT | `/appointmentrequests/{id}/approve` | Approve appointment | Staff, Admin |
| PUT | `/appointmentrequests/{id}/reject` | Reject appointment | Staff, Admin |
| PUT | `/appointmentrequests/{id}/cancel` | Cancel appointment | Member (owner) |
| GET | `/appointmentrequests/my` | Get user's appointments | Member |

#### Blood Inventory (`/api/bloodinventories`)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/bloodinventories` | List inventory items | Staff, Admin |
| GET | `/bloodinventories/{id}` | Get inventory item | Staff, Admin |
| POST | `/bloodinventories` | Add to inventory | Staff, Admin |
| PUT | `/bloodinventories/{id}` | Update inventory | Staff, Admin |
| DELETE | `/bloodinventories/{id}` | Remove from inventory | Admin |
| GET | `/bloodinventories/statistics` | Get inventory stats | Staff, Admin |
| GET | `/bloodinventories/expiring` | Get expiring units | Staff, Admin |
| GET | `/bloodinventories/bloodgroup/{type}` | Filter by blood group | Staff, Admin |

#### Locations (`/api/locations`)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/locations` | List all locations | All |
| GET | `/locations/{id}` | Get location details | All |
| POST | `/locations` | Create location | Admin |
| PUT | `/locations/{id}` | Update location | Admin |
| DELETE | `/locations/{id}` | Delete location | Admin |
| GET | `/locations/nearby` | Find nearby locations | All |

#### Notifications (`/api/notifications`)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/notifications` | Get user notifications | All (authenticated) |
| GET | `/notifications/unread` | Get unread count | All (authenticated) |
| PUT | `/notifications/{id}/read` | Mark as read | Owner |
| PUT | `/notifications/mark-all-read` | Mark all as read | Owner |
| DELETE | `/notifications/{id}` | Delete notification | Owner |

#### Blog Posts (`/api/blogposts`)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/blogposts` | List all posts | All |
| GET | `/blogposts/{id}` | Get post details | All |
| POST | `/blogposts` | Create post | Admin, Staff |
| PUT | `/blogposts/{id}` | Update post | Admin, Staff |
| DELETE | `/blogposts/{id}` | Delete post | Admin |
| GET | `/blogposts/category/{category}` | Posts by category | All |

#### Dashboard (`/api/dashboard`)
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/dashboard/admin` | Admin dashboard stats | Admin |
| GET | `/dashboard/staff` | Staff dashboard stats | Staff |
| GET | `/dashboard/member` | Member dashboard stats | Member |

### SignalR Hubs

#### Notification Hub (`/notificationHub`)
Connect to receive real-time notifications:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5222/notificationHub", {
        accessTokenFactory: () => yourJWTToken
    })
    .build();

connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
});

await connection.start();
```

### Request/Response Examples

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@bdss.com",
  "password": "Admin@123"
}
```

Response:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "abc123...",
    "user": {
      "id": 1,
      "email": "admin@bdss.com",
      "fullName": "System Administrator",
      "role": "Admin"
    }
  }
}
```

#### Create Donation Event
```http
POST /api/donationevents
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Blood Drive at City Hospital",
  "description": "Annual blood donation event",
  "locationId": 1,
  "startDate": "2025-06-15T08:00:00",
  "endDate": "2025-06-15T17:00:00",
  "maxDonors": 100
}
```

### Interactive API Documentation

Access the full Swagger/OpenAPI documentation at:
- **Development:** `https://localhost:5222/swagger`
- **Production:** `https://blood-donation-api-asb9esgvbfhwfhbf.southeastasia-01.azurewebsites.net/swagger`

The Swagger UI provides:
- Complete endpoint documentation
- Request/response schemas
- Try-it-out functionality
- Authentication testing
- Example requests and responses

---

## 🗄️ Database Schema

### Core Tables

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│    Users     │────▶│    Roles     │     │   Locations  │
└──────────────┘     └──────────────┘     └──────────────┘
       │                                          │
       ▼                                          ▼
┌──────────────┐                          ┌──────────────┐
│DonorProfiles │                          │  Capacities  │
└──────────────┘                          └──────────────┘
       │                                          │
       ▼                                          ▼
┌──────────────┐                          ┌──────────────┐
│DonationEvents│◀─────────────────────────┤ Appointments │
└──────────────┘                          └──────────────┘
       │
       ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Donations   │────▶│BloodSamples  │────▶│ Inventories  │
└──────────────┘     └──────────────┘     └──────────────┘
       │                                          │
       ▼                                          ▼
┌──────────────┐                          ┌──────────────┐
│Notifications │                          │ BloodGroups  │
└──────────────┘                          └──────────────┘
```

### Key Entities

#### Users
- **Id** (PK)
- Email (Unique)
- PasswordHash
- FullName
- PhoneNumber
- DateOfBirth
- Gender
- RoleId (FK)
- IsActive
- FailedLoginAttempts
- LockoutEnd
- CreatedAt
- UpdatedAt

#### DonorProfiles
- **Id** (PK)
- UserId (FK, Unique)
- BloodGroupId (FK)
- Weight
- Height
- MedicalConditions
- Medications
- Allergies
- LastDonationDate
- EligibilityStatus
- TotalDonations

#### DonationEvents
- **Id** (PK)
- Name
- Description
- LocationId (FK)
- StartDate
- EndDate
- MaxDonors
- CurrentDonors
- Status (Upcoming, Ongoing, Completed, Cancelled)
- CreatedBy (FK)

#### BloodInventories
- **Id** (PK)
- BloodGroupId (FK)
- ComponentTypeId (FK)
- LocationId (FK)
- Quantity
- UnitType (Bags, ML)
- CollectionDate
- ExpirationDate
- StorageTemperature
- Status (Available, Reserved, Expired, Used)

#### Notifications
- **Id** (PK)
- UserId (FK)
- Title
- Message
- Type (Info, Warning, Success, Reminder)
- IsRead
- CreatedAt
- ReadAt

### Database Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName --project BusinessObjects --startup-project BloodDonationSupportSystem

# Update database
dotnet ef database update --project BusinessObjects --startup-project BloodDonationSupportSystem

# Rollback migration
dotnet ef database update PreviousMigrationName --project BusinessObjects --startup-project BloodDonationSupportSystem

# Generate SQL script
dotnet ef migrations script --project BusinessObjects --startup-project BloodDonationSupportSystem --output migration.sql
```

---

## 🤝 Contributing

While this is currently a solo project for portfolio purposes, suggestions and feedback are welcome!

### How to Contribute

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Standards
- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Write XML documentation for public APIs
- Include unit tests for new features
- Ensure all tests pass before submitting PR

---

## 📄 License

This project is developed for educational and portfolio purposes.

---

## 📊 Project Statistics

- **Development Time:** 4 months (5/2025 - 8/2025)
- **Lines of Code:** ~15,000+
- **API Endpoints:** 40+ RESTful endpoints
- **Database Tables:** 20+ normalized tables
- **Controllers:** 15+ API controllers
- **Services:** 20+ business logic services
- **Background Jobs:** 3+ scheduled tasks

---

<div align="center">

### ⭐ If you find this project helpful, please consider giving it a star!

**Built with ❤️ and ☕ by Son**

[Back to Top](#-bdss-backend)

</div>
