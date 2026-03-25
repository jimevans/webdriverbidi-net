// <copyright file="TypeResolver.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Emit;

using WebDriverBiDi.CodeGen.Ast;
using WebDriverBiDi.CodeGen.Model;

/// <summary>
/// Resolves a CDDL <see cref="AstTypeExpr"/> (or a list of them) to a C# type string.
/// </summary>
public static class TypeResolver
{
    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Resolves a property Type list to a C# type string.
    /// A "null" entry in the list makes the result nullable.
    /// Returns <c>isList = true</c> when the resolved type is a List&lt;T&gt;.
    /// </summary>
    /// <param name="types">The list of CDDL type expressions to resolve.</param>
    /// <param name="byName">The map of CDDL name → AST node, for reference when resolving group references.</param>
    /// <param name="nodeKinds">The map of CDDL name → node kind, for reference when resolving group references.</param>
    /// <returns>The resolved C# type string, whether it's a list, and the element type if it's a list.</returns>
    public static (string CSharpType, bool IsList, string ListElementType) Resolve(
        IReadOnlyList<AstTypeExpr> types,
        IReadOnlyDictionary<string, AstNode> byName,
        IReadOnlyDictionary<string, NodeKind> nodeKinds)
    {
        bool hasNull = types.Any(t => t is AstTypePrimitiveExpr { TypeName: "null" });
        List<AstTypeExpr> nonNull = types.Where(t => t is not AstTypePrimitiveExpr { TypeName: "null" }).ToList();

        if (nonNull.Count == 0)
        {
            return ("object", false, string.Empty);
        }

        // Single non-null type (most common)
        if (nonNull.Count == 1)
        {
            if (nonNull[0] is AstTypeArrayExpr arr)
            {
                string elem = ResolveElementType(arr, byName, nodeKinds);
                return ($"List<{elem}>", true, elem);
            }

            string csharp = ResolveSingle(nonNull[0], byName, nodeKinds);

            // If the resolved type is List<T> (e.g. from an ArrayAlias group ref), treat as a list
            if (csharp.StartsWith("List<") && csharp.EndsWith(">"))
            {
                string elem = csharp[5..^1];
                return (csharp, true, elem);
            }

            // Value types (primitives) can't be nullable with the '?' suffix here in the same way —
            // we still suffix with '?' and callers can strip it for non-nullable contexts.
            if (hasNull)
            {
                csharp += "?";
            }

            return (csharp, false, string.Empty);
        }

        // Multiple non-null types — unusual; use object.
        return (hasNull ? "object?" : "object", false, string.Empty);
    }

    /// <summary>
    /// Collects all CDDL names directly referenced by a list of type expressions.
    /// Used to build 'using' directives for a generated file.
    /// </summary>
    /// <param name="types">The list of type expressions to collect references from.</param>
    /// <returns>A sequence of CDDL names referenced by the type expressions.</returns>
    public static IEnumerable<string> ReferencedCddlNames(IReadOnlyList<AstTypeExpr> types)
    {
        foreach (AstTypeExpr t in types)
        {
            foreach (string name in ReferencedNames(t))
            {
                yield return name;
            }
        }
    }

    /// <summary>
    /// Returns the CDDL names of non-trivial types (data classes, enums, discriminated unions,
    /// command results) that are transitively referenced by the given type expressions.
    /// Primitives, type aliases that resolve to primitives, registry/infra nodes, and array aliases
    /// (which are inlined) are excluded; array aliases are followed into their element types.
    /// This is used to determine which namespaces to add as 'using' directives in generated files.
    /// </summary>
    /// <param name="types">The list of type expressions to examine.</param>
    /// <param name="byName">The map of CDDL name → AST node.</param>
    /// <param name="nodeKinds">The map of CDDL name → node kind.</param>
    /// <returns>A sequence of CDDL names whose namespaces need to be imported.</returns>
    public static IEnumerable<string> ReferencedNonTrivialCddlNames(
        IReadOnlyList<AstTypeExpr> types,
        IReadOnlyDictionary<string, AstNode> byName,
        IReadOnlyDictionary<string, NodeKind> nodeKinds)
    {
        foreach (AstTypeExpr t in types)
        {
            foreach (string name in NonTrivialNames(t, byName, nodeKinds, visited: null))
            {
                yield return name;
            }
        }
    }

