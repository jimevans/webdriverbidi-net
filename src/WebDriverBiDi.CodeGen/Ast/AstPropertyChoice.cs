// <copyright file="AstPropertyChoice.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// Gets a CDDL group-choice group (the "//" operator), represented in the AST as
/// a nested array inside the parent group's Properties array.
/// Each option is a property entry that could appear in place of this choice.
/// </summary>
public record AstPropertyChoice(List<AstPropertyEntry> Options) : AstPropertyItem;
