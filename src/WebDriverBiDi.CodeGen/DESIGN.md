# WebDriverBiDi.CodeGen — Design & Progress

A standalone code-generation tool that reads the WebDriver BiDi specification AST
(pre-parsed from CDDL into JSON) and writes `.cs` files into `src/WebDriverBiDi/`.

## Usage

```
dotnet run --project src/WebDriverBiDi.CodeGen -- \
  --ast third_party/spec/webdriver-bidi.ast.json \
  --out src/WebDriverBiDi
```

Options:
- `--ast <path>` — path to the AST JSON file (default: `third_party/spec/webdriver-bidi.ast.json` relative to the repo root)
- `--out <path>` — root output directory (default: `src/WebDriverBiDi`)
- `--dry-run` — print what would be generated without writing any files

---

## Architecture

```
src/WebDriverBiDi.CodeGen/
  DESIGN.md                          this file
  WebDriverBiDi.CodeGen.csproj
  Program.cs                         CLI entry point, pipeline orchestration
  Ast/
    AstNode.cs                       Top-level definition nodes (group / variable)
    AstPropertyItem.cs               Property entries and choice groups
    AstTypeExpr.cs                   Type expressions (primitives, refs, arrays, ranges)
    Converters/
      TypeExprListConverter.cs       Handles Type field: array, single object, or string
      PropertiesConverter.cs         Handles Properties field: objects and nested arrays
  Model/
    NodeKind.cs                      Enum: Command, Event, CommandResult, Enum, TypeAlias, etc.
    SpecDefs.cs                      CommandDef, EventDef, EnumDef, TypeAliasDef, etc.
    SpecModel.cs                     Typed semantic model (typed lists per NodeKind)
    ModelBuilder.cs                  AST -> SpecModel (registry walk + classification)
  Emit/
    NamingConventions.cs             CDDL names -> C# identifiers, namespaces, subdirs
    CSharpEmitter.cs                 Semantic model -> (relativePath, sourceText) pairs
```

Future work (not yet implemented):
```
  Emit/
    (extend CSharpEmitter.cs for data classes, full results, events, discriminated unions)
```

---

## AST JSON Schema

The file is a flat JSON array of 455 top-level definition nodes (341 groups, 109 variables).
Every node has a `"Type"` field that serves as a discriminator.

### Top-level node types

Three top-level `"Type"` values exist: `"group"`, `"variable"`, `"array"`.

### Top-level node: `"Type": "group"`

Represents a named map or struct type, or a command/event definition.

```json
{
  "Type": "group",
  "Name": "browser.ClientWindowInfo",
  "Properties": [ /* see below */ ],
  "IsChoiceAddition": false,
  "Comments": []
}
```

`Properties` is an array where each element is either:
- A **property entry** object `{ HasCut, Occurrence, Name, Type, Comments }`, or
- A **nested array** of property entry objects — a CDDL choice group (`//` operator),
  modelled as `AstPropertyChoice`.

A property entry's `Type` field can be:
- A JSON array of type expressions `[...]`
- A single type-expression object `{...}` (appears inside choice groups)
- A plain string like `"text"` (appears in array element definitions)

### Top-level node: `"Type": "variable"`

Represents a type alias or union type.

```json
{
  "Type": "variable",
  "Name": "browser.CloseResult",
  "IsChoiceAddition": false,
  "PropertyType": [ /* type expressions */ ],
  "Comments": []
}
```

`PropertyType` is always a JSON array of type expressions.

### Occurrence

```json
{ "n": 1, "m": 1 }    // required (exactly once)
{ "n": 0, "m": null }  // optional (zero or more — effectively 0 or 1 for fields)
{ "n": 0, "m": 1 }     // optional (zero or one)
{ "n": 1, "m": null }  // one or more
```

A field is **optional** when `n == 0`.

### Top-level node: `"Type": "array"`

A named array alias. Five exist in the spec (e.g. `browsingContext.InfoList`).

```json
{
  "Type": "array",
  "Name": "browsingContext.InfoList",
  "Values": [ /* property entries describing element type */ ],
  "Comments": []
}
```

### Type expressions

Type expressions appear in property `Type` fields and variable `PropertyType` arrays.
They have several shapes:

