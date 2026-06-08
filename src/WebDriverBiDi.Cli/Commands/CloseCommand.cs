namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Browser;
using WebDriverBiDi.Cli.Infrastructure;

/// <summary>
/// Closes a browser that was started by the launch command.
/// Sends browser.close over the BiDi session, which terminates all contexts and the browser process.
/// </summary>
internal sealed class CloseCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        await session.Driver.Browser.CloseAsync(timeoutOverride: ctx.Timeout).ConfigureAwait(false);
        OutputWriter.WriteSuccess(ctx.Pretty);
    }
}
