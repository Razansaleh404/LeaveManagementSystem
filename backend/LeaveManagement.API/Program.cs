using LeaveManagement.API.Data;
using LeaveManagement.API.Middleware;
using LeaveManagement.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = NormalizePostgresConnectionString(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured."));

builder.Services.AddDbContext<LeaveManagementDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        var allowedOrigins = new List<string> { "http://localhost:4200", "https://localhost:4200" };
        var frontendUrl = builder.Configuration["FRONTEND_URL"];

        if (!string.IsNullOrWhiteSpace(frontendUrl))
        {
            allowedOrigins.Add(frontendUrl.Trim().TrimEnd('/'));
        }

        policy.WithOrigins(allowedOrigins.Distinct().ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("ENABLE_SWAGGER"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AngularClient");
app.MapControllers();

app.Run();


static string NormalizePostgresConnectionString(string connectionString)
{
    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var databaseUri)
        || (databaseUri.Scheme != "postgres" && databaseUri.Scheme != "postgresql"))
    {
        return connectionString;
    }

    var userInfo = databaseUri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty);
    var password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty);
    var database = databaseUri.AbsolutePath.TrimStart('/');
    var portPart = databaseUri.IsDefaultPort ? string.Empty : $"Port={databaseUri.Port};";

    return $"Host={databaseUri.Host};{portPart}Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}
