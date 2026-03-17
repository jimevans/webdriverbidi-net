// <copyright file="CommandDef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A command definition.
/// </summary>
/// <param name="AstNode">The group node that defines this command.</param>
/// <param name="MethodLiteral">The wire method string, e.g. "browser.close".</param>
/// <param name="ParamsTypeName">The CDDL name of the params type, e.g. "EmptyParams" or "browser.CreateUserContextParameters".</param>
/// <param name="ResultTypeName">The CDDL name of the result type, e.g. "browser.CloseResult".</param>
public record CommandDef(AstGroupNode AstNode, string MethodLiteral, string ParamsTypeName, string ResultTypeName);
