# WebDriverBiDi.CodeGen — Enhancement Backlog

Tracked here alongside DESIGN.md. Items are roughly ordered by priority / dependency.

---

## ~~1. Fix settability regression for params-reachable data classes~~ ✅ DONE

`CollectParamsReachableTypes` in `CSharpEmitter.cs` now walks the type graph transitively from
all non-EmptyParams `CommandParameters` types. Data classes in the reachable set are emitted
with `public ClassName()` (no `[JsonConstructor]`, no `internal`) and `public set` on all
properties. List properties on these types are plain `public List<T> { get; set; } = []`
(no `IList<T>` / `AsReadOnly()` wrapper, no backing field).

Deserialization-only data classes (reachable only from results or events) continue to use
`[JsonConstructor] internal` + `internal set` + `IList<T>` wrapper.

---

## 2. Config file: timestamp property overrides  *(introduces --config flag)*

**Affected hand-crafted pattern:**
```csharp
[JsonPropertyName("timestamp")]
[JsonRequired][JsonInclude]
internal ulong EpochTimestamp { get; set; }   // JSON-serialized

[JsonIgnore]
public DateTime Timestamp { get; internal set; } // computed public API
```

Add a `--config <path>` CLI flag pointing to a JSON file.
Add a `propertyOverrides` section that maps a CDDL type name + property name to emit the
timestamp split pattern (internal `ulong` serializable property + computed public `DateTime`).

---

## 3. Config file: type and property name remapping

Extend the config file with a `nameMap` section allowing the generated .NET class or property
name to differ from the CDDL-derived name. Examples:

- `browsingContext.BrowsingContext` type alias property `context` → C# property `BrowsingContextId`
- `browsingContext.Navigation` property `navigation` → `NavigationId`
- `browsingContext.Info` class → `BrowsingContextInfo`
- `script.Realm` property `realm` → `RealmId`

---

## 4. Config file: XML documentation comment injection

Extend the config file with a `docs` section providing XML summary comments for generated
types and their properties, keyed by CDDL type name and optionally by property name.
The emitter injects `/// <summary>...</summary>` blocks above the class/property declaration.

---

## 5. Config file: class vs record kind override

The generator emits `abstract record` for discriminated union bases. All existing hand-crafted
union bases are `abstract class` — switching to `abstract record` is an API-breaking change, and
a `record` subclass cannot inherit from a non-`record` base.

Extend the config file with a `classKind` section that maps a CDDL type name to `class` or
`record` (default `record`). Also applies to concrete data classes where `class` is needed for
compatibility.

---

## 6. Config file: per-property JSON converter injection

Many `float`/`double` properties need `[JsonConverter(typeof(FixedDoubleJsonConverter))]` to
avoid exponential notation on the wire. Examples:
- All 13 timing fields in `network.FetchTimingInfo`
- `x`, `y`, `width`, `height` in `browsingContext.BoxClipRectangle`
- Margin/dimension fields in `browsingContext.PrintPageParameters`

Extend the config file with a `converterOverrides` section mapping CDDL type+property to a
named C# converter class. The emitter adds the `[JsonConverter(typeof(X))]` attribute on the
property.

---

## 7. Config file: event args wrapping pattern

Some event args hold a reference to a params data type and delegate a curated subset of
properties to it, rather than flattening all fields directly onto the event args class:

```csharp
public record BrowsingContextEventArgs : WebDriverBiDiEventArgs
{
    private readonly BrowsingContextInfo info;
    public BrowsingContextEventArgs(BrowsingContextInfo info) { this.info = info; }
    public string BrowsingContextId => this.info.BrowsingContextId;
    // ...
}
```

Extend the config file with a `wrapParamsType` section specifying which events should use this
pattern, the inner type to wrap, and the property subset to expose (with optional name remaps).

---

## 8. Config file: skipGeneration list

Add a `skipGeneration` list of CDDL names that are permanently excluded from all emission,
even with `--force`. These types require hand-crafted custom JSON converters that express
runtime type dispatch the generator cannot reproduce from the CDDL alone:

| CDDL name | Converter | Reason |
|---|---|---|
| `script.RemoteValue` (+ all ~20 subtypes) | `RemoteValueJsonConverter` | `value` field type depends on `type` at runtime |
| `script.Target` | `ScriptTargetJsonConverter` | Discriminates context vs realm by shape |
| `script.RealmInfo` (+ realm subclasses) | `RealmInfoJsonConverter` | Discriminates by `type` field |
| `script.EvaluateResult` (+ success/exception) | `ScriptEvaluateResultJsonConverter` | Success vs exception polymorphism |
| `log.Entry` / `log.*LogEntry` | `LogEntryJsonConverter` | Discriminates log entry kind by `type` |
| `session.ProxyConfiguration` (+ subtypes) | `ProxyConfigurationJsonConverter` | Discriminates by `proxyType` field |

These types will always be hand-crafted and should never be accidentally overwritten.
