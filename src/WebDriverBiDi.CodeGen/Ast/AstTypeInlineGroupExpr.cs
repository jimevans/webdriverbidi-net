// <copyright file="AstTypeInlineGroupExpr.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// An inline anonymous group definition embedded directly in a property Type field:
/// {"Type":"group","Name":"","Properties":[...]}.
/// Represents a struct defined in-place rather than by reference to a named type.
/// </summary>
public record AstTypeInlineGroupExpr(List<AstPropertyItem> Properties) : AstTypeExpr;
