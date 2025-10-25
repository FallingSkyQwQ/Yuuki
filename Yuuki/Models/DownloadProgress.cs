using System;

namespace Yuuki.Models;

/// <summary>
/// Download progress information
/// </summary>
public class DownloadProgress
{
    /// <summary>
    /// Total bytes to download
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Bytes downloaded so far
    /// </summary>
    public long DownloadedBytes { get; set; }

    /// <summary>
    /// Download percentage (0-100)
    /// </summary>
    public double Percentage => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes * 100 : 0;

    /// <summary>
    /// Current download speed in bytes/second
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// Current file being downloaded
    /// </summary>
    public string? CurrentFile { get; set; }

    /// <summary>
    /// Current operation description
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Whether the download is complete
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Whether the download failed
    /// </summary>
    public bool IsFailed { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of a download operation
/// </summary>
public class DownloadResult
{
    /// <summary>
    /// Whether the download was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Downloaded file path
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Total bytes downloaded
    /// </summary>
    public long BytesDownloaded { get; set; }

    /// <summary>
    /// Creates a successful download result
    /// </summary>
    public static DownloadResult Successful(string filePath, long bytesDownloaded)
    {
        return new DownloadResult
        {
            Success = true,
            FilePath = filePath,
            BytesDownloaded = bytesDownloaded
        };
    }

    /// <summary>
    /// Creates a failed download result
    /// </summary>
    public static DownloadResult Failed(string errorMessage)
    {
        return new DownloadResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
