# Employee Leave Management System

A complete leave management application with an ASP.NET Core Web API backend, Entity Framework Core, SQL Server, Angular, and Bootstrap.

## Project structure

```text
backend/LeaveManagement.API   ASP.NET Core Web API (.NET 8)
frontend                      Angular Bootstrap frontend
database                      SQL Server setup script
docs                          Additional project documentation
```

## Backend

### Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server LocalDB

### Configuration

The default connection string is in `backend/LeaveManagement.API/appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LeaveManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Update it if you are using a different SQL Server instance.

### Run the API

```bash
cd backend/LeaveManagement.API
dotnet restore
# Create the database first by running database/create_leave_management.sql
dotnet run
```

Swagger is available in development mode at the URL printed by `dotnet run`, followed by `/swagger`.

## Database

Create and seed the SQL Server database with the included SQL script:

```bash
sqlcmd -S "(localdb)\\MSSQLLocalDB" -i database/create_leave_management.sql
```

The script creates `Employees`, `LeaveTypes`, and `LeaveRequests`, then seeds these leave types:

- Annual Leave
- Sick Leave
- Unpaid Leave

## Frontend

### Prerequisites

- Node.js
- npm

### Run the Angular app

```bash
cd frontend
npm install
npm start
```

Open `http://localhost:4200` in your browser. The frontend expects the API at `https://localhost:7047/api`. If your backend uses another URL, update `frontend/src/app/services/api.service.ts`.

## Main features

- Employee management with add, edit, delete/deactivate, and search.
- New leave request workflow with inclusive day calculation.
- Request history with status and date filters.
- Manager approval page for approving or rejecting pending requests with comments.
- Server-side validation and clean JSON error responses.
