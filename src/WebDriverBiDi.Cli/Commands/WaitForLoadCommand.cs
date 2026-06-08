namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Cli.Infrastructure;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Waits for the next browsingContext.load event on the current context.
/// Useful after triggering navigation via a click or form submit.
///
/// To avoid a race condition where the page finishes loading before this command
/// is called, we subscribe to the event first, then check document.readyState.
/// If the page is already complete, we return immediately without waiting for
/// an event that has already fired.
/// </summary>
internal sealed class WaitForLoadCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        TaskCompletionSource<string> loadTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // Subscribe to the event before checking readyState so that any load
        // event fired after this point will be captured.
        SubscribeCommandParameters subscribe = new([session.Driver.BrowsingContext.OnLoad.EventName]);
        await session.Driver.Session.SubscribeAsync(subscribe, ctx.Timeout).ConfigureAwait(false);

        await using EventObserver<NavigationEventArgs> observer = session.Driver.BrowsingContext.OnLoad.AddObserver(e =>
        {
            if (e.BrowsingContextId == session.ContextId)
            {
                loadTaskCompletionSource.TrySetResult(e.Url);
            }

            return Task.CompletedTask;
        });

        // Now that the subscription is live, check whether the page has already
        // finished loading. If so, resolve immediately — no event is coming.
        EvaluateCommandParameters readyStateParams = new(
            "document.readyState",
            new ContextTarget(session.ContextId),
            awaitPromise: false);

        EvaluateResult readyStateResult = await session.Driver.Script
            .EvaluateAsync(readyStateParams, ctx.Timeout)
            .ConfigureAwait(false);

        if (readyStateResult is EvaluateResultSuccess { Result: StringRemoteValue { Value: "complete" } })
        {
            // Page is already loaded. Retrieve the current URL from the context tree.
            GetTreeCommandParameters treeParams = new() { MaxDepth = 1 };
            GetTreeCommandResult tree = await session.Driver.BrowsingContext
                .GetTreeAsync(treeParams, ctx.Timeout)
                .ConfigureAwait(false);

            string currentUrl = tree.ContextTree
                .FirstOrDefault(c => c.BrowsingContextId == session.ContextId)
                ?.Url ?? string.Empty;

            OutputWriter.WriteSuccess(ctx.Pretty, ("url", currentUrl));
            return;
        }

        // Page is not yet loaded — wait for the event that will arrive.
        using CancellationTokenSource cts = new(ctx.Timeout);
        cts.Token.Register(() => loadTaskCompletionSource.TrySetCanceled());

        string url = await loadTaskCompletionSource.Task.ConfigureAwait(false);
        OutputWriter.WriteSuccess(ctx.Pretty, ("url", url));
    }
}
