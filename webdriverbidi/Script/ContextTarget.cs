// <copyright file="ContextTarget.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object representing a script target that is a browsing context.
/// </summary>
public class ContextTarget : ScriptTarget
{
    private string browsingContextId;
    private string? sandbox;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextTarget"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context of the script target.</param>
    public ContextTarget(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the ID of the browsing context used as a script target.
    /// </summary>
    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the name of the sandbox.
    /// </summary>
    [JsonProperty("sandbox", NullValueHandling = NullValueHandling.Ignore)]
    public string? Sandbox { get => this.sandbox; internal set => this.sandbox = value; }
}