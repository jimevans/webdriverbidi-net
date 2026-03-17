// <copyright file="DataClassDef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A data class (group node not classified as command, event, or result).
/// </summary>
/// <param name="AstNode">The group node.</param>
public record DataClassDef(AstGroupNode AstNode);