| JSON shape | Model class | Meaning |
|---|---|---|
| `"text"` (string) | `AstPrimitiveExpr("text")` | primitive: string |
| `"bool"` (string) | `AstPrimitiveExpr("bool")` | primitive: bool |
| `"null"` (string) | `AstPrimitiveExpr("null")` | null (makes type nullable) |
| `{"Type":"group","Value":"X.Y"}` | `AstGroupRef` | reference to another named type |
| `{"Type":"group","Name":"","Properties":[...]}` | `AstInlineGroupExpr` | inline anonymous struct |
| `{"Type":"literal","Value":"foo"}` | `AstLiteralExpr` | string or number literal |
| `{"Type":"array","Name":"","Values":[...]}` | `AstArrayExpr` | list/array type |
| `{"Type":"range","Value":{...}}` | `AstRangeExpr` | numeric range (bounds are `double`) |
| `{"Type":"float","Operator":{...}}` | `AstConstrainedPrimExpr("float")` | float with constraint |
| `{"Type":"bool","Operator":{...}}` | `AstConstrainedPrimExpr("bool")` | bool with default |
| `{"Type":"text","Operator":{...}}` | `AstConstrainedPrimExpr("text")` | text with default |
| `{"Type":{type-ref},"Operator":{...}}` | resolved to inner type | constrained named type (constraint discarded) |

---

## C# Generation Rules (planned)

### Namespace mapping

| CDDL prefix | C# namespace | Subdirectory |
|---|---|---|
| `session.*` | `WebDriverBiDi.Session` | `Session/` |
| `browser.*` | `WebDriverBiDi.Browser` | `Browser/` |
| `browsingContext.*` | `WebDriverBiDi.BrowsingContext` | `BrowsingContext/` |
| `network.*` | `WebDriverBiDi.Network` | `Network/` |
| `script.*` | `WebDriverBiDi.Script` | `Script/` |
| `log.*` | `WebDriverBiDi.Log` | `Log/` |
| `input.*` | `WebDriverBiDi.Input` | `Input/` |
| `storage.*` | `WebDriverBiDi.Storage` | `Storage/` |
| `emulation.*` | `WebDriverBiDi.Emulation` | `Emulation/` |
| `bluetooth.*` | `WebDriverBiDi.Bluetooth` | `Bluetooth/` |
| `webExtension.*` | `WebDriverBiDi.WebExtension` | `WebExtension/` |
| `speculation.*` | `WebDriverBiDi.Speculation` | `Speculation/` |

### Primitive type mapping

| CDDL | C# |
|---|---|
| `text` | `string` |
| `bool` | `bool` |
| `js-uint` (range 0..2^53-1) | `ulong` |
| `js-int` (range -2^53..2^53) | `long` |
| `float` (constrained) | `double` |
| `null` in union | makes containing type nullable |
| type alias `X = text` | resolves to `string` at point of use |

### Command detection

A `group` node is a **command** when:
- It has a `method` property whose type is a single `AstLiteralExpr`, AND
- It has a `params` property, AND
- The method name string appears in the corresponding `*Command` union in the registry.

A command `X.MethodName` with method `"x.methodName"` generates:
- `MethodNameCommandParameters : CommandParameters<MethodNameCommandResult>` with
  `MethodName => "x.methodName"` and properties for each field in the params type
  (public setters for required fields taken from constructor; optional fields nullable).

### Result detection

A `variable` or `group` node is a **result** when its name ends in `Result`.

- `variable` where `PropertyType = [AstGroupRef("EmptyResult")]`
  → `record MethodNameCommandResult : EmptyResult { }`

- `group` named `X.YResult` with fields
  → `record YCommandResult : CommandResult` with `[JsonConstructor] internal YCommandResult()`,
    all properties `internal set`, `[JsonRequired]`/`[JsonIgnore]`, `[JsonInclude]`, `[JsonPropertyName]`.

### Event detection

A `group` node is an **event** when:
- It has a `method` property whose type is a single `AstLiteralExpr`, AND
- The method name string appears in the corresponding `*Event` union in the registry.

An event `X.EventName` generates:
- `EventNameEventArgs : WebDriverBiDiEventArgs` — the params type's fields become
  properties with `[JsonConstructor]`, `internal set`, `[JsonRequired]`/`[JsonIgnore]`,
  `[JsonInclude]`, `[JsonPropertyName]`.

