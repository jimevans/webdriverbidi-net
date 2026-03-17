// <copyright file="AstArrayNode.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

using System.Text.Json.Serialization;

/// <summary>
/// A named array alias at the top level, e.g. browsingContext.InfoList = [* browsingContext.Info].
/// </summary>
public record AstArrayNode : AstNode
{
    /// <summary>
    /// Gets the list of property entries that define the element type of this array alias.
    /// </summary>
    [JsonPropertyName("Values")]
    public List<AstPropertyEntry> Values { get; init; } = [];
}
