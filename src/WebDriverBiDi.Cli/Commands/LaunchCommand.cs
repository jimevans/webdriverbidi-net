namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;
using WebDriverBiDi.Client.Launchers;

/// <summary>
/// Launches a browser and prints its WebSocket URL, then exits — leaving the browser running.
/// The caller is expected to pass the returned WebSocket URL as --ws to subsequent commands.
/// </summary>
internal sealed class LaunchCommand
{
    internal static async Task<int> RunAsync(CommandContext ctx)
    {
        if (ctx.Browser is null)
        {
            throw new ArgumentException("--browser <chrome|firefox> is required for the launch command");
        }

        if (ctx.BrowserPath is not null && ctx.SystemBrowser)
        {
            throw new ArgumentException("--browser-path and --system-browser are mutually exclusive");
        }

        BrowserLauncherBuilder builder = BrowserLauncher.Configure(ctx.Browser.Value);

        // Location strategy — default is auto-locate/download
        if (ctx.BrowserPath is not null)
        {
            builder.AtLocation(ctx.BrowserPath);
        }
        else if (ctx.SystemBrowser)
        {
            builder.AtDefaultInstallationLocation();
        }
        else
        {
            builder.AtAutomaticallyDownloadedLocation();
        }

        builder.WithReleaseChannel(ctx.Channel);

        if (ctx.Headless)
        {
            builder.WithHeadlessOption();
        }

        if (ctx.Port.HasValue)
        {
            builder.WithPort(ctx.Port.Value);
        }

        if (ctx.LaunchViaDriver)
        {
            builder.LaunchUsingDriver();
        }

        BrowserLauncher launcher = builder.Build();
        BrowserInstance instance = await launcher.LaunchAsync().ConfigureAwait(false);

        OutputWriter.WriteSuccess(
            ctx.Pretty,
            ("ws", instance.WebSocketUrl),
            ("pid", instance.ProcessId > 0 ? (object)instance.ProcessId : null));

        return 0;
    }
}
