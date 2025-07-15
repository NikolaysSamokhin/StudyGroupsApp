# StudyGroupsApp

StudyGroupsApp is a web application for students to create and join study groups by subject. It includes RESTful API endpoints, uses ASP.NET Core and Entity Framework Core with a SQL Server database, and exposes Swagger documentation.

---

## 📚 Features

- Create a new study group for a specific subject (`Math`, `Chemistry`, `Physics`)
- Join and leave study groups
- List and filter all available study groups
- Search by subject and sort by creation date
- Swagger UI for testing endpoints

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- SQL Server (local or remote instance)

---

### Configuration

Edit your `appsettings.json` file to configure the connection string for SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudyGroupsDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```

## 🛠️ Build and Run

### Step 1: Restore dependencies

```bash
dotnet restore
```
### Step 2: Build the project
```bash
dotnet build
```
### Step 3: Run the application
```bash
dotnet run
```
### By default, the app will be available at:

- Application: http://localhost:5000

- Swagger UI: http://localhost:5000/swagger

