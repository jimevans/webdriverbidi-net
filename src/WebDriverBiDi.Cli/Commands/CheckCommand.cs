namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;
using WebDriverBiDi.Client.Elements;

internal sealed class CheckCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        ElementLocator locator = await LocatorParser.ResolveAsync(ctx, session).ConfigureAwait(false);
        await locator.CheckAsync(timeout: ctx.Timeout).ConfigureAwait(false);
        OutputWriter.WriteSuccess(ctx.Pretty);
    }
}
