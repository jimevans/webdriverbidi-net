namespace WebDriverBiDi.Cli.Infrastructure;

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Writes JSON results to stdout and errors to stderr.
/// All public methods are thread-safe (Console is synchronized internally).
/// </summary>
internal static class OutputWriter
{
    private static readonly JsonSerializerOptions PrettyOptions = new() { WriteIndented = true };

    internal static void WriteSuccess(bool pretty)
    {
        Console.WriteLine(Serialize(new JsonObject { ["ok"] = true }, pretty));
    }

    internal static void WriteSuccess(bool pretty, params (string Key, object? Value)[] fields)
    {
        JsonObject obj = new() { ["ok"] = true };
        foreach ((string key, object? value) in fields)
        {
            obj[key] = JsonValue.Create(value);
        }

        Console.WriteLine(Serialize(obj, pretty));
    }

    internal static void WriteSuccessNode(bool pretty, params (string Key, JsonNode? Node)[] fields)
    {
        JsonObject obj = new() { ["ok"] = true };
        foreach ((string key, JsonNode? node) in fields)
        {
            obj[key] = node;
        }

        Console.WriteLine(Serialize(obj, pretty));
    }

    internal static void WriteError(bool pretty, string message)
    {
        JsonObject obj = new()
        {
            ["ok"] = false,
            ["error"] = message,
        };
        Console.Error.WriteLine(Serialize(obj, pretty));
    }

    private static string Serialize(JsonObject obj, bool pretty) =>
        obj.ToJsonString(pretty ? PrettyOptions : null);
}
