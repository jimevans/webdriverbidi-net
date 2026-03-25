// <copyright file="GeneratorConfig.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Config;

using System.Text.Json.Serialization;

/// <summary>
/// Optional code-generation configuration loaded from a JSON file via <c>--config</c>.
/// All sections are optional; omitted sections have no effect.
/// </summary>
public sealed class GeneratorConfig
{
    /// <summary>
    /// Gets CDDL names that are permanently excluded from all emission, even with <c>--force</c>.
    /// Use this for types that require hand-crafted custom JSON converters that the generator
    /// cannot reproduce from the CDDL alone (e.g. runtime-dispatch polymorphism).
    /// </summary>
    [JsonPropertyName("skipGeneration")]
    public IReadOnlyList<string> SkipGeneration { get; init; } = [];

    /// <summary>
    /// Gets type and property name remapping overrides.
    /// </summary>
    [JsonPropertyName("nameMap")]
    public NameMapConfig NameMap { get; init; } = new();
}
