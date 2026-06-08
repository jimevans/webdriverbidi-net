namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;
using WebDriverBiDi.Client.Elements;

internal sealed class GetTextCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        ElementLocator locator = await LocatorParser.ResolveAsync(ctx, session).ConfigureAwait(false);
        string text = await locator.GetTextAsync(timeout: ctx.Timeout).ConfigureAwait(false);
        OutputWriter.WriteSuccess(ctx.Pretty, ("text", text));
    }
}
