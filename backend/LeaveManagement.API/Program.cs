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
builder.Services.AddScoped<LeaveRequestValidator>();

var databaseProvider = builder.Configuration["DatabaseProvider"]
    ?? throw new InvalidOperationException("DatabaseProvider is not configured. Use 'PostgreSQL' or 'SqlServer'.");

builder.Services.AddDbContext<LeaveManagementDbContext>(options =>
{
    if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
    {
        var postgreSqlConnection = builder.Configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("ConnectionStrings:PostgreSQL is not configured.");
        options.UseNpgsql(postgreSqlConnection);
    }
    else if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer is not configured.");
        options.UseSqlServer(sqlServerConnection);
    }
    else
    {
        throw new InvalidOperationException("Invalid DatabaseProvider value. Use 'PostgreSQL' or 'SqlServer'.");
    }
});

var allowedOrigins = new List<string> { "http://localhost:4200" };
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
if (!string.IsNullOrWhiteSpace(frontendUrl))
{
    allowedOrigins.Add(frontendUrl.TrimEnd('/'));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularCors", policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AngularCors");
app.UseAuthorization();
app.MapControllers();

app.Run();