### Enum detection

A `variable` node is an **enum** when all entries of `PropertyType` are `AstLiteralExpr`
with string values.

Generates:
```csharp
[JsonConverter(typeof(EnumValueJsonConverter<ReadinessState>))]
public enum ReadinessState { None, Interactive, Complete }
```

### Data class detection

A `group` node that is not a command or event, and is not a result, generates a data
`record` with `[JsonConstructor] internal ClassName()` and `internal set` properties.

### Discriminated union detection

A `variable` node where `PropertyType` contains multiple `AstGroupRef` entries generates
a base class with `[JsonDerivedType]` attributes and subclasses. These require custom
review and are flagged in the generation report.

### Fields

For properties in results, events, and data classes:

| Field | C# |
|---|---|
| Required (`n=1`), string/primitive | `[JsonPropertyName][JsonRequired][JsonInclude] public T Prop { get; internal set; } = default!` |
| Optional (`n=0`), any type | `[JsonPropertyName][JsonIgnore(Condition=WhenWritingNull)][JsonInclude] public T? Prop { get; internal set; }` |
| List (`AstArrayExpr`) | Private `List<T>` field + public `IReadOnlyList<T>` property |

For properties in command parameter classes:

| Field | C# |
|---|---|
| Required | Constructor parameter + `[JsonPropertyName] public T Prop { get; }` |
| Optional | `[JsonPropertyName][JsonIgnore(Condition=WhenWritingNull)] public T? Prop { get; set; }` |

---

## Progress

### Phase 1 — AST deserialisation
- [x] Define C# model for all AST node types
- [x] Implement custom converters for heterogeneous JSON shapes
- [x] Program.cs: load AST, print summary
- [x] Verified: all 455 nodes deserialize without error
  - 87 commands/events (63 commands + 24 events), 17 enums, 61 results,
    21 type aliases, 17 discriminated unions, 212 data classes, 5 array aliases
  - Discovered and handled: top-level `array` nodes, inline anonymous groups,
    constrained named types (`{"Type":{ref},"Operator":{...}}`), float range bounds

### Phase 2 — Semantic model & type registry
- [x] Walk the top-level union registries (CommandData, ResultData, EventData, etc.)
  to classify each node as command, result, event, data type, enum, or alias
  - Verified: 63 commands, 24 events, 61 results, 17 enums, 21 type aliases,
    17 discriminated unions, 212 data classes, 5 array aliases, 35 registry/infra
- [x] Build cross-reference map: command → params type, command → result type, event → params type
  (stored in CommandDef.ParamsTypeName / ResultTypeName and EventDef.ParamsTypeName)
- [ ] Resolve type aliases (e.g. `browser.UserContext = text` → `string`) — deferred to Phase 3/4
  (will be done at point-of-use during code emission)

### Phase 3 — Code emission: simple cases
- [x] Emit enums (variable nodes where all PropertyType are string literals)
  - `[JsonEnumValue]` added when `csharpName.ToLowerInvariant() != jsonValue`
  - `EnumMemberName` handles hyphens, spaces, leading `-`, digits
  - 4 new files written; 13 existing files preserved (skip-on-exists)
- [x] Emit EmptyResult command results (36 total, all existing → skipped)
- [x] Emit EmptyParams command parameter classes (5 total, all existing → skipped)
- [x] `--dry-run` mode implemented (prints files, writes nothing)
- New files: `Emit/NamingConventions.cs`, `Emit/CSharpEmitter.cs`

### Phase 4 — Code emission: data classes and non-empty results/events
- [x] Emit data record classes (required + optional fields, list properties)
  - CDDL group inclusion (nameless AstGroupRef properties) flattened recursively
  - ArrayAlias group refs resolved to `List<ElementType>` at point of use
  - Required-but-nullable fields (CDDL `T / null` with n=1) keep `?` + `[JsonRequired]`
  - List properties use internal `List<T>` backing field + public `IList<T>` wrapper (deserialization pattern)
  - Params-reachable data classes (transitively referenced from CommandParameters) use:
    - `public ClassName()` constructor (no `[JsonConstructor]`, no `internal`)
    - `public set` on all properties (not `internal set`)
    - Plain `public List<T> { get; set; } = []` (no IList wrapper / backing field)
  - `CollectParamsReachableTypes` in `CSharpEmitter.cs` walks the full type graph via BFS
