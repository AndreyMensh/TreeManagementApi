using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeManagementApi.Domain.Entities;

/// <summary>
/// Entity for storing exception logs in the database.
/// Every exception that occurs in the application is logged here with full details.
/// </summary>
public class ExceptionJournal
{
    /// <summary>
    /// Unique identifier for the exception log entry.
    /// Used as EventId in the API responses to track specific exceptions.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// EventId that is returned to the client in error responses.
    /// This is the same as Id but explicitly named for clarity.
    /// </summary>
    [NotMapped]
    public long EventId => Id;

    /// <summary>
    /// Timestamp when the exception occurred
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Serialized JSON containing query parameters from the HTTP request.
    /// Helps in reproducing the error conditions.
    /// </summary>
    public string? QueryParameters { get; set; }

    /// <summary>
    /// Serialized JSON containing the request body parameters.
    /// Only populated for requests with body content (POST, PUT, etc.).
    /// </summary>
    public string? BodyParameters { get; set; }

    /// <summary>
    /// Full stack trace of the exception.
    /// Critical for debugging and identifying the exact location of errors.
    /// </summary>
    [Required]
    public string StackTrace { get; set; } = string.Empty;

    /// <summary>
    /// Full type name of the exception (e.g., "TreeManagementApi.Application.Exceptions.SecureException").
    /// Used to determine how to handle the exception in the global handler.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// The original exception message.
    /// For SecureExceptions, this message may be returned to the client.
    /// For other exceptions, this is kept for internal debugging only.
    /// </summary>
    [Required]
    public string ExceptionMessage { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method of the request that caused the exception (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    [StringLength(10)]
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Request path that caused the exception
    /// </summary>
    [StringLength(2000)]
    public string? RequestPath { get; set; }

    /// <summary>
    /// User agent string from the request headers
    /// </summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// IP address of the client that made the request
    /// </summary>
    [StringLength(45)] // IPv6 addresses can be up to 45 characters
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// Determines if this exception is a SecureException that should expose its message to clients
    /// </summary>
    [NotMapped]
    public bool IsSecureException => ExceptionType.Contains("SecureException", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a new ExceptionJournal entry from an exception and HTTP context
    /// </summary>
    /// <param name="exception">The exception to log</param>
    /// <param name="queryParameters">Serialized query parameters</param>
    /// <param name="bodyParameters">Serialized body parameters</param>
    /// <param name="httpMethod">HTTP method</param>
    /// <param name="requestPath">Request path</param>
    /// <param name="userAgent">User agent</param>
    /// <param name="clientIp">Client IP address</param>
    /// <returns>New ExceptionJournal instance</returns>
    public static ExceptionJournal Create(
        Exception exception,
        string? queryParameters = null,
        string? bodyParameters = null,
        string? httpMethod = null,
        string? requestPath = null,
        string? userAgent = null,
        string? clientIp = null)
    {
        return new ExceptionJournal
        {
            Timestamp = DateTime.UtcNow,
            QueryParameters = queryParameters,
            BodyParameters = bodyParameters,
            StackTrace = exception.StackTrace ?? string.Empty,
            ExceptionType = exception.GetType().FullName ?? "Unknown",
            ExceptionMessage = exception.Message,
            HttpMethod = httpMethod,
            RequestPath = requestPath,
            UserAgent = userAgent,
            ClientIpAddress = clientIp
        };
    }
}