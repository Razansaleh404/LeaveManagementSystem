# Employee Leave Management System

A beginner-friendly full-stack web application for submitting, tracking, approving, and rejecting employee leave requests.

## Technologies Used

- **Backend:** ASP.NET Core Web API (.NET 8)
- **Frontend:** Angular with Bootstrap
- **Database:** SQL Server
- **ORM:** Entity Framework Core with `Microsoft.EntityFrameworkCore.SqlServer`
- **Deployment:** GitHub to Render using `render.yaml`

## Features

- Employee management: list, add, edit, delete, search, and active/inactive status.
- Leave request creation with employee and leave type dropdowns.
- Automatic inclusive day calculation from From Date and To Date.
- Client-side and API validation for date ranges, past dates, required reason, valid status, and existing foreign keys.
- Request history with status and date-range filters.
- Manager module for pending requests, approvals, rejections, and comments.
- SQL Server-backed EF Core migrations and default leave type seed data.
- Swagger API documentation.
- Global exception handling, friendly error messages, error logging, and CORS for Angular.

## Project Structure

```text
/backend/LeaveManagement.API
/frontend
/database
README.md
render.yaml
```

## Database Tables

### Employees

- `EmployeeID` primary key, auto generated
- `FirstName` required, max 50
- `LastName` required, max 50
- `Email` required, max 100, unique
- `Department` required, max 50
- `Role` required, Employee or Manager, defaults to Employee
- `PasswordHash` required for authentication
- `PasswordSalt` required for authentication
- `IsActive` defaults to true

### LeaveTypes

- `LeaveTypeID` primary key, auto generated
- `LeaveName` required, max 50
- Seeded values: Annual Leave, Sick Leave, Unpaid Leave

### LeaveRequests

- `RequestID` primary key, auto generated
- `EmployeeID` foreign key to Employees
- `LeaveTypeID` foreign key to LeaveTypes
- `FromDate`, `ToDate`, `NumberOfDays`
- `Reason` required, max 500
- `Status`: Pending, Approved, or Rejected
- `ManagerComments` optional, max 500
- `CreatedDate`


## Authentication and Demo Login

The API uses ASP.NET Core JWT bearer authentication with HS256 signing. The JWT configuration lives under `JwtSettings`:

```json
"JwtSettings": {
  "Key": "DevelopmentOnlySecretKeyForLeaveManagementJwtAuth12345",
  "Issuer": "LeaveManagement.API",
  "Audience": "LeaveManagement.Angular",
  "DurationInMinutes": 120
}
```

For production, override `JwtSettings__Key` with a long secret value instead of relying on the development key. Demo users are seeded automatically when the API can connect to the database:

- Manager: `manager@leave.local` / `Password123!`
- Employee: `employee@leave.local` / `Password123!`

## Install SQL Server Locally

1. Install SQL Server Developer, Express, LocalDB, or use an available company SQL Server instance.
2. Create a database named `leave_management_db` in SQL Server Management Studio, Azure Data Studio, or `sqlcmd`:

```sql
CREATE DATABASE leave_management_db;
```

## Update Backend Connection String

Open `backend/LeaveManagement.API/appsettings.json` and update the SQL Server connection string for your machine. The default local Windows SQL Server connection is:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=leave_management_db;Trusted_Connection=True;TrustServerCertificate=True"
}
```

For SQL Server authentication, use a connection string like this instead:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=leave_management_db;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
}
```

Deployment environments can override this with the environment variable `ConnectionStrings__DefaultConnection`.

## Run Backend Locally

```bash
cd backend/LeaveManagement.API
dotnet restore
dotnet ef database update
dotnet run
```

Swagger is available at `http://localhost:5000/swagger`.

If `dotnet ef` is not installed, run:

```bash
dotnet tool install --global dotnet-ef
```

## Run Frontend Locally

```bash
cd frontend
npm install
npm start
```

