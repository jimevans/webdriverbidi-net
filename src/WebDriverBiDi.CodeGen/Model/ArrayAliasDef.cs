// <copyright file="ArrayAliasDef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A named array alias (top-level array node).
/// </summary>
/// <param name="AstNode">The array node.</param>
public record ArrayAliasDef(AstArrayNode AstNode);