    // -----------------------------------------------------------------------
    // Primitive and type-level resolution
    // -----------------------------------------------------------------------
    private static string ResolveSingle(
        AstTypeExpr expr,
        IReadOnlyDictionary<string, AstNode> byName,
        IReadOnlyDictionary<string, NodeKind> nodeKinds)
    {
        return expr switch
        {
            AstTypePrimitiveExpr { TypeName: "text" } => "string",
            AstTypePrimitiveExpr { TypeName: "bool" } => "bool",
            AstTypePrimitiveExpr { TypeName: "float" } => "double",
            AstTypePrimitiveExpr { TypeName: "uint" } => "ulong",
            AstTypePrimitiveExpr { TypeName: "int" } => "long",
            AstTypePrimitiveExpr { TypeName: "bytes" } => "string",
            AstTypePrimitiveExpr { TypeName: "bstr" } => "string",
            AstTypePrimitiveExpr { TypeName: "tstr" } => "string",
            AstTypePrimitiveExpr => "object",

            AstTypeConstrainedPrimitiveExpr { TypeName: "float" } => "double",
            AstTypeConstrainedPrimitiveExpr { TypeName: "bool" } => "bool",
            AstTypeConstrainedPrimitiveExpr { TypeName: "text" } => "string",
            AstTypeConstrainedPrimitiveExpr => "object",

            AstTypeRangeExpr r => ResolveRange(r),

            AstTypeLiteralExpr => "string",

            AstTypeGroupRef gr => ResolveGroupRef(gr.Value, byName, nodeKinds),

            AstTypeArrayExpr arr => $"List<{ResolveElementType(arr, byName, nodeKinds)}>",

            AstTypeInlineGroupExpr => "System.Text.Json.JsonElement",

            _ => "object",
        };
    }

    private static string ResolveRange(AstTypeRangeExpr r)
    {
        // Float-range bounds: if either is non-integer → double
        if (r.Min != Math.Truncate(r.Min) || r.Max != Math.Truncate(r.Max))
        {
            return "double";
        }

        // Negative lower bound → signed
        return r.Min < 0 ? "long" : "ulong";
    }

    private static string ResolveGroupRef(
        string cddlName,
        IReadOnlyDictionary<string, AstNode> byName,
        IReadOnlyDictionary<string, NodeKind> nodeKinds)
    {
        if (!nodeKinds.TryGetValue(cddlName, out NodeKind kind))
        {
            return NamingConventions.LocalName(cddlName); // unknown → best guess
        }

        switch (kind)
        {
            case NodeKind.TypeAlias:
                // Follow the alias chain to the underlying primitive/type
                if (byName.TryGetValue(cddlName, out AstNode? node) && node is AstVariableNode v)
                {
                    return ResolveSingle(v.PropertyType[0], byName, nodeKinds);
                }

                return NamingConventions.LocalName(cddlName);

            case NodeKind.CommandResult:
                return NamingConventions.CommandResultClassName(cddlName);

            case NodeKind.ArrayAlias:
                // Inline the array alias as List<ElementType> at point of use
                if (byName.TryGetValue(cddlName, out AstNode? arrNode) && arrNode is AstArrayNode arrayAlias)
                {
                    string elem = ResolveElementType(new AstTypeArrayExpr(string.Empty, arrayAlias.Values), byName, nodeKinds);
                    return $"List<{elem}>";
                }

                return NamingConventions.LocalName(cddlName);

            default:
                // Enum, DataClass, DiscriminatedUnion — use the local C# name
                return NamingConventions.LocalName(cddlName);
        }
    }

