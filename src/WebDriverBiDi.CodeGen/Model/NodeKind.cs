// <copyright file="NodeKind.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

/// <summary>
/// Semantic classification of a top-level AST node.
/// </summary>
public enum NodeKind
{
    /// <summary>A command (group node with a literal "method" and a "params" property).</summary>
    Command,

    /// <summary>A command parameters type (the group node referenced by a command's "params" field).</summary>
    CommandParams,

    /// <summary>A command result type (listed in a *Result registry variable).</summary>
    CommandResult,

    /// <summary>An event (group node with a literal "method" in the EventData registry).</summary>
    Event,

    /// <summary>An enum (variable node where every PropertyType entry is a string literal).</summary>
    Enum,

    /// <summary>A simple type alias (variable node with a single non-literal PropertyType).</summary>
    TypeAlias,

    /// <summary>A discriminated union (variable node with multiple AstGroupRef entries).</summary>
    DiscriminatedUnion,

    /// <summary>A data class (group node not classified as command, event, or result).</summary>
    DataClass,

    /// <summary>A named array alias (top-level array node).</summary>
    ArrayAlias,

    /// <summary>A registry/infrastructure node — not emitted as a C# type.</summary>
    Registry,
}
