// <copyright file="HandleUserPromptCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.handleUserPrompt command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class HandleUserPromptCommandSettings : CommandData<EmptyResult>
{
    private string browsingContextId;
    private bool? accept;
    private string? userText;

    /// <summary>
    /// Initializes a new instance of the <see cref="HandleUserPromptCommandSettings" /> class.
    /// </summary>
    /// <param name="browsingContextId">The browsing context ID for which to handle the user prompt.</param>
    public HandleUserPromptCommandSettings(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.handleUserPrompt";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to handle the user prompt.
    /// </summary>
    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the user prompt should be accepted (if true) or canceled (if false).
    /// </summary>
    [JsonProperty("accept", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Accept { get => this.accept; set => this.accept = value; }

    /// <summary>
    /// Gets or sets the text sent to the user prompt.
    /// </summary>
    [JsonProperty("userText", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserText { get => this.userText; set => this.userText = value; }
}