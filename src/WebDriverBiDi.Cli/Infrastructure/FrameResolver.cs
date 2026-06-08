namespace WebDriverBiDi.Cli.Infrastructure;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// Resolves a CSS selector matching an &lt;iframe&gt; element to its child browsing context ID
/// by evaluating <c>element.contentWindow</c> in the parent context, which returns a
/// <see cref="WindowProxyRemoteValue"/> whose <c>Value.Context</c> is the frame's context ID.
/// </summary>
internal static class FrameResolver
{
    /// <summary>
    /// Returns the browsing context ID of the frame matched by <paramref name="cssSelector"/>
    /// within <paramref name="parentContextId"/>.
    /// </summary>
    internal static async Task<string> ResolveAsync(
        BiDiDriver driver,
        string parentContextId,
        string cssSelector,
        TimeSpan timeout)
    {
        // Locate the iframe element node first so we have a SharedReference to pass as `this`.
        LocateNodesCommandParameters locateParams = new(parentContextId, new CssLocator(cssSelector))
        {
            MaxNodeCount = 1,
        };
        LocateNodesCommandResult locateResult = await driver.BrowsingContext
            .LocateNodesAsync(locateParams, timeout)
            .ConfigureAwait(false);

        if (locateResult.Nodes.Count == 0)
        {
            throw new InvalidOperationException($"No frame element matched --frame selector '{cssSelector}'");
        }

        NodeRemoteValue frameNode = locateResult.Nodes[0];
        SharedReference frameRef = frameNode.ToSharedReference();

        // Call `function() { return this.contentWindow; }` with the iframe element as `this`.
        // The result is a WindowProxyRemoteValue whose Value.Context is the child context ID.
        CallFunctionCommandParameters callParams = new(
            "function() { return this.contentWindow; }",
            new ContextTarget(parentContextId),
            awaitPromise: false)
        {
            ThisObject = frameRef,
        };

        EvaluateResult evalResult = await driver.Script
            .CallFunctionAsync(callParams, timeout)
            .ConfigureAwait(false);

        if (evalResult is EvaluateResultSuccess { Result: WindowProxyRemoteValue windowProxy })
        {
            return windowProxy.Value.Context;
        }

        if (evalResult is EvaluateResultException exception)
        {
            throw new InvalidOperationException(
                $"Script exception resolving frame '{cssSelector}': {exception.ExceptionDetails.Text}");
        }

        throw new InvalidOperationException(
            $"Could not resolve frame context for selector '{cssSelector}': " +
            $"expected a WindowProxy but got {evalResult.GetType().Name}");
    }
}
