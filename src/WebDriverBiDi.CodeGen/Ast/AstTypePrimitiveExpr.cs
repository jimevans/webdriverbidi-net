// <copyright file="AstTypePrimitiveExpr.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A primitive type expressed as a plain JSON string: "text", "bool", "null", "bytes", etc.
/// Also used when a type expression object has no special structure (e.g. "text" inside an array Values).
/// </summary>
public record AstTypePrimitiveExpr(string TypeName) : AstTypeExpr;
