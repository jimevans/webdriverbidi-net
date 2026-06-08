using WebDriverBiDi.Cli.Commands;
using WebDriverBiDi.Cli.Infrastructure;

// argv[0] is the verb; the rest are flag pairs.
if (args.Length == 0 || args[0] is "--help" or "-h")
{
    PrintHelp();
    return 0;
}

string verb = args[0];
string[] rest = args[1..];

CommandContext ctx;
try
{
    ctx = CommandContext.Parse(rest);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"{{\"ok\":false,\"error\":{System.Text.Json.JsonSerializer.Serialize(ex.Message)}}}");
    return 2;
}

// launch is session-free: it starts a browser and exits, leaving it running.
if (verb == "launch")
{
    try
    {
        return await LaunchCommand.RunAsync(ctx).ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
        OutputWriter.WriteError(ctx.Pretty, $"launch timed out after {ctx.Timeout.TotalSeconds}s");
        return 1;
    }
    catch (Exception ex)
    {
        OutputWriter.WriteError(ctx.Pretty, ex.Message);
        return 1;
    }
}

ICommand? command = verb switch
{
    "close"                  => new CloseCommand(),
    "navigate"               => new NavigateCommand(),
    "click"                  => new ClickCommand(),
    "type"                   => new TypeCommand(),
    "clear"                  => new ClearCommand(),
    "get-text"               => new GetTextCommand(),
    "get-attr"               => new GetAttrCommand(),
    "check"                  => new CheckCommand(),
    "uncheck"                => new UncheckCommand(),
    "select"                 => new SelectCommand(),
    "screenshot"             => new ScreenshotCommand(),
    "eval"                   => new EvalCommand(),
    "hover"                  => new HoverCommand(),
    "wait-for-load"          => new WaitForLoadCommand(),
    "create-user-context"    => new CreateUserContextCommand(),
    "list-user-contexts"     => new ListUserContextsCommand(),
    "remove-user-context"    => new RemoveUserContextCommand(),
    _                        => null,
};

if (command is null)
{
    OutputWriter.WriteError(ctx.Pretty, $"Unknown command '{verb}'. Run with --help for usage.");
    return 2;
}

try
{
    await using BrowserSession session = await BrowserSession.OpenAsync(ctx).ConfigureAwait(false);
    await command.ExecuteAsync(ctx, session).ConfigureAwait(false);
    return 0;
}
catch (OperationCanceledException)
{
    OutputWriter.WriteError(ctx.Pretty, $"Command '{verb}' timed out after {ctx.Timeout.TotalSeconds}s");
    return 1;
}
catch (Exception ex)
{
    OutputWriter.WriteError(ctx.Pretty, ex.Message);
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("""
        bidi-cli <verb> [options]

        Verbs:
          launch        Launch a browser and print its WebSocket URL
          close         Close a browser started by launch        --ws <url>
          navigate      Navigate to a URL                        --url <url>
          click         Click an element
          type          Type text into an element                --value <text>
          clear         Clear an input element
          get-text      Get visible text of an element
          get-attr      Get an attribute value                   --name <attr>
          check         Check a checkbox or radio
          uncheck       Uncheck a checkbox
          select        Select a <select> option                 --option-value|--option-text|--option-index
          screenshot    Capture a screenshot                     [--output <path>]
          eval          Evaluate JavaScript                      --script <js>
          hover         Move the mouse over an element
          wait-for-load        Wait for the next load event
          create-user-context  Create an isolated browser profile (user context)
          list-user-contexts   List all user contexts
          remove-user-context  Remove a user context           --user-context <id>

        Connection (required, mutually exclusive):
          --ws <url>              Connect to a running browser WebSocket
          --browser <chrome|firefox>  Launch a new browser (closed after command, or use 'launch' verb)
          --headless              Used with --browser

        Launch options (only for the 'launch' verb):
          --channel <stable|beta|dev|alpha>  Browser release channel (default: stable)
          --browser-path <path>   Use a specific browser executable
          --system-browser        Use the system-installed browser
          --port <n>              Port for the browser debug endpoint (default: random)
          --launch-via driver     Launch via a WebDriver Classic driver executable

        Locator (required for element commands, mutually exclusive):
          --css <selector>        CSS selector
          --xpath <expression>    XPath expression
          --text <string>         Inner-text match

        Common options:
          --nth <n>               Zero-based index when multiple elements match
          --frame <css-selector>  Target an element inside a frame matching this selector
          --context <id>          Browsing context ID (default: first in tree)
          --user-context <id>     Open a new tab in this user context (used with --ws)
          --timeout <seconds>     Element/command timeout (default: 30)
          --pretty                Pretty-print JSON output

        Output: JSON on stdout. Exit 0 = success, 1 = error, 2 = usage error.
        """);
}
