// <copyright file="SpecModel.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

/// <summary>
/// The typed semantic model built from the WebDriver BiDi AST.
/// All nodes are classified by <see cref="NodeKind"/> and the
/// interesting ones are collected into typed lists.
/// </summary>
public sealed class SpecModel
{
    /// <summary>
    /// Gets the classification of every top-level node by its CDDL name.
    /// </summary>
    public required IReadOnlyDictionary<string, NodeKind> NodeKinds { get; init; }

    /// <summary>
    /// Gets the commands, in alphabetical order by CDDL name.
    /// </summary>
    public required IReadOnlyList<CommandDef> Commands { get; init; }

    /// <summary>
    /// Gets the events, in alphabetical order by CDDL name.
    /// </summary>
    public required IReadOnlyList<EventDef> Events { get; init; }

    /// <summary>
    /// Gets the command result types, in alphabetical order by CDDL name.
    /// </summary>
    public required IReadOnlyList<CommandResultDef> CommandResults { get; init; }

    /// <summary>
    /// Gets the enum types, in alphabetical order by CDDL name.
    /// </summary>
    public required IReadOnlyList<EnumDef> Enums { get; init; }

    /// <summary>
    /// Gets the simple type aliases, in alphabetical order by CDDL name.
    /// </summary>
    public required IReadOnlyList<TypeAliasDef> TypeAliases { get; init; }

    /// <summary>
    /// Gets the discriminated union types, in alphabetical order by CDDL name.
    /// </summary>
    public required IReadOnlyList<DiscriminatedUnionDef> DiscriminatedUnions { get; init; }

    /// <summary>
    /// Gets the data class types (group nodes not otherwise classified), in alphabetical order.
    /// </summary>
    public required IReadOnlyList<DataClassDef> DataClasses { get; init; }

    /// <summary>
    /// Gets the named array aliases, in alphabetical order by CDDL name.
    /// </summary>
    public required IReadOnlyList<ArrayAliasDef> ArrayAliases { get; init; }
}
