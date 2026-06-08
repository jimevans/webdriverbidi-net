namespace WebDriverBiDi.Cli.Commands;

using System.Text.Json.Nodes;
using WebDriverBiDi.Browser;
using WebDriverBiDi.Cli.Infrastructure;

internal sealed class ListUserContextsCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        GetUserContextsCommandResult result = await session.Driver.Browser
            .GetUserContextsAsync(timeoutOverride: ctx.Timeout)
            .ConfigureAwait(false);

        JsonArray userContexts = new(
            result.UserContexts
                .Select(uc => (JsonNode)new JsonObject { ["userContextId"] = uc.UserContextId })
                .ToArray());

        OutputWriter.WriteSuccessNode(ctx.Pretty, ("userContexts", userContexts));
    }
}
