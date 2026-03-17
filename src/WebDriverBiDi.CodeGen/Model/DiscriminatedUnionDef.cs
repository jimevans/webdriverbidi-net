// <copyright file="DiscriminatedUnionDef.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// A discriminated union (variable node with multiple AstGroupRef entries).
/// </summary>
/// <param name="AstNode">The variable node.</param>
/// <param name="MemberTypeNames">CDDL names of the concrete member types.</param>
public record DiscriminatedUnionDef(AstVariableNode AstNode, IReadOnlyList<string> MemberTypeNames);
