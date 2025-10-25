using System;

namespace Yuuki.Exceptions;

/// <summary>
/// Exception thrown when game launch fails
/// </summary>
public class LaunchException : YuukiException
{
    public LaunchException(string message)
        : base("LAUNCH_ERROR", message)
    {
    }

    public LaunchException(string errorCode, string message)
        : base(errorCode, message)
    {
    }

    public LaunchException(string message, Exception innerException)
        : base("LAUNCH_ERROR", message, innerException)
    {
    }
}
