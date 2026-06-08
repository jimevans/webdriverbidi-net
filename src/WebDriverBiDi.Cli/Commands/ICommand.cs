namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;

/// <summary>
/// A single automatable browser action.
/// Implementations are stateless; all inputs arrive via <see cref="CommandContext"/>
/// and <see cref="BrowserSession"/>.
/// </summary>
internal interface ICommand
{
    Task ExecuteAsync(CommandContext ctx, BrowserSession session);
}
