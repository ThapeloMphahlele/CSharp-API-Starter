```markdown
# Enterprise ASP.NET Core 10 Backend Boilerplate

> A production-grade, highly scalable backend template built with ASP.NET Core 10, Entity Framework Core 9, and JWT/Cookie-based authentication. Designed with a modular domain-subfolder structure to mirror enterprise software engineering standards.

---

## Key Features

* **Modular Domain Architecture:** Code is organized by domain subfolders (`Auth`, etc.) inside technical layers, ensuring high cohesion and low coupling as the application scales.
* **Secure Authentication:** JWT-based access tokens with HttpOnly, Secure `access_token` and `refresh_token` cookie rotation to mitigate XSS and CSRF risks.
* **Robust Validation:** Integrated **FluentValidation** pipeline with automated model state action filtering.
* **Database Agnostic:** Configured for seamless switching between **MySQL** (via Pomelo provider) and **SQL Server** using EF Core Code-First migrations.
* **Enterprise Security:** Password hashing via BCrypt, centralized error handling, and cryptographically secure token generation for password resets.
* **API Documentation:** Fully integrated Swagger/OpenAPI UI with JWT Bearer authorization support.
* **Containerized Development:** Out-of-the-box `docker-compose` setup for rapid local database provisioning.

---

## 🛠️ Tech Stack

* **Framework:** ASP.NET Core 10 (.NET 10 SDK)
* **ORM:** Entity Framework Core 9 (Pinned for provider stability)
* **Database:** MySQL 8.0 (Docker)
* **Validation:** FluentValidation
* **Security:** System.IdentityModel.Tokens.Jwt, BCrypt.Net-Next
* **Logging:** Serilog

---

## 📂 Project Architecture

```text
BareBonesApi/
├── Controllers/
│   └── Auth/          # Domain-scoped HTTP endpoints & HttpOnly cookie management
├── DTOs/
│   └── Auth/          # Immutable C# record types for request/response payloads
├── Entities/
│   └── Auth/          # EF Core Database Models (User, RefreshToken)
├── Filters/           # Custom action filters (automated validation interceptor)
├── Interfaces/
│   └── Auth/          # Service and Repository contracts
├── Repositories/
│   └── Auth/          # Data access layer using LINQ and EF Core
├── Services/
│   └── Auth/          # Core business logic & cryptographic operations
├── Validators/        # FluentValidation rule definitions
├── ApplicationDbContext.cs
└── Program.cs

```

---

## ⚙️ Getting Started

### Prerequisites

* .NET 10 SDK installed on your machine
* Docker Desktop (for running the local database container)

### 1. Clone the Repository

```bash
git clone [https://github.com/ThapeloMphahlele/CSharp-API-Starter.git](https://github.com/ThapeloMphahlele/CSharp-API-Starter.git)
cd BareBonesApi

```

### 2. Spin Up the Local Database

Ensure Docker is running, then start the MySQL container:

```bash
docker compose up -d

```

### 3. Configure Environment Settings

Create an `appsettings.Development.json` file in the root directory and map your local environment variables:

```json
{
  "ConnectionStrings": {
    "MySQL": "Server=localhost;Port=3308;Database=BareBonesDb;User=root;Password=your_password;"
  },
  "JwtSettings": {
    "Secret": "YOUR_SUPER_SECRET_128_CHARACTER_HEX_KEY",
    "Issuer": "BareBonesApi",
    "Audience": "BareBonesClient",
    "ExpiryInMinutes": 60
  }
}

```

### 4. Apply Database Migrations & Run

Ensure you have the EF Core CLI tools installed (`dotnet tool install --global dotnet-ef --version 9.0.0`), then run:

```bash
dotnet ef database update
dotnet run

```

Navigate to the Swagger UI URL provided in your terminal output to explore and test the endpoints.

---

## 🛡️ Auth Architecture Flow

1. **Login:** Validates user credentials against hashed database records. Issues a short-lived JWT stored in an `access_token` HttpOnly cookie and a long-lived rotation token stored in a `refresh_token` cookie.
2. **Token Extraction:** Custom middleware automatically falls back to intercepting the `access_token` cookie if the `Authorization: Bearer` header is absent.
3. **Password Reset:** Generates a 256-bit cryptographically secure token, hashes it using SHA-256 before saving to the database, and enforces a strict 15-minute expiration window.

---