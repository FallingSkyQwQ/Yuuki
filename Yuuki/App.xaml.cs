using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Yuuki.Services;

namespace Yuuki;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private readonly ILogger<App> _logger;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        // Initialize dependency injection container
        ServiceProvider.Initialize();

        // Get logger instance
        _logger = ServiceProvider.GetRequiredService<ILogger<App>>();
        _logger.LogInformation("Yuuki application starting...");

        InitializeComponent();

        // Set up unhandled exception handlers
        UnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Unhandled exception occurred");
        e.Handled = true;
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _logger.LogInformation("Application launched");
        _window = new MainWindow();
        _window.Activate();
    }
}
