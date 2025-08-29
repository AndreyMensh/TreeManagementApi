namespace TreeManagementApi.Application.Exceptions;

/// <summary>
/// Base class for exceptions that are safe to expose to API clients.
/// Unlike generic exceptions, SecureException messages can be returned to the client
/// because they don't contain sensitive internal information.
/// </summary>
public class SecureException : Exception
{
    /// <summary>
    /// Initializes a new instance of the SecureException class.
    /// </summary>
    public SecureException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the SecureException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SecureException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the SecureException class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SecureException(string message, Exception innerException) : base(message, innerException)
    {
    }
}