namespace WebDriverBiDi.Cli.Infrastructure;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Elements;

/// <summary>
/// Builds a <see cref="Locator"/> from the mutually exclusive locator flags in a <see cref="CommandContext"/>.
/// Exactly one of --css, --xpath, or --text must be present when a locator is needed.
/// </summary>
internal static class LocatorParser
{
    internal static Locator Parse(CommandContext ctx)
    {
        int count = (ctx.CssSelector is not null ? 1 : 0)
                  + (ctx.XPath is not null ? 1 : 0)
                  + (ctx.InnerText is not null ? 1 : 0);

        if (count == 0)
        {
            throw new ArgumentException("A locator is required: specify one of --css, --xpath, or --text");
        }

        if (count > 1)
        {
            throw new ArgumentException("Only one locator flag may be specified at a time (--css, --xpath, --text)");
        }

        if (ctx.CssSelector is not null)
        {
            return new CssLocator(ctx.CssSelector);
        }

        if (ctx.XPath is not null)
        {
            return new XPathLocator(ctx.XPath);
        }

        // ctx.InnerText is not null here (count == 1 and the other two are null)
        return new InnerTextLocator(ctx.InnerText!);
    }

    /// <summary>
    /// Builds a <see cref="Locator"/> and resolves it to a single <see cref="ElementLocator"/>.
    /// When <see cref="CommandContext.FrameSelector"/> is set, the locator is scoped to the
    /// child browsing context of the matched frame element rather than the top-level context.
    /// When <see cref="CommandContext.NthIndex"/> is set, the nth match is selected.
    /// </summary>
    internal static async Task<ElementLocator> ResolveAsync(CommandContext ctx, BrowserSession session)
    {
        Locator locator = Parse(ctx);

        // If --frame is given, resolve the frame's browsing context ID and construct
        // an ElementLocator scoped to that child context instead of the top-level page.
        ElementLocator elementLocator;
        if (ctx.FrameSelector is not null)
        {
            string frameContextId = await FrameResolver
                .ResolveAsync(session.Driver, session.ContextId, ctx.FrameSelector, ctx.Timeout)
                .ConfigureAwait(false);

            elementLocator = new ElementLocator(session.Driver, frameContextId, locator);
        }
        else
        {
            elementLocator = session.Page.LocateElement(locator);
        }

        if (ctx.NthIndex.HasValue)
        {
            elementLocator = await elementLocator.NthAsync(ctx.NthIndex.Value, ctx.Timeout).ConfigureAwait(false);
        }

        return elementLocator;
    }
}
