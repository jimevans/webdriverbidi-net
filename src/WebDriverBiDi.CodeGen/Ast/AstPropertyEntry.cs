// <copyright file="AstPropertyEntry.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

using System.Text.Json.Serialization;

/// <summary>
/// A single named or anonymous field within a group.
/// Name is empty for embedded/anonymous group references.
/// </summary>
public record AstPropertyEntry : AstPropertyItem
{
    /// <summary>
    /// Gets a value indicating whether this field is marked with the CDDL cut operator ("/").
    /// </summary>
    [JsonPropertyName("HasCut")]
    public bool HasCut { get; init; }

    /// <summary>
    /// Gets the occurrence bounds for this field. Optional when n == 0.
    /// </summary>
    [JsonPropertyName("Occurrence")]
    public AstPropertyOccurrence Occurrence { get; init; } = new(1, 1);

    /// <summary>
    /// Gets the JSON field name. Empty string for anonymous embedded group references.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the type of this field. Can be one or more type expressions.
    /// Multiple entries indicate a union (e.g. a type-or-null pair).
    /// </summary>
    [JsonPropertyName("Type")]
    [JsonConverter(typeof(TypeExprListConverter))]
    public List<AstTypeExpr> Type { get; init; } = [];
}
