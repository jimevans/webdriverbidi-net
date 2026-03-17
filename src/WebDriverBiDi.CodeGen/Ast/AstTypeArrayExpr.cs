// <copyright file="AstTypeArrayExpr.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// An array/list type: {"Type":"array","Name":"","Values":[...]}.
/// Values describes the element type as a list of property entries.
/// </summary>
public record AstTypeArrayExpr(string Name, List<AstPropertyEntry> Values) : AstTypeExpr;
