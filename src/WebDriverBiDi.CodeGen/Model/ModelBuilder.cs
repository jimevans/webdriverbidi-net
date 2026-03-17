// <copyright file="ModelBuilder.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Model;

using WebDriverBiDi.CodeGen.Ast;

/// <summary>
/// Builds a <see cref="SpecModel"/> from the flat list of deserialised AST nodes by:
/// <list type="number">
///   <item>Walking the global CommandData / ResultData / EventData registries to collect
///         which nodes are commands, results, and events.</item>
///   <item>Classifying remaining variable nodes as enums, type aliases, or discriminated unions.</item>
///   <item>Classifying remaining group nodes as data classes.</item>
///   <item>Marking registry/infrastructure nodes so they are skipped during code generation.</item>
/// </list>
/// </summary>
public static class ModelBuilder
{
    // Infrastructure nodes that exist only to structure the registry — never emit C#.
    private static readonly IReadOnlySet<string> KnownRegistryNodes = new HashSet<string>
    {
        "CommandData",
        "ResultData",
        "EventData",

        // Per-module command registries
        "BrowserCommand",
        "BrowsingContextCommand",
        "EmulationCommand",
        "InputCommand",
        "NetworkCommand",
        "ScriptCommand",
        "SessionCommand",
        "StorageCommand",
        "WebExtensionCommand",
        "SpeculationCommand",

        // Per-module result registries
        "BrowserResult",
        "BrowsingContextResult",
        "EmulationResult",
        "InputResult",
        "NetworkResult",
        "ScriptResult",
        "SessionResult",
        "StorageResult",
        "WebExtensionResult",
        "SpeculationResult",

        // Per-module event registries
        "BrowsingContextEvent",
        "InputEvent",
        "LogEvent",
        "NetworkEvent",
        "ScriptEvent",
        "SpeculationEvent",

        // CDDL infrastructure
        "Command",
        "EmptyParams",
        "EmptyResult",
        "Extensible",
        "ExtensibleObject",
        "Message",
        "CommandResponse",
        "ErrorResponse",
        "Event",
    };

