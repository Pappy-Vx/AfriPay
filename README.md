# AfriPay - Digital Banking Platform

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

A modern, enterprise-grade digital banking platform built with .NET 8, implementing Domain-Driven Design (DDD), Clean Architecture, and event-driven patterns. AfriPay provides seamless customer onboarding with automated BVN verification and virtual account creation.

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [API Documentation](#-api-documentation)
- [Development](#-development)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

### Core Capabilities
- 🔐 **Automated Customer Onboarding** - Streamlined registration with BVN verification
- 💳 **Virtual Account Creation** - Instant bank account provisioning
- 📊 **Real-time Status Tracking** - Monitor onboarding progress
- 🔄 **Event-Driven Architecture** - Decoupled, scalable system design
- 🛡️ **Enterprise Security** - Built-in validation and error handling
- 📝 **Comprehensive Logging** - Full audit trail and monitoring

### Business Features
- Bank Verification Number (BVN) validation
- Customer identity verification
- Virtual account provisioning with bank integration
- Multi-account support per customer
- Transaction-ready accounts
- Regulatory compliance ready

## 🏗️ Architecture

AfriPay follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                     Presentation Layer                   │
│                      (AfriPay.API)                       │
│              Controllers, Middleware, DTOs               │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│                   Application Layer                      │
│                     (AfriPay.APP)                        │
│         Services, Event Handlers, Use Cases              │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│                     Domain Layer                         │
│                    (AfriPay.Core)                        │
│    Entities, Value Objects, Domain Events, Interfaces    │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│                 Infrastructure Layer                     │
│                     (AfriPay.DAL)                        │
│         Repositories, EF Core, Data Persistence          │
└─────────────────────────────────────────────────────────┘
```

### Design Patterns Implemented

- **Domain-Driven Design (DDD)** - Rich domain models with business logic
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **CQRS Foundation** - Separation of read/write operations
- **Event Sourcing Ready** - Domain events for audit and integration
- **Saga Pattern** - Complex workflow orchestration
- **Result Pattern** - Functional error handling

## 🛠️ Technology Stack

### Core Technologies
- **.NET 8** - Latest LTS framework
- **C# 12** - Modern language features
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for data access

### Data & Persistence
- **SQL Server** - Primary database
- **Entity Framework Core** - Code-first migrations
- **LINQ** - Query operations

### Libraries & Tools
- **Swashbuckle (Swagger)** - API documentation
- **AutoMapper** - Object-object mapping (optional)
- **Serilog** - Structured logging (recommended)
- **FluentValidation** - Input validation (recommended)

### Development Tools
- **Visual Studio 2022** / **Visual Studio Code**
- **SQL Server Management Studio** / **Azure Data Studio**
- **Postman** / **Swagger UI** - API testing
- **Git** - Version control

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server Express
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/afripay.git
cd afripay
```

2. **Create the solution and projects**
```bash
# Create solution
dotnet new sln -n AfriPay

# Create projects
dotnet new classlib -n AfriPay.Core
dotnet new classlib -n AfriPay.DAL
dotnet new classlib -n AfriPay.APP
dotnet new webapi -n AfriPay.API

# Add projects to solution
dotnet sln add AfriPay.Core/AfriPay.Core.csproj
dotnet sln add AfriPay.DAL/AfriPay.DAL.csproj
dotnet sln add AfriPay.APP/AfriPay.APP.csproj
dotnet sln add AfriPay.API/AfriPay.API.csproj
```

3. **Add project references**
```bash
cd AfriPay.DAL
dotnet add reference ../AfriPay.Core/AfriPay.Core.csproj

cd ../AfriPay.APP
dotnet add reference ../AfriPay.Core/AfriPay.Core.csproj
dotnet add reference ../AfriPay.DAL/AfriPay.DAL.csproj

cd ../AfriPay.API
dotnet add reference ../AfriPay.Core/AfriPay.Core.csproj
dotnet add reference ../AfriPay.DAL/AfriPay.DAL.csproj
dotnet add reference ../AfriPay.APP/AfriPay.APP.csproj
```

4. **Install NuGet packages**
```bash
# AfriPay.DAL
cd ../AfriPay.DAL
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# AfriPay.API
cd ../AfriPay.API
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Design
```

5. **Configure database connection**

Update `appsettings.json` in AfriPay.API:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AfriPayDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

6. **Create and apply database migrations**
```bash
cd AfriPay.API
dotnet ef migrations add InitialCreate --project ../AfriPay.DAL/AfriPay.DAL.csproj --startup-project AfriPay.API.csproj
dotnet ef database update --project ../AfriPay.DAL/AfriPay.DAL.csproj --startup-project AfriPay.API.csproj
```

7. **Run the application**
```bash
dotnet run
```

8. **Access Swagger UI**
```
https://localhost:5001/swagger
```

## 📁 Project Structure

```
AfriPay/
├── AfriPay.Core/                   # Domain Layer (No external dependencies)
│   ├── Common/                     # Shared abstractions
│   │   ├── AggregateRoot.cs
│   │   ├── IDomainEvent.cs
│   │   └── Result.cs
│   ├── Entities/                   # Domain entities
│   │   ├── Account.cs
│   │   ├── Customer.cs
│   │   ├── OnboardingRequest.cs
│   │   └── Transaction.cs
│   ├── Enums/                      # Enumerations
│   │   ├── AccountType.cs
│   │   ├── OnboardingStatus.cs
│   │   └── TransactionType.cs
│   ├── Events/                     # Domain events
│   │   ├── BvnVerifiedEvent.cs
│   │   ├── CustomerCreatedEvent.cs
│   │   ├── OnboardingCompletedEvent.cs
│   │   └── ...
│   ├── Interfaces/                 # Contracts
│   │   ├── IAccountRepository.cs
│   │   ├── IBvnVerificationService.cs
│   │   ├── ICustomerRepository.cs
│   │   ├── IEventPublisher.cs
│   │   ├── IOnboardingRequestRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── IVirtualAccountProvider.cs
│   └── ValueObjects/               # Value objects
│       ├── AccountId.cs
│       ├── AccountNumber.cs
│       ├── BVN.cs
│       ├── CustomerId.cs
│       ├── CustomerReference.cs
│       └── Money.cs
│
├── AfriPay.DAL/                    # Data Access Layer
│   ├── Configurations/             # EF Core configurations
│   │   ├── AccountConfiguration.cs
│   │   ├── CustomerConfiguration.cs
│   │   └── OnboardingRequestConfiguration.cs
│   ├── Data/
│   │   └── AfriPayDbContext.cs
│   ├── Migrations/                 # EF Core migrations
│   ├── Repositories/               # Repository implementations
│   │   ├── AccountRepository.cs
│   │   ├── CustomerRepository.cs
│   │   └── OnboardingRequestRepository.cs
│   └── UnitOfWork.cs
│
├── AfriPay.APP/                    # Application Layer
│   ├── DTOs/                       # Data transfer objects
│   │   ├── OnboardingStartRequest.cs
│   │   └── OnboardingResponse.cs
│   ├── EventHandlers/              # Domain event handlers
│   │   ├── BvnVerifiedForOnboardingHandler.cs
│   │   ├── CustomerCreatedHandler.cs
│   │   └── OnboardingRequestedHandler.cs
│   └── Services/                   # Application services
│       ├── IOnboardingService.cs
│       └── OnboardingService.cs
│
└── AfriPay.API/                    # Presentation Layer
    ├── Controllers/
    │   └── OnboardingController.cs
    ├── Infrastructure/             # Infrastructure implementations
    │   ├── InMemoryEventPublisher.cs
    │   ├── MockBvnVerificationService.cs
    │   └── MockVirtualAccountProvider.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    └── Program.cs
```

## 📚 API Documentation

### Base URL
```
https://localhost:5001/api
```

### Endpoints

#### 1. Start Onboarding
**POST** `/onboarding/start`

Initiates the customer onboarding process.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "08012345678",
  "bvn": "12345678901"
}
```

**Response (200 OK):**
```json
{
  "onboardingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "requestReference": "ONB-3FA85F6457174562B3FC2C963F66AFA6",
  "status": "Initiated",
  "message": "Onboarding request created successfully. BVN verification will be processed.",
  "requestedAt": "2024-11-29T10:00:00Z"
}
```

**Response (400 Bad Request):**
```json
{
  "title": "Onboarding Failed",
  "detail": "Customer with this email already exists",
  "status": 400
}
```

#### 2. Get Onboarding Status
**GET** `/onboarding/{onboardingId}/status`

Retrieves the current status of an onboarding request.

**Response (200 OK):**
```json
{
  "onboardingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "requestReference": "ONB-3FA85F6457174562B3FC2C963F66AFA6",
  "status": "Completed",
  "message": "Onboarding completed successfully",
  "customerReference": "CUST-7B94D82E3F4C4A9F8E1B5C2D9A6F3E8B",
  "accountNumber": "1000000001",
  "requestedAt": "2024-11-29T10:00:00Z",
  "completedAt": "2024-11-29T10:05:23Z"
}
```

**Response (404 Not Found):**
```json
{
  "title": "Onboarding Not Found",
  "detail": "Onboarding request not found",
  "status": 404
}
```

### Onboarding Status Values

| Status | Description |
|--------|-------------|
| `Initiated` | Onboarding request created |
| `BvnVerificationPending` | BVN verification in progress |
| `BvnVerified` | BVN successfully verified |
| `BvnVerificationFailed` | BVN verification failed |
| `CustomerCreated` | Customer record created |
| `VirtualAccountCreationPending` | Virtual account creation in progress |
| `VirtualAccountCreated` | Virtual account created |
| `VirtualAccountCreationFailed` | Virtual account creation failed |
| `Completed` | Onboarding successfully completed |
| `Failed` | Onboarding failed |

## 💻 Development

### Code Standards

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Write XML documentation comments for public APIs
- Keep methods focused and small (Single Responsibility Principle)
- Write unit tests for business logic

### Branching Strategy

```
main            - Production-ready code
├── develop     - Integration branch
│   ├── feature/onboarding-api
│   ├── feature/transaction-module
│   └── bugfix/account-validation
```

### Commit Message Convention

```
feat: Add customer onboarding endpoint
fix: Resolve BVN validation issue
docs: Update API documentation
refactor: Improve repository implementation
test: Add unit tests for OnboardingService
```

### Running Migrations

**Add a new migration:**
```bash
dotnet ef migrations add <MigrationName> --project AfriPay.DAL --startup-project AfriPay.API
```

**Update database:**
```bash
dotnet ef database update --project AfriPay.DAL --startup-project AfriPay.API
```

**Remove last migration:**
```bash
dotnet ef migrations remove --project AfriPay.DAL --startup-project AfriPay.API
```

## 🧪 Testing

### Unit Testing

```bash
# Create test project
dotnet new xunit -n AfriPay.Tests
dotnet add AfriPay.Tests reference AfriPay.Core
dotnet add AfriPay.Tests reference AfriPay.APP

# Run tests
dotnet test
```

### Integration Testing

```bash
# Create integration test project
dotnet new xunit -n AfriPay.IntegrationTests
dotnet add AfriPay.IntegrationTests reference AfriPay.API

# Run integration tests
dotnet test AfriPay.IntegrationTests
```

### API Testing with Postman

Import the Postman collection from `/docs/postman/AfriPay.postman_collection.json`

## 🚢 Deployment

### Prerequisites for Production

1. **Replace Mock Services**
   - Implement real `IBvnVerificationService` with actual BVN provider API
   - Implement real `IVirtualAccountProvider` (e.g., Flutterwave, Paystack, Mono)

2. **Add Message Queue**
   - Replace `InMemoryEventPublisher` with RabbitMQ or Azure Service Bus
   - Ensures reliable event processing

3. **Security Enhancements**
   - Implement JWT authentication
   - Add API rate limiting
   - Enable HTTPS enforcement
   - Implement API key management

4. **Monitoring & Logging**
   - Integrate Application Insights or ELK stack
   - Add health check endpoints
   - Implement distributed tracing

### Deployment to Azure

```bash
# Publish the application
dotnet publish -c Release -o ./publish

# Deploy to Azure App Service
az webapp deployment source config-zip \
  --resource-group AfriPayResourceGroup \
  --name afripay-api \
  --src publish.zip
```

### Environment Variables

Set these in your hosting environment:

```bash
ConnectionStrings__DefaultConnection="<production-connection-string>"
BvnVerification__ApiUrl="<bvn-provider-url>"
BvnVerification__ApiKey="<bvn-api-key>"
VirtualAccountProvider__ApiUrl="<va-provider-url>"
VirtualAccountProvider__ApiKey="<va-api-key>"
```

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'feat: Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and development process.

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Team PAPSS** - *Initial work* - [YourGitHub](https://github.com/yourusername)

## 🙏 Acknowledgments

- Inspired by modern banking systems and fintech platforms
- Built with Clean Architecture principles by Robert C. Martin
- Domain-Driven Design concepts by Eric Evans
- Microsoft .NET documentation and best practices

## 📞 Support

For support, email support@afripay.com or open an issue in the GitHub repository.

## 🗺️ Roadmap

- [x] Customer onboarding with BVN verification
- [x] Virtual account creation
- [ ] Fund transfer functionality
- [ ] Bill payment integration
- [ ] Transaction history and reporting
- [ ] Multi-currency support
- [ ] Mobile app integration
- [ ] KYC document upload
- [ ] Admin dashboard
- [ ] Real-time notifications

---

**Built with ❤️ using .NET 8 and Clean Architecture principles**
