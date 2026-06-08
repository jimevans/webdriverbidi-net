namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;
using WebDriverBiDi.Client.Elements;

internal sealed class GetAttrCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        if (ctx.AttributeName is null)
        {
            throw new ArgumentException("--name is required for the get-attr command");
        }

        ElementLocator locator = await LocatorParser.ResolveAsync(ctx, session).ConfigureAwait(false);
        string? value = await locator.GetAttributeAsync(ctx.AttributeName, timeout: ctx.Timeout).ConfigureAwait(false);
        OutputWriter.WriteSuccess(ctx.Pretty, ("value", value));
    }
}
