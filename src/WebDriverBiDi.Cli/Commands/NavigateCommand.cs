namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;

internal sealed class NavigateCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        if (ctx.Url is null)
        {
            throw new ArgumentException("--url is required for the navigate command");
        }

        string finalUrl = await session.Page.NavigateAsync(ctx.Url, timeout: ctx.Timeout).ConfigureAwait(false);
        OutputWriter.WriteSuccess(ctx.Pretty, ("url", finalUrl));
    }
}
