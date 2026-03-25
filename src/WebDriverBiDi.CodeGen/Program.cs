// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json;
using WebDriverBiDi.CodeGen.Ast;
using WebDriverBiDi.CodeGen.Config;
using WebDriverBiDi.CodeGen.Emit;
using WebDriverBiDi.CodeGen.Model;

// ---------------------------------------------------------------------------
// Argument parsing
// ---------------------------------------------------------------------------
string astPath = "third_party/spec/webdriver-bidi.ast.json";
string outPath = "src/WebDriverBiDi";
string? configPath = null;
bool dryRun = false;
bool force = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--ast" when i + 1 < args.Length:
            astPath = args[++i];
            break;
        case "--out" when i + 1 < args.Length:
            outPath = args[++i];
            break;
        case "--config" when i + 1 < args.Length:
            configPath = args[++i];
            break;
        case "--dry-run":
            dryRun = true;
            break;
        case "--force":
            force = true;
            break;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            Console.Error.WriteLine("Usage: WebDriverBiDi.CodeGen [--ast <path>] [--out <path>] [--config <path>] [--dry-run] [--force]");
            return 1;
    }
}

// ---------------------------------------------------------------------------
// Load optional config file
// ---------------------------------------------------------------------------
GeneratorConfig config = new();
if (configPath is not null)
{
    if (!File.Exists(configPath))
    {
        Console.Error.WriteLine($"Config file not found: {Path.GetFullPath(configPath)}");
        return 1;
    }

    Console.WriteLine($"Loading config: {Path.GetFullPath(configPath)}");
    string configJson = await File.ReadAllTextAsync(configPath);
    JsonSerializerOptions configOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        AllowTrailingCommas = true,
    };
    config = JsonSerializer.Deserialize<GeneratorConfig>(configJson, configOptions)
        ?? new GeneratorConfig();
}

// ---------------------------------------------------------------------------
// Load and deserialise the AST
// ---------------------------------------------------------------------------
if (!File.Exists(astPath))
{
    Console.Error.WriteLine($"AST file not found: {Path.GetFullPath(astPath)}");
    return 1;
}

Console.WriteLine($"Loading AST: {Path.GetFullPath(astPath)}");

string json = await File.ReadAllTextAsync(astPath);

JsonSerializerOptions serializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = false,
    AllowTrailingCommas = true,
    Converters =
    {
        new PropertiesConverter(),
        new TypeExprListConverter(),
    },
};

List<AstNode> nodes;
try
{
    nodes = JsonSerializer.Deserialize<List<AstNode>>(json, serializerOptions)
        ?? throw new InvalidOperationException("Deserialised null.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to deserialise AST: {ex.Message}");
    Console.Error.WriteLine(ex.ToString());
    return 1;
}

Console.WriteLine($"Loaded {nodes.Count} definitions");
Console.WriteLine();

// ---------------------------------------------------------------------------
// Build semantic model
// ---------------------------------------------------------------------------
Dictionary<string, AstNode> byName = nodes.ToDictionary(n => n.Name);
SpecModel model = ModelBuilder.Build(nodes);

Console.WriteLine("=== Classification summary ===");
Console.WriteLine($"  Commands:             {model.Commands.Count}");
Console.WriteLine($"  Events:               {model.Events.Count}");
Console.WriteLine($"  Command results:      {model.CommandResults.Count}");
Console.WriteLine($"  Enums:                {model.Enums.Count}");
Console.WriteLine($"  Type aliases:         {model.TypeAliases.Count}");
Console.WriteLine($"  Discriminated unions: {model.DiscriminatedUnions.Count}");
Console.WriteLine($"  Data classes:         {model.DataClasses.Count}");
Console.WriteLine($"  Array aliases:        {model.ArrayAliases.Count}");
Console.WriteLine($"  Registry/infra:       {model.NodeKinds.Values.Count(k => k == NodeKind.Registry)}");
Console.WriteLine();

// ---------------------------------------------------------------------------
// Emit C# files (Phase 3: enums, EmptyResult results, EmptyParams params)
// ---------------------------------------------------------------------------
IList<(string RelativePath, string Source)> files = CSharpEmitter.Emit(model, byName, config).ToList();

