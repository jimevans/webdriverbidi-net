// <copyright file="AstTypeGroupRef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A reference to another named type: {"Type":"group","Value":"browser.ClientWindowInfo"}.
/// </summary>
public record AstTypeGroupRef(string Value, bool Unwrapped) : AstTypeExpr;
