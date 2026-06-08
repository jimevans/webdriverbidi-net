namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;
using WebDriverBiDi.Client.Elements;

internal sealed class SelectCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        int strategies = (ctx.OptionValue is not null ? 1 : 0)
                       + (ctx.OptionText is not null ? 1 : 0)
                       + (ctx.OptionIndex.HasValue ? 1 : 0);

        if (strategies == 0)
        {
            throw new ArgumentException("One of --option-value, --option-text, or --option-index is required for the select command");
        }

        if (strategies > 1)
        {
            throw new ArgumentException("Only one of --option-value, --option-text, or --option-index may be specified");
        }

        ElementLocator locator = await LocatorParser.ResolveAsync(ctx, session).ConfigureAwait(false);

        if (ctx.OptionValue is not null)
        {
            await locator.SelectOptionAsync(ctx.OptionValue, timeout: ctx.Timeout).ConfigureAwait(false);
        }
        else if (ctx.OptionText is not null)
        {
            await locator.SelectOptionByTextAsync(ctx.OptionText, timeout: ctx.Timeout).ConfigureAwait(false);
        }
        else
        {
            await locator.SelectOptionByIndexAsync(ctx.OptionIndex!.Value, timeout: ctx.Timeout).ConfigureAwait(false);
        }

        OutputWriter.WriteSuccess(ctx.Pretty);
    }
}
