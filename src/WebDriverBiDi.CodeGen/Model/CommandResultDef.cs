// <copyright file="CommandResultDef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A command result type (either a variable alias or a group with fields).
/// </summary>
/// <param name="AstNode">The AST node (variable or group).</param>
public record CommandResultDef(AstNode AstNode);
