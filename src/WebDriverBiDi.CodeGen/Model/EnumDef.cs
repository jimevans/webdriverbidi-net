// <copyright file="EnumDef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// An enum type (variable node where every PropertyType entry is a string literal).
/// </summary>
/// <param name="AstNode">The variable node.</param>
/// <param name="Values">The string literal values in declaration order.</param>
public record EnumDef(AstVariableNode AstNode, IReadOnlyList<string> Values);
