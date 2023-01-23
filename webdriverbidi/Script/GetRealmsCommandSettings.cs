// <copyright file="GetRealmsCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the script.getRealms command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class GetRealmsCommandSettings : CommandData<GetRealmsCommandResult>
{
    private string? browsingContextId;
    private RealmType? realmType;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRealmsCommandSettings"/> class.
    /// </summary>
    public GetRealmsCommandSettings()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "script.getRealms";

    /// <summary>
    /// Gets or sets the ID of the browsing context of the realms to get.
    /// </summary>
    [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
    public string? BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the type of realms to get.
    /// </summary>
    public RealmType? RealmType { get => this.realmType; set => this.realmType = value; }

    /// <summary>
    /// Gets the type of the reamls to get for serialization purposes.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    internal string? SerializableRealmType
    {
        get
        {
            if (this.realmType is null)
            {
                return null;
            }

            string typeValue = this.realmType.Value.ToString().ToLowerInvariant();
            if (typeValue.IndexOf("worker") > 0)
            {
                typeValue = typeValue.Insert(typeValue.IndexOf("worker"), "-");
            }

            if (typeValue.IndexOf("worklet") > 0)
            {
                typeValue = typeValue.Insert(typeValue.IndexOf("worklet"), "-");
            }

            return typeValue;
        }
    }
}