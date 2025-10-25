using System;

namespace Yuuki.Exceptions;

/// <summary>
/// Base exception class for all Yuuki-specific exceptions
/// </summary>
public abstract class YuukiException : Exception
{
    /// <summary>
    /// Error code for categorizing the exception
    /// </summary>
    public string ErrorCode { get; }

    protected YuukiException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected YuukiException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