int enumFiles = files.Count(f => IsEnumFile(f.Source));
int unionFiles = files.Count(f => IsUnionFile(f.Source));
int resultFiles = files.Count(f => f.RelativePath.EndsWith("CommandResult.cs"));
int paramsFiles = files.Count(f => f.RelativePath.EndsWith("CommandParameters.cs"));
int eventFiles = files.Count(f => f.RelativePath.EndsWith("EventArgs.cs"));
int dataFiles = files.Count - enumFiles - unionFiles - resultFiles - paramsFiles - eventFiles;

Console.WriteLine("=== Emission summary ===");
Console.WriteLine($"  Enums:                     {enumFiles}");
Console.WriteLine($"  Discriminated union bases: {unionFiles}");
Console.WriteLine($"  Command results:           {resultFiles}");
Console.WriteLine($"  Command parameter classes: {paramsFiles}");
Console.WriteLine($"  Event args classes:        {eventFiles}");
Console.WriteLine($"  Data classes:              {dataFiles}");
Console.WriteLine($"  Total files:               {files.Count}");
Console.WriteLine();

static bool IsEnumFile(string source) => source.Contains("public enum ");
static bool IsUnionFile(string source) => source.Contains("public abstract record ");

// ---------------------------------------------------------------------------
// Process files: write new, update auto-generated, skip hand-crafted
//
// Skip logic: a file is "hand-crafted" if it exists on disk but does NOT
// contain the "// <auto-generated />" marker.  Auto-generated files that
// already exist on disk are overwritten so they stay in sync with the spec.
// ---------------------------------------------------------------------------
const string AutoGeneratedMarker = "// <auto-generated />";

int written = 0;
int updated = 0;
int skipped = 0;

foreach ((string relativePath, string source) in files)
{
    string fullPath = Path.Combine(outPath, relativePath);

    if (File.Exists(fullPath))
    {
        string existing = await File.ReadAllTextAsync(fullPath);
        if (!force && !existing.Contains(AutoGeneratedMarker))
        {
            // Hand-crafted file — skip unless --force is set
            if (dryRun)
            {
                Console.WriteLine($"  SKIP   {relativePath}  (hand-crafted)");
            }

            skipped++;
            continue;
        }

        // Previously generated and unchanged — skip silently
        if (existing == source)
        {
            continue;
        }

        // Previously generated but content changed — update it
        if (!dryRun)
        {
            await File.WriteAllTextAsync(fullPath, source);
        }

        Console.WriteLine($"  UPDATE {relativePath}");
        updated++;
    }
    else
    {
        // New file
        if (!dryRun)
        {
            string dir = Path.GetDirectoryName(fullPath)!;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(fullPath, source);
        }

        Console.WriteLine($"  WRITE  {relativePath}");
        written++;
    }
}

if (dryRun)
{
    Console.WriteLine();
    Console.WriteLine("=== Dry-run file listing ===");
    string skipNote = force
        ? "(--force is set: hand-crafted files will be overwritten)"
        : "(files marked SKIP are hand-crafted; use --force to overwrite them)";
    Console.WriteLine(skipNote);
    Console.WriteLine();

    // Print full content of files that would be written or updated
    foreach ((string path, string source) in files)
    {
        string fullPath = Path.Combine(outPath, path);
        bool exists = File.Exists(fullPath);
        bool isHandCrafted = !force && exists && !File.ReadAllText(fullPath).Contains(AutoGeneratedMarker);
        if (!isHandCrafted)
        {
            Console.WriteLine($"--- {path} ---");
            Console.WriteLine(source);
        }
    }

    Console.WriteLine("[dry-run] No files written.");
}

Console.WriteLine();
Console.WriteLine($"=== Generation report ===");
Console.WriteLine($"  Written (new):           {written}");
Console.WriteLine($"  Updated (re-generated):  {updated}");
Console.WriteLine($"  Skipped (hand-crafted):  {skipped}");
Console.WriteLine($"  Unchanged:               {files.Count - written - updated - skipped}");

return 0;
