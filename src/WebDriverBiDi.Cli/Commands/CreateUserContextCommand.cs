namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Browser;
using WebDriverBiDi.Cli.Infrastructure;

internal sealed class CreateUserContextCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        CreateUserContextCommandResult result = await session.Driver.Browser
            .CreateUserContextAsync(timeoutOverride: ctx.Timeout)
            .ConfigureAwait(false);

        OutputWriter.WriteSuccess(ctx.Pretty, ("userContextId", result.UserContextId));
    }
}
