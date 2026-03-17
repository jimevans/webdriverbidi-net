// <copyright file="TypeAliasDef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A type alias (variable node with a single non-literal PropertyType).
/// </summary>
/// <param name="AstNode">The variable node.</param>
/// <param name="AliasedType">The single type expression this aliases.</param>
public record TypeAliasDef(AstVariableNode AstNode, AstTypeExpr AliasedType);
