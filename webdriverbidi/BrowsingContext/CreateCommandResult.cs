// <copyright file="CreateCommandResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Result for creating a new browsing context using the browserContext.create command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CreateCommandResult : ResponseData
{
    private string contextId = string.Empty;

    [JsonConstructor]
    private CreateCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the created browsing context.
    /// </summary>
    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.contextId; internal set => this.contextId = value; }
}