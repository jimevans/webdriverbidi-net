// <copyright file="NameMapConfig.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Config;

using System.Text.Json.Serialization;

/// <summary>
/// Type and property name remapping configuration.
/// </summary>
public sealed class NameMapConfig
{
    /// <summary>
    /// Gets overrides for generated C# class names, keyed by CDDL type name.
    /// The value replaces the entire C# class name (including any suffix such as
    /// "CommandResult" or "EventArgs" that the generator would normally append).
    /// Example: <c>"browsingContext.Info" → "BrowsingContextInfo"</c>.
    /// </summary>
    [JsonPropertyName("types")]
    public IReadOnlyDictionary<string, string> Types { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets overrides for generated C# property names, keyed by CDDL type name,
    /// then by the JSON property name within that type.
    /// Example: <c>"browsingContext.Info" → { "context" → "BrowsingContextId" }</c>.
    /// </summary>
    [JsonPropertyName("properties")]
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Properties { get; init; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
}