    /// <summary>
    /// Builds a <see cref="SpecModel"/> from the flat list of deserialised AST nodes.
    /// </summary>
    /// <param name="nodes">The list of deserialised AST nodes.</param>
    /// <returns>The built specification model.</returns>
    public static SpecModel Build(IReadOnlyList<AstNode> nodes)
    {
        Dictionary<string, AstNode> byName = nodes.ToDictionary(n => n.Name);
        Dictionary<string, NodeKind> kinds = new(nodes.Count);

        // ----------------------------------------------------------------
        // 1. Mark all known registry/infrastructure nodes upfront
        // ----------------------------------------------------------------
        foreach (AstNode n in nodes)
        {
            if (KnownRegistryNodes.Contains(n.Name))
            {
                kinds[n.Name] = NodeKind.Registry;
            }
        }

        // ----------------------------------------------------------------
        // 2. Collect command names from CommandData registry (2 levels deep)
        // ----------------------------------------------------------------
        HashSet<string> commandNames = new();
        if (byName.TryGetValue("CommandData", out AstNode? commandDataNode) &&
            commandDataNode is AstGroupNode commandData)
        {
            foreach (string moduleRef in CollectGroupRefs(commandData))
            {
                if (byName.TryGetValue(moduleRef, out AstNode? moduleNode) &&
                    moduleNode is AstGroupNode moduleGroup)
                {
                    foreach (string cmdRef in CollectGroupRefs(moduleGroup))
                    {
                        commandNames.Add(cmdRef);
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // 3. Collect result names from ResultData registry (2 levels deep)
        // ----------------------------------------------------------------
        HashSet<string> resultNames = new();
        if (byName.TryGetValue("ResultData", out AstNode? resultDataNode) &&
            resultDataNode is AstVariableNode resultData)
        {
            foreach (string moduleRef in CollectVariableRefs(resultData))
            {
                if (byName.TryGetValue(moduleRef, out AstNode? moduleNode) &&
                    moduleNode is AstVariableNode moduleVar)
                {
                    foreach (string resRef in CollectVariableRefs(moduleVar))
                    {
                        resultNames.Add(resRef);
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // 4. Collect event names from EventData registry (2 levels deep)
        // ----------------------------------------------------------------
        HashSet<string> eventNames = new();
        if (byName.TryGetValue("EventData", out AstNode? eventDataNode) &&
            eventDataNode is AstGroupNode eventData)
        {
            foreach (string moduleRef in CollectGroupRefs(eventData))
            {
                if (byName.TryGetValue(moduleRef, out AstNode? moduleNode) &&
                    moduleNode is AstGroupNode moduleGroup)
                {
                    foreach (string evtRef in CollectGroupRefs(moduleGroup))
                    {
                        eventNames.Add(evtRef);
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // 5. Build CommandDef list
        // ----------------------------------------------------------------
        List<CommandDef> commands = new();
        foreach (string name in commandNames.OrderBy(x => x))
        {
            kinds[name] = NodeKind.Command;
            if (byName.TryGetValue(name, out AstNode? cmdNode) && cmdNode is AstGroupNode cmdGroup)
            {
                string? method = ExtractLiteral(cmdGroup, "method");
                string? paramsType = ExtractGroupRef(cmdGroup, "params") ?? "EmptyParams";
                string resultType = name + "Result";
                if (method is not null)
                {
                    commands.Add(new CommandDef(cmdGroup, method, paramsType, resultType));
                    if (paramsType != "EmptyParams")
                    {
                        kinds[paramsType] = NodeKind.CommandParams;
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // 6. Build EventDef list
        // ----------------------------------------------------------------
        List<EventDef> events = new();
        foreach (string name in eventNames.OrderBy(x => x))
        {
            kinds[name] = NodeKind.Event;
            if (byName.TryGetValue(name, out AstNode? evtNode) && evtNode is AstGroupNode evtGroup)
            {
                string? eventLiteral = ExtractLiteral(evtGroup, "method");
                string? paramsType = ExtractGroupRef(evtGroup, "params") ?? "EmptyParams";
                if (eventLiteral is not null)
                {
                    events.Add(new EventDef(evtGroup, eventLiteral, paramsType));
                }
            }
        }

        // ----------------------------------------------------------------
        // 7. Build CommandResultDef list
        // ----------------------------------------------------------------
        List<CommandResultDef> commandResults = new();
        foreach (string name in resultNames.OrderBy(x => x))
        {
            kinds[name] = NodeKind.CommandResult;
            if (byName.TryGetValue(name, out AstNode? resNode))
            {
                commandResults.Add(new CommandResultDef(resNode));
            }
        }

        // ----------------------------------------------------------------
        // 8. Classify remaining variable nodes
        // ----------------------------------------------------------------
        List<EnumDef> enums = new();
        List<TypeAliasDef> typeAliases = new();
        List<DiscriminatedUnionDef> discriminatedUnions = new();

        foreach (AstVariableNode node in nodes.OfType<AstVariableNode>().OrderBy(v => v.Name))
        {
            if (kinds.ContainsKey(node.Name))
            {
                continue; // already classified
            }

            if (node.PropertyType.Count > 0 &&
                node.PropertyType.All(t => t is AstTypeLiteralExpr { Value: not null }))
            {
                // All entries are string/numeric literals → enum
                List<string> values = node.PropertyType
                    .Cast<AstTypeLiteralExpr>()
                    .Select(l => l.Value)
                    .ToList();
                kinds[node.Name] = NodeKind.Enum;
                enums.Add(new EnumDef(node, values));
            }
            else if (node.PropertyType.Count == 1 && node.PropertyType[0] is not AstTypeLiteralExpr)
            {
                // Single non-literal type → alias
                kinds[node.Name] = NodeKind.TypeAlias;
                typeAliases.Add(new TypeAliasDef(node, node.PropertyType[0]));
            }
            else if (node.PropertyType.Count > 1 &&
                     node.PropertyType.All(t => t is AstTypeGroupRef))
            {
                // Multiple group refs → discriminated union
                List<string> members = node.PropertyType
                    .Cast<AstTypeGroupRef>()
                    .Select(r => r.Value)
                    .ToList();
                kinds[node.Name] = NodeKind.DiscriminatedUnion;
                discriminatedUnions.Add(new DiscriminatedUnionDef(node, members));
            }
            else
            {
                // Mixed or unrecognised pattern — mark as registry/unknown for now
                kinds[node.Name] = NodeKind.Registry;
            }
        }

        // ----------------------------------------------------------------
        // 9. Classify remaining group nodes as data classes
        // ----------------------------------------------------------------
        List<DataClassDef> dataClasses = new();
        foreach (AstGroupNode node in nodes.OfType<AstGroupNode>().OrderBy(g => g.Name))
        {
            if (kinds.ContainsKey(node.Name))
            {
                continue;
            }

            kinds[node.Name] = NodeKind.DataClass;
            dataClasses.Add(new DataClassDef(node));
        }

        // ----------------------------------------------------------------
        // 10. Classify top-level array nodes
        // ----------------------------------------------------------------
        List<ArrayAliasDef> arrayAliases = new();
        foreach (AstArrayNode node in nodes.OfType<AstArrayNode>().OrderBy(a => a.Name))
        {
            if (kinds.ContainsKey(node.Name))
            {
                continue;
            }

            kinds[node.Name] = NodeKind.ArrayAlias;
            arrayAliases.Add(new ArrayAliasDef(node));
        }

        // ----------------------------------------------------------------
        // 11. Ensure every node has a classification (fallback to Registry)
        // ----------------------------------------------------------------
        foreach (AstNode node in nodes)
        {
            if (!kinds.ContainsKey(node.Name))
            {
                kinds[node.Name] = NodeKind.Registry;
            }
        }

        return new SpecModel
        {
            NodeKinds = kinds,
            Commands = commands,
            Events = events,
            CommandResults = commandResults,
            Enums = enums,
            TypeAliases = typeAliases,
            DiscriminatedUnions = discriminatedUnions,
            DataClasses = dataClasses,
            ArrayAliases = arrayAliases,
        };
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Yields the Value of every AstGroupRef found in the Properties of a group node,
    /// including refs inside AstPropertyChoice alternatives.
    /// </summary>
    private static IEnumerable<string> CollectGroupRefs(AstGroupNode group)
    {
        foreach (AstPropertyItem item in group.Properties)
        {
            if (item is AstPropertyEntry entry)
            {
                foreach (AstTypeExpr typeExpr in entry.Type)
                {
                    if (typeExpr is AstTypeGroupRef gr)
                    {
                        yield return gr.Value;
                    }
                }
            }
            else if (item is AstPropertyChoice choice)
            {
                foreach (AstPropertyEntry opt in choice.Options)
                {
                    foreach (AstTypeExpr typeExpr in opt.Type)
                    {
                        if (typeExpr is AstTypeGroupRef gr)
                        {
                            yield return gr.Value;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Yields the Value of every AstGroupRef in a variable node's PropertyType list.
    /// </summary>
    private static IEnumerable<string> CollectVariableRefs(AstVariableNode variable)
    {
        foreach (AstTypeExpr typeExpr in variable.PropertyType)
        {
            if (typeExpr is AstTypeGroupRef gr)
            {
                yield return gr.Value;
            }
        }
    }

    /// <summary>
    /// Returns the string value of a single AstLiteralExpr in the named property, or null.
    /// </summary>
    private static string? ExtractLiteral(AstGroupNode node, string propName)
    {
        return node.Properties
            .OfType<AstPropertyEntry>()
            .Where(p => p.Name == propName &&
                        p.Type.Count == 1 &&
                        p.Type[0] is AstTypeLiteralExpr)
            .Select(p => ((AstTypeLiteralExpr)p.Type[0]).Value)
            .FirstOrDefault();
    }

    /// <summary>
    /// Returns the referenced type name from a single AstGroupRef in the named property, or null.
    /// </summary>
    private static string? ExtractGroupRef(AstGroupNode node, string propName)
    {
        return node.Properties
            .OfType<AstPropertyEntry>()
            .Where(p => p.Name == propName &&
                        p.Type.Count == 1 &&
                        p.Type[0] is AstTypeGroupRef)
            .Select(p => ((AstTypeGroupRef)p.Type[0]).Value)
            .FirstOrDefault();
    }
}
