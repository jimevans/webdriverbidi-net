// <copyright file="AstGroupNode.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

using System.Text.Json.Serialization;

/// <summary>
/// A named map/struct type, or a command/event definition.
/// Properties may contain plain entries or nested arrays (CDDL choice groups).
/// </summary>
public record AstGroupNode : AstNode
{
    /// <summary>
    /// Gets the list of properties defined in this group, either as plain field entries or nested choice groups.
    /// </summary>
    [JsonPropertyName("Properties")]
    [JsonConverter(typeof(PropertiesConverter))]
    public List<AstPropertyItem> Properties { get; init; } = [];
}
