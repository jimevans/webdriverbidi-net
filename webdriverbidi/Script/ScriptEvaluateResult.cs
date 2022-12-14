// <copyright file="ScriptEvaluateResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Base class for the result of a script evaluation.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(ScriptEvaluateResultJsonConverter))]
public class ScriptEvaluateResult : CommandResult
{
    private string realmId = string.Empty;
    private ScriptEvaluateResultType resultType = ScriptEvaluateResultType.Success;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptEvaluateResult"/> class.
    /// </summary>
    protected ScriptEvaluateResult()
    {
    }

    /// <summary>
    /// Gets the type of the result of the script execution.
    /// </summary>
    public ScriptEvaluateResultType ResultType => this.resultType;

    /// <summary>
    /// Gets the ID of the realm in which the script was executed.
    /// </summary>
    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }

    /// <summary>
    /// Gets or sets the type of the result of the script execution for serialization purposes.
    /// </summary>
    [JsonProperty("type")]
    [JsonRequired]
    internal string SerializableResultType
    {
        get
        {
            return this.resultType.ToString().ToLowerInvariant();
        }

        set
        {
            if (!Enum.TryParse<ScriptEvaluateResultType>(value, true, out ScriptEvaluateResultType type))
            {
                throw new WebDriverBidiException($"Malformed response: Invalid value {value} for RealmInfo 'type' property");
            }

            this.resultType = type;
        }
    }
}