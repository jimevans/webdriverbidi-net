// <copyright file="TypeExprListConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts the Type field on a property entry or the PropertyType array on a variable node.
///
/// The JSON value may be:
///   - A JSON array  [...] containing one or more type expressions
///   - A single JSON object {...} representing one type expression (appears inside choice groups)
///   - A plain JSON string like "text" or "bool" (appears inside array Values entries)
///
/// All three shapes are normalised to List&lt;AstTypeExpr&gt;.
/// </summary>
public class TypeExprListConverter : JsonConverter<List<AstTypeExpr>>
{
    /// <summary>
    /// Reads the JSON value of a property Type field or variable PropertyType field and converts it to a list of type expressions.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The list of type expressions.</returns>
    public override List<AstTypeExpr> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        List<AstTypeExpr> result = [];

        if (reader.TokenType == JsonTokenType.StartArray)
        {
             // consume StartArray
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                result.Add(ReadSingleExpr(ref reader, options));
                reader.Read();
            }

            // reader is now at EndArray
        }
        else
        {
            // Single expression not wrapped in an array
            result.Add(ReadSingleExpr(ref reader, options));

            // reader is at last token of the value (string or EndObject)
        }

        return result;
    }

    /// <summary>
    /// Writes a list of type expressions to JSON. Not supported since this converter is only used for deserialization.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The list of type expressions.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <exception cref="NotSupportedException">Thrown when attempting to write JSON.</exception>
    public override void Write(Utf8JsonWriter writer, List<AstTypeExpr> value, JsonSerializerOptions options)
        => throw new NotSupportedException($"{nameof(TypeExprListConverter)} is read-only.");

    private static AstTypeExpr ReadSingleExpr(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => new AstTypePrimitiveExpr(reader.GetString()!),
            JsonTokenType.StartObject => ReadObjectExpr(ref reader, options),
            _ => throw new JsonException(
                $"Unexpected token {reader.TokenType} reading type expression"),
        };
    }

    private static AstTypeExpr ReadObjectExpr(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        // Parse the whole object so we can inspect its Type field without
        // manually navigating the Utf8JsonReader state machine.
        // After ParseValue, reader is positioned at the closing }.
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("Type", out JsonElement typeProp))
        {
            throw new JsonException($"Type expression object has no 'Type' field: {root.GetRawText()}");
        }

        // Constrained named type: {"Type": {inner-type-expr}, "Operator": {...}}
        // The base type is embedded in the "Type" field as an object rather than a string.
        // Parse the inner type expression and discard the constraint (not needed for code gen).
        if (typeProp.ValueKind == JsonValueKind.Object)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(typeProp.GetRawText());
            Utf8JsonReader innerReader = new(bytes);
            innerReader.Read(); // advance to StartObject
            return ReadObjectExpr(ref innerReader, options);
        }

        string typeName = typeProp.GetString() ?? throw new JsonException($"Type expression 'Type' field is null: {root.GetRawText()}");

        return typeName switch
        {
            // Named type reference: {"Type":"group","Value":"module.Name","Unwrapped":bool}
            "group" when root.TryGetProperty("Value", out _) => new AstTypeGroupRef(
                root.GetProperty("Value").GetString()!,
                root.GetProperty("Unwrapped").GetBoolean()),

            // Inline anonymous group: {"Type":"group","Name":"","Properties":[...]}
            "group" when root.TryGetProperty("Properties", out _) => new AstTypeInlineGroupExpr(
                root.GetProperty("Properties").Deserialize<List<AstPropertyItem>>(options) ?? []),

            "group" => throw new JsonException(
                $"'group' type expression has neither 'Value' nor 'Properties': {root.GetRawText()}"),

            "literal" => new AstTypeLiteralExpr(
                root.GetProperty("Value").ToString(),   // handles both string and number
                root.GetProperty("Unwrapped").GetBoolean()),

            "array" => new AstTypeArrayExpr(
                root.GetProperty("Name").GetString() ?? string.Empty,
                root.GetProperty("Values").Deserialize<List<AstPropertyEntry>>(options) ?? []),

            "range" => ParseRange(root),

            // Primitive types that carry an Operator (constraint or default):
            // {"Type":"float","Operator":{...}} or {"Type":"bool","Operator":{...}}
            "float" or "bool" or "text" => new AstTypeConstrainedPrimitiveExpr(typeName),

            _ => throw new JsonException($"Unknown type expression kind: '{typeName}'"),
        };
    }

    private static AstTypeRangeExpr ParseRange(JsonElement root)
    {
        JsonElement value = root.GetProperty("Value");
        JsonElement inclusive = value.GetProperty("Inclusive");
        JsonElement minVal = value.GetProperty("Min").GetProperty("Value");
        JsonElement maxVal = value.GetProperty("Max").GetProperty("Value");

        // Bounds may be integer or float (e.g. 0.0..1.0 for print margins)
        double min = minVal.ValueKind == JsonValueKind.Number ? minVal.GetDouble() : 0;
        double max = maxVal.ValueKind == JsonValueKind.Number ? maxVal.GetDouble() : 0;
        return new AstTypeRangeExpr(inclusive.GetBoolean(), min, max);
    }
}
