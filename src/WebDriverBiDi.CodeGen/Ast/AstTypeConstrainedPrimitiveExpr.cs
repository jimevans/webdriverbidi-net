// <copyright file="AstTypeConstrainedPrimitiveExpr.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A primitive type that carries an additional operator (constraint or default value):
/// {"Type":"float"/"bool"/"text","Operator":{...}}.
/// The Operator is either a comparison constraint (ge/gt) or a default value.
/// For C# generation purposes only the TypeName is needed; the operator is advisory.
/// </summary>
public record AstTypeConstrainedPrimitiveExpr(string TypeName) : AstTypeExpr;
