// <copyright file="AstTypeRangeExpr.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A numeric range: {"Type":"range","Value":{"Inclusive":true,"Min":{...},"Max":{...}}}.
/// Used for integer ranges (js-uint, js-int) and float ranges (0.0..1.0 for print margins).
/// </summary>
public record AstTypeRangeExpr(bool Inclusive, double Min, double Max) : AstTypeExpr;
