// <copyright file="PropertiesConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts the Properties array inside a group node.
///
/// Each element of the array is either:
///   - A JSON object {...}  — a single AstPropertyEntry
///   - A JSON array  [...]  — a CDDL choice group, modelled as AstPropertyChoice
///
/// Both are returned as List&lt;AstPropertyItem&gt;.
/// </summary>
public class PropertiesConverter : JsonConverter<List<AstPropertyItem>>
{
    /// <summary>
    /// Reads the JSON value of a group's Properties field and converts it to a list of property items.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The list of property items.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public override List<AstPropertyItem> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected StartArray at start of Properties, got {reader.TokenType}");
        }

        List<AstPropertyItem> result = [];

        // consume outer StartArray
        reader.Read();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                // A nested array: CDDL choice group
                List<AstPropertyEntry> choiceOptions = [];

                // consume inner StartArray
                reader.Read();

                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    AstPropertyEntry entry = JsonSerializer.Deserialize<AstPropertyEntry>(ref reader, options)!;
                    choiceOptions.Add(entry);

                    // advance past EndObject of this entry
                    reader.Read();
                }

                // reader is at inner EndArray
                result.Add(new AstPropertyChoice(choiceOptions));
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                AstPropertyEntry entry = JsonSerializer.Deserialize<AstPropertyEntry>(ref reader, options)!;
                result.Add(entry);

                // reader is at EndObject of this entry
            }
            else
            {
                throw new JsonException($"Unexpected token {reader.TokenType} inside Properties array");
            }

            // advance to next element or outer EndArray
            reader.Read();
        }

        // reader is at outer EndArray
        return result;
    }

    /// <summary>
    /// Writes a list of property items to JSON. Not supported since this converter is only used for deserialization.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The list of property items.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <exception cref="NotSupportedException">Thrown when attempting to write JSON.</exception>
    public override void Write(Utf8JsonWriter writer, List<AstPropertyItem> value, JsonSerializerOptions options)
        => throw new NotSupportedException($"{nameof(PropertiesConverter)} is read-only.");
}
