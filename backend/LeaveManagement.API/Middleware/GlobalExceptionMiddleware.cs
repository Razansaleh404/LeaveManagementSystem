using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "A database error occurred.");
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "A database error occurred. Please check your request and try again.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred.");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    }
}
