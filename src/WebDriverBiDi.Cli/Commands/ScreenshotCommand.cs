namespace WebDriverBiDi.Cli.Commands;

using WebDriverBiDi.Cli.Infrastructure;

internal sealed class ScreenshotCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext ctx, BrowserSession session)
    {
        byte[] pngBytes = await session.Page.GetScreenshotAsync(timeout: ctx.Timeout).ConfigureAwait(false);

        if (ctx.OutputPath is not null)
        {
            await File.WriteAllBytesAsync(ctx.OutputPath, pngBytes).ConfigureAwait(false);
            OutputWriter.WriteSuccess(ctx.Pretty, ("path", ctx.OutputPath));
        }
        else
        {
            string base64 = Convert.ToBase64String(pngBytes);
            OutputWriter.WriteSuccess(ctx.Pretty, ("data", base64));
        }
    }
}
