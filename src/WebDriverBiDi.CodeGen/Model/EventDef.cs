// <copyright file="EventDef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// An event definition.
/// </summary>
/// <param name="AstNode">The group node that defines this event.</param>
/// <param name="EventLiteral">The wire event name, e.g. "browsingContext.contextCreated".</param>
/// <param name="ParamsTypeName">The CDDL name of the params type, e.g. "browsingContext.Info".</param>
public record EventDef(AstGroupNode AstNode, string EventLiteral, string ParamsTypeName);
