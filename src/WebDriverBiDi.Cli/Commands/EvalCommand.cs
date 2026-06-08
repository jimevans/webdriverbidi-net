namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Cli.Infrastructure;
using WebDriverBiDi.Script;

internal sealed class EvalCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        if (ctx.Script is null)
        {
            throw new ArgumentException("--script is required for the eval command");
        }

        EvaluateCommandParameters parameters = new(
            ctx.Script,
            new ContextTarget(session.ContextId),
            awaitPromise: true);

        EvaluateResult result = await session.Driver.Script.EvaluateAsync(parameters, ctx.Timeout).ConfigureAwait(false);

        if (result is EvaluateResultSuccess success)
        {
            OutputWriter.WriteSuccess(ctx.Pretty, ("result", FormatRemoteValue(success.Result)));
        }
        else if (result is EvaluateResultException exception)
        {
            throw new InvalidOperationException($"Script exception: {exception.ExceptionDetails.Text}");
        }
        else
        {
            OutputWriter.WriteSuccess(ctx.Pretty, ("result", null));
        }
    }

    private static string FormatRemoteValue(RemoteValue value) => value switch
    {
        StringRemoteValue s => s.Value,
        NumberRemoteValue n => n.Value.ToString(),
        BooleanRemoteValue b => b.Value.ToString().ToLowerInvariant(),
        NullRemoteValue => "null",
        UndefinedRemoteValue => "undefined",
        _ => value.Type.ToString(),
    };
}
