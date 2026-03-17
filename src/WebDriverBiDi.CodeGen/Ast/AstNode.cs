// <copyright file="AstNode.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

using System.Text.Json.Serialization;

/// <summary>
/// Base class for all top-level definition nodes in the WebDriver BiDi AST.
/// Discriminated on the JSON "Type" field: "group" or "variable".
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(AstGroupNode), "group")]
[JsonDerivedType(typeof(AstVariableNode), "variable")]
[JsonDerivedType(typeof(AstArrayNode), "array")]
public abstract record AstNode
{
    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this node is an addition to an existing choice group.
    /// </summary>
    [JsonPropertyName("IsChoiceAddition")]
    public bool IsChoiceAddition { get; init; }
}
