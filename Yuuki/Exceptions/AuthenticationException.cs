using System;

namespace Yuuki.Exceptions;

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class AuthenticationException : YuukiException
{
    public AuthenticationException(string message)
        : base("AUTH_ERROR", message)
    {
    }

    public AuthenticationException(string message, Exception innerException)
        : base("AUTH_ERROR", message, innerException)
    {
    }
}
