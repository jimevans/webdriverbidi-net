namespace WebDriverBiDi.Cli.Infrastructure;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Session;

/// <summary>
/// Owns an open browser connection and a resolved top-level <see cref="Page"/>.
/// Callers acquire a session via <see cref="OpenAsync"/> and must await <see cref="DisposeAsync"/>.
///
/// Extension point for future modes: add an <see cref="ISessionSource"/> implementation (e.g. a
/// persistent WebSocket server) and a corresponding factory method here; command implementations
/// never need to change because they only consume <see cref="Page"/> and <see cref="Driver"/>.
/// </summary>
internal sealed class BrowserSession : IAsyncDisposable
{
    // Exactly one of these is non-null; the other is null. Both are disposed in DisposeAsync.
    private readonly Browser? ownedBrowser;
    private readonly BiDiDriver? ownedDriver;

    private BrowserSession(Browser ownedBrowser, string contextId)
    {
        this.ownedBrowser = ownedBrowser;
        this.Page = ownedBrowser.GetPage();
        this.Driver = ownedBrowser.Driver;
        this.ContextId = contextId;
    }

    private BrowserSession(BiDiDriver ownedDriver, Page page, string contextId)
    {
        this.ownedDriver = ownedDriver;
        this.Page = page;
        this.Driver = ownedDriver;
        this.ContextId = contextId;
    }

    public Page Page { get; }
    public BiDiDriver Driver { get; }
    public string ContextId { get; }

    /// <summary>
    /// Opens a browser session according to the connection flags in <paramref name="ctx"/>.
    /// Exactly one of --ws or --browser must be specified.
    /// </summary>
    public static Task<BrowserSession> OpenAsync(CommandContext ctx) =>
        ctx switch
        {
            { WebSocketUrl: not null } => FromWebSocketAsync(ctx),
            { Browser: not null } => FromLauncherAsync(ctx),
            _ => throw new ArgumentException("Specify a connection: --ws <url> or --browser <chrome|firefox>"),
        };

    public async ValueTask DisposeAsync()
    {
        if (this.ownedBrowser is not null)
        {
            await this.ownedBrowser.DisposeAsync().ConfigureAwait(false);
        }

        if (this.ownedDriver is not null)
        {
            await this.ownedDriver.DisposeAsync().ConfigureAwait(false);
        }
    }

    // --ws path: connect to a running browser; the caller owns the process lifetime.
    private static async Task<BrowserSession> FromWebSocketAsync(CommandContext ctx)
    {
        BiDiDriver driver = new(ctx.Timeout);
        try
        {
            await driver.StartAsync(ctx.WebSocketUrl!).ConfigureAwait(false);

            string contextId;

            if (ctx.ContextId is not null)
            {
                // Caller pinned a specific context — use it directly.
                contextId = ctx.ContextId;
            }
            else if (ctx.UserContextId is not null)
            {
                // Open a new tab inside the requested user context.
                CreateCommandParameters createParams = new(CreateType.Tab)
                {
                    UserContextId = ctx.UserContextId,
                };
                CreateCommandResult created = await driver.BrowsingContext
                    .CreateAsync(createParams)
                    .ConfigureAwait(false);
                contextId = created.BrowsingContextId;
            }
            else
            {
                // Default: use the first existing top-level context.
                GetTreeCommandParameters treeParams = new() { MaxDepth = 1 };
                GetTreeCommandResult tree = await driver.BrowsingContext
                    .GetTreeAsync(treeParams)
                    .ConfigureAwait(false);
                if (tree.ContextTree.Count == 0)
                {
                    throw new InvalidOperationException("No top-level browsing context found in the connected browser.");
                }

                contextId = tree.ContextTree[0].BrowsingContextId;
            }

            Page page = new(driver, contextId);
            return new BrowserSession(driver, page, contextId);
        }
        catch
        {
            await driver.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    // --browser path: launch a fresh browser; we own its lifetime and will close it on dispose.
    private static async Task<BrowserSession> FromLauncherAsync(CommandContext ctx)
    {
        Browser browser = await Browser.LaunchAsync(ctx.Browser!.Value, ctx.Headless).ConfigureAwait(false);
        try
        {
            Page page = browser.GetPage();
            return new BrowserSession(browser, page.BrowsingContextId);
        }
        catch
        {
            await browser.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