- [x] Emit command result classes (group results with fields)
- [x] Emit command parameter classes with params fields
  - Required fields go in constructor; optional fields are nullable mutable properties
- [x] Emit event args classes
- New files: `Emit/TypeResolver.cs`
- Total: 369 files (17 enums, 61 command results, 63 command parameters, 24 event args, 212 data classes)

### Phase 5 — Code emission: discriminated unions
- [x] Emit abstract base record for each discriminated union with `[JsonDerivedType]` per member
- [x] Member data classes get `: BaseClass` inheritance when in exactly one union
  - Members in multiple unions (e.g. `input.PauseAction`) are excluded — no base class added
  - Union members that are themselves unions chain correctly (e.g. `PrimitiveProtocolValue : RemoteValue`)
- [x] Skip-on-exists preserves all hand-crafted union implementations (14 existing files)
- Total: 386 files (17 enums, 17 union bases, 56 command results, 62 params, 22 events, 212 data classes)

### Phase 6 — Tooling
- [x] `--dry-run` mode (already done in Phase 3; prints file content for non-hand-crafted files)
- [x] Generation report: `Written (new)` / `Updated (re-generated)` / `Skipped (hand-crafted)` / `Unchanged`
- [x] Skip list via `// <auto-generated />` marker detection:
  - File exists **without** the marker → hand-crafted → never overwrite (SKIP)
  - File exists **with** the marker → previously generated → overwrite if content changed (UPDATE)
  - File doesn't exist → new file (WRITE)
  - File exists with marker and content is identical → silently skip (counted in Unchanged)
- `--force` flag overrides the hand-crafted guard, treating all existing files as overwritable
- Dry-run report result: 140 new, 0 updated, 242 skipped (hand-crafted), 4 unchanged

### Phase 7 — First real generation run & build fixes
- [x] Fixed cross-namespace `using` generation: `FindTypeNamespace` was a stub returning null;
  replaced with `TypeResolver.ReferencedNonTrivialCddlNames` + `NamingConventions.Namespace`
  - `ReferencedNonTrivialCddlNames` follows TypeAlias and ArrayAlias chains transitively;
    skips Registry nodes; yields CDDL name for DataClass/Enum/Union/CommandResult types
- [x] Added `HandCraftedDataClasses` skip set for 16 `script.*RemoteValue` types whose
  hand-crafted base (`RemoteValue`) has abstract members (`ToLocalValue()`) and an
  internal-only constructor; `RemoteValueJsonConverter` already handles all 16 via
  `CollectionRemoteValue`, `KeyValuePairCollectionRemoteValue`, `ObjectReferenceRemoteValue`
- [x] Updated generated file header: added `#nullable enable`, `#pragma warning disable CS1591`
  and `#pragma warning disable CS0108` so auto-generated files compile cleanly
- [x] Ran the generator: 82 new files written (311 total emitted, 245 hand-crafted skipped)
- [x] Library and test project build with **0 warnings, 0 errors**

### Phase 8 — Config file: skipGeneration list  *(TODO items 2 & 8)*
- [x] `--config <path>` CLI flag added to `Program.cs`; config is optional (empty default if omitted)
- [x] `Config/GeneratorConfig.cs`: `GeneratorConfig` with `SkipGeneration: IReadOnlyList<string>`
  - Loaded via `System.Text.Json` from the path given by `--config`
- [x] `CSharpEmitter.Emit()` now accepts `GeneratorConfig? config = null`
  - `skipSet` built from `config.SkipGeneration` at the top of `Emit()`
  - All six emit loops (enums, results, params, group results, events, unions, data classes)
    check `skipSet.Contains(node.Name)` before yielding
- [x] Removed hardcoded `HandCraftedDataClasses` set from `CSharpEmitter.cs`
- [x] `webdriver-bidi.codegen.json` created at repo root with the 16 `script.*RemoteValue` types
- Dry-run with config: 154 classified data classes, 138 emitted (16 skipped via config),
  311 total files, 66 unchanged

