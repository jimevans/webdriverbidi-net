// <copyright file="AstTypeLiteralExpr.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A string or numeric literal value: {"Type":"literal","Value":"fullscreen"} or
/// {"Type":"literal","Value":42}.
/// Used both for string enum alternatives and for numeric range bounds.
/// </summary>
public record AstTypeLiteralExpr(string Value, bool Unwrapped) : AstTypeExpr;
