namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Browser;
using WebDriverBiDi.Cli.Infrastructure;

internal sealed class RemoveUserContextCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        if (ctx.UserContextId is null)
        {
            throw new ArgumentException("--user-context <id> is required for the remove-user-context command");
        }

        RemoveUserContextCommandParameters parameters = new(ctx.UserContextId);
        await session.Driver.Browser
            .RemoveUserContextAsync(parameters, timeoutOverride: ctx.Timeout)
            .ConfigureAwait(false);

        OutputWriter.WriteSuccess(ctx.Pretty);
    }
}
