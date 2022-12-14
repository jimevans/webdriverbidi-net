// <copyright file="NavigationEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for events raised during navigation.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class NavigationEventArgs : EventArgs
{
    private string? id;

    private string browsingContextId;

    private string url;

    private long epochTimestamp;

    private DateTime timestamp;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationEventArgs" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context being navigated.</param>
    /// <param name="url">The URL of the navigation.</param>
    /// <param name="timestamp">The timestamp of the navigation.</param>
    /// <param name="navigationId">The ID of the navigation.</param>
    [JsonConstructor]
    public NavigationEventArgs(string browsingContextId, string url, long timestamp, string? navigationId)
    {
        this.browsingContextId = browsingContextId;
        this.url = url;
        this.EpochTimestamp = timestamp;
        this.id = navigationId;
    }

    /// <summary>
    /// Gets the ID of the navigation operation.
    /// </summary>
    [JsonProperty("navigation")]
    public string? NavigationId { get => this.id; internal set => this.id = value; }

    /// <summary>
    /// Gets the ID of the browsing context being navigated.
    /// </summary>
    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the URL to which the browsing context is being navigated.
    /// </summary>
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }

    /// <summary>
    /// Gets the timestamp of the navigation.
    /// </summary>
    public DateTime Timestamp => this.timestamp;

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonProperty("timestamp")]
    [JsonRequired]
    public long EpochTimestamp
    {
        get
        {
            return this.epochTimestamp;
        }

        private set
        {
            this.epochTimestamp = value;
            this.timestamp = DateTime.UnixEpoch.AddMilliseconds(value);
        }
    }
}