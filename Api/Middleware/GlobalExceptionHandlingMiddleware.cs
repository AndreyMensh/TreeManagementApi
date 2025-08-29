using System.Net;
using System.Text;
using System.Text.Json;
using TreeManagementApi.Application.DTOs;
using TreeManagementApi.Application.Exceptions;
using TreeManagementApi.Application.Interfaces;
using TreeManagementApi.Domain.Entities;

namespace TreeManagementApi.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions,
/// logs them to the database, and returns appropriate error responses to clients.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request processing");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var eventId = await LogExceptionAsync(context, exception);

        var errorResponse = CreateErrorResponse(exception, eventId);
        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(jsonResponse, Encoding.UTF8);
    }

    /// <summary>
    /// Logs the exception to the database and returns the EventId
    /// </summary>
    private async Task<long> LogExceptionAsync(HttpContext context, Exception exception)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var queryParameters = await SerializeQueryParametersAsync(context);
            var bodyParameters = await SerializeBodyParametersAsync(context);

            var exceptionJournal = ExceptionJournal.Create(
                exception,
                queryParameters,
                bodyParameters,
                context.Request.Method,
                context.Request.Path.Value,
                context.Request.Headers.UserAgent.FirstOrDefault(),
                GetClientIpAddress(context)
            );

            var savedEntry = await unitOfWork.ExceptionJournals.AddExceptionAsync(exceptionJournal);
            return savedEntry.Id;
        }
        catch (Exception loggingEx)
        {
            _logger.LogError(loggingEx, "Failed to log exception to database");
            // Return a timestamp-based ID as fallback
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    /// <summary>
    /// Creates the appropriate error response based on exception type
    /// </summary>
    private static ErrorResponseDto CreateErrorResponse(Exception exception, long eventId)
    {
        if (exception is SecureException)
        {
            return new ErrorResponseDto
            {
                Type = "Secure",
                Id = eventId.ToString(),
                Data = new ErrorDataDto
                {
                    Message = exception.Message
                }
            };
        }

        return new ErrorResponseDto
        {
            Type = "Exception",
            Id = eventId.ToString(),
            Data = new ErrorDataDto
            {
                Message = $"Internal server error ID = {eventId}"
            }
        };
    }

    /// <summary>
    /// Serializes query parameters to JSON
    /// </summary>
    private static Task<string?> SerializeQueryParametersAsync(HttpContext context)
    {
        try
        {
            if (context.Request.Query.Any())
            {
                var queryDict = context.Request.Query.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToArray()
                );
                return Task.FromResult<string?>(JsonSerializer.Serialize(queryDict));
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail the exception handling
            var logger = context.RequestServices.GetService<ILogger<GlobalExceptionHandlingMiddleware>>();
            logger?.LogWarning(ex, "Failed to serialize query parameters");
        }

        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Serializes request body parameters to JSON
    /// </summary>
    private static async Task<string?> SerializeBodyParametersAsync(HttpContext context)
    {
        try
        {
            if (context.Request.ContentLength > 0 && 
                context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Enable buffering to allow multiple reads
                context.Request.EnableBuffering();
                
                // Reset position to beginning
                context.Request.Body.Position = 0;
                
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                
                // Reset position again for any subsequent reads
                context.Request.Body.Position = 0;
                
                return string.IsNullOrWhiteSpace(body) ? null : body;
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail the exception handling
            var logger = context.RequestServices.GetService<ILogger<GlobalExceptionHandlingMiddleware>>();
            logger?.LogWarning(ex, "Failed to serialize body parameters");
        }

        return null;
    }

    /// <summary>
    /// Gets the client IP address from the HTTP context
    /// </summary>
    private static string? GetClientIpAddress(HttpContext context)
    {
        try
        {
            // Try X-Forwarded-For header first (for reverse proxies)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Try X-Real-IP header
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Extension method to easily register the global exception handling middleware
/// </summary>
public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}