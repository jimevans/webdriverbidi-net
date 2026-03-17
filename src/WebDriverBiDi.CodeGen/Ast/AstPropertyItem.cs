// <copyright file="AstPropertyItem.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// Base class for items inside a group's Properties array.
/// Each item is either a single field entry or a CDDL choice group.
/// </summary>
public abstract record AstPropertyItem;
