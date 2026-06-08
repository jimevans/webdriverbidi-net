namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;
using WebDriverBiDi.Client.Elements;

internal sealed class HoverCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        ElementLocator locator = await LocatorParser.ResolveAsync(ctx, session).ConfigureAwait(false);
        await locator.HoverAsync(timeout: ctx.Timeout).ConfigureAwait(false);
        OutputWriter.WriteSuccess(ctx.Pretty);
    }
}
