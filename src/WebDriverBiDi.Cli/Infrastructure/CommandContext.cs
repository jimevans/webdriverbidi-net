namespace WebDriverBiDi.Cli.Infrastructure;

using WebDriverBiDi.Client.Launchers;

/// <summary>
/// Holds all parsed flag values for a single CLI invocation.
/// Parsing is intentionally simple: flags are --key value pairs; boolean flags are bare.
/// </summary>
internal sealed class CommandContext
{
    // Connection
    public string? WebSocketUrl { get; private set; }
    public BrowserKind? Browser { get; private set; }
    public bool Headless { get; private set; }

    // Launch-specific
    public BrowserReleaseChannel Channel { get; private set; } = BrowserReleaseChannel.Stable;
    public string? BrowserPath { get; private set; }
    public bool SystemBrowser { get; private set; }
    public int? Port { get; private set; }
    public bool LaunchViaDriver { get; private set; }

    // Targeting
    public string? ContextId { get; private set; }
    public string? FrameSelector { get; private set; }
    public string? UserContextId { get; private set; }

    // Locator (at most one may be set)
    public string? CssSelector { get; private set; }
    public string? XPath { get; private set; }
    public string? InnerText { get; private set; }
    public int? NthIndex { get; private set; }

    // Verb-specific
    public string? Url { get; private set; }
    public string? Value { get; private set; }
    public string? AttributeName { get; private set; }
    public string? Script { get; private set; }
    public string? OutputPath { get; private set; }

    // Select options
    public string? OptionValue { get; private set; }
    public string? OptionText { get; private set; }
    public int? OptionIndex { get; private set; }

    // Output
    public bool Pretty { get; private set; }

    // Timeout
    public TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(30);

    // Remaining positional args after parsing (currently unused but available for future use)
    public IReadOnlyList<string> Positional { get; private set; } = [];

    public static CommandContext Parse(string[] args)
    {
        CommandContext ctx = new();
        List<string> positional = [];
        int i = 0;

        while (i < args.Length)
        {
            string arg = args[i];

            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                positional.Add(arg);
                i++;
                continue;
            }

            string flag = arg[2..];
            string? next = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal)
                ? args[i + 1]
                : null;

            switch (flag)
            {
                case "ws":
                    ctx.WebSocketUrl = RequireValue(flag, next);
                    i += 2;
                    break;
                case "browser":
                    string browserName = RequireValue(flag, next);
                    ctx.Browser = browserName.ToLowerInvariant() switch
                    {
                        "chrome" => BrowserKind.Chrome,
                        "firefox" => BrowserKind.Firefox,
                        _ => throw new ArgumentException($"Unknown browser '{browserName}'. Supported: chrome, firefox"),
                    };
                    i += 2;
                    break;
                case "headless":
                    ctx.Headless = true;
                    i++;
                    break;
                case "context":
                    ctx.ContextId = RequireValue(flag, next);
                    i += 2;
                    break;
                case "frame":
                    ctx.FrameSelector = RequireValue(flag, next);
                    i += 2;
                    break;
                case "user-context":
                    ctx.UserContextId = RequireValue(flag, next);
                    i += 2;
                    break;
                case "css":
                    ctx.CssSelector = RequireValue(flag, next);
                    i += 2;
                    break;
                case "xpath":
                    ctx.XPath = RequireValue(flag, next);
                    i += 2;
                    break;
                case "text":
                    ctx.InnerText = RequireValue(flag, next);
                    i += 2;
                    break;
                case "nth":
                    ctx.NthIndex = int.Parse(RequireValue(flag, next));
                    i += 2;
                    break;
                case "url":
                    ctx.Url = RequireValue(flag, next);
                    i += 2;
                    break;
                case "value":
                    ctx.Value = RequireValue(flag, next);
                    i += 2;
                    break;
                case "name":
                    ctx.AttributeName = RequireValue(flag, next);
                    i += 2;
                    break;
                case "script":
                    ctx.Script = RequireValue(flag, next);
                    i += 2;
                    break;
                case "output":
                    ctx.OutputPath = RequireValue(flag, next);
                    i += 2;
                    break;
                case "option-value":
                    ctx.OptionValue = RequireValue(flag, next);
                    i += 2;
                    break;
                case "option-text":
                    ctx.OptionText = RequireValue(flag, next);
                    i += 2;
                    break;
                case "option-index":
                    ctx.OptionIndex = int.Parse(RequireValue(flag, next));
                    i += 2;
                    break;
                case "channel":
                    string channelName = RequireValue(flag, next);
                    ctx.Channel = channelName.ToLowerInvariant() switch
                    {
                        "stable" => BrowserReleaseChannel.Stable,
                        "beta" => BrowserReleaseChannel.Beta,
                        "dev" or "developer-preview" => BrowserReleaseChannel.DeveloperPreview,
                        "alpha" or "canary" or "nightly" => BrowserReleaseChannel.Alpha,
                        _ => throw new ArgumentException($"Unknown channel '{channelName}'. Supported: stable, beta, dev, alpha"),
                    };
                    i += 2;
                    break;
                case "browser-path":
                    ctx.BrowserPath = RequireValue(flag, next);
                    i += 2;
                    break;
                case "system-browser":
                    ctx.SystemBrowser = true;
                    i++;
                    break;
                case "port":
                    ctx.Port = int.Parse(RequireValue(flag, next));
                    i += 2;
                    break;
                case "launch-via":
                    string launchVia = RequireValue(flag, next);
                    if (launchVia.ToLowerInvariant() != "driver")
                    {
                        throw new ArgumentException($"Unknown --launch-via value '{launchVia}'. Supported: driver");
                    }

                    ctx.LaunchViaDriver = true;
                    i += 2;
                    break;
                case "pretty":
                    ctx.Pretty = true;
                    i++;
                    break;
                case "timeout":
                    ctx.Timeout = TimeSpan.FromSeconds(double.Parse(RequireValue(flag, next)));
                    i += 2;
                    break;
                default:
                    // Unknown flags are silently ignored to allow forward-compatible invocations.
                    i++;
                    break;
            }
        }

        ctx.Positional = positional.AsReadOnly();
        return ctx;
    }

    private static string RequireValue(string flag, string? value) =>
        value ?? throw new ArgumentException($"Flag --{flag} requires a value");
}