The Angular app runs at `http://localhost:4200` and calls the API configured in `frontend/src/environments/environment.ts`.

## API URL Configuration

Local API URL:

```ts
// frontend/src/environments/environment.ts
apiUrl: 'http://localhost:5000/api'
```

Production API URL:

```ts
// frontend/src/environments/environment.prod.ts
// Replace this after Render gives you the backend URL.
apiUrl: 'https://YOUR-BACKEND-RENDER-URL.onrender.com/api'
```

After updating the production API URL, commit and push the change so Render can rebuild the frontend.

## Main API Endpoints

### Employees

- `GET /api/employees`
- `GET /api/employees/{id}`
- `POST /api/employees`
- `PUT /api/employees/{id}`
- `DELETE /api/employees/{id}`
- `GET /api/employees/search?term=value`

### LeaveTypes

- `GET /api/leavetypes`
- `POST /api/leavetypes`
- `PUT /api/leavetypes/{id}`
- `DELETE /api/leavetypes/{id}`

### LeaveRequests

- `GET /api/leaverequests`
- `GET /api/leaverequests/{id}`
- `POST /api/leaverequests`
- `PUT /api/leaverequests/{id}`
- `DELETE /api/leaverequests/{id}`
- `GET /api/leaverequests/history`
- `GET /api/leaverequests/filter?status=Pending&fromDate=YYYY-MM-DD&toDate=YYYY-MM-DD`
- `GET /api/leaverequests/pending`
- `PUT /api/leaverequests/{id}/approve`
- `PUT /api/leaverequests/{id}/reject`

Approval/rejection body example:

```json
{
  "managerComments": "Approved for the requested dates."
}
```

## Manual Database Script

If you do not want to use EF Core migrations, run `database/create_tables.sql` against SQL Server. The script is idempotent for the authentication columns, so it can also update an older manually-created `Employees` table with `Role`, `PasswordHash`, and `PasswordSalt`. Migrations are still recommended for normal development.

## Deploy to Render from GitHub

1. Push this repository to GitHub.
2. In Render, create a new **Blueprint** and select this repository.
3. Render reads `render.yaml` and creates:
   - ASP.NET Core backend web service
   - Angular static frontend site
4. The backend service uses:
   - Root directory: `backend/LeaveManagement.API`
   - Build command: `dotnet publish -c Release -o out`
   - Start command: `dotnet out/LeaveManagement.API.dll`
   - `ConnectionStrings__DefaultConnection` set manually to your SQL Server connection string
   - `JwtSettings__Key` set manually to a long production JWT signing key
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `FRONTEND_URL` set to the frontend URL
5. The frontend service uses:
   - Root directory: `frontend`
   - Build command: `npm install && npm run build`
   - Publish directory: `dist/leave-management-frontend/browser`

> Render does not provision SQL Server from `render.yaml`. Use a reachable SQL Server instance, then set `ConnectionStrings__DefaultConnection` in the Render backend environment variables.

## Important Render Follow-up Steps

1. After Render gives you the backend URL, update `frontend/src/environments/environment.prod.ts`:

```ts
apiUrl: 'https://YOUR-ACTUAL-BACKEND.onrender.com/api'
```

2. Commit and push the frontend change.
3. Redeploy the frontend static site.
4. Set the backend `ConnectionStrings__DefaultConnection` environment variable to your SQL Server connection string.
5. Set `JwtSettings__Key` to a long production JWT signing key.
6. Update the backend `FRONTEND_URL` environment variable if your frontend URL is different from the placeholder in `render.yaml`.
7. Redeploy the backend after changing environment variables.

## Notes

- The backend listens on Render's `PORT` environment variable automatically.
- EF Core uses parameterized queries through LINQ and avoids raw SQL.
- CORS allows `http://localhost:4200` in development and the deployed frontend URL through `FRONTEND_URL`.
- Authentication/login is intentionally not included to keep the assignment simple.
