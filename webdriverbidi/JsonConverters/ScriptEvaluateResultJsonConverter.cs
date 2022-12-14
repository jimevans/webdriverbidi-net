// <copyright file="ScriptEvaluateResultJsonConverter.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDriverBidi.Script;

/// <summary>
/// The JSON converter for the ScriptEvaluateResult object.
/// </summary>
public class ScriptEvaluateResultJsonConverter : JsonConverter<ScriptEvaluateResult>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns true for this converter (converter used for deserialization
    /// only).
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON values.
    /// Returns false for this converter (converter not used for
    /// serialization).
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, ScriptEvaluateResult? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override ScriptEvaluateResult ReadJson(JsonReader reader, Type objectType, ScriptEvaluateResult? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        if (jsonObject.ContainsKey("type"))
        {
            var typeToken = jsonObject["type"];
            if (typeToken is not null && typeToken.Type == JTokenType.String)
            {
                string resultType = typeToken.Value<string>() ?? string.Empty;
                if (resultType == "success")
                {
                    ScriptEvaluateResultSuccess successResult = new();
                    serializer.Populate(jsonObject.CreateReader(), successResult);
                    return successResult;
                }
                else if (resultType == "exception")
                {
                    ScriptEvaluateResultException exceptionResult = new();
                    serializer.Populate(jsonObject.CreateReader(), exceptionResult);
                    return exceptionResult;
                }
                else
                {
                    throw new WebDriverBidiException($"Malformed response: unknown type '{resultType}' for script result");
                }
            }
        }

        throw new WebDriverBidiException("Malformed response: Script response must contain a 'type' property that contains a non-null string value");
    }
}