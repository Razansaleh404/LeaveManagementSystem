# Employee Leave Management System

A leave management application with an ASP.NET Core .NET 8 Web API backend, Entity Framework Core 8, PostgreSQL, Angular, and Bootstrap.

## Project structure

```text
backend/LeaveManagement.API   ASP.NET Core Web API (.NET 8)
frontend                      Angular Bootstrap frontend
database                      PostgreSQL setup script
docs                          Additional project documentation
render.yaml                   Render blueprint for database, API, and static frontend
```


## Merge conflict resolution notes

The deployment branch keeps the PostgreSQL/Render-ready versions of the files that commonly conflict with the earlier local-only implementation:

- `backend/LeaveManagement.API/LeaveManagement.API.csproj` uses `Npgsql.EntityFrameworkCore.PostgreSQL` instead of the previous relational EF provider.
- `backend/LeaveManagement.API/Program.cs` uses `UseNpgsql`, reads Render's `PORT`, accepts `ConnectionStrings__DefaultConnection`, and allows the deployed frontend through `FRONTEND_URL`.
- `backend/LeaveManagement.API/appsettings.json` contains the local PostgreSQL connection string.
- `database/create_leave_management.sql` is a PostgreSQL script.
- `frontend/src/app/services/api.service.ts` reads the API URL from Angular environment files instead of hardcoding localhost.

## Backend configuration

The backend uses PostgreSQL through `Npgsql.EntityFrameworkCore.PostgreSQL` and reads the `DefaultConnection` connection string from normal .NET configuration. It accepts normal Npgsql keyword connection strings and Render-style `postgres://...` URLs.

Local default in `backend/LeaveManagement.API/appsettings.json`:

```text
Host=localhost;Port=5432;Database=leave_management;Username=postgres;Password=postgres
```

On Render, set the environment variable below. Render's blueprint maps it automatically from the managed PostgreSQL database:

```text
ConnectionStrings__DefaultConnection
```

Other backend environment variables:

```text
ASPNETCORE_ENVIRONMENT=Production
FRONTEND_URL=https://your-render-frontend-url.onrender.com
ENABLE_SWAGGER=true
PORT=<set automatically by Render>
```

`FRONTEND_URL` is used by CORS. For local development, the API also allows `http://localhost:4200` and `https://localhost:4200`.

## Run locally with PostgreSQL

### 1. Prerequisites

- .NET 8 SDK
- PostgreSQL 14+ or a local PostgreSQL container
- Node.js and npm

### 2. Start PostgreSQL

Using Docker is one simple option:

```bash
docker run --name leave-management-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=leave_management \
  -p 5432:5432 \
  -d postgres:16
```

If you already have PostgreSQL installed, create the database manually:

```bash
createdb -h localhost -p 5432 -U postgres leave_management
```

### 3. Create or update the database

Recommended EF Core migration approach:

```bash
cd backend/LeaveManagement.API
dotnet restore
dotnet tool restore 2>/dev/null || true
dotnet ef database update
```

If `dotnet ef` is not installed, install it locally or globally:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

Alternative SQL script approach from the repository root:

```bash
psql -h localhost -p 5432 -U postgres -d leave_management -f database/create_leave_management.sql
```

### 4. Run the backend

```bash
cd backend/LeaveManagement.API
dotnet run
```

Swagger is available in development at the URL printed by `dotnet run`, followed by `/swagger`.

### 5. Run the frontend

```bash
cd frontend
npm install
npm start
```

Open `http://localhost:4200`.

The local Angular API URL is configured in:

```text
frontend/src/environments/environment.ts
```

Update it if your backend runs on a different local URL.

## Deploy to Render from GitHub

This repository includes a Render blueprint in `render.yaml` that creates:

1. A managed PostgreSQL database named `leave-management-db`
2. A backend web service from `backend/LeaveManagement.API`
3. A frontend static site from `frontend`

### 1. Push to GitHub

Commit and push your branch to GitHub.

### 2. Create a Render Blueprint

In Render:

1. Choose **New +**.
2. Choose **Blueprint**.
3. Connect your GitHub repository.
4. Select this repository and allow Render to read `render.yaml`.
5. Apply the blueprint.

### 3. Backend service settings

The backend service uses:

```text
Build Command: dotnet restore && dotnet publish -c Release -o publish
Start Command: dotnet publish/LeaveManagement.API.dll
Root Directory: backend/LeaveManagement.API
```

The blueprint sets:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<Render PostgreSQL connection string>
ENABLE_SWAGGER=true
```

After Render creates the frontend static site, set this backend environment variable to the exact frontend URL:

```text
FRONTEND_URL=https://your-render-frontend-url.onrender.com
```

Then redeploy the backend so CORS allows the deployed frontend.

### 4. Apply database migrations on Render

Render does not automatically run EF Core migrations from this project. After the database and backend service are created, apply migrations from your local machine against the Render PostgreSQL external connection string:

```bash
cd backend/LeaveManagement.API
ConnectionStrings__DefaultConnection="<Render external PostgreSQL connection string>" dotnet ef database update
```

If your shell does not support inline environment variables, temporarily set `ConnectionStrings__DefaultConnection` through your shell's normal environment variable syntax and then run:

```bash
dotnet ef database update
```

### 5. Frontend service settings

The frontend static site uses:

```text
Build Command: npm install && npm run build
Root Directory: frontend
Publish Directory: dist/leave-management-frontend/browser
```

The publish directory matches Angular's application builder output for this project.

### 6. Update the production API URL after Render creates the backend

Render will assign the backend API a URL like:

```text
https://leave-management-api.onrender.com
```

Edit this file:

```text
frontend/src/environments/environment.prod.ts
```

Replace the placeholder API URL with your real backend API URL plus `/api`:

```ts
apiBaseUrl: 'https://leave-management-api.onrender.com/api'
```

Commit and push that change, then redeploy the Render frontend static site.

## Useful verification commands

From the repository root:

```bash
rg "Npgsql|UseNpgsql" backend/LeaveManagement.API
```

The command should show the PostgreSQL provider package and `UseNpgsql` configuration.

Backend build:

```bash
dotnet build backend/LeaveManagement.API/LeaveManagement.API.csproj
```

Frontend build:

```bash
cd frontend
npm install
npm run build
```

Backend smoke test after running the API:

```bash
curl -k https://localhost:7047/api/leavetypes
```
