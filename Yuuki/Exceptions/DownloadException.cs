using System;

namespace Yuuki.Exceptions;

/// <summary>
/// Exception thrown when file download fails
/// </summary>
public class DownloadException : YuukiException
{
    public DownloadException(string message)
        : base("DOWNLOAD_ERROR", message)
    {
    }

    public DownloadException(string message, Exception innerException)
        : base("DOWNLOAD_ERROR", message, innerException)
    {
    }
}
