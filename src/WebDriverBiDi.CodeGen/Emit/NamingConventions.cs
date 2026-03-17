// <copyright file="NamingConventions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Emit;

using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Naming convention helpers: CDDL names → C# identifiers, namespaces, and file paths.
/// </summary>
public static partial class NamingConventions
{
    // -----------------------------------------------------------------------
    // Module → namespace / subdirectory mapping
    // -----------------------------------------------------------------------
    private static readonly IReadOnlyDictionary<string, (string Namespace, string Subdir)> ModuleMap =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            ["session"] = ("WebDriverBiDi.Session", "Session"),
            ["browser"] = ("WebDriverBiDi.Browser", "Browser"),
            ["browsingContext"] = ("WebDriverBiDi.BrowsingContext", "BrowsingContext"),
            ["network"] = ("WebDriverBiDi.Network", "Network"),
            ["script"] = ("WebDriverBiDi.Script", "Script"),
            ["log"] = ("WebDriverBiDi.Log", "Log"),
            ["input"] = ("WebDriverBiDi.Input", "Input"),
            ["storage"] = ("WebDriverBiDi.Storage", "Storage"),
            ["emulation"] = ("WebDriverBiDi.Emulation", "Emulation"),
            ["bluetooth"] = ("WebDriverBiDi.Bluetooth", "Bluetooth"),
            ["webExtension"] = ("WebDriverBiDi.WebExtension", "WebExtension"),
            ["speculation"] = ("WebDriverBiDi.Speculation", "Speculation"),
        };

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the C# namespace for a CDDL name (e.g. "browser.Close" → "WebDriverBiDi.Browser").
    /// Unqualified names (no dot) map to the root "WebDriverBiDi" namespace.
    /// </summary>
    /// <param name="cddlName">The CDDL name (e.g. a command or event name).</param>
    /// <returns>The C# namespace to use for this name.</returns>
    public static string Namespace(string cddlName)
    {
        string? prefix = ModulePrefix(cddlName);
        return prefix is not null && ModuleMap.TryGetValue(prefix, out (string Namespace, string Subdir) entry)
            ? entry.Namespace
            : "WebDriverBiDi";
    }

    /// <summary>
    /// Returns the output subdirectory for a CDDL name (e.g. "browser.Close" → "Browser").
    /// Unqualified names map to an empty string (root output directory).
    /// </summary>
    /// <param name="cddlName">The CDDL name (e.g. a command or event name).</param>
    /// <returns>The output subdirectory for this name, or empty string for the root output.</returns>
    public static string Subdir(string cddlName)
    {
        string? prefix = ModulePrefix(cddlName);
        return prefix is not null && ModuleMap.TryGetValue(prefix, out (string Namespace, string Subdir) entry)
            ? entry.Subdir
            : string.Empty;
    }

    /// <summary>
    /// Returns the local (unqualified) part of a CDDL name.
    /// "browser.Close" → "Close"; "EmptyParams" → "EmptyParams".
    /// </summary>
    /// <param name="cddlName">The CDDL name.</param>
    /// <returns>The local (unqualified) part of the name.</returns>
    public static string LocalName(string cddlName)
    {
        int dot = cddlName.IndexOf('.');
        return dot >= 0 ? cddlName[(dot + 1)..] : cddlName;
    }

    /// <summary>
    /// Returns the C# class name for a command parameters class.
    /// "browser.Close" → "CloseCommandParameters".
    /// </summary>
    /// <param name="cddlCommandName">The CDDL command name.</param>
    /// <returns>The C# class name for the command parameters class.</returns>
    public static string CommandParamsClassName(string cddlCommandName)
        => LocalName(cddlCommandName) + "CommandParameters";

    /// <summary>
    /// Returns the C# class name for a command result class.
    /// The CDDL result name ends with "Result"; we strip that and append "CommandResult".
    /// "browser.CloseResult" → "CloseCommandResult".
    /// </summary>
    /// <param name="cddlResultName">The CDDL result name.</param>
    /// <returns>The C# class name for the command result class.</returns>
    public static string CommandResultClassName(string cddlResultName)
    {
        string local = LocalName(cddlResultName);
        if (local.EndsWith("Result", StringComparison.Ordinal))
        {
            local = local[..^6]; // strip "Result"
        }

        return local + "CommandResult";
    }

    /// <summary>
    /// Converts a CDDL JSON enum literal value into a PascalCase C# identifier.
    /// "portrait-primary"  → "PortraitPrimary"
    /// "dedicated-worker"  → "DedicatedWorker"
    /// "invalid argument"  → "InvalidArgument"
    /// "beforeRequestSent" → "BeforeRequestSent"
    /// "NaN"               → "Nan"
    /// "-0"                → "NegativeZero"
    /// "-Infinity"         → "NegativeInfinity"
    /// These must follow valid C# identifier rules.
    /// </summary>
    /// <param name="jsonValue">The JSON enum literal value.</param>
    /// <returns>The corresponding C# enum member name.</returns>
    public static string EnumMemberName(string jsonValue)
    {
        if (string.IsNullOrEmpty(jsonValue))
        {
            return "Unknown";
        }

        string value = jsonValue;

        // Handle leading minus — prepend "Negative"
        bool hasLeadingMinus = value.StartsWith('-');
        if (hasLeadingMinus)
        {
            value = value[1..];
        }

        // Split on word boundaries: hyphens, underscores, spaces, dots
        List<string> parts = WordSplitRegex().Split(value)
            .Where(p => p.Length > 0)
            .Select(p => char.ToUpperInvariant(p[0]) + p[1..])
            .ToList();

        // Combine parts
        string combined = string.Concat(parts);

        // If after stripping, we got nothing (was just "-"), use "Negative"
        if (combined.Length == 0)
        {
            combined = "Value";
        }

        // If starts with a digit (e.g. "0" after stripping "-"), use it as-is
        // (it will be prefixed below)
        if (hasLeadingMinus)
        {
            combined = "Negative" + combined;
        }

        // If still starts with a digit, prefix with underscore
        if (char.IsDigit(combined[0]))
        {
            combined = "_" + combined;
        }

        return combined;
    }

    /// <summary>
    /// Returns true when the C# member name's default JSON representation
    /// (ToLowerInvariant) does not match the original JSON literal value,
    /// meaning a [JsonEnumValue("...")] attribute is required.
    /// </summary>
    /// <param name="csharpMemberName">The C# enum member name.</param>
    /// <param name="jsonValue">The original JSON enum literal value.</param>
    /// <returns>True if a [JsonEnumValue("...")] attribute is required; otherwise false.</returns>
    public static bool NeedsJsonEnumValueAttr(string csharpMemberName, string jsonValue)
        => !string.Equals(csharpMemberName.ToLowerInvariant(), jsonValue, StringComparison.Ordinal);

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------
    private static string? ModulePrefix(string cddlName)
    {
        int dot = cddlName.IndexOf('.');
        return dot >= 0 ? cddlName[..dot] : null;
    }

    [GeneratedRegex(@"[-_\s.]+")]
    private static partial Regex WordSplitRegex();
}