    private static string ResolveElementType(
        AstTypeArrayExpr arr,
        IReadOnlyDictionary<string, AstNode> byName,
        IReadOnlyDictionary<string, NodeKind> nodeKinds)
    {
        if (arr.Values.Count == 0)
        {
            return "object";
        }

        // Values entries describe the element type; take the first entry's Type list
        List<AstTypeExpr> elemTypes = arr.Values[0].Type;
        (string csharpType, bool _, string _) = Resolve(elemTypes, byName, nodeKinds);

        // Strip trailing '?' from element type in the List<T> context
        return csharpType.TrimEnd('?');
    }

    // -----------------------------------------------------------------------
    // Reference collection (for 'using' directives)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Recursively walks a single type expression and yields CDDL names of non-trivial types.
    /// Follows type aliases and array aliases transitively.
    /// </summary>
    private static IEnumerable<string> NonTrivialNames(
        AstTypeExpr expr,
        IReadOnlyDictionary<string, AstNode> byName,
        IReadOnlyDictionary<string, NodeKind> nodeKinds,
        HashSet<string>? visited)
    {
        switch (expr)
        {
            case AstTypeGroupRef gr:
                if (!nodeKinds.TryGetValue(gr.Value, out NodeKind kind))
                {
                    // Unknown name — yield as best-guess so the caller can attempt the namespace lookup
                    yield return gr.Value;
                    yield break;
                }

                switch (kind)
                {
                    case NodeKind.Registry:
                        // Pure structural/registry nodes — no C# type emitted
                        yield break;

                    case NodeKind.TypeAlias:
                        // Follow the alias: if it ultimately leads to a non-trivial group, yield that name
                        if (byName.TryGetValue(gr.Value, out AstNode? aliasNode) &&
                            aliasNode is AstVariableNode aliasVar)
                        {
                            visited ??= new HashSet<string>(StringComparer.Ordinal);
                            if (visited.Add(gr.Value))
                            {
                                foreach (AstTypeExpr inner in aliasVar.PropertyType)
                                {
                                    foreach (string n in NonTrivialNames(inner, byName, nodeKinds, visited))
                                    {
                                        yield return n;
                                    }
                                }
                            }
                        }

                        yield break;

                    case NodeKind.ArrayAlias:
                        // Array aliases are inlined as List<T> — recurse into element types
                        if (byName.TryGetValue(gr.Value, out AstNode? arrNode) &&
                            arrNode is AstArrayNode arrayAlias)
                        {
                            foreach (AstPropertyEntry entry in arrayAlias.Values)
                            {
                                foreach (AstTypeExpr t in entry.Type)
                                {
                                    foreach (string n in NonTrivialNames(t, byName, nodeKinds, visited))
                                    {
                                        yield return n;
                                    }
                                }
                            }
                        }

                        yield break;

                    default:
                        // DataClass, Enum, DiscriminatedUnion, CommandResult, Event, Command
                        yield return gr.Value;
                        yield break;
                }

            case AstTypeArrayExpr arr:
                // Recurse into the array element types
                foreach (AstPropertyEntry entry in arr.Values)
                {
                    foreach (AstTypeExpr t in entry.Type)
                    {
                        foreach (string n in NonTrivialNames(t, byName, nodeKinds, visited))
                        {
                            yield return n;
                        }
                    }
                }

                break;
        }
    }

    private static IEnumerable<string> ReferencedNames(AstTypeExpr expr)
    {
        switch (expr)
        {
            case AstTypeGroupRef gr:
                yield return gr.Value;
                break;
            case AstTypeArrayExpr arr:
                foreach (var entry in arr.Values)
                {
                    foreach (var t in entry.Type)
                    {
                        foreach (var n in ReferencedNames(t))
                        {
                            yield return n;
                        }
                    }
                }

                break;
            case AstTypeInlineGroupExpr ig:
                foreach (var item in ig.Properties)
                {
                    if (item is AstPropertyEntry pe)
                    {
                        foreach (var t in pe.Type)
                        {
                            foreach (var n in ReferencedNames(t))
                            {
                                yield return n;
                            }
                        }
                    }
                }

                break;
        }
    }
}
