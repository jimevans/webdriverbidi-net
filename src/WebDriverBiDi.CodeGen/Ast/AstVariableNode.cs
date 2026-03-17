// <copyright file="AstVariableNode.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

using System.Text.Json.Serialization;

/// <summary>
/// A type alias or union type definition.
/// </summary>
public record AstVariableNode : AstNode
{
    /// <summary>
    /// Gets the list of type expressions that define this variable, either as a single
    /// reference to a named type or as an inline anonymous group or array definition.
    /// </summary>
    [JsonPropertyName("PropertyType")]
    [JsonConverter(typeof(TypeExprListConverter))]
    public List<AstTypeExpr> PropertyType { get; init; } = [];
}
